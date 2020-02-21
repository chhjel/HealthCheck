import SequenceDiagramStepViewModel from "./SequenceDiagramStepViewModel";

export default interface SequenceDiagramViewModel {
    Name: string;
    Description: string | null;
    Steps: Array<SequenceDiagramStepViewModel>;
}
