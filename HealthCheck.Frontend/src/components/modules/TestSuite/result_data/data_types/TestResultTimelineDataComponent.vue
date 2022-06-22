<!-- src/components/modules/TestSuite/result_data/data_types/TestResultTimelineDataComponent.vue -->
<template>
    <div>
      <stepper-component :steps="steps" @stepClicked="onStepClicked" ref="stepperElement" />
      
      <dialog-component
        v-model:value="showStepDialog"
        max-width="640">
        <template #header v-if="dialogStep != null">
          <icon-component large left v-if="dialogStep.Icon != null">
            {{ dialogStep.Icon }}
          </icon-component>
          <span class="title">{{ dialogStep.Title }}</span>
        </template>
        <template #footer>
          <btn-component color="secondary" @click="showStepDialog = false">Close</btn-component>
        </template>
        <div v-if="dialogStep != null">
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
        </div>
      </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestResultDataDumpViewModel } from '@generated/Models/Core/TestResultDataDumpViewModel';
import DateUtils from '@util/DateUtils';
import IdUtils from "@util/IdUtils";
import { StepperComponentStepViewModel } from '@components/Common/Basic/StepperComponent.vue.models';
import StepperComponent from "@components/Common/Basic/StepperComponent.vue";

@Options({
    components: {
    }
})
export default class TestResultTimelineDataComponent extends Vue {
    @Prop({ required: true })
    resultData!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;

    @Ref() readonly stepperElement!: StepperComponent;

    id: string = IdUtils.generateId();
    dialogStep: StepperComponentStepViewModel | null = null;
    showStepDialog: boolean = false;

    get steps(): Array<StepperComponentStepViewModel> {
      let items = JSON.parse(this.resultData.Data) as Array<StepperComponentStepViewModel>;
      
      items.forEach((item) => {
        let dateStr = (<any>item.Timestamp) as string;
        item.Timestamp = (dateStr === null || dateStr.length === 0) ? null : new Date(parseInt(dateStr))
      });
      
      return items;
    }

    onStepClicked(step: StepperComponentStepViewModel): void
    {
      this.dialogStep = step;
      this.showStepDialog = true;
    }

    stepHasDate(step: StepperComponentStepViewModel): boolean { return this.stepperElement.stepHasDate(step); }
    formatStepDate(step: StepperComponentStepViewModel): string { return this.stepperElement.formatStepDate(step); }
}
</script>

<style scoped>
</style>
