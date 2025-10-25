namespace UI {
    const browser: BrowserMp = mp.browsers.new("package://ui/index.html");

    export function show(name: string) {
        browser.execute(`UI.enableInterface('${name}')`);
    }

    export function hide() {
        browser.execute(`UI.enableInterface('')`);
    }

    export function loadData(entity: string, value: number | string | boolean | any[] | null) {
        browser.execute(typeof value == "string"
            ? `UI.loadData('${entity}', '${value}');`
            : `UI.loadData("${entity}", ${value});`);
    }

    export function invokeStoreAction(store: string, action: string, value: number | string | boolean | any[] | object | null = null) {
        browser.execute(typeof value == "string"
            ? `UI.invokeStoreAction('${store}', '${action}', '${value}')`
            : `UI.invokeStoreAction('${store}', '${action}', ${value})`);
    }
}