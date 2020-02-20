export default interface DataFlowPropertyDisplayInfo {
    PropertyName: string;
    DisplayName: string;
    IsFilterable: boolean;
    Visibility: DataFlowPropertyUIVisibilityOption,
    UIOrder: number;
    UIHint: DataFlowPropertyUIHint;
    DateTimeFormat: string;
}

export enum DataFlowPropertyUIHint
{
    Raw = 'Raw',
    DateTime = 'DateTime',
    Dictionary = 'Dictionary',
    List = 'List',
    Link = 'Link',
    Image = 'Image'
}

export enum DataFlowPropertyUIVisibilityOption
{
    Always = 'Always',
    OnlyWhenExpanded = 'OnlyWhenExpanded',
    OnlyInList = 'OnlyInList',
    Hidden = 'Hidden'
}
