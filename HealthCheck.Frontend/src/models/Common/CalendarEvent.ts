export default interface CalendarEvent<T> {
    data: T;
    title: string;
    details: string;
    date: string;
    time?: string;
    duration?: number;
    open: boolean;
    dateTime: Date;
}