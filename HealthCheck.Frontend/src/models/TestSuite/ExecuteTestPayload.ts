export default interface ExecuteTestPayload {
    TestId: string;
    Parameters: Array<string | null>;
}