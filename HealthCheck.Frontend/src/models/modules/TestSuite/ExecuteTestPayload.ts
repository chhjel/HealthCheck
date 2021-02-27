export default interface ExecuteTestPayload {
    TestId: string;
    Parameters: Array<ExecuteTestParameterInputData>;
}

export interface ExecuteTestParameterInputData {
    Value: string | null;
    IsUnsupportedJson: boolean;
}
