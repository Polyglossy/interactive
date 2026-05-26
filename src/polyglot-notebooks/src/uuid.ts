// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export function createUuid(): string {
    const randomUuid = globalThis.crypto?.randomUUID?.();
    if (randomUuid) {
        return randomUuid;
    }

    const getRandomValues = globalThis.crypto?.getRandomValues?.bind(globalThis.crypto);
    if (!getRandomValues) {
        throw new Error("Unable to create a UUID because this environment does not provide crypto.randomUUID or crypto.getRandomValues.");
    }

    const bytes = new Uint8Array(16);
    getRandomValues(bytes);

    bytes[6] = (bytes[6] & 0x0f) | 0x40;
    bytes[8] = (bytes[8] & 0x3f) | 0x80;

    return formatUuid(bytes);
}

function formatUuid(bytes: Uint8Array): string {
    const hex = Array.from(bytes, b => b.toString(16).padStart(2, "0"));
    return `${hex.slice(0, 4).join("")}-${hex.slice(4, 6).join("")}-${hex.slice(6, 8).join("")}-${hex.slice(8, 10).join("")}-${hex.slice(10, 16).join("")}`;
}