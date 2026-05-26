// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { expect } from "chai";
import { createUuid } from "../src/uuid";

describe("createUuid", () => {
    let originalCrypto: Crypto | undefined;

    beforeEach(() => {
        originalCrypto = globalThis.crypto;
    });

    afterEach(() => {
        Object.defineProperty(globalThis, "crypto", {
            configurable: true,
            value: originalCrypto
        });
    });

    it("uses randomUUID when available", () => {
        Object.defineProperty(globalThis, "crypto", {
            configurable: true,
            value: {
                randomUUID: () => "11111111-1111-4111-8111-111111111111"
            }
        });

        expect(createUuid()).to.equal("11111111-1111-4111-8111-111111111111");
    });

    it("uses getRandomValues to create a version 4 UUID when randomUUID is unavailable", () => {
        Object.defineProperty(globalThis, "crypto", {
            configurable: true,
            value: {
                getRandomValues: (bytes: Uint8Array) => {
                    for (let i = 0; i < bytes.length; i++) {
                        bytes[i] = i;
                    }
                    return bytes;
                }
            }
        });

        const uuid = createUuid();

        expect(uuid).to.equal("00010203-0405-4607-8809-0a0b0c0d0e0f");
        expect(uuid).to.match(/^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/);
    });

    it("throws when no cryptographic UUID source is available", () => {
        Object.defineProperty(globalThis, "crypto", {
            configurable: true,
            value: undefined
        });

        expect(() => createUuid()).to.throw("crypto.randomUUID or crypto.getRandomValues");
    });
});