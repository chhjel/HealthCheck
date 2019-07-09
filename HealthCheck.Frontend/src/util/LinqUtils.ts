import KeyArray from "./models/KeyArray";

export default class LinqUtils
{
    // static GroupBy<T>(list: Array<T>, propertyName: string): KeyArray<T>
    // {
    //     return list.reduce((rv:any, x:any) => {
    //         (rv[x[propertyName]] = rv[x[propertyName]] || []).push(x);
    //         return rv;
    //       }, {});
    // }

    // static GroupByInto<T, G>(
    //     list: Array<T>,
    //     propertyName: string,
    //     groupFactory: (key:string, items:Array<T>) => G): Array<G>
    // {
    //     let groups = Array<G>();
    //     let groupedData = LinqUtils.GroupBy(list, propertyName);
    //     for (let key in groupedData)
    //     {
    //         let group = groupFactory(key, groupedData[key]);
    //         groups.push(group);
    //     }
    //     return groups;
    // }

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

