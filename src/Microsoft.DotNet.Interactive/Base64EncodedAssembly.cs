using System;

namespace Polyglossy.Interactive;

public class Base64EncodedAssembly
{
    public string Value { get; }

    public Base64EncodedAssembly(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }
}