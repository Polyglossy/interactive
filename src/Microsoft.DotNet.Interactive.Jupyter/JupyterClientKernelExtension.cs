// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Polyglossy.Interactive.Commands;
using Polyglossy.Interactive.CSharp;
using Polyglossy.Interactive.Events;
using Polyglossy.Interactive.PowerShell;

namespace Polyglossy.Interactive.Jupyter;

public class JupyterClientKernelExtension
{
    public static Task LoadAsync(Kernel kernel)
    {
        if (kernel is CompositeKernel root)
        {
            root.Add(
                new JavaScriptKernel(),
                new[] { "js" });

            root.VisitSubkernels(k =>
            {
                switch (k)
                {
                    case CSharpKernel csharpKernel:
                        csharpKernel.UseJupyterHelpers();
                        break;

                    case PowerShellKernel powerShellKernel:
                        powerShellKernel.UseJupyterHelpers();
                        break;
                }
            });

            root.SetDefaultTargetKernelNameForCommand(
                typeof(RequestInput),
                root.Name);

            root.RegisterCommandHandler<RequestInput>((input, context) =>
            {
                string result;

                if (!input.IsPassword)
                {
                    result = TopLevelMethods.input(input.Prompt);
                }
                else
                {
                    result = TopLevelMethods.password(input.Prompt).GetClearTextPassword();
                }

                context.Publish(new InputProduced(result, input));

                return Task.CompletedTask;
            });
        }

        return Task.CompletedTask;
    }
}