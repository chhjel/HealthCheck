export default class UrlUtils
{
    static EncodeHashPart(value: string): string {
        return value.replace(/ /g, '-').replace('/', '-');
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
}
