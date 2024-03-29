export default interface TestParameterViewModel {
    Index: number;
    Name: string;
    Description: string;
    Type: string;
    DefaultValue: string;
    PossibleValues: Array<string>;
    Value: string | null;
    NotNull: boolean;
    NullName: string;
    ReadOnlyList: boolean;
    ShowTextArea: boolean;
    ShowCodeArea: boolean;
    FullWidth: boolean;
    IsCustomReferenceType: boolean;
    IsUnsupportedJson: boolean;
    Hidden: boolean;
    Feedback?: string | null;
}

export interface TestParameterReferenceChoiceViewModel {
    Id: string;
    Name: string;
    Description: string;
}
