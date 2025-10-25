namespace UI {
    const browser: BrowserMp = mp.browsers.new("package://ui/index.html");

    export function show(name: string) {
        browser.execute(`UI.enableInterface('${name}')`);
    }

    export function hide() {
        browser.execute(`UI.enableInterface('')`);
    }

    export function loadData(store: string, value: number | string | boolean | any[] | null) {
        browser.execute(typeof value == "string"
            ? `UI.loadData('${store}', '${value}');`
            : `UI.loadData("${store}", ${value});`);
    }

    export function invokeStoreAction(store: string, action: string, value: number | string | boolean | any[] | object | null = null) {
        browser.execute(typeof value == "string"
            ? `UI.invokeStoreAction('${store}', '${action}', '${value}')`
            : `UI.invokeStoreAction('${store}', '${action}', ${value})`);
    }

}
