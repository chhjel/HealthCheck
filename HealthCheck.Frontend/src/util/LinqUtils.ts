import KeyArray from "./models/KeyArray";

export default class LinqUtils
{
    static SortByThenBy<TItem, TPropA, TPropB>(
        a:TItem, b:TItem,
        firstPropSelector: (item:TItem) => TPropA,
        secondPropSelector: (item:TItem) => TPropB,
        invertFirst: boolean = false,
        invertSecond: boolean = false
    ): number {
        // Order by..
        if (firstPropSelector(a) > firstPropSelector(b)) {
            return invertFirst ? 1 : -1;
        } else if (firstPropSelector(a) < firstPropSelector(b)) { 
            return invertFirst ? -1 : 1;
        }
    
        // Then by..
        if (secondPropSelector(a) > secondPropSelector(b)) {
            return invertSecond ? 1 : -1;
        } else if (secondPropSelector(a) < secondPropSelector(b)) { 
            return invertSecond ? -1 : 1;
        } else {
            return 0;
        }
    }

    static GroupBy<T>(list: Array<T>, keyFactory: (item:T) => string): KeyArray<T>
    {
        return list.reduce((rv:any, x:any) => {
            (rv[keyFactory(x)] = rv[keyFactory(x)] || []).push(x);
            return rv;
          }, {});
    }

    static GroupByInto<T, G>(
        list: Array<T>,
        keyFactory: (item:T) => string,
        groupFactory: (key:string, items:Array<T>) => G): Array<G>
    {
        let groups = Array<G>();
        let groupedData = LinqUtils.GroupBy(list, keyFactory);
        for (let key in groupedData)
        {
            let group = groupFactory(key, groupedData[key]);
            groups.push(group);
        }
        return groups;
    }
}

