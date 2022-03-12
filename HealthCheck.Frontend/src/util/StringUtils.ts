export default class StringUtils
{
    static stringOrFirstOfArray(val: string | string[] | null): string | null {
        if (val == null || val == undefined) return val;
        return Array.isArray(val) ? val[0] : val;
    }
}
