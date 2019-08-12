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

    static EncodeHashPart(value: string): string {
        return value.replace(/ /g, '-').replace('/', '-');
    }


    static FirstHashPartIs(value: string): boolean {
        return this.GetHashParts()[0] == value;
    }

    static SetHashParts(parts: Array<string>,
        method: UpdateHistoryStateMethod = UpdateHistoryStateMethod.Replace): void {

        const hashPart = (parts.length == 0) ? '' : `#/${parts.join('/')}`;
        const newUrl = `${window.location.href.split("#")[0]}${hashPart}`;

        if (method == UpdateHistoryStateMethod.Replace) {
            window.history.replaceState(null, window.name, newUrl);
        } else if (method == UpdateHistoryStateMethod.Push) {
            window.history.pushState(null, window.name, newUrl);
        } else {
            console.error(`Unknown UpdateHistoryStateMethod enum value given: '${method}'`);
        }
    }

    static GetHashParts(): Array<string>
    {
        const hash = location.hash;
        if (hash.length == 0 || hash === '#') {
            return [];
        }

        return hash.substring(1).split('/').filter(x => x.length > 0);
    }

    static GetHashPart(index: number): string | null
    {
        const parts = this.GetHashParts();
        return (index >= 0 && index < parts.length) ? parts[index] : null;
    }

    static SetHashPart(index: number, value: string): void {
        const parts = this.GetHashParts();
        parts[index] = value;
        this.SetHashParts(parts);
    }

    static GetQueryStringParameter(key: string, fallbackValue: string | null = null): string | null
    {
        let params = new URLSearchParams(location.search);
        if (params.has(key)) {
            return params.get(key);
        } else {
            return fallbackValue;
        }
    }
}
