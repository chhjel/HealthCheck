export interface FilterableListGroup
{
    title: string;
    items: Array<FilterableListItem>;
}

export interface FilterableListItem
{
    title: string;
    subtitle: string | null;
    subTitle?: string | null;
    data: any;
}


