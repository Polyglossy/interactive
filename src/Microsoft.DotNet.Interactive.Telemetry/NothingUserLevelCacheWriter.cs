// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Polyglossy.Interactive.Telemetry;

public sealed class NothingUserLevelCacheWriter : IUserLevelCacheWriter
{
    public string RunWithCache(string cacheKey, Func<string> getValueToCache)
    {
        return getValueToCache();
    }
}