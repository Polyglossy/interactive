// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Tags;
using Polyglossy.Interactive.Directives;
using Polyglossy.Interactive.Events;
using Polyglossy.Interactive.Jupyter.Connection;
using Polyglossy.Interactive.Jupyter.ZMQ;

namespace Polyglossy.Interactive.Jupyter;

public sealed class JupyterLocalKernelConnectionOptions : IJupyterKernelConnectionOptions
{
    private readonly IReadOnlyCollection<KernelDirectiveParameter> _parameters;

    public JupyterLocalKernelConnectionOptions()
    {
        CondaEnv.AddCompletions(async context =>
        {
            foreach (var name in await CondaEnvironment.GetEnvironmentNamesAsync())
            {
                context.CompletionItems.Add(new CompletionItem(name, WellKnownTags.Parameter));
            }
        });

        _parameters = new List<KernelDirectiveParameter>
        {
            CondaEnv
        };
    }

    public KernelDirectiveParameter CondaEnv { get; } = new("--conda-env", "The Conda environment to use. (The default is base.)");

    public IJupyterConnection GetConnection(ConnectJupyterKernel connectCommand)
    {
        var condaEnv = connectCommand.CondaEnv;
        IJupyterEnvironment environment = null;
        if (condaEnv is not null)
        {
            environment = new CondaEnvironment(condaEnv);
        }

        return new JupyterConnection(new JupyterKernelSpecModule(environment));
    }

    public IReadOnlyCollection<KernelDirectiveParameter> GetParameters()
    {
        return _parameters;
    }
}