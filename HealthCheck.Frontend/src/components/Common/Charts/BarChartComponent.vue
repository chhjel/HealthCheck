<!-- src/components/Common/Charts/BarChartComponent.vue -->
<template>
    <div class="bar-chart-component" :style="{ 'height': height }">
        <canvas ref="canvas" />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import { Chart, LinearTickOptions, ChartPoint, ChartDataSets } from 'chart.js';
import DateUtils from "../../../util/DateUtils";
import LinqUtils from "../../../util/LinqUtils";

export interface BarChartBar {
    label: string;
    values: number[];
}
export interface BarChartSet {
	label: string;
	group: string;
    color: string;
}

interface ChartDataPoint extends Chart.ChartPoint {
}

@Component({
	components: {}
})
export default class BarChartComponent extends Vue {
    @Prop({ required: true })
	sets!: Array<BarChartSet>;

    @Prop({ required: true })
	bars!: Array<BarChartBar>;

    @Prop({ required: false, default: '' })
	title!: string;

    @Prop({ required: false, default: '' })
	ylabel!: string;

    chart!: Chart;
	chartPoints: Array<Array<ChartDataPoint>> = [];
	chartDatasets: Array<ChartDataSets> = [];

	//////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
		this.initChart();
		this.updateChart();
    }

    ////////////////
    //  GETTERS  //
	//////////////
	get height(): string {
		const height = 100 + (this.bars.length * 30);
		return `${height}px`;
	}

    ////////////////
    //  METHODS  //
	//////////////
	createDatasets(): Array<ChartDataSets> {
		var color = Chart.helpers.color;
		return this.sets.map((set, index) => {
			return {
				pointHitRadius: 20,
				label: set.label,
				borderCapStyle: 'butt',
				backgroundColor: color(set.color).alpha(0.5).rgbString(),
				borderColor: color(set.color),
				// pointHighlightStroke: colors.purple.stroke,
				// pointBackgroundColor: colors.purple.stroke,
				fill: true,
				data: this.chartPoints[index]
			}
		});
	}

	createPoints(): Array<Array<ChartDataPoint>> {
		let pointSets: Array<Array<ChartDataPoint>> = [];
		
		for (let set of this.sets) {
			pointSets.push([]);
		}

		for(let i=0;i<this.bars.length;i++)
		{
			const values = this.bars[i].values;
			
			for(let j=0;j<values.length;j++)
			{
				pointSets[j].push({
					x: values[j]
				});
			}
		}

		return pointSets;
	}

    initChart(): void
    {
		this.updateData();

		var color = Chart.helpers.color;
		var config: Chart.ChartConfiguration = {
			type: 'horizontalBar',
			data: {
				labels: this.bars.map(x => x.label),
				datasets: this.chartDatasets
			},
			options: {
				responsive: true,
				maintainAspectRatio: false,
				title: {
					display: (this.title != null && this.title.length > 0),
					//text: this.title
                },
                elements: {
                    line: {
                        tension: 0.1
                    }
                },
				scales: {
					xAxes: [{
                        stacked: true,
					}],
					yAxes: [{
						display: true,
                        stacked: true,
						scaleLabel: {
							display: (this.ylabel != null && this.ylabel.length > 0),
							labelString: this.ylabel
						},
                        ticks: {
							beginAtZero: true
                        }
					}]
				},
				animation: {
					duration: 750,
				}
			}
		};
		
        const canvas = <HTMLCanvasElement>this.$refs.canvas;
        const ctx = canvas.getContext('2d');
        if (ctx != null) {
            this.chart = new Chart(ctx, config);;
		}

		(<any>config).options.scales.xAxes[0].ticks.precision = 0;
		
		this.updateTitle();
	}

	updateData(): void {
		this.chartPoints = this.createPoints();
		this.chartDatasets = this.createDatasets();
		if (this.chart != null)
		{
			this.chart.data.labels = this.bars.map(x => x.label)
		}
	}

    updateChart(): void {
        const datasets = this.chart.data.datasets;
        if (datasets) {
			for (let i=0;i<this.chartPoints.length;i++)
			{
				if (datasets[i])
				{
            		datasets[i].data = this.chartPoints[i];
				}
			}
		}
		
        this.chart.update();
	}
	
	updateTitle(): void {
		if (this.chart == null
		|| this.chart.options == null
		|| this.chart.options.title == null) 
		{
			return;
		}

		let text = this.title;
		this.chart.options.title.text = text;
	}

    @Watch("bars")
    onBarsChanged(): void {
        if (this.chart != null) {
			this.updateData();
			this.updateChart();
			this.updateTitle();
        }
    }

    @Watch("sets")
    onSetsChanged(): void {
        if (this.chart != null) {
			this.updateData();
           	this.updateChart();
        }
    }

    @Watch("title")
    onTitleChanged(): void {
        if (this.chart != null) {
			this.updateTitle();
        }
    }
}
</script>

<style scoped lang="scss">
.bar-chart-component {
	position: relative;
}
</style>
