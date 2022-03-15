<!-- src/components/modules/TestSuite/result_data/data_types/TestResultTimingsDataComponent.vue -->
<template>
    <div>
      <div v-for="(item, itemIndex) in items"
        :key="`${id}-timing-${itemIndex}`"
        class="timingbar">
        
        <tooltip-component bottom>
            <template v-slot:activator="{ on }">
              <span class="timingbar-label" v-on="on">{{ item.Description }}</span>
            </template>
            <span>{{ item.Description }}</span>
        </tooltip-component>

        <div class="timingbar-bar-wrapper">
          <tooltip-component bottom>
              <template v-slot:activator="{ on }">
                <div class="timingbar-bar" :style="getBarStyle(item, itemIndex)" v-on="on">
                  <div class="timing-bar-details-wrapper">
                    <div class="timing-bar-details">{{ item.DurationText }}</div>
                  </div>
                  <div class="timingbar-bar-inner" :style="getInnerBarStyle(item, itemIndex)"></div>
                </div>
              </template>
              <span>{{ item.DurationText }}</span>
          </tooltip-component>
        </div>
          
      </div>
    </div>
</template>

<script lang="ts">
import IdUtils from "@util/IdUtils";
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import TestResultDataDumpViewModel from '@models/modules/TestSuite/TestResultDataDumpViewModel';
import DateUtils from "@util/DateUtils";

interface Timing {
  Description: string;
  OffsetMilliseconds: number;
  DurationMilliseconds: number;
  EndMilliseconds: number;
}
interface TimingExt extends Timing {
  StartPercentage: number;
  EndPercentage: number;
}

@Options({
    components: {
    }
})
export default class TestResultTimingsDataComponent extends Vue {
    @Prop({ required: true })
    resultData!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;
    
    id: string = IdUtils.generateId();
    items: Array<TimingExt> = [];
    barColors: Array<string> = [
      '#66bb6a', '#42a5f5',
      '#ab47bc', '#26a69a',
      '#26c6da', '#ffca28', '#ef5350',
      '#8d6e63', '#ec407a', '#78909c'
    ];

    mounted(): void {
      let rawTimings: Array<Timing> = JSON.parse(this.resultData.Data);
      let maxValue = Math.max(...rawTimings.map(x => x.EndMilliseconds));

      this.items = rawTimings.map(x => {
        return {...x, ...{
          StartPercentage: Math.round(x.OffsetMilliseconds / maxValue * 10) * 10,
          EndPercentage: Math.round(x.EndMilliseconds / maxValue * 10) * 10,
          DurationText: DateUtils.prettifyDuration(x.DurationMilliseconds)
        }};
      });
    }

    getBarStyle(bar: TimingExt, index: number): any
    {
      const colorIndex = index % this.barColors.length;

      const base = {
        left: `${bar.StartPercentage}%`,
        width: `${(bar.EndPercentage - bar.StartPercentage)}%`,
        border: `2px solid ${this.barColors[colorIndex]}`
      };
      return base;
    }

    getInnerBarStyle(bar: TimingExt, index: number): any
    {
      const colorIndex = index % this.barColors.length;

      const base = {
        'background-color': this.barColors[colorIndex]
      };
      return base;
    }
}
</script>

<style scoped lang="scss">
.data-dump-timings {
  padding: 1rem;
  border: #eaeaea 1px solid;
  border-radius: 15px;
}
.timingbar {
  display: flex;
  flex-flow: nowrap;
  margin-bottom: 10px;

  .timingbar-label {
    width: 120px;
    min-width: 120px;
    max-width: 120px;
    align-self: center;
    padding-right: 5px;
    text-overflow: ellipsis;
    white-space: nowrap;
    overflow: hidden;
  }

  .timingbar-bar-wrapper
  {
    width: 100%;
    border-left:  1px solid silver;
    border-right: 1px solid silver;
    background-color: #eee;

    .timingbar-bar {
      position: relative;
      min-width: 5px;
      height: 30px;
      border-radius: 15px;

      .timingbar-bar-inner {
        width: 100%;
        height: 100%;
        border-radius: 15px;
        opacity: 0.4;
      }

      .timing-bar-details-wrapper {
        position: absolute;
        width: 100%;
        height: 100%;
        overflow: hidden;
      
        display: flex;
        align-items: center;
        justify-content: center;

        .timing-bar-details {
          /* color: #fff; */
          font-size: small;
        }
      }
    }
  }
}
</style>
