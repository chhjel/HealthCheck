declare interface String {
    padZero(length : number) : string;
    trunc(length : number) : string;
}
declare interface Array<T> {
    joinForSentence<T>(delimiter: string, word: string): string;
}
