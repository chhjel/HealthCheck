<!-- src/components/Common/Charts/DataOverTimeChartComponent.vue -->
<template>
    <div class="dot-chart-component">
        <canvas ref="canvas" />
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { Chart, LinearTickOptions, ChartPoint, ChartDataSets } from 'chart.js';
import DateUtils from '@util/DateUtils';
import LinqUtils from '@util/LinqUtils';

export interface ChartEntry {
    date: Date;
    group: string;
}
export interface ChartSet {
	label: string;
	group: string;
    color: string;
}

interface ChartDataPoint extends Chart.ChartPoint {
}

@Options({
	components: {}
})
export default class DataOverTimeChartComponent extends Vue {
    @Prop({ required: true })
	entries!: Array<ChartEntry>;

    @Prop({ required: true })
	sets!: Array<ChartSet>;

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

    getDateRange(customMax: Date | null = null): number {
		if (this.entries.length <= 2)
		{
			return 0;
		}

        const dates = this.entries.map(x => x.date);
        const min = dates.reduce((a, b) => { return a < b ? a : b; }); 
        const max = customMax ?? dates.reduce((a, b) => { return a > b ? a : b; });
        return max.getTime() - min.getTime();
    }

    getGroupedEntryDate(entry: ChartEntry, dateRange: number): Date {
		const date = new Date(entry.date);
        if (dateRange > (7 * 24 * 60 * 60 * 1000)) {
            date.setHours(0);
        }
        if (dateRange > (8 * 60 * 60 * 1000)) {
            date.setMinutes(0);
        }
        date.setSeconds(0);
        date.setMilliseconds(0);
        return date;
    }

    getGroupedEntryKey(entry: ChartEntry, dateRange: number): string {
        return this.getGroupedEntryDate(entry, dateRange).toString();
    }

	createPoints(): Array<Array<ChartDataPoint>> {
		let pointSets: Array<Array<ChartDataPoint>> = [];
		const dateRange = this.getDateRange();
		
		for (let set of this.sets) {
			pointSets.push([]);
		}

		const entriesCopy = (<Array<ChartEntry>>JSON.parse(JSON.stringify(this.entries)));
		const sortedEntries = entriesCopy.sort((a,b) => LinqUtils.SortBy(a, b, x => x.date));
        const groupedEntries = LinqUtils.GroupByInto(
            sortedEntries,
            (entry) => this.getGroupedEntryKey(entry, dateRange),
            (key, items) => {
                const date = items[0].date;

				for(let i=0;i<this.sets.length;i++)
				{
					const set = this.sets[i];
					const setEntries = items.filter(x => x.group == set.group);
					const count = setEntries.length;

					pointSets[i].push({
						x: date,
						y: count
					});
				}

                return [] as Array<ChartDataPoint>;
            });

		return pointSets;
	}

    initChart(): void
    {
		this.updateData();

		var color = Chart.helpers.color;
		var config: Chart.ChartConfiguration = {
			type: 'line',
			data: {
				datasets: this.chartDatasets
			},
			options: {
				responsive: true,
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
						type: 'time',
						display: true,
                        stacked: true,
						scaleLabel: {
							display: true,
							labelString: 'Time'
						},
						ticks: {
							major: {
								fontStyle: 'bold',
								fontColor: '#FF0000'
							}
						},
                        time: {
                            // displayFormats: {
                            //     millisecond: 'HH:mm';
                            //     second: 'HH:mm';
                            //     minute: 'HH:mm';
                            //     hour: 'HH:mm'
                            // }
                        }
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
		
		(<any>config).options.scales.yAxes[0].ticks.precision = 0;
        
        const canvas = <HTMLCanvasElement>this.$refs.canvas;
        const ctx = canvas.getContext('2d');
        if (ctx != null) {
            this.chart = new Chart(ctx, config);;
		}
		
		this.updateTitle();
	}

	updateData(): void {
		this.chartPoints = this.createPoints();
		this.chartDatasets = this.createDatasets();
	}

    updateChart(): void {
        const datasets = this.chart.data.datasets;
        if (datasets != undefined) {
			for (let i=0;i<this.chartPoints.length;i++)
			{
            	datasets[i].data = this.chartPoints[i];
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

		let text = this.title
			.replace('[timespan]', this.timespanText())
			.replace('[count]', this.entries.length.toString());

		this.chart.options.title.text = text;
	}

	timespanText(): string {
		const dateRange = this.getDateRange(new Date());

		const minute = 60 * 1000;
		const hour = 60 * minute;
		const day = 24 * hour;

		const days = Math.floor(dateRange / day);
		const hours = Math.floor(dateRange / hour);
		const minutes = Math.floor(dateRange / minute);

        if (dateRange > (2 * day)) {
			return (days <= 1) ? `day` : `${days} days`;
        }
        else if (dateRange > (3 * hour)) {
			return (hours <= 1) ? `hour` : `${hours} hours`;
		}
		else {
			return (minutes <= 1) ? `minute` : `${minutes} minutes`;
		}
	}

    @Watch("entries")
    onEntriesChanged(): void {
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
.dot-chart-component {
	position: relative;
}
</style>
