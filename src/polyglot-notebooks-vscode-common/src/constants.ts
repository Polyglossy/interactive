// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// languages
export const CellLanguageIdentifier = 'polyglot-notebook';

// notebook controllers
export const NotebookControllerId = 'polyglot-notebook';
export const PolyglossyNotebookControllerId = 'polyglossy-notebook';
export const JupyterNotebookControllerId = 'polyglot-notebook-for-jupyter';

// notebook kernel
export const JupyterKernelId = 'polyglot-notebook-for-jupyter';

// view types
export const NotebookViewType = 'polyglot-notebook';
export const PolyglossyNotebookViewType = 'polyglossy-notebook';
export const JupyterNotebookViewType = 'polyglot-notebook-jupyter';
export const JupyterViewType = 'jupyter-notebook';

export function getNotebookViewTypeForFormat(notebookFormat: string): string {
    switch (notebookFormat.toLowerCase()) {
        case 'dib':
            return PolyglossyNotebookViewType;
        case 'ipynb':
            return JupyterViewType;
        default:
            return NotebookViewType;
    }
}

// other
export const PolyglossyConfigurationSectionName = 'polyglossy-interactive';
export const DotnetConfigurationSectionName = PolyglossyConfigurationSectionName;
export const LegacyDotnetConfigurationSectionName = 'dotnet-interactive';
export const PolyglotConfigurationSectionName = 'polyglossy-notebook';
export const LegacyPolyglotConfigurationSectionName = 'polyglot-notebook';
export const InteractiveWindowControllerId = 'polyglot-notebook-window';
export const PolyglossyInteractiveWindowControllerId = 'polyglossy-notebook-window';
export const LegacyInteractiveWindowControllerId = 'dotnet-interactive-window';
