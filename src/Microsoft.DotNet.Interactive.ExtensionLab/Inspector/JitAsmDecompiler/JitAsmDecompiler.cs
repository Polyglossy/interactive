/*
Copyright (c) 2016-2017, Andrey Shchekin
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Iced.Intel;

using Microsoft.Diagnostics.Runtime;

namespace Microsoft.DotNet.Interactive.ExtensionLab.Inspector.JitAsmDecompiler;

internal sealed class JitAsmDecompiler
{
    private static readonly FormatterOptions FormatterOptions = new FormatterOptions
    {
        HexPrefix = "0x",
        HexSuffix = null,
        UppercaseHex = false,
        SpaceAfterOperandSeparator = true
    };

    public static string Decompile(Stream dll)
    {
        if (dll is null)
            throw new ArgumentNullException(nameof(dll));

        dll.Seek(0, SeekOrigin.Begin);

        var jitDecompilation = new StringWriter();
        var jitDecompiler = new JitAsmDecompiler();
        jitDecompiler.Decompile(dll, jitDecompilation);

        return jitDecompilation.ToString();
    }

    private void Decompile(Stream dll, TextWriter codeWriter)
    {
        using (var loadContext = new CustomAssemblyLoadContext(shouldShareAssembly: _ => true))
        {
            var assembly = loadContext.LoadFromStream(dll);
            ValidateStaticConstructors(assembly);

            // Everything has to be JIT-compiled *before* the snapshot of this process is taken:
            // ClrMD cannot attach to its own process, so it works on a point-in-time snapshot
            // that cannot see methods compiled after it was created.
            foreach (var type in assembly.DefinedTypes)
            {
                if (type.IsNested)
                    continue;
                PrepareMembers(type);
            }

            DataTarget dataTarget;
            ClrRuntime runtime;
            try
            {
                using var currentProcess = Process.GetCurrentProcess();
                dataTarget = DataTarget.CreateSnapshotAndAttach(currentProcess.Id);
                try
                {
                    runtime = dataTarget.ClrVersions.First(c => c.Flavor == ClrFlavor.Core).CreateRuntime();
                }
                catch
                {
                    dataTarget.Dispose();
                    throw;
                }
            }
            catch (Exception exception)
            {
                codeWriter.WriteLine($"; JIT assembly is not available: unable to inspect the current process ({exception.GetType().Name}: {exception.Message}).");
                return;
            }

            // The snapshot is only valid while the DataTarget is alive; the runtime is disposed before it.
            using (dataTarget)
            using (runtime)
            {
                var context = new JitWriteContext(codeWriter, runtime);

                WriteJitInfo(runtime.ClrInfo, codeWriter);
                foreach (var type in assembly.DefinedTypes)
                {
                    if (type.IsNested)
                        continue; // it's easier to handle nested generic types recursively, so we suppress all nested for consistency
                    DisassembleAndWriteMembers(context, type);
                }
            }
        }
    }

    private static void PrepareMembers(TypeInfo type)
    {
        if (type.IsGenericTypeDefinition)
            return; // open generics are not JIT-compiled, see DisassembleAndWriteMethod

        foreach (var constructor in type.DeclaredConstructors)
        {
            PrepareMethod(constructor);
        }

        foreach (var method in type.DeclaredMethods)
        {
            if (method.IsAbstract)
                continue;
            PrepareMethod(method);
        }

        foreach (var nested in type.DeclaredNestedTypes)
        {
            PrepareMembers(nested);
        }
    }

    private static void PrepareMethod(MethodBase method)
    {
        // Mirrors the cases DisassembleAndWriteMethod actually disassembles.
        if ((method.MethodImplementationFlags & MethodImplAttributes.Runtime) == MethodImplAttributes.Runtime)
            return;

        if (method.IsGenericMethodDefinition || (method.DeclaringType?.IsGenericTypeDefinition ?? false))
            return;

        RuntimeHelpers.PrepareMethod(method.MethodHandle);
    }

    private void ValidateStaticConstructors(Assembly assembly)
    {
        foreach (var type in assembly.DefinedTypes)
        {
            foreach (var constructor in type.DeclaredConstructors)
            {
                if (constructor.IsStatic)
                    throw new NotSupportedException($"Type {type} has a static constructor, which is not supported by SharpLab JIT decompiler.");
            }
        }
    }

    private void WriteJitInfo(ClrInfo clr, TextWriter writer)
    {
        writer.WriteLine(
            "; {0:G} CLR {1} on {2}",
            clr.Flavor, clr.Version, clr.DebuggingLibraries.First().TargetArchitecture.ToString("G").ToLowerInvariant()
        );
    }

    private void DisassembleAndWriteMembers(JitWriteContext context, TypeInfo type, ImmutableArray<Type>? genericArgumentTypes = null)
    {
        if (type.IsGenericTypeDefinition)
        {
            if (TryDisassembleAndWriteMembersOfGeneric(context, type, genericArgumentTypes))
                return;
        }

        foreach (var constructor in type.DeclaredConstructors)
        {
            DisassembleAndWriteMethod(context, constructor);
        }

        foreach (var method in type.DeclaredMethods)
        {
            if (method.IsAbstract)
                continue;
            DisassembleAndWriteMethod(context, method);
        }

        foreach (var nested in type.DeclaredNestedTypes)
        {
            DisassembleAndWriteMembers(context, nested, genericArgumentTypes);
        }
    }

    private bool TryDisassembleAndWriteMembersOfGeneric(JitWriteContext context, TypeInfo type, ImmutableArray<Type>? parentArgumentTypes = null)
    {
        if (parentArgumentTypes is not null)
        {
            var genericInstance = type.MakeGenericType(parentArgumentTypes.Value.ToArray());
            DisassembleAndWriteMembers(context, genericInstance.GetTypeInfo(), parentArgumentTypes);
            return true;
        }

        return false;
    }

    private void DisassembleAndWriteMethod(JitWriteContext context, MethodBase method)
    {
        if ((method.MethodImplementationFlags & MethodImplAttributes.Runtime) == MethodImplAttributes.Runtime)
        {
            WriteSignatureFromReflection(context, method);
            context.Writer.WriteLine("    ; Cannot produce JIT assembly for runtime-implemented method.");
            return;
        }

        if (method.DeclaringType?.IsGenericTypeDefinition ?? false)
        {
            WriteIgnoredOpenGeneric(context, method);
            return;
        }

        if (method.IsGenericMethodDefinition)
        {
            DisassembleAndWriteGenericMethod(context, (MethodInfo)method);
            return;
        }

        DisassembleAndWriteSimpleMethod(context, method);
    }

    private void DisassembleAndWriteGenericMethod(JitWriteContext context, MethodInfo method) => WriteIgnoredOpenGeneric(context, method);

    private void DisassembleAndWriteSimpleMethod(JitWriteContext context, MethodBase method)
    {
        // The method was already JIT-compiled by PrepareMembers, before the snapshot was taken.
        var handle = method.MethodHandle;

        var clrMethod = context.Runtime.GetMethodByHandle((ulong)handle.Value.ToInt64());
        var regions = FindNonEmptyHotColdInfo(clrMethod);

        if (clrMethod is null || regions is null)
        {
            var address = (ulong)handle.GetFunctionPointer().ToInt64();
            clrMethod = context.Runtime.GetMethodByInstructionPointer(address);
            regions = FindNonEmptyHotColdInfo(clrMethod);
        }

        var writer = context.Writer;
        if (clrMethod is not null)
        {
            writer.WriteLine();
            writer.WriteLine(clrMethod.Signature);
        }
        else
        {
            WriteSignatureFromReflection(context, method);
        }

        if (regions is null)
        {
            if (method.IsGenericMethod || (method.DeclaringType?.IsGenericType ?? false))
            {
                writer.WriteLine("    ; Failed to find HotColdInfo for generic method (reference types?).");
                return;
            }
            writer.WriteLine("    ; Failed to find HotColdRegions.");
            return;
        }

        var methodAddress = regions.Value.HotStart;
        var methodLength = regions.Value.HotSize;

        var reader = new MemoryCodeReader(new IntPtr(unchecked((long)methodAddress)), methodLength);
        var decoder = Decoder.Create(MapArchitectureToBitness(context.Runtime.DataTarget.DataReader.Architecture), reader);

        var instructions = new InstructionList();
        decoder.IP = methodAddress;
        while (decoder.IP < (methodAddress + methodLength))
        {
            decoder.Decode(out instructions.AllocUninitializedElement());
        }

        var resolver = new JitAsmSymbolResolver(context.Runtime, methodAddress, methodLength);
        var formatter = new IntelFormatter(FormatterOptions, resolver);
        var output = new StringOutput();
        foreach (ref var instruction in instructions)
        {
            formatter.Format(instruction, output);

            writer.Write("    L");
            writer.Write((instruction.IP - methodAddress).ToString("x4"));
            writer.Write(": ");
            writer.WriteLine(output.ToStringAndReset());
        }
    }

    private void WriteIgnoredOpenGeneric(JitWriteContext context, MethodBase method)
    {
        WriteSignatureFromReflection(context, method);
        var writer = context.Writer;
        writer.WriteLine("    ; Open generics cannot be JIT-compiled.");
    }

    private static void WriteSignatureFromReflection(JitWriteContext context, MethodBase method)
    {
        context.Writer.WriteLine();

        var md = (ulong)method.MethodHandle.Value.ToInt64();
        var signature = context.Runtime.GetMethodByHandle(md)?.Signature;

        context.Writer.WriteLine(signature ?? "Unknown Method");
    }

    private HotColdRegions? FindNonEmptyHotColdInfo(ClrMethod method)
    {
        if (method is null)
            return null;

        // I can't really explain this, but it seems that some methods
        // are present multiple times in the same type -- one compiled
        // and one not compiled. A bug in clrmd?
        if (method.HotColdInfo.HotSize > 0)
            return method.HotColdInfo;

        if (method.Type is null)
            return null;

        var methodSignature = method.Signature;
        foreach (var other in method.Type.Methods)
        {
            if (other.MetadataToken == method.MetadataToken && other.Signature == methodSignature && other.HotColdInfo.HotSize > 0)
                return other.HotColdInfo;
        }

        return null;
    }

    private int MapArchitectureToBitness(Architecture architecture) => architecture switch
    {
        Architecture.X64 => 64,
        Architecture.X86 => 32,
        _ => throw new Exception($"Unsupported architecture {architecture}.")
    };

    private class JitWriteContext
    {
        public JitWriteContext(TextWriter writer, ClrRuntime runtime)
        {
            Writer = writer;
            Runtime = runtime;
        }

        public TextWriter Writer { get; }
        public ClrRuntime Runtime { get; }
    }
}