import Module = require('module');

type ModuleLoad = (request: string, parent: NodeJS.Module | undefined, isMain: boolean) => unknown;

const vscodeStub = {
    workspace: {
        getConfiguration: () => ({
            get: <T>() => undefined as T | undefined,
        }),
    },
};

const moduleWithLoad = Module as typeof Module & { _load: ModuleLoad };
const originalLoad = moduleWithLoad._load;

moduleWithLoad._load = function patchedLoad(request: string, parent: NodeJS.Module | undefined, isMain: boolean) {
    if (request === 'vscode') {
        return vscodeStub;
    }

    return originalLoad.call(this, request, parent, isMain);
};