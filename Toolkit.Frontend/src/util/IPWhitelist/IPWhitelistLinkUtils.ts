export default class IPWhitelistLinkUtils
{
    static getAbsoluteLinkUrl(endpointBase: string, ruleId: string, secret: string): string {
        let path = endpointBase;
        if (path == '/') path = '';
        else if (path.endsWith('/')) path = path.substring(0, path.length - 1);
        return `${window.location.origin.trim()}${path}/IPWLLink/${ruleId}_${secret}`;
    }
}
