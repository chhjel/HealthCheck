import FlowChartConnectionViewModel from "./FlowChartConnectionViewModel";

export default interface FlowChartStepViewModel {
    Title: string;
    Type: FlowDiagramStepType | null | undefined;
	Connections: Array<FlowChartConnectionViewModel>;
	ClassName: string;
	MethodName: string;
}

export enum FlowDiagramStepType {
	Element = 'Element',
	If = 'If',
	Start = 'Start',
	End = 'End'
}