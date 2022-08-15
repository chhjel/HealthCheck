export interface FlowDiagramStep<T> {
    title: string;
	connections: Array<FlowDiagramConnection>;
	type?: FlowDiagramStepType | undefined | null;
    data: T;
}

export enum FlowDiagramStepType {
	Element = 'Element',
	If = 'If',
	Start = 'Start',
	End = 'End'
}

export interface FlowDiagramConnection {
	target: string;
	label: string | null;
}


