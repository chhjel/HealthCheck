export default class RegexUtils
{
    static inverseReplaceRegex(regex: RegExp, value: string, replacement: string): string {
        if (regex.test(value)) return value;
        
        // Quick hacky solution, replace matching chars with ½'s and keep those indices.
        const placeholder = '½';
        const replaceMap = value.replace(regex, m => placeholder.repeat(m.length));
        let result = '';
        for (let i=0;i<value.length;i++)
        {
            if (replaceMap[i] == placeholder) {
                result += value[i];
            } else {
                result += replacement;
            }
        }
        // console.log(`${value} => ${result} [${replaceMap}]`, regex);
        return result;
    }
}
