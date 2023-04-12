export interface SequenceDiagramStep<T> {
    from: string;
    to: string;
    description: string;
    note?: string;
    remark?: string;
    optional?: string;
    style?: SequenceDiagramLineStyle;
    data: T;
}

export enum SequenceDiagramLineStyle {
    Default = 0,
    Dashed = 1
}

export enum SequenceDiagramStyle {
    Default = 'Default',
    Test = 'Test'
}


