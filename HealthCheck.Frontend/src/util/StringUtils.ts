export default class StringUtils
{
    static stringOrFirstOfArray(val: string | string[] | null): string | null {
        if (val == null || val === '') return null;
        else if(val == undefined) return undefined;
        else return Array.isArray(val) ? val[0] || null : val;
    }
}
