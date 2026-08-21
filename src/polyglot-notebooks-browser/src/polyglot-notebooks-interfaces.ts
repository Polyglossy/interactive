// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import * as commandsAndEvents from "./polyglot-notebooks/commandsAndEvents";
import { DisposableSubscription } from "./polyglot-notebooks/disposables";
import { IKernelCommandHandler } from "./polyglot-notebooks/kernel";

export interface VariableRequest {
    [kernelName: string]: Array<any>;
}

export interface VariableResponse {
    [kernelName: string]: {
        [variableName: string]: any
    }
}

export interface KernelClient {
    getVariable(variableName: string): Promise<any>;
    submitCode(code: string): Promise<string>;
    submitCommand(commandType: string, command?: any): Promise<string>;
}

export interface PolyglossyInteractiveClient {
    subscribeToKernelEvents(observer: commandsAndEvents.KernelEventEnvelopeObserver): DisposableSubscription;
    registerCommandHandler(handler: IKernelCommandHandler): void;
    getVariable(kernelName: string, variableName: string): Promise<any>;
    getVariables(variableRequest: VariableRequest): Promise<VariableResponse>;
    getResource(resource: string): Promise<Response>;
    getResourceUrl(resource: string): string;
    getExtensionResource(extensionName: string, resource: string): Promise<Response>;
    getExtensionResourceUrl(extensionName: string, resource: string): string;
    loadKernels(): Promise<void>;
    submitCode(code: string, targetKernelName?: string): Promise<string>;
    submitCommand(commandType: string, command?: any, targetKernelName?: string): Promise<string>;
    configureRequire(config: any): any;

    getConsole(commandToken: string): any;
    markExecutionComplete(commandToken: string): Promise<void>;
    failCommand(err: any, commandToken: string): void;
    waitForAllEventsToPublish(commandToken: string): Promise<void>;
}

// Implemented by the client-side kernel.
export type DotnetInteractiveClient = PolyglossyInteractiveClient;

export interface KernelClientContainer {
    [key: string]: KernelClient;
}

export class PolyglossyInteractiveScope {
    [key: string]: any
}

export class PolyglossyInteractiveScopeContainer {
    [key: string]: PolyglossyInteractiveScope
}

// Legacy compatibility aliases for the transition window.
export class DotnetInteractiveScope extends PolyglossyInteractiveScope {
}

// Legacy compatibility aliases for the transition window.
export class DotnetInteractiveScopeContainer extends PolyglossyInteractiveScopeContainer {
}

export interface ClientFetch {
    (input: RequestInfo, init?: RequestInit | undefined): Promise<Response>
}
