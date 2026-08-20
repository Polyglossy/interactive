// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { expect } from 'chai';
import { createUuid } from '../src/commandsAndEvents';

describe('commandsAndEvents', () => {
    const originalCryptoDescriptor = Object.getOwnPropertyDescriptor(globalThis, 'crypto');

    function setCrypto(value: any) {
        Object.defineProperty(globalThis, 'crypto', {
            value,
            configurable: true,
        });
    }

    afterEach(() => {
        if (originalCryptoDescriptor) {
            Object.defineProperty(globalThis, 'crypto', originalCryptoDescriptor);
        } else {
            delete (globalThis as { crypto?: Crypto }).crypto;
        }
    });

    it('createUuid uses crypto.randomUUID when available', () => {
        setCrypto({
            randomUUID: () => '12345678-1234-4234-8234-123456789abc',
        });

        expect(createUuid()).to.equal('12345678-1234-4234-8234-123456789abc');
    });

    it('createUuid falls back to crypto.getRandomValues when randomUUID is unavailable', () => {
        setCrypto({
            getRandomValues: (values: Uint8Array) => {
                values.set([
                    0x00, 0x11, 0x22, 0x33,
                    0x44, 0x55, 0x06, 0x77,
                    0x88, 0x99, 0xaa, 0xbb,
                    0xcc, 0xdd, 0xee, 0xff,
                ]);
                return values;
            },
        });

        expect(createUuid()).to.equal('00112233-4455-4677-8899-aabbccddeeff');
    });
});
