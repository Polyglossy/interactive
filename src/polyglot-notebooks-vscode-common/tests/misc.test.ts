// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { expect } from 'chai';
import * as fs from 'fs';
import * as path from 'path';
import * as vscode from 'vscode';
import { DisplayElement, ErrorElement, TextElement } from '../../src/vscode-common/polyglot-notebooks/contracts';
import { isDisplayOutput, isErrorOutput, isTextOutput, reshapeOutputValueForVsCode } from '../../src/vscode-common/interfaces/utilities';
import { createOutput, createUri, debounce, executeSafe, getConfigurationValue, getVersionNumber, getWorkingDirectoryForNotebook, parse, processArguments, stringify, toolManifestExists } from '../../src/vscode-common/utilities';
import { decodeToString, withFakeGlobalStorageLocation } from './utilities';

import * as constants from '../../src/vscode-common/constants';
import * as vscodeLike from '../../src/vscode-common/interfaces/vscode-like';
import { areEquivalentObjects, sortInPlace } from '../../src/vscode-common/metadataUtilities';

describe('Miscellaneous tests', () => {
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

    it(`verify command and argument replacement is as expected`, () => {
        let template = {
            args: [
                '{dotnet_path}',
                'tool',
                'run',
                'polyglossy-interactive',
                '--',
                'stdio',
                '--working-dir',
                '{working_dir}'
            ],
            workingDirectory: '{global_storage_path}'
        };
        let actual = processArguments(template, 'replacement-working-dir', 'replacement-dotnet-path', 'replacement-global-storage-path');
        expect(actual).to.deep.equal({
            command: 'replacement-dotnet-path',
            args: [
                'tool',
                'run',
                'polyglossy-interactive',
                '--',
                'stdio',
                '--working-dir',
                'replacement-working-dir'
            ],
            workingDirectory: 'replacement-global-storage-path',
            env: {}
        });
    });

    it('notebook working directory comes from notebook uri if local', () => {
        const notebookUri = createUri('path/to/notebook.dib');
        const workspaceFolderUris = [
            createUri('not/used/1'),
            createUri('not/used/2'),
        ];
        const workingDir = getWorkingDirectoryForNotebook(notebookUri, workspaceFolderUris, 'fallback-not-used');
        expect(workingDir).to.equal('path/to');
    });

    it('notebook working directory comes from local workspace if notebook is untitled', () => {
        const notebookUri = createUri('path/to/notebook.dib', 'untitled');
        const workspaceFolderUris = [
            createUri('not/used', 'remote'),
            createUri('this/is/local/and/used'),
        ];
        const workingDir = getWorkingDirectoryForNotebook(notebookUri, workspaceFolderUris, 'fallback-not-used');
        expect(workingDir).to.equal('this/is/local/and/used');
    });

    it('dib notebooks prefer the polyglossy view type', () => {
        expect(constants.getNotebookViewTypeForFormat('dib')).to.equal(constants.PolyglossyNotebookViewType);
    });

    it('extension manifests advertise the polyglossy notebook identity for .dib files', () => {
        const workspaceRoot = path.resolve(__dirname, '..', '..', '..', '..', '..');
        const manifestPaths = [
            path.join(workspaceRoot, 'src', 'polyglot-notebooks-vscode', 'package.json'),
            path.join(workspaceRoot, 'src', 'polyglot-notebooks-vscode-insiders', 'package.json')
        ];

        for (const manifestPath of manifestPaths) {
            const manifest = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
            const activationEvents = manifest.activationEvents ?? [];
            const notebookContributions = manifest.contributes?.notebooks ?? [];

            expect(activationEvents, `${manifestPath} should activate for polyglossy notebook view types`).to.include('onNotebook:polyglossy-notebook');
            expect(notebookContributions.some((contribution: { type: string; selector?: Array<{ filenamePattern?: string }> }) => contribution.type === 'polyglossy-notebook' && contribution.selector?.some(({ filenamePattern }) => filenamePattern === '*.dib')), `${manifestPath} should contribute a .dib notebook type for the polyglossy notebook identity`).to.be.true;
        }
    });

    it('extension manifests advertise the polyglossy command and settings identities', () => {
        const workspaceRoot = path.resolve(__dirname, '..', '..', '..', '..', '..');
        const manifestPaths = [
            path.join(workspaceRoot, 'src', 'polyglot-notebooks-vscode', 'package.json'),
            path.join(workspaceRoot, 'src', 'polyglot-notebooks-vscode-insiders', 'package.json')
        ];

        const expectedCommands = [
            'polyglossy-notebook.openNotebook',
            'polyglossy-notebook.saveAsNotebook',
            'polyglossy-notebook.fileNew',
            'polyglossy-notebook.newNotebook',
            'polyglossy-notebook.newNotebookNoDefaults',
            'polyglossy-notebook.setNewNotebookDefaults',
            'polyglossy-notebook.restartCurrentNotebookKernel',
            'polyglossy-notebook.stopCurrentNotebookKernel',
            'polyglossy-notebook.stopAllNotebookKernels',
            'polyglossy-notebook.shareValueWith',
            'polyglossy-notebook.notebookEditor.restartKernel',
            'polyglossy-notebook.notebookEditor.openValueViewer',
        ];

        for (const manifestPath of manifestPaths) {
            const manifest = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
            const activationEvents = manifest.activationEvents ?? [];
            const commands = manifest.contributes?.commands ?? [];
            const configurationProperties = manifest.contributes?.configuration?.properties ?? {};

            expect(activationEvents, `${manifestPath} should activate for the polyglossy new-notebook command`).to.include('onCommand:polyglossy-notebook.newNotebook');
            for (const commandId of expectedCommands) {
                expect(commands.some((command: { command: string }) => command.command === commandId), `${manifestPath} should contribute ${commandId}`).to.be.true;
            }
            expect(configurationProperties['polyglossy-notebook.defaultNotebookExtension'], `${manifestPath} should contribute the polyglossy default notebook extension setting`).to.exist;
        }
    });

    it('notebook toolbar menu entries support the polyglossy notebook identity', () => {
        const workspaceRoot = path.resolve(__dirname, '..', '..', '..', '..', '..');
        const manifestPaths = [
            path.join(workspaceRoot, 'src', 'polyglot-notebooks-vscode', 'package.json'),
            path.join(workspaceRoot, 'src', 'polyglot-notebooks-vscode-insiders', 'package.json')
        ];

        for (const manifestPath of manifestPaths) {
            const manifest = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
            const toolbarEntries = manifest.contributes?.menus?.['notebook/toolbar'] ?? [];
            const restartEntry = toolbarEntries.find((entry: { command: string }) => entry.command === 'polyglossy-notebook.notebookEditor.restartKernel');
            const valueViewerEntry = toolbarEntries.find((entry: { command: string }) => entry.command === 'polyglossy-notebook.notebookEditor.openValueViewer');

            expect(restartEntry, `${manifestPath} should contribute the Polyglossy restart-kernel toolbar entry`).to.exist;
            expect(restartEntry.when, `${manifestPath} should expose the restart-kernel toolbar entry for the polyglossy notebook identity`).to.include('polyglossy-notebook');
            expect(valueViewerEntry, `${manifestPath} should contribute the Polyglossy value-viewer toolbar entry`).to.exist;
            expect(valueViewerEntry.when, `${manifestPath} should expose the value-viewer toolbar entry for the polyglossy notebook identity`).to.include('polyglossy-notebook');
        }
    });

    it('notebook working directory comes from fallback if notebook is remote', () => {
        const notebookUri = createUri('path/to/notebook.dib', 'remote');
        const workspaceFolderUris = [
            createUri('not/used/1'),
            createUri('not/used/2'),
        ];
        const workingDir = getWorkingDirectoryForNotebook(notebookUri, workspaceFolderUris, 'fallback-is-used');
        expect(workingDir).to.equal('fallback-is-used');
    });

    it('tool manifest existence check detects manifests in the .config directory', async () => {
        await withFakeGlobalStorageLocation(true, async globalStoragePath => {
            const manifestDir = path.join(globalStoragePath, '.config');
            fs.mkdirSync(manifestDir, { recursive: true });
            fs.writeFileSync(path.join(manifestDir, 'dotnet-tools.json'), '{}');

            expect(toolManifestExists(globalStoragePath)).to.be.true;
        });
    });

    it('tool manifest existence check detects manifests in the storage root', async () => {
        await withFakeGlobalStorageLocation(true, async globalStoragePath => {
            fs.writeFileSync(path.join(globalStoragePath, 'dotnet-tools.json'), '{}');

            expect(toolManifestExists(globalStoragePath)).to.be.true;
        });
    });

    it('configuration lookup prefers the new section and falls back to the legacy section', () => {
        const originalGetConfiguration = (vscode.workspace as typeof vscode.workspace & { getConfiguration: typeof vscode.workspace.getConfiguration }).getConfiguration;
        const configMap = new Map<string, Record<string, unknown>>();
        (vscode.workspace as typeof vscode.workspace & { getConfiguration: typeof vscode.workspace.getConfiguration }).getConfiguration = ((section?: string) => ({
            get<T>(key: string): T | undefined {
                return (configMap.get(section ?? '')?.[key] as T | undefined) ?? undefined;
            }
        })) as typeof vscode.workspace.getConfiguration;

        try {
            configMap.set('polyglossy-interactive', { requiredInteractiveToolVersion: '2.0' });
            configMap.set('dotnet-interactive', { requiredInteractiveToolVersion: '1.0' });
            expect(getConfigurationValue('requiredInteractiveToolVersion', 'polyglossy-interactive', 'dotnet-interactive')).to.equal('2.0');

            configMap.set('polyglossy-interactive', {});
            expect(getConfigurationValue('requiredInteractiveToolVersion', 'polyglossy-interactive', 'dotnet-interactive')).to.equal('1.0');

            expect(getConfigurationValue('missingKey', 'polyglossy-interactive', 'dotnet-interactive')).to.equal(undefined);
        } finally {
            (vscode.workspace as typeof vscode.workspace & { getConfiguration: typeof vscode.workspace.getConfiguration }).getConfiguration = originalGetConfiguration;
        }
    });

    it('cell output ids use crypto.getRandomValues when randomUUID is unavailable', () => {
        setCrypto({
            getRandomValues: (values: Uint8Array) => {
                values.set([
                    0xde, 0xad, 0xbe, 0xef,
                    0xca, 0xfe, 0x0a, 0xbe,
                    0x10, 0x20, 0x30, 0x40,
                    0x50, 0x60, 0x70, 0x80,
                ]);
                return values;
            },
        });

        const output = createOutput([]);
        expect(output.id).to.equal('deadbeef-cafe-4abe-9020-304050607080');
    });

    it('debounce test', async () => {
        let callbackHit = false;
        let debounceDelay = 500;

        // repeatedly call `debounce` with small delays
        for (let i = 0; i < 10; i++) {
            debounce('key', debounceDelay, () => {
                if (callbackHit) {
                    throw new Error('Deboune callback was called more than once; this should never happen.');
                }

                callbackHit = true;
            });

            await new Promise(resolve => setTimeout(resolve, 100));
        }

        // wait for the debounced callback to actually have a chance
        await new Promise(resolve => setTimeout(resolve, debounceDelay * 1.1));
        expect(callbackHit).to.be.true;
    }).timeout(5000);

    it('cell error output shape can be detected', () => {
        // strongly typed to catch interface changes
        const error: ErrorElement = {
            errorName: 'ename',
            errorValue: 'evalue',
            stackTrace: [
                'stack trace line 1',
                'stack trace line 2'
            ]
        };
        expect(isErrorOutput(error)).to.be.true;
    });

    it('cell display output shape can be detected', () => {
        // strongly typed to catch interface changes
        const display: DisplayElement = {
            data: {
                'text/html': 'html',
                'text/plain': 'text'
            },
            metadata: {}
        };
        expect(isDisplayOutput(display)).to.be.true;
    });

    it('cell text output shape can be detected', () => {
        // strongly typed to catch interface changes
        const text: TextElement = {
            name: 'some name',
            text: 'some text'
        };
        expect(isTextOutput(text)).to.be.true;
    });

    it('parse function can handle base64-encoded Uint8Array', () => {
        const numbers = [1, 2, 3];
        const base64 = Buffer.from(numbers).toString('base64');
        const value = parse(`{"rawData":"${base64}"}`);
        expect(value).to.deep.equal({
            rawData: Uint8Array.from(numbers)
        });
    });

    it('stringify function can handle Uint8Array', () => {
        const numbers = [1, 2, 3];
        const text = stringify({
            rawData: new Uint8Array(numbers)
        });
        const expectedBase64 = Buffer.from(numbers).toString('base64');
        expect(text).to.equal(`{"rawData":"${expectedBase64}"}`);
    });

    it('stringify function can handle Buffer', () => {
        const numbers = [1, 2, 3];
        const text = stringify({
            rawData: Buffer.from(numbers)
        });
        const expectedBase64 = Buffer.from(numbers).toString('base64');
        expect(text).to.equal(`{"rawData":"${expectedBase64}"}`);
    });

    it('stringify function can handle cell output', () => {
        const numbers = [97, 98, 99];
        const text = stringify({
            'text/html': Buffer.from(numbers)
        });
        expect(text).to.equal(`{"text/html":"abc"}`);
    });

    describe('vs code output value reshaping', () => {
        it('error string is reshaped', () => {
            const reshaped = reshapeOutputValueForVsCode('some error message', vscodeLike.ErrorOutputMimeType);
            const decoded = JSON.parse(decodeToString(reshaped));
            expect(decoded).to.deep.equal({
                ename: 'Error',
                evalue: 'some error message',
                traceback: [],
            });
        });

        it(`non-error mime type doesn't reshape output`, () => {
            const reshaped = reshapeOutputValueForVsCode('some error message', 'text/plain');
            const decoded = decodeToString(reshaped);
            expect(decoded).to.equal('some error message');
        });
    });

    it('executing a non-existent process still returns', async () => {
        const result = await executeSafe('this-is-a-command-that-will-fail', []);
        expect(result).to.deep.equal({
            code: -1,
            output: '',
            error: 'Error: spawn this-is-a-command-that-will-fail ENOENT',
        });
    });

    describe('dotnet version checking', () => {
        it('version number can be obtained from simple value', () => {
            const version = getVersionNumber('5.0');
            expect(version).to.equal('5.0');
        });

        it(`version number from empty string doesn't throw`, () => {
            const version = getVersionNumber('');
            expect(version).to.equal('');
        });

        for (const newline of ['\n', '\r\n']) {
            it(`version number with leading and trailing newlines returns correct result with ${JSON.stringify(newline)} newlines`, () => {
                const version = getVersionNumber(`${newline}5.0${newline}`);
                expect(version).to.equal('5.0');
            });

            it(`version number with first-run text is properly pulled out with ${JSON.stringify(newline)} newlines`, () => {
                const version = getVersionNumber(`${newline}Welcome to .NET 5.0!${newline}--------${newline}5.0.101`);
                expect(version).to.equal('5.0.101');
            });
        }
    });

    describe('sorted objects', () => {
        it('sorts object keys', () => {
            const source = {
                'key2': "value",
                'key1': [1, 2, 3, 4]
            };

            const sorted = sortInPlace(source);

            expect(sorted).to.deep.equal({
                key1: [1, 2, 3, 4],
                key2: 'value'
            });
        });

        it('sorts object recursively', () => {
            const source = {
                'key2': "value",
                'key1': [1, 2, 3, 4],
                'key3': {
                    'key2': "value",
                    'key1': [1, 2, 3, 4],
                }
            };

            const sorted = sortInPlace(source);

            expect(sorted).to.deep.equal({
                key1: [1, 2, 3, 4],
                key2: 'value',
                key3: {
                    key1: [1, 2, 3, 4],
                    key2: 'value'
                }
            });
        });

        it('sorts object containing arrays', () => {
            const source = {
                'key2': "value",
                'key1': [6, 2, 3, 4],
                'key3': {
                    'key2': "value",
                    'key1': [1, 5, 3, 4],
                }
            };

            const sorted = sortInPlace(source);

            expect(sorted).to.deep.equal({
                key1: [2, 3, 4, 6],
                key2: 'value',
                key3: {
                    key1: [1, 3, 4, 5],
                    key2: 'value'
                }
            });
        });

        it('sorts kernel infos by local name', () => {
            const source = {
                'key2': "value",
                'key1': [6, 2, 3, 4],
                'key3': {
                    'key2': "value",
                    'key1': [{ localName: "d", displayName: "1" }, { localName: "a", displayName: "3" }, { localName: "b", displayName: "2" }],
                }
            };

            const sorted = sortInPlace(source);

            expect(sorted).to.deep.equal({
                key1: [2, 3, 4, 6],
                key2: 'value',
                key3:
                {
                    key1:
                        [
                            { displayName: '3', localName: 'a' },
                            { displayName: '2', localName: 'b' },
                            { displayName: '1', localName: 'd' }
                        ],
                    key2: 'value'
                }
            });
        });
    });

    describe('comparing objects', () => {
        it('objects are equivalent regardles of key order', () => {
            const data1 = {
                'key2': "value",
                'key1': [1, 2, 3, 4]
            };

            const data2 = {
                'key1': [1, 2, 3, 4],
                'key2': "value"
            };

            const d = areEquivalentObjects(data1, data2);
            expect(d).to.be.true;
        });

        it('objects are equivalent regardles of array order', () => {
            const data1 = {
                'key2': "value",
                'key1': [1, 2, 3, 4]
            };

            const data2 = {
                'key1': [1, 3, 2, 4],
                'key2': "value"
            };

            const d = areEquivalentObjects(data1, data2);
            expect(d).to.be.true;
        });

        it('can ingore keys when checking if objects are equivalent', () => {
            const data1 = {
                'key2': "value",
                'key1': [1, 2, 3, 4],
                'key5': "value"
            };

            const data2 = {
                'key1': [1, 3, 2, 4],
                'key2': "value",
                'key4': "value"
            };

            const ingoreKeys = new Set<string>();
            ingoreKeys.add("key4");
            ingoreKeys.add("key5");
            const d = areEquivalentObjects(data1, data2, ingoreKeys);
            expect(d).to.be.true;
        });

        it('objects are not equivalent if they have different data', () => {
            const data1 = {
                'key2': "value",
                'key1': [1, 2, 3, 4]
            };

            const data2 = {
                'key1': [1, 2, 3, 4, 5],
                'key2': "value"
            };

            const d = areEquivalentObjects(data1, data2);
            expect(d).to.be.false;
        });

        it('objects are not equivalent if they have different keys', () => {
            const data1 = {
                'key2': "value",
                'key1': [1, 2, 3, 4]
            };

            const data2 = {
                'key12': [1, 2, 3, 4],
                'key22': "value"
            };

            const d = areEquivalentObjects(data1, data2);
            expect(d).to.be.false;
        });
    });
});
