<!-- src/components/modules/LogViewer/ItemPerDateChartComponent.vue -->
<template>
    <div class="item-per-date-chart" v-show="entries.length > 0">
        <canvas ref="canvas" />
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import LinqUtils from '@util/LinqUtils';
import { LogEntrySearchResultItem } from '@generated/Models/Core/LogEntrySearchResultItem';
import { FilterDelimiterMode } from '@models/modules/LogViewer/FilterDelimiterMode';
import DateUtils from '@util/DateUtils';
import { Chart, LinearTickOptions, ChartPoint } from 'chart.js';
import { LogEntrySeverity } from '@generated/Enums/Core/LogEntrySeverity';

export interface ChartEntry {
    date: Date;
    label: string;
    severity: LogEntrySeverity;
}
interface ChartDataPoint extends Chart.ChartPoint {
    pointTitle: string | null;
    pointLabel: string | null;
    severity: LogEntrySeverity;
}

@Options({
    components: { }
})
export default class ItemPerDateChartComponent extends Vue {
    @Prop({ required: true })
    entries!: Array<ChartEntry>;

    chart!: Chart;
    chartPoints: Array<Array<ChartDataPoint>> = [];

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

    getGroupedEntryKey(entry: ChartEntry, dateRange: number): string {
        return this.getGroupedEntryDate(entry, dateRange).toString();
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

    createDateItems(): Array<Array<ChartDataPoint>> {
        let errorList = new Array<ChartDataPoint>();
        let warningList = new Array<ChartDataPoint>();
        let infoList = new Array<ChartDataPoint>();

        const dateRange = this.getDateRange();
        const groupedEntries = LinqUtils.GroupByInto(
            this.entries,
            (entry) => this.getGroupedEntryKey(entry, dateRange),
            (key, items) => {
                const date = items[0].date;

                const errors = items.filter(x => x.severity == LogEntrySeverity.Error);
                const errorCount = errors.length;
                const errorEntryWord = (errorCount == 1) ? 'error' : 'errors';

                const warnings = items.filter(x => x.severity == LogEntrySeverity.Warning);
                const warningCount = warnings.length;
                const warningEntryWord = (warningCount == 1) ? 'warning' : 'warnings';

                const infos = items.filter(x => x.severity == LogEntrySeverity.Info);
                const infoCount = infos.length;
                const infoEntryWord = (infoCount == 1) ? 'other' : 'others';

                errorList.push({
                    pointTitle: this.getPointTitle(date, dateRange),
                    pointLabel: `${errorCount} ${errorEntryWord}`,
                    x: date,
                    y: errorCount,
                    severity: LogEntrySeverity.Error
                });
                warningList.push({
                    pointTitle: this.getPointTitle(date, dateRange),
                    pointLabel: `${warningCount} ${warningEntryWord}`,
                    x: date,
                    y: warningCount,
                    severity: LogEntrySeverity.Warning
                });
                infoList.push({
                    pointTitle: this.getPointTitle(date, dateRange),
                    pointLabel: `${infoCount} ${infoEntryWord}`,
                    x: date,
                    y: infoCount,
                    severity: LogEntrySeverity.Info
                });

                return [] as Array<ChartDataPoint>;
            });

        return [
            errorList,
            warningList,
            infoList
        ];
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
                        label: `errors`,
                        backgroundColor: color('#FF0000').alpha(0.5).rgbString(),
                        borderColor: color('#FF0000'),
                        fill: true,
                        data: this.chartPoints[0]
                    },
                    {
                        pointHitRadius: 20,
                        label: `warnings`,
                        backgroundColor: color('#f36f04').alpha(0.5).rgbString(),
                        borderColor: color('#f36f04'),
                        fill: true,
                        data: this.chartPoints[1]
                    },
                    {
                        pointHitRadius: 20,
                        label: `other`,
                        backgroundColor: color('#0000FF').alpha(0.5).rgbString(),
                        borderColor: color('#0000FF'),
                        fill: true,
                        data: this.chartPoints[2]
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
                            const datasetIndex = tooltipItem[0].datasetIndex;
                            const dataset = this.chartPoints[datasetIndex];
                            const index = <number>tooltipItem[0].index;
                            return dataset[index].pointTitle || "";
                        },
                        label: (tooltipItem:any, data:any) => {
                            const datasetIndex = tooltipItem.datasetIndex;
                            const dataset = this.chartPoints[datasetIndex];
                            const index = <number>tooltipItem.index;
                            return dataset[index].pointLabel || "";
                        }
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
            datasets[0].data = this.chartPoints[0];
            datasets[1].data = this.chartPoints[1];
            datasets[2].data = this.chartPoints[2];
            // datasets[0].data = this.chartPoints;
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