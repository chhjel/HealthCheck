import FlowChartStepViewModel from "./FlowChartStepViewModel";

export default interface FlowChartViewModel {
    Name: string;
    Steps: Array<FlowChartStepViewModel>;
}
