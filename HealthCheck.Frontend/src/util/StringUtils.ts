export default class StringUtils
{
    static stringOrFirstOfArray(val: string | string[] | null): string | null {
        if (val == null) return null;
        else if(val == undefined) return undefined;
        else return Array.isArray(val) ? val[0] : val;
    }
}
