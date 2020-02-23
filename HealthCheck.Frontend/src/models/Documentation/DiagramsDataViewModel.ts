import SequenceDiagramViewModel from "./SequenceDiagrams/SequenceDiagramViewModel";
import FlowChartViewModel from "./FlowCharts/FlowChartViewModel";

export default interface DiagramsDataViewModel {
    SequenceDiagrams: Array<SequenceDiagramViewModel>;
    FlowCharts: Array<FlowChartViewModel>;
}
