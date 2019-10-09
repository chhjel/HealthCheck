<!-- src/components/Overview/ItemPerDateChartComponent.vue -->
<template>
    <div class="item-per-date-chart" v-show="entries.length > 0">
        <canvas ref="canvas" />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import LinqUtils from "../../util/LinqUtils";
import LogEntrySearchResultItem from '../../models/LogViewer/LogEntrySearchResultItem';
import { FilterDelimiterMode } from '../../models/LogViewer/FilterDelimiterMode';
import DateUtils from "../../util/DateUtils";
import { Chart, LinearTickOptions, ChartPoint } from 'chart.js';

export interface ChartEntry {
    date: Date;
    label: string;
}
interface ChartDataPoint extends Chart.ChartPoint {
    pointTitle: string | null;
    pointLabel: string | null;
}

@Component({
    components: { }
})
export default class ItemPerDateChartComponent extends Vue {
    @Prop({ required: true })
    entries!: Array<ChartEntry>;

    chart!: Chart;
    chartPoints: Array<ChartDataPoint> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.initChart();
    }

    ////////////////
    //  GETTERS  //
    //////////////

    ////////////////
    //  METHODS  //
    //////////////
    getDateRange(): number {
        const dates = this.entries.map(x => x.date);
        const min = dates.reduce((a, b) => { return a < b ? a : b; }); 
        const max = dates.reduce((a, b) => { return a > b ? a : b; });
        return max.getTime() - min.getTime();
    }
    createDateItems(): Array<ChartDataPoint> {
        const dateRange = this.getDateRange();
        const groupedEntries = LinqUtils.GroupByInto(
            this.entries,
            (entry) => this.getGroupedEntryKey(entry, dateRange),
            (key, items) => {
                const date = items[0].date;
                const count = items.length;
                const entryWord = (count == 1) ? 'match' : 'matches';
                return {
                    pointTitle: this.getPointTitle(date, dateRange),
                    pointLabel: `${count} ${entryWord}`,
                    x: date,
                    y: count
                };
            });

        return groupedEntries;
    }

    getGroupedEntryKey(entry: ChartEntry, dateRange: number): string {
        return this.getGroupedEntryDate(entry, dateRange).toString();
    }

    getSeriesTitle(): string {
        const dateRange = this.getDateRange();
        if (dateRange > 7 * 24 * 60 * 60 * 1000) {
            return 'Matching log entries per day';
        }
        else if (dateRange > 8 * 60 * 60 * 1000) {
            return 'Matching log entries per hour';
        }
        else {
            return 'Matching log entries per minute';
        }
    }

    getPointTitle(date: Date, dateRange: number): string {
        if (dateRange > 7 * 24 * 60 * 60 * 1000) {
            return DateUtils.FormatDate(date, 'd. MMM. yyyy');
        }
        else if (dateRange > 8 * 60 * 60 * 1000) {
            return DateUtils.FormatDate(date, 'HH') + ':xx ' + DateUtils.FormatDate(date, 'd. MMM. yyyy');
        }
        else {
            return DateUtils.FormatDate(date, 'HH:mm') + ':xx ' + DateUtils.FormatDate(date, 'd. MMM. yyyy');
        }
    }

    getGroupedEntryDate(entry: ChartEntry, dateRange: number): Date {
        const date = entry.date;
        if (dateRange > 7 * 24 * 60 * 60 * 1000) {
            date.setHours(0);
        }
        if (dateRange > 8 * 60 * 60 * 1000) {
            date.setMinutes(0);
        }
        date.setSeconds(0);
        date.setMilliseconds(0);
        return date;
    }

    initChart(): void
    {
        this.chartPoints = this.createDateItems();
		var color = Chart.helpers.color;
		var config: Chart.ChartConfiguration = {
			type: 'bar',
			data: {
				datasets: [
                    {
                        pointHitRadius: 20,
                        label: this.getSeriesTitle(),
                        backgroundColor: color('#FF0000').alpha(0.5).rgbString(),
                        borderColor: color('#0000FF'),
                        fill: true,
                        data: this.chartPoints
                    }
                ]
			},
			options: {
				responsive: true,
				title: {
					display: false,
					text: 'Matching log entries per date'
                },
                elements: {
                    line: {
                        tension: 0.1
                    }
                },
                tooltips: {
                    callbacks: {
                        title: (tooltipItem:any, data:any) => {
                            const index = <number>tooltipItem[0].index;
                            return this.chartPoints[index].pointTitle || "";
                        },
                        label: (tooltipItem:any, data:any) => {
                            const index = <number>tooltipItem.index;
                            return this.chartPoints[index].pointLabel || "";
                        }
                    }
                },
				scales: {
					xAxes: [{
						type: 'time',
						display: true,
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
						scaleLabel: {
							display: true,
							labelString: 'Matches'
						},
                        ticks: {
                            beginAtZero: true
                        }
					}]
				}
			}
        };
        
        const canvas = <HTMLCanvasElement>this.$refs.canvas;
        const ctx = canvas.getContext('2d');
        if (ctx != null) {
            this.chart = new Chart(ctx, config);;
        }
    }

    updateChart(): void {
        this.chartPoints = this.createDateItems();
        const datasets = this.chart.data.datasets;
        if (datasets != undefined) {
            datasets[0].data = this.chartPoints;
            datasets[0].label = this.getSeriesTitle();
        }
        this.chart.update();
    }

    @Watch("entries")
    onEntriesChanged(): void {
        if (this.chart != null) {
           this.updateChart();
        }
    }
}
</script>

<style scoped>
</style>