export default interface ParsedQuery {
    MustContain: Array<string>;
    MustContainOneOf: Array<Array<string>>;
    RegexPattern: string;
    IsRegex: boolean;
}