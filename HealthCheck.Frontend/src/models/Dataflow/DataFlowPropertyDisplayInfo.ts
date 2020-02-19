export default interface DataFlowPropertyDisplayInfo {
    PropertyName: string;
    DisplayName: string;
    Hidden: boolean;
    Visibility: DataFlowPropertyUIVisibilityOption,
    UIOrder: number;
    UIHint: DataFlowPropertyUIHint;
    DateTimeFormat: string;
}

export enum DataFlowPropertyUIHint
{
    Raw = 'Raw',
    DateTime = 'DateTime'
}

export enum DataFlowPropertyUIVisibilityOption
{
    Always = 'Always',
    OnlyWhenExpanded = 'OnlyWhenExpanded',
    OnlyInList = 'OnlyInList',
    Hidden = 'Hidden'
}
