// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import './vscodeTestSetup';
import { run } from './suite';

async function main() {
    try {
        await run();
    } catch (error) {
        console.error('Failed to run extension tests.');
        console.error(error);
        process.exit(1);
    }
}

main();
