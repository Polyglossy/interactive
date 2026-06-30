// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PolyglossyInteractiveScopeContainer, PolyglossyInteractiveScope } from "./polyglot-notebooks-interfaces";
import { createPolyglossyInteractiveClient } from "./kernel-client-impl";

export function init(global: any) {
    global.getPolyglossyInteractiveScope = (key: string) => {
        if (!global.interactiveScopes) {
            global.interactiveScopes = new PolyglossyInteractiveScopeContainer();
        }

        if (!global.interactiveScopes[key]) {
            global.interactiveScopes[key] = new PolyglossyInteractiveScope();
        }

        return global.interactiveScopes[key];
    }

    global.getDotnetInteractiveScope = global.getPolyglossyInteractiveScope;

    global.configureRequire = (config: any) => {
        return (<any>require).config(config) || require;
    }

    global.createPolyglossyInteractiveClient = createPolyglossyInteractiveClient;
    global.createDotnetInteractiveClient = createPolyglossyInteractiveClient;
}