export default class UrlUtils
{
    static openRouteInNewTab(route: string): void {
        let url = window.location.href.replace(window.location.hash, route);
        url = UrlUtils.RemoveQueryStringParameter(url, 'h');
        window.open(url, '_blank');
    }

    static EncodeHashPart(value: string): string {
        return value.replace(/ /g, '-').replace('/', '-');
    }

    static updatePerstentQueryStringKey(): void {
        UrlUtils.SetQueryStringParameter('h', window.location.hash);
    }

    static SetQueryStringParameter(key: string, value: string): void {
        const params = new URLSearchParams(location.search);
        params.set(key, value);
        window.history.replaceState({}, '', `${location.pathname}?${params.toString()}${location.hash}`);
    }

    static ClearQueryStringParameter(key: string): void {
        const params = new URLSearchParams(location.search);
        params.delete(key);
        window.history.replaceState({}, '', `${location.pathname}?${params.toString()}${location.hash}`);
    }

    static RemoveQueryStringParameter(url: string, key: string): string {
        const urlObj = new URL(url);
        urlObj.searchParams.delete('h');
        return urlObj.toString();
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
    
    static makeAbsolute(base: string, relative: string): string
    {
        let stack = base.split("/"),
            parts = relative.split("/");
        stack.pop()

        for (var i=0; i<parts.length; i++) {
            if (parts[i] == ".")
            {
                continue;
            }
            if (parts[i] == "..")
            {
                stack.pop();
            }
            else
            {
                stack.push(parts[i]);
            }
        }
        return stack.join("/");
    }
    
    static getRelativeToCurrent(relative: string, includeQuery: boolean = true, includeHash: boolean = true): string
    {
        const base = window.location.origin + window.location.pathname;
        let suffix = '';
        if (includeQuery) suffix += window.location.search;
        if (includeHash) suffix += (window.location.hash == '#' ? '' : window.location.hash);

        let stack = base.split("/"), parts = relative.split("/");
        stack.pop();

        for (var i=0; i<parts.length; i++) {
            if (parts[i] == ".")
            {
                continue;
            }
            if (parts[i] == "..")
            {
                stack.pop();
            }
            else
            {
                stack.push(parts[i]);
            }
        }
        return stack.join("/") + suffix;
    }
}
