export default interface TimelineStepViewModel {
    Index: number;
    Title: string;
    Description: string | null;
    Error: string | null;
    Links: Array<string[]>; // [href, title]
    Icon: string | null;
    Timestamp: Date | null;
    HideTimeInTimestamp: boolean;
    IsCompleted: boolean;
}