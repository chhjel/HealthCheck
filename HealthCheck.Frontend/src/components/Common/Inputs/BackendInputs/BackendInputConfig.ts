
export default interface BackendInputConfig
{
    notNull: boolean;
    defaultValue: string | null;
    flags: Array<string>;
    possibleValues: Array<string>;
}
