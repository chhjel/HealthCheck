
export default interface BackendInputConfig
{
    notNull: boolean;
    nullable: boolean;
    defaultValue: string | null;
    flags: Array<string>;
    possibleValues: Array<string>;
}
