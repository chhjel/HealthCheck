import SequenceDiagramStepViewModel from "./SequenceDiagramStepViewModel";

export default interface SequenceDiagramViewModel {
    Name: string;
    Steps: Array<SequenceDiagramStepViewModel>;
}
