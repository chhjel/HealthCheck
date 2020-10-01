export default interface TestParameterViewModel {
    Index: number;
    Name: string;
    Description: string;
    Type: string;
    DefaultValue: string;
    PossibleValues: Array<string>;
    Value: string | null;
    NotNull: boolean;
    ReadOnlyList: boolean;
    ShowTextArea: boolean;
    FullWidth: boolean;
    IsCustomReferenceType: boolean;
    ReferenceChoices: Array<TestParameterReferenceChoiceViewModel>;
}

export interface TestParameterReferenceChoiceViewModel {
    Id: string;
    Name: string;
}