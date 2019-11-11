export default interface TimelineStepViewModel {
    Index: number;
    Title: string;
    Description: string | null;
    Error: string | null;
    LinkUrl: string | null;
    LinkTitle: string | null;
    Icon: string | null;
    Timestamp: Date | null;
    HideTimeInTimestamp: boolean;
    IsCompleted: boolean;
}