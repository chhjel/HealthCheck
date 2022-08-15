export interface DataTableGroup
{
    title: string;
    items: Array<DataTableItem>;
}

export interface DataTableItem
{
    values: Array<any>;
    expandedValues: Array<any>;
}


