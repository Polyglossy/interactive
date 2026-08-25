import { strict as assert } from "assert";
import { JSDOM } from "jsdom";
import { afterEach, describe, it } from "mocha";
import React from "react";
import { createRoot } from "react-dom/client";
import type { Root } from "react-dom/client";
import { act } from "react-dom/test-utils";
import { VariableGrid } from "./VariableGrid";

type VariableGridRow = {
    id?: string;
    name: string;
    value: string;
    typeName: string;
    kernelDisplayName: string;
    kernelName: string;
};

type DomWindowWithGlobals = Window & {
    MessageEvent: typeof MessageEvent;
    HTMLElement: typeof HTMLElement;
    SVGElement: typeof SVGElement;
};

describe("VariableGrid", () => {
    let root: Root | undefined;
    let container: HTMLElement | undefined;
    let domWindow: Window | undefined;
    let previousWindow: typeof globalThis.window | undefined;
    let previousDocument: typeof globalThis.document | undefined;
    let previousNavigator: Navigator | undefined;

    function setDomGlobals(windowValue: DomWindowWithGlobals) {
        previousWindow = globalThis.window;
        previousDocument = globalThis.document;
        previousNavigator = globalThis.navigator;

        Object.defineProperty(globalThis, "window", { value: windowValue, configurable: true });
        Object.defineProperty(globalThis, "document", { value: windowValue.document, configurable: true });
        Object.defineProperty(globalThis, "navigator", { value: windowValue.navigator, configurable: true });
        Object.defineProperty(globalThis, "MessageEvent", { value: windowValue.MessageEvent, configurable: true });
        Object.defineProperty(globalThis, "HTMLElement", { value: windowValue.HTMLElement, configurable: true });
        Object.defineProperty(globalThis, "SVGElement", { value: windowValue.SVGElement, configurable: true });
        (globalThis as typeof globalThis & { IS_REACT_ACT_ENVIRONMENT?: boolean }).IS_REACT_ACT_ENVIRONMENT = true;
    }

    function restoreDomGlobals() {
        Object.defineProperty(globalThis, "window", { value: previousWindow, configurable: true });
        Object.defineProperty(globalThis, "document", { value: previousDocument, configurable: true });
        Object.defineProperty(globalThis, "navigator", { value: previousNavigator, configurable: true });
        delete (globalThis as typeof globalThis & { IS_REACT_ACT_ENVIRONMENT?: boolean }).IS_REACT_ACT_ENVIRONMENT;
    }

    async function renderGrid(rows: VariableGridRow[]) {
        const dom = new JSDOM("<!doctype html><html><body><div id=\"root\"></div></body></html>", {
            url: "http://localhost"
        });

        domWindow = dom.window as unknown as DomWindowWithGlobals;
        setDomGlobals(domWindow as DomWindowWithGlobals);
        domWindow.getComputedStyle = ((() => ({ width: "100px" })) as unknown) as typeof domWindow.getComputedStyle;
        container = domWindow.document.getElementById("root") as HTMLElement;
        root = createRoot(container);

        await act(async () => {
            root!.render(React.createElement(VariableGrid, { rows }));
        });
    }

    afterEach(async () => {
        if (root) {
            await act(async () => {
                root!.unmount();
            });
        }

        domWindow?.close();
        root = undefined;
        container = undefined;
        domWindow = undefined;
        restoreDomGlobals();
    });

    it("renders rows and assigns fallback ids on mount", async () => {
        await renderGrid([
            {
                kernelDisplayName: "C#",
                kernelName: "csharp",
                name: "answer",
                typeName: "System.Int32",
                value: "42"
            }
        ]);

        assert.equal(document.querySelector("caption")?.textContent?.trim(), "Polyglot Notebook variables");
        assert.ok(document.getElementById("csharp-answer-0"));
        assert.match(document.body.textContent ?? "", /answer/);
    });

    it("updates rows and localization in response to window messages", async () => {
        await renderGrid([]);

        await act(async () => {
            window.dispatchEvent(new window.MessageEvent("message", {
                data: {
                    command: "set-rows",
                    rows: [
                        {
                            kernelDisplayName: "F#",
                            kernelName: "fsharp",
                            name: "total",
                            typeName: "System.Int64",
                            value: "9001"
                        }
                    ],
                    localizationStrings: {
                        actionsColumnHeader: "Actions",
                        gridCaption: "Updated variables",
                        kernelNameColumnHeader: "Kernel",
                        nameColumnHeader: "Variable",
                        shareTemplate: "Share {value-name} from {kernel-name}",
                        typeColumnHeader: "Type",
                        valueColumnHeader: "Value"
                    }
                }
            }));
        });

        assert.equal(document.querySelector("caption")?.textContent?.trim(), "Updated variables");
        assert.equal(document.querySelector("th.name-column")?.textContent?.trim(), "Variable");
        assert.ok(document.getElementById("fsharp-total-0"));
    });
});