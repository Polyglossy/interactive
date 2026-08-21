// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Assent;
using Polyglossy.Interactive.CSharp;
using Polyglossy.Interactive.CSharpProject;
using Polyglossy.Interactive.Documents;
using Polyglossy.Interactive.DuckDB;
using Polyglossy.Interactive.Formatting;
using Polyglossy.Interactive.FSharp;
using Polyglossy.Interactive.Http;
using Polyglossy.Interactive.Journey;
using Polyglossy.Interactive.Jupyter;
using Polyglossy.Interactive.Kql;
using Polyglossy.Interactive.Mermaid;
using Polyglossy.Interactive.PackageManagement;
using Polyglossy.Interactive.PowerShell;
using Polyglossy.Interactive.SQLite;
using Polyglossy.Interactive.SqlServer;
using Polyglossy.Interactive.Tests.Utility;
using Xunit;

namespace Polyglossy.Interactive.ApiCompatibility.Tests;

[Trait("Category", "Contracts and serialization")]
public class ApiCompatibilityTests
{
    private readonly Configuration _configuration;

    public ApiCompatibilityTests()
    {
        _configuration = new Configuration()
                         .SetInteractive(Debugger.IsAttached)
                         .UsingExtension("txt");
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void Interactive_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<Kernel>();
        this.Assent(contract, _configuration);
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void Formatting_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<FormatContext>();
        this.Assent(contract, _configuration);
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void Document_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<InteractiveDocument>();
        this.Assent(contract, _configuration);
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void PackageManagement_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<PackageRestoreContext>();
        this.Assent(contract, _configuration);
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void Journey_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<Lesson>();
        this.Assent(contract, _configuration);
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void csharp_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<CSharpKernel>();
        this.Assent(contract, _configuration);
    }

    [Fact(Skip = "this api is in early design stage.")]
    public void csharpProject_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<CSharpProjectKernel>();
        this.Assent(contract, _configuration);
    }

    [Fact(Skip = "need to use signature files")]
    public void fsharp_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<FSharpKernel>();
        this.Assent(contract, _configuration);
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void powershell_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<PowerShellKernel>();
        this.Assent(contract, _configuration);
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void sqLite_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<SQLiteKernel>();
        this.Assent(contract, _configuration);
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void mssql_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<MsSqlKernelExtension>();
        this.Assent(contract, _configuration);
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void kql_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<KqlKernelExtension>();
        this.Assent(contract, _configuration);
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void mermaid_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<MermaidKernel>();
        this.Assent(contract, _configuration);
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void jupyter_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<ConnectionInformation>();
        this.Assent(contract, _configuration);
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void httpRequest_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<HttpKernel>();
        this.Assent(contract, _configuration);
    }

    [FactSkipLinux("Testing api contract changes, not needed on Linux too")]
    public void DuckDB_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContract<DuckDBKernel>();
        this.Assent(contract, _configuration);
    }
}