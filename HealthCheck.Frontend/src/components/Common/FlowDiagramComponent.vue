<!-- src/components/Common/FlowDiagramComponent.vue -->
<template>
	<div class="flowchart">
		<div class="flowchart__header" v-if="title != null">{{ title }}</div>
		<div ref="diagram" style="margin: auto;"></div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import * as joint from "jointjs";
import 'jointjs/dist/joint.min.css'
import * as dagre from 'dagre/index';
import IdUtils from '@util/IdUtils';

import { FlowDiagramStep<T> , FlowDiagramStepType , FlowDiagramConnection  } from './FlowDiagramComponent.vue.models';
@Options({
	components: {}
})
export default class FlowDiagramComponent<T> extends Vue
{
	@Prop({ required: true})
	title!: string;

	@Prop({ required: true})
	steps!: Array<FlowDiagramStep<T>>;

	shapeDefault!: joint.dia.Cell.Constructor<joint.dia.Element>;
	shapeIf!: joint.dia.Cell.Constructor<joint.dia.Element>;

	//////////////////
	//  LIFECYCLE  //
	////////////////
	mounted(): void {
		this.initDiagram();
	}

	//////////////
	initDiagram(): void {
		let self = this;

		var Shape = joint.dia.Element.define(
			"flowchart-element-default",
			{
				z: 2,
				size: {
					width: 100,
					height: 50
				},
				attrs: {
					body: {
						refWidth: "100%",
						refHeight: "100%",
						fill: "white",
						// stroke: "gray",
						// strokeWidth: 1,
						rx: 0,
						ry: 0,
						cursor: 'default'
					},
					// bodyline: {
					// 	refWidth: "100%",
					// 	// refHeight: "100%",
					// 	height: 1,
					// 	fill: "gray",
					// 	cursor: 'default'
					// },
					label: {
						refX: "50%",
						refY: "50%",
						yAlignment: "middle",
						xAlignment: "middle",
						fontSize: 18,
						// fontFamily: 'monospace',
						cursor: 'default'
					}
				}
			},
			{
				markup: [
					{
						tagName: 'path',
						selector: 'line',
						attributes: {
							z: -1,
							type: "path",
							fill: "red",
							stroke: "none",
							cursor: 'default'
						}
					},
					{
						tagName: "rect",
						selector: "body"
					},
					// {
					// 	tagName: "rect",
					// 	selector: "bodyline"
					// },
					{
						tagName: "text",
						selector: "label"
					}
				],

				setStepData: function(step: FlowDiagramStep<T>)
				{
					let text = step.title;

					let lineLengths = text.split('\n').map(x => x.length);
					let maxLineLength = lineLengths.reduce((a, b) => Math.max(a, b));
					let width = (maxLineLength * 11) + 20;
					let height = (lineLengths.length * 20) + 20;

					this.prop("size/width", width);
					this.prop("size/height", height);
					
					if (step.type == FlowDiagramStepType.Start
						|| step.type == FlowDiagramStepType.End)
					{
						this.attr("label/fontWeight", 600);
						this.attr("body/rx", 10);
						this.attr("body/ry", 10);
					}

					if (step.type == FlowDiagramStepType.Start)
					{
						this.attr("body/fill", "#babaa5");
					}
					else if (step.type == FlowDiagramStepType.End)
					{
						this.attr("body/fill", "#aed581");
					}
					else if (step.type == FlowDiagramStepType.If)
					{
						this.attr("body/fill", "#d6cba4");
						this.attr("line/fill", "#d6cba4");

						let arrowSize = 10;
						let polyPoints = [
							{ x: -arrowSize,     y: height/2 }, // Left mid
							{ x:   1, 	  y: 0 },		 // Left top
							{ x: width-1, y: 0 },		 // Right top
							{ x: width+arrowSize, y: height/2 },// Right mid
							{ x: width-1, y: height },   // Right bottom
							{ x:   1,     y: height }	 // Left bottom
						];
						let polyD = polyPoints.map(x => `L ${x.x} ${x.y}`).join(' ');
						polyD = polyD.substring(2);
						this.attr("line/d", `M ${polyD} Z`);
					}
					else {
						this.attr("body/fill", "#e3e0d3");
					}

					return this.attr("label/text", text || "");
				}
			}
		);
		
		var ShapeIf = Shape;

		this.shapeDefault = Shape;
		this.shapeIf = ShapeIf;

		var Link: joint.dia.Cell.Constructor<joint.dia.Link> = joint.dia.Link.define(
			"flowchart-link",
			{
				attrs: {
					line: {
						connection: true,
						stroke: "#afafaf",
						strokeWidth: 2,
						pointerEvents: "none",
						targetMarker: {
							type: "path",
							fill: "#afafaf",
							stroke: "none",
							d: "M 10 -10 0 0 10 10 z"
						}
					}
				},
				connector: {
					name: "rounded"
				},
				z: 1,
				weight: 1,
				minLen: 1,
				labelPosition: "c",
				labelOffset: 10,
				labelSize: {
					width: 50,
					height: 30
				},
				labels: [
					{
						markup: [
							{
								tagName: "rect",
								selector: "labelBody"
							},
							{
								tagName: "text",
								selector: "labelText"
							}
						],
						attrs: {
							labelText: {
								fill: "#90a4ae",
								textAnchor: "middle",
								refY: 5,
								refY2: "-50%",
								fontSize: 16,
								// fontFamily: 'monospace'
								// cursor: "pointer"
							},
							labelBody: {
								fill: "#eeeeee",
								stroke: "#afafaf",
								strokeWidth: 1,
								refWidth: "100%",
								refHeight: "100%",
								refX: "-50%",
								refY: "-50%",
								rx: 5,
								ry: 5
							}
						},
						size: {
							width: 50,
							height: 30
						}
					}
				]
			},
			{
				markup: [
					{
						tagName: "path",
						selector: "line",
						attributes: {
							fill: "none"
						}
					}
				],

				connect: function(sourceId: string, targetId: string) {
					return this.set({
						source: { id: sourceId },
						target: { id: targetId }
					});
				},

				setConnectionData: function(connection: FlowDiagramConnection)
				{
					let text = connection.label;

					if (text == null || text.trim().length == 0)
					{
						this.prop("labels/0/attrs/labelBody/hidden", true);
						this.prop("labels/0/attrs/labelText/hidden", true);
					}
					else
					{
						let lineLengths = text.split('\n').map(x => x.length);
						let maxLineLength = lineLengths.reduce((a, b) => Math.max(a, b));
						let width = (maxLineLength * 9) + 10;
						let height = (lineLengths.length * 16) + 10;
						this.prop("labels/0/size/width", width);
						this.prop("labels/0/size/height", height);
					}

					return this.prop("labels/0/attrs/labelText/text", text || "");
				}
			}
		);

		var LayoutControls = (<any>joint.mvc.View).extend({
			events: {
				change: "onChange",
				input: "onChange"
			},

			options: {
				padding:
				{
					left: 200,
					top: 30,
					right: 200,
					bottom: 50
				}
			},

			init: function() {
				var options = this.options;
				options.cells = this.buildGraphFromAdjacencyList();
			},

			onChange: function() {
				this.layout();
				this.trigger("layout");
			},

			layout: function() {
				var paper = this.options.paper;
				var graph = paper.model;
				var cells = this.options.cells;

				paper.freeze();

				joint.layout.DirectedGraph.layout(cells, this.getLayoutOptions());

				if (graph.getCells().length === 0) {
					// The graph could be empty at the beginning to avoid cells rendering
					// and their subsequent update when elements are translated
					graph.resetCells(cells);
				}

				paper.fitToContent({
					padding: this.options.padding,
					allowNewOrigin: "any",
					useModelGeometry: true
				});

				paper.unfreeze();
			},

			getLayoutOptions: function() {
				return <joint.layout.DirectedGraph.LayoutOptions>{
					dagre: dagre,
					graphlib: dagre.graphlib,
					setVertices: true,
					setLabels: true,
					ranker: 'network-simplex',
					rankDir: 'TB',
					align: 'DL',
					rankSep: 60,
					edgeSep: 50,
					nodeSep: 60
				};
			},

			buildGraphFromAdjacencyList: () => {
				let elementIds: any = {};
				let elements: Array<any> = [];
				let links: Array<any> = [];

				// Add elements
				self.steps.forEach(step => {
					let id = elementIds[step.title] || IdUtils.generateId();
					elementIds[step.title] = id;
					let ctor = self.getStepElementType(step);
					elements.push((<any>new ctor({id: id })).setStepData(step));
				});

				// Add links
				self.steps.forEach(step => {
					let fromId = elementIds[step.title];
					step.connections.forEach(connection => {
						let toId = elementIds[connection.target];
						links.push((<any>new Link())
							.connect(fromId, toId)
							.setConnectionData(connection)
						);
					});
				});
				
				return elements.concat(links);
			}
		});

		var controls = new LayoutControls({
			paper: new joint.dia.Paper(<any>{
				el: this.$refs.diagram,
				sorting: joint.dia.Paper.sorting.APPROX,
				interactive: (cellView: any) => {
					return false; //cellView.model.isElement();
				}
			})
		});

		controls.layout();
	}

	////////////////
	//  GETTERS  //
	//////////////

	////////////////
	//  METHODS  //
	//////////////
	getStepElementType(step: FlowDiagramStep<T>): joint.dia.Cell.Constructor<joint.dia.Element>
	{
		let type = this.getStepType(step);
		step.type = type;
		if (type == FlowDiagramStepType.If)
		{
			return this.shapeIf;
		}
		else
		{
			return this.shapeDefault;
		}
	}

	getStepType(step: FlowDiagramStep<T>): FlowDiagramStepType
	{
		let type = step.type;
		let text = step.title;

		if (type != null)
		{
			return type;
		}
		
		if (text.trim().endsWith("?"))
		{
			return FlowDiagramStepType.If;
		}
		else if (text.trim().toLowerCase() == "start" 
				|| !this.steps.some(x => x.connections.some(c => c.target == text)))
		{
			return FlowDiagramStepType.Start;
		}
		else if (step.connections == null || step.connections.length == 0)
		{
			return FlowDiagramStepType.End;
		}

		return FlowDiagramStepType.Element;
	}

	///////////////////////
	//  EVENT HANDLERS  //
	/////////////////////
}
</script>

<style scoped lang="scss">
.flowchart {
    overflow: auto;

	.flowchart__header {
        font-weight: 600;
        text-align: center;
        font-size: 20px;
    }
}
</style>
