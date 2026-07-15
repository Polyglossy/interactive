// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import * as path from 'path';
import { runTests } from 'vscode-test';

function getVscodeExecutablePath() {
    return process.env.VSCODE_EXECUTABLE_PATH ?? 'C:/Windows/System32/WindowsPowerShell/v1.0/powershell.exe';
}

function getLaunchArgs() {
    const configuredPath = process.env.VSCODE_EXECUTABLE_PATH;
    if (configuredPath) {
        return ['-NoProfile', '-Command', `& '${configuredPath}'`];
    }

    const defaultPath = 'C:/Program Files/Microsoft VS Code Insiders/bin/code-insiders.cmd';
    return ['-NoProfile', '-Command', `& '${defaultPath}'`];
}

async function main() {
    const extensionDevelopmentPath = path.resolve(__dirname, '..', '..', '..');
    const extensionTestsPath = path.resolve(__dirname, 'suite', 'index');
    const vscodeExecutablePath = getVscodeExecutablePath();

    try {
        await runTests({
            extensionDevelopmentPath,
            extensionTestsPath,
            vscodeExecutablePath,
            launchArgs: [...getLaunchArgs(), '--disable-extensions'],
        });
    } catch (error) {
        console.error('Failed to run extension tests.');
        console.error(error);
        process.exit(1);
    }
}

main();
