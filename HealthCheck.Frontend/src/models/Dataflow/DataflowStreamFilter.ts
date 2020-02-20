export default interface DataflowStreamFilter {
    Skip: number;
    Take: number;
    FromDate: Date | null;
    ToDate: Date | null;
    PropertyFilters: any;
}
