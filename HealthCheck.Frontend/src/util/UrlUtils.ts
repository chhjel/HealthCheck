export default class UrlUtils
{
    static getCurrentUrlWithoutParamsAndHash(suffix: string | null = null): string {
        let clean = location.protocol + '//' + location.host + location.pathname;
        if (suffix)
        {
            if (clean.endsWith('/') && suffix.startsWith('/'))
            {
                suffix = suffix.substring(1);
            }
            clean += suffix;
        }
        return clean;
    }
    static openRouteInNewTab(route: string): void {
        const url = UrlUtils.getOpenRouteInNewTabUrl(route);
        window.open(url, '_blank');
    }

    static getOpenRouteInNewTabUrl(route: string): string {
        let url = (window.location.hash == null || window.location.hash.length == 0)
            ? window.location.href + route
            : window.location.href.replace(window.location.hash, route);
        url = UrlUtils.RemoveQueryStringParameter(url, 'h');
        return url;
    }

    static EncodeHashPart(value: string): string {
        return value.replace(/ /g, '-').replace('/', '-');
    }

    static updatePerstentQueryStringKey(): void {
        UrlUtils.SetQueryStringParameter('h', window.location.hash);
    }

    static SetQueryStringParameter(key: string, value: string, reload?: boolean): void {
        const params = new URLSearchParams(location.search);
        params.set(key, value);
        const newUrl = `${location.pathname}?${params.toString()}${location.hash}`;
        if (reload === true) {
            window.location.href = newUrl;
        } else {
            window.history.replaceState({}, '', newUrl);
        }
    }

    static ClearQueryStringParameter(key: string): void {
        const params = new URLSearchParams(location.search);
        params.delete(key);
        window.history.replaceState({}, '', `${location.pathname}?${params.toString()}${location.hash}`);
    }

    static RemoveRelativeQueryStringParameter(search: string, key: string): string {
        if (!search) return search;
        let prefix = '';
        if (search.startsWith('?')) prefix = '?';
        if (search.startsWith('&')) prefix = '&';

        const params = new URLSearchParams(search);
        if (params == null || !params.has(key)) return search;
        params.delete(key);

        let result = params.toString();
        if (!result) return result;
        return prefix + params.toString();
    }

    static RemoveQueryStringParameter(url: string, key: string): string {
        const urlObj = new URL(url);
        urlObj.searchParams.delete(key);
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
