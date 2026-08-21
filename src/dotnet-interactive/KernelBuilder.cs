// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.Json;
using Polyglossy.Interactive.App.CommandLine;
using Polyglossy.Interactive.App.Connection;
using Polyglossy.Interactive.Connection;
using Polyglossy.Interactive.CSharp;
using Polyglossy.Interactive.Formatting;
using Polyglossy.Interactive.FSharp;
using Polyglossy.Interactive.Http;
using Polyglossy.Interactive.Jupyter;
using Polyglossy.Interactive.Mermaid;
using Polyglossy.Interactive.PowerShell;
using Polyglossy.Interactive.Telemetry;
using Pocket;
using static Polyglossy.Interactive.App.CodeExpansion;
using static Pocket.Logger;
using Formatter = Polyglossy.Interactive.Formatting.Formatter;

namespace Polyglossy.Interactive.App;

public static class KernelBuilder
{
    internal static CompositeKernel CreateKernel(
        string defaultKernelName,
        FrontendEnvironment frontendEnvironment,
        StartupOptions startupOptions,
        TelemetrySender telemetrySender = null)
    {
        using var _ = Log.OnEnterAndExit("Creating kernels");

        var compositeKernel = new CompositeKernel();
        compositeKernel.FrontendEnvironment = frontendEnvironment;

        compositeKernel.Add(
            new CSharpKernel()
                .UseNugetDirective()
                .UseKernelHelpers()
                .UseWho()
                .UseValueSharing(),
            ["c#", "C#"]);

        compositeKernel.Add(
            new FSharpKernel()
                .UseDefaultFormatting()
                .UseNugetDirective()
                .UseKernelHelpers()
                .UseWho()
                .UseValueSharing(),
            ["f#", "F#"]);

        var powerShellKernel = new PowerShellKernel()
                               .UseProfiles()
                               .UseValueSharing();
        compositeKernel.Add(
            powerShellKernel,
            ["powershell"]);

        compositeKernel.Add(
            new HtmlKernel());

        compositeKernel.Add(
            new KeyValueStoreKernel()
                .UseWho());

        compositeKernel.Add(
            new MermaidKernel());

        compositeKernel.Add(
            new HttpKernel()
                .UseValueSharing());

        var secretManager = new SecretManager(powerShellKernel);

        var kernel = compositeKernel
                     .UseDefaultMagicCommands()
                     .UseAboutMagicCommand()
                     .UseImportMagicCommand()
                     .UseSecretManager(secretManager)
                     .UseFormsForMultipleInputs(secretManager)
                     .UseNuGetExtensions(telemetrySender)
                     .UseCodeExpansions(GetCodeExpansionConfiguration(secretManager));

        kernel.AddConnectDirective(new ConnectSignalRDirective());
        kernel.AddConnectDirective(new ConnectStdIoDirective(startupOptions.KernelHostUri));

        kernel.AddConnectDirective(
            new ConnectJupyterKernelDirective()
                .AddConnectionOptions(new JupyterHttpKernelConnectionOptions())
                .AddConnectionOptions(new JupyterLocalKernelConnectionOptions()));

        SetUpFormatters(frontendEnvironment);

        kernel.DefaultKernelName = defaultKernelName;

        if (telemetrySender is not null)
        {
            kernel.UseTelemetrySender(telemetrySender);
        }

        return kernel;
    }

    private static CodeExpansionConfiguration GetCodeExpansionConfiguration(
        SecretManager secretManager)
    {
        return new(GetDataKernelCodeExpansions(), new JupyterKernelSpecModule())
        {
            GetRecentConnections = () => GetRecentConnectionListFromSecretManager(secretManager),
            SaveRecentConnections = list => SaveRecentConnectionListToSecretManager(list, secretManager)
        };
    }

    private static RecentConnectionList GetRecentConnectionListFromSecretManager(
        SecretManager secretManager)
    {
        RecentConnectionList recentlyConnections;

        if (secretManager.TryGetValue("dotnet-interactive.RecentlyUsedConnections", out var json))
        {
            recentlyConnections = JsonSerializer.Deserialize<RecentConnectionList>(json, Serializer.JsonSerializerOptions);
        }
        else
        {
            recentlyConnections = new();
        }

        return recentlyConnections;
    }

    private static void SaveRecentConnectionListToSecretManager(
        RecentConnectionList list, 
        SecretManager secretManager)
    {
        var json = JsonSerializer.Serialize(list, Serializer.JsonSerializerOptions);
        secretManager.SetValue("dotnet-interactive.RecentlyUsedConnections", json);
    }

    public static IEnumerable<CodeExpansion> GetDataKernelCodeExpansions()
    {
        return [
            new([
                    new("""
                        #r "nuget:Polyglossy.Interactive.Kql, *-*"
                        """, "csharp"),
                    new("""
                        #!connect kql --kernel-name @input --cluster @input --database @input
                        """, "csharp")
                ],
                new("Kusto Query Language", CodeExpansionKind.DataConnection)),
            new([
                    new("""
                        #r "nuget:Polyglossy.Interactive.SqlServer, *-*"
                        """, "csharp"),
                    new("""
                        #!connect mssql --kernel-name @input --connection-string @password
                        """, "csharp")
                ],
                new("Microsoft SQL Database", CodeExpansionKind.DataConnection)),
        ];
    }

    internal static void SetUpFormatters(FrontendEnvironment frontendEnvironment)
    {
        if (frontendEnvironment is BrowserFrontendEnvironment)
        {
            Formatter.DefaultMimeType = HtmlFormatter.MimeType;
            Formatter.SetPreferredMimeTypesFor(typeof(string), PlainTextFormatter.MimeType);
        }
    }
}