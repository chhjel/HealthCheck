export interface CalendarComponentEvent<T> {
  id: string,
  title: string,
  start: Date,
  // Note: This value is exclusive. For example, if you have an all-day event that has an end of 2018-09-03, then it will span through 2018-09-02 and end before the start of 2018-09-03.
  end: Date,
  startTime?: string,
  endTime?: string,
  allDay?: boolean,
  classNames?: string | string[],
  data: T
}
