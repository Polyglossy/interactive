// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import * as path from 'path';
import * as Mocha from 'mocha';
import * as glob from 'glob';

const cwd = process.cwd();

export function run(): Promise<void> {
    const mocha = new Mocha({
        ui: 'bdd',
        color: true,
        timeout: 10000,
    });

    const testsRoot = path.resolve(cwd, 'out', 'tests');
    const testFiles = glob.sync('**/*.test.js', { cwd: testsRoot });

    for (const file of testFiles) {
        mocha.addFile(path.join(testsRoot, file));
    }

    return new Promise((resolve, reject) => {
        try {
            mocha.run((failures: number) => {
                if (failures > 0) {
                    reject(new Error(`${failures} test(s) failed.`));
                    return;
                }

                resolve();
            });
        } catch (err) {
            reject(err);
        }
    });
}
