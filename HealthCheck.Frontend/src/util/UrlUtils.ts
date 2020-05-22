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
}
