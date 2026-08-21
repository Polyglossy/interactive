// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Polyglossy.Interactive.Commands;

namespace Polyglossy.Interactive.Events;

public class CodeSubmissionReceived : KernelEvent
{
    public CodeSubmissionReceived(SubmitCode command) : base(command)
    {
    }

    public string Code => ((SubmitCode)Command).Code;

    public override string ToString() => $"{base.ToString()}: {Code.TruncateForDisplay()}";
}