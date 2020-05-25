import { MarkerSeverity } from 'monaco-editor'

export interface DynamicCodeScript {
    Id: string;
    Title: string;
    Code: string;
}

export interface ICodeMark {
    line: number;
    startColumn: number;
    endColumn: number;
    message: string;
    severity: MarkerSeverity;
}

export interface DynamicCodeExecutionSourceModel
{
    Code:string;
    DisabledPreProcessorIds: Array<string>;
}

export interface DynamicCodeExecutionResultModel
{
    Success: boolean;
    Message: string;
    Code: string;
    CodeExecutionResult: CodeExecutionResult | null;
}

export interface CodeExecutionResult
{
    Code: string;
    Status: CodeExecutionResultStatusTypes;
    StatusCode: number;
    StatusString: string;
    Output: string;
    Errors: Array<CodeError>;
    AppliedPreProcessorIds: Array<string>;
    Dumps: Array<DataDump>;
    Diffs: Array<DataDiffDump>;
}

export interface DataDump {
    Title: string;
    Type: string;
    Data: string;
}

export interface DataDiffDump {
    Title: string;
    Left: DataDump;
    Right: DataDump;
}

export enum CodeExecutionResultStatusTypes
{
    Executed = 0,
    CompilerError,
    RuntimeError
}

export interface CodeError {
    Line: number;
    Column: number;
    Message: string;
}
