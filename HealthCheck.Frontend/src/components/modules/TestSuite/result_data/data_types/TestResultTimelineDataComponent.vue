<!-- src/components/modules/TestSuite/result_data/data_types/TestResultTimelineDataComponent.vue -->
<template>
    <div>
      <stepper-component alt-labels non-linear>
        <v-stepper-header>
          <template v-for="(step, index) in steps" :key="`${id}-step-${index}`">
            <v-stepper-step :rules="[() => step.Error == null]" :step="step.Index + 1"
              class="stepper-step"
              @click="onStepClicked(step)"
              :complete-icon="step.Icon || undefined"
              :edit-icon="step.Icon || undefined"
              :complete=step.IsCompleted>
              {{ step.Title }}
              <small v-if="stepHasDate(step)">{{ formatStepDate(step) }}</small>
              <small v-if="step.Error != null" class="step-error">{{ step.Error.trunc(20) }}</small>
            </v-stepper-step>
            <v-divider
              v-if="index < steps.length - 1" 
              :key="`${id}-step-divider-${index}`"
            ></v-divider>
          </template>
        </v-stepper-header>
      </stepper-component>
      
      <dialog-component
        v-model:value="showStepDialog"
        max-width="640">
        <card-component v-if="dialogStep != null">
          <v-card-title class="headline">
            <icon-component large left v-if="dialogStep.Icon != null">
              {{ dialogStep.Icon }}
            </icon-component>
            <span class="title">{{ dialogStep.Title }}</span>
          </v-card-title>
          <v-card-text>
            <div v-if="stepHasDate(dialogStep)" class="mb-3">{{ formatStepDate(dialogStep) }}</div>

            {{ dialogStep.Description }}

            <div v-if="dialogStep.Links.length > 0" class="mt-2">
              <ul>
                <li v-for="(link, linkIndex) in dialogStep.Links"
                    :key="`${id}-step-${linkIndex}`">
                  <a :href="link[0]" target="_blank">{{ link[1] || link[0] }}</a>
                </li>
              </ul>
            </div>

            <div v-if="dialogStep.Error != null" class="step-error mt-3"><b>{{ dialogStep.Error }}</b></div>
          </v-card-text>

          <v-card-actions>
            <v-spacer></v-spacer>
            <btn-component color="green darken-1" flat @click="showStepDialog = false">
              Close
            </btn-component>
          </v-card-actions>
        </card-component>
      </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestResultDataDumpViewModel } from '@generated/Models/Core/TestResultDataDumpViewModel';
import TimelineStepViewModel from '@models/modules/TestSuite/TimelineStepViewModel';
import DateUtils from '@util/DateUtils';

@Options({
    components: {
    }
})
export default class TestResultTimelineDataComponent extends Vue {
    @Prop({ required: true })
    resultData!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;

    id!: string;
    dialogStep: TimelineStepViewModel | null = null;
    showStepDialog: boolean = false;

    created(): void {
      this.id = this.createGuid();
    }

    get steps(): Array<TimelineStepViewModel> {
      let items = JSON.parse(this.resultData.Data) as Array<TimelineStepViewModel>;
      
      items.forEach((item) => {
        let dateStr = (<any>item.Timestamp) as string;
        item.Timestamp = (dateStr === null || dateStr.length === 0) ? null : new Date(parseInt(dateStr))
      });
      
      return items;
    }

    onStepClicked(step: TimelineStepViewModel): void
    {
      this.dialogStep = step;
      this.showStepDialog = true;
    }

    createGuid(): string
    {
      let S4 = function() {
        return (((1+Math.random())*0x10000)|0).toString(16).substring(1);
      };
      return (S4()+S4()+"-"+S4()+"-"+S4()+"-"+S4()+"-"+S4()+S4()+S4());
    }

    stepHasDate(step: TimelineStepViewModel): boolean
    {
      return step.Timestamp != null;
    }

    formatStepDate(step: TimelineStepViewModel): string
    {
      if (step.HideTimeInTimestamp)
      {
        return DateUtils.FormatDate(<Date>step.Timestamp, 'd. MMM');
      } 
      else
      {
        return DateUtils.FormatDate(<Date>step.Timestamp, 'd. MMM HH:mm:ss');
      }
    }
}
</script>

<style scoped>
.stepper-step {
  cursor: pointer;
}
.step-error {
  color: var(--v-error-base) !important;
  font-weight: bold;
}
.stepper-step:hover {
  background-color: #eee;
}
</style>

<style>
.data-dump-timeline .v-stepper__label {
  text-align: center;
}
</style>
