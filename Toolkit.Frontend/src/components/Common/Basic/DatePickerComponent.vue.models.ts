import DateUtils from "@util/DateUtils";

export interface DatePickerPresetDateRange {
    name: string;
    from: Date;
    to: Date;
}

export function presetRangesBackInTime(): Array<DatePickerPresetDateRange> {
    const endOfToday = new Date();
    endOfToday.setHours(23);
    endOfToday.setMinutes(59);

    return [
        { name: 'Last hour', from: DateUtils.CreateDateWithMinutesOffset(-60), to: endOfToday },
        { name: 'Today', from: DateUtils.CreateDateWithDayOffset(0), to: endOfToday },
        { name: 'Last 3 days', from: DateUtils.CreateDateWithDayOffset(-3), to: endOfToday },
        { name: 'Last 7 days', from: DateUtils.CreateDateWithDayOffset(-7), to: endOfToday },
        { name: 'Last 30 days', from: DateUtils.CreateDateWithDayOffset(-30), to: endOfToday },
        { name: 'Last 60 days', from: DateUtils.CreateDateWithDayOffset(-60), to: endOfToday },
        { name: 'Last 90 days', from: DateUtils.CreateDateWithDayOffset(-90), to: endOfToday }
    ];
}
