<!-- src/components/modules/Metrics/RequestMetricsSummaryComponent.vue -->
<template>
    <div>
        <shadow-root>

            <div @click="toggleVisibility" :style="styleToggleMetrics">{{ toggleButtonContent }}</div>

            <div :style="styleRoot" v-if="visible">

                <metrics-block-component title="Timeline" v-if="items.length > 0">
                    <div v-for="(item, itemIndex) in items"
                        :key="`${id}-timing-${itemIndex}`"
                        :style="styleTimingbar">
                        <span :style="styleTimingbarLabel" :title="item.Description || item.Id">
                            {{ item.Description || item.Id }}
                        </span>

                        <div :style="styleTimingbarBarWrapper">
                            <div :style="getBarStyle(item, itemIndex)">
                                <div :style="styleTimingBarDetailsWrapper">
                                    <span :style="styleTimingBarDetails"
                                        :title="item.DurationText"
                                        v-if="item.Type == 'Timing'"
                                        >{{ item.DurationText }}</span>
                                    <span v-if="item.Type == 'Error'" style="font-size: 10px">❌</span>
                                </div>
                                <div :style="getInnerBarStyle(item, itemIndex)"></div>
                            </div>
                        </div>
                    </div>
                </metrics-block-component>

                <metrics-block-component title="Errors"
                                         v-if="errorItems.length > 0"
                                         style="margin-top: 10px">
                    <div v-for="(item, itemIndex) in errorItems"
                        :key="`${id}-error-${itemIndex}`"
                        :style="styleErrorItem">
                        <code :style="styleErrorDescription">
                            {{ item.Description }}
                        </code>

                        <code :style="styleErrorExceptionDetails" v-if="item.ExceptionDetails">
                            {{ item.ExceptionDetails }}
                        </code>
                    </div>
                </metrics-block-component>

                <metrics-block-component title="Global values"
                                         v-if="globalValues.length > 0"
                                         style="margin-top: 10px">
                    <ul>
                        <li v-for="(item, itemIndex) in globalValues"
                            :key="`${id}-gvalues-${itemIndex}`">
                            <b>{{ item.key }}:</b> {{ item.values }}
                        </li>
                    </ul>
                </metrics-block-component>

                <metrics-block-component title="Global counters incremented"
                                         v-if="globalCounters.length > 0"
                                         style="margin-top: 10px">
                    <ul>
                        <li v-for="(item, itemIndex) in globalCounters"
                            :key="`${id}-gcounter-${itemIndex}`">
                            <b>{{ item.key }}:</b> {{ item.value }}
                        </li>
                    </ul>
                </metrics-block-component>
            </div>
        </shadow-root>
    </div>
</template>

<script lang="ts">
import IdUtils from "util/IdUtils";
import { Vue, Component, Prop, } from "vue-property-decorator";
import { HCMetricsContext } from 'generated/Models/Core/HCMetricsContext';
import DateUtils from "util/DateUtils";
import MetricsBlockComponent from 'components/modules/Metrics/MetricsBlockComponent.vue';
import { HCMetricsItem } from "generated/Models/Core/HCMetricsItem";
import { MetricItemType } from "generated/Enums/Core/MetricItemType";

interface TimingExt extends HCMetricsItem {
  StartPercentage: number;
  EndPercentage: number;
}
@Component({
    components: {
        MetricsBlockComponent
    }
})
export default class RequestMetricsSummaryComponent extends Vue {
    @Prop()
    config!: HCMetricsContext;

    id: string = IdUtils.generateId();
    items: Array<TimingExt> = [];
    visible: boolean = false;
    barColors: Array<string> = [
      '#66bb6a', '#42a5f5',
      '#ab47bc', '#26a69a',
      '#26c6da', '#ffca28', '#ef5350',
      '#8d6e63', '#ec407a', '#78909c'
    ];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
      let maxValue = Math.max(...this.config.Items.map(x => x.EndMilliseconds));

      this.items = this.config.Items.map(x => {
        return {...x, ...{
          StartPercentage: Math.round(x.OffsetMilliseconds / maxValue * 10) * 10,
          EndPercentage: Math.round(x.EndMilliseconds / maxValue * 10) * 10,
          DurationText: DateUtils.prettifyDuration(x.DurationMilliseconds)
        }};
      });
    }

    getBarStyle(bar: TimingExt, index: number): any
    {
      const color = this.getBarColor(bar, index);

      let base: any = {
        left: `${bar.StartPercentage}%`,
        width: `${(bar.EndPercentage - bar.StartPercentage)}%`,
        border: `2px solid ${color}`
      };

      if (bar.Type !== MetricItemType.Timing)
      {
        base['height'] = '16px';
        base['width'] = '16px';
        base['margin-left'] = '-8px';
        base['border-radius'] = '50%';
      }

      return { ...this.styleTimingbarBar, ...base };
    }

    getInnerBarStyle(bar: TimingExt, index: number): any
    {
      const color = this.getBarColor(bar, index);

      const base = {
        'background-color': color
      };
      return { ...this.styleTimingbarBarInner, ...base };
    }

    getBarColor(bar: TimingExt, index: number): string {
        const hasError = bar.Type == MetricItemType.Error;
        const colorIndex = index % this.barColors.length;
        return hasError ? 'red' : this.barColors[colorIndex];
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalCounters(): Array<any> {
        return Object.keys(this.config.GlobalCounters).map(x => {
            return {
                key: x,
                value: this.config.GlobalCounters[x]
            };
        });
    }

    get globalValues(): Array<any> {
        return Object.keys(this.config.GlobalValues).map(x => {
            return {
                key: x,
                values: this.config.GlobalValues[x]
            };
        });
    }

    get errorItems(): Array<any> {
        return this.items.filter(x => x.Type == MetricItemType.Error);
    }

    get hasError(): boolean {
        return this.config.Items.some(x => x.Type == MetricItemType.Error);
    }

    get toggleButtonContent(): string {
        return this.hasError ? '❌' : '⌚';
    }

    //////////////
    //  STYLE  //
    ////////////
    get styleToggleMetrics(): any {
        const base: any = {
            "height": "1cm",
            "width": "1cm",
            "border-radius": "50%",

            "position": "fixed",
            "left": "10px",
            "bottom": "10px",
            
            "cursor": "pointer",
            "opacity": "0.8",
            "font-size": "0.6cm",

            "display": "flex",
            "justify-content": "center",
            "align-items": "center",
            "user-select": "none"
        };

        let colors: any = {
            "background-color": "#bed2ff",
            "border": "1px solid #929fbd",
        };
        if (this.hasError)
        {
            colors["background-color"] = "#f1b1b1";
            colors["border"] = "1px solid #a51e1e";
        }

        return { ...base, ...colors };
    }

    get styleRoot(): any {
        return {
            'margin': '40px',
            'padding': '20px',
            'font-family': 'sans-serif',
            'color': '#333'
        };
    }

    get styleTimingbar(): any {
        return {
            'display': 'flex',
            'flex-flow': 'nowrap',
            'margin-bottom': '10px',
            "position": "relative"
        };
    }

    get styleTimingbarLabel() : any {
        return {
            "width": "120px",
            "min-width": "120px",
            "max-width": "120px",
            "align-self": "center",
            "padding-right": "5px",
            "text-overflow": "ellipsis",
            "white-space": "nowrap",
            "overflow": "hidden"
        }
    }

    get styleTimingbarBarWrapper() : any {
        return {
            "width": "100%",
            "position": "relative"
        }
    }

    get styleTimingbarBar() : any {
        return {
            "position": "relative",
            "min-width": "5px",
            "height": "30px",
            "border-radius": "15px"
        }
    }

    get styleTimingbarBarInner() : any {
        return {
            "width": "100%",
            "height": "100%",
            "border-radius": "15px",
            "opacity": "0.4"
        }
    }

    get styleTimingBarDetailsWrapper() : any {
        return {
            "position": "absolute",
            "width": "100%",
            "height": "100%",
            "overflow": "hidden",
            "display": "flex",
            "align-items": "center",
            "justify-content": "center"
        }
    }

    get styleTimingBarDetails() : any {
        return {
            "font-size": "small",
            "display": "block"
        }
    }

    get styleErrorItem() : any {
        return {
        }
    }

    get styleErrorDescription() : any {
        return {
            "display": "inline-block",
            "margin": "5px 0",
            "padding": "5px",
            "font-weight": "600",
            "background": "#ffd6d6",
            "overflow-wrap": "break-word"
        }
    }

    get styleErrorExceptionDetails() : any {
        return {
            "display": "block",
            "padding": "5px",
            "margin-bottom": "10px",
            "border": "1px solid #b36b6b",
            "background": "#ffd6d6",
            "overflow-wrap": "break-word"
        }
    }

    ////////////////
    //  METHODS  //
    //////////////
    toggleVisibility(): void {
        this.visible = !this.visible;

        if (this.visible)
        {
            setTimeout(() => {
                window.scrollTo({
                    top: (window.pageYOffset || document.documentElement.scrollTop) 
                        + this.$el.getBoundingClientRect().top - 100,
                    behavior: 'smooth'
                });
            }, 10);
        }
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
</style>
