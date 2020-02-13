import { SequenceDiagramDirection } from "./SequenceDiagramDirection"

export default interface SequenceDiagramStepViewModel {
    Index: number;
    From: string;
    To: string;
    Description: string | null;
    Note: string | null;
    Remarks: string | null;
    RemarkNumber: number | null;
    OptionalGroupName: string | null;
    Direction: SequenceDiagramDirection,
    Branches: Array<string>;
}
