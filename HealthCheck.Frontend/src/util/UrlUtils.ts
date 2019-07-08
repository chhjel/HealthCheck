import { UpdateHistoryStateMethod } from "./enums/UpdateHistoryStateMethod";

export default class LinqUtils
{
    static SetQueryStringParameter(key: string, value: string,
        method: UpdateHistoryStateMethod = UpdateHistoryStateMethod.Replace): void
    {
        let params = new URLSearchParams(location.search);
        params.set(key, value);
        let newUrl = `${window.location.pathname}?${params.toString()}`;
        if (method == UpdateHistoryStateMethod.Replace) {
            window.history.replaceState(null, window.name, newUrl);
        } else if (method == UpdateHistoryStateMethod.Push) {
            window.history.pushState(null, window.name, newUrl);
        } else {
            console.error(`Unknown UpdateHistoryStateMethod enum value given: '${method}'`);
        }
    }
}
