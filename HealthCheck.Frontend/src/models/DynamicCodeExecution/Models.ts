import { MarkerSeverity } from 'monaco-editor'

export interface ICodeMark {
    line: number;
    startColumn: number;
    endColumn: number;
    message: string;
    severity: MarkerSeverity;
}