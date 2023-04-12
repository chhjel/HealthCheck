export default class StringUtils
{
    static stringOrFirstOfArray(val: string | string[] | null): string | null {
        if (val == null || val === '') return null;
        else if(val == undefined) return undefined;
        else return Array.isArray(val) ? val[0] || null : val;
    }

    static anyIndicesAreValid(value: string): boolean {
        let startCount = 0;
        let endCount = 0;
        let currentIndex = '';
        let isInsideIndexer = false;
        for (let i = 0; i < value.length; i++)
        {
            if (value[i] == '[')
            {
                startCount++;
                currentIndex = '';
                isInsideIndexer = true;
            }
            else if (value[i] == ']')
            {
                isInsideIndexer = false;
                endCount++;

                if (currentIndex.length == 0 || isNaN(Number(currentIndex))) {
                    return false;
                }
            }
            else if (isInsideIndexer) {
                currentIndex += value[i];
            }
        }
        return startCount == endCount;
    }

    static stripIndices(value: string, includeWrappers: boolean = false, newIndex: number | null = null): string
    {
        if (!value) return value;

        let output = '';
        let isInsideIndexer = false;
        for (let i = 0; i < value.length; i++)
        {
            if (value[i] == '[')
            {
                isInsideIndexer = true;
                if (includeWrappers)
                {
                    output += value[i];
                }
                if (newIndex != null) {
                    output += newIndex;
                }
            }
            else if (value[i] == ']')
            {
                isInsideIndexer = false;
                if (!includeWrappers) continue;
            }

            if (!isInsideIndexer)
            {
                output += value[i];
            }
        }
        return output;
    }
}
