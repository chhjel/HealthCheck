<template>
    <div class="stepper-component" :class="rootClasses">
        <template v-for="(step, index) in steps" :key="`${id}-step-${index}`">
            <div class="stepper-component__step"
                :class="stepClasses(step)"
                @click="onStepClicked(step)">
                <div class="stepper-component__title">
                    {{ step.Title }}
                    <icon-component v-if="step.Icon" class="stepper-component__icon">{{ step.Icon }}</icon-component>
                </div>
                <div v-if="stepHasDate(step)" class="stepper-component__date">{{ formatStepDate(step) }}</div>
                <div v-if="step.Error != null" class="stepper-component__error">{{ step.Error.trunc(20) }}</div>
            </div>
            <div class="stepper-component__arrow" v-if="index < steps.length-1">
                <icon-component large>arrow_right_alt</icon-component>
            </div>
        </template>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { StepperComponentStepViewModel } from '@components/Common/Basic/StepperComponent.vue.models';
import DateUtils from "@util/DateUtils";
import IdUtils from "@util/IdUtils";

@Options({
    components: {}
})
export default class StepperComponent extends Vue {
    @Prop({ required: true })
    steps: Array<StepperComponentStepViewModel>;

    id: string = IdUtils.generateId();
    
    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {

    }

    ////////////////
    //  GETTERS  //
    //////////////
    get rootClasses(): any {
        return {
        };
    }

    ////////////////
    //  METHODS  //
    //////////////
    stepClasses(step: StepperComponentStepViewModel): any {
        let classes: any = {
            'completed': step.IsCompleted
        }
        return classes;
    }
    onStepClicked(step: StepperComponentStepViewModel): void
    {
        this.$emit('stepClicked', step);
    }

    public stepHasDate(step: StepperComponentStepViewModel): boolean
    {
      return step.Timestamp != null;
    }

    public formatStepDate(step: StepperComponentStepViewModel): string
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

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
	
    /////////////////
    //  WATCHERS  //
    ///////////////

}
</script>

<style scoped lang="scss">
.stepper-component {
	padding: 5px;
    display: flex;
    flex-wrap: wrap;
    justify-content: center;

    &__arrow {
        align-self: center;
        margin-left: 5px;
        margin-right: 5px;
        margin-bottom: 10px;
    }
    &__step {
        cursor: pointer;
        padding: 10px;
        margin-bottom: 10px;
        border: 4px solid #eee;
        min-width: 160px;
        &:hover {
            background-color: #eee;
            border: 4px solid #ddd;
        }
    }
    &__title {
        font-weight: 600;
        display: flex;
        align-items: center;
    }
    &__icon {
        margin-left: 5px;
        color: #333;
    }
    &__date {
        font-size: smaller;
    }
    &__error {
        font-size: smaller;
        color: var(--color--error-base) !important;
        font-weight: bold;
        margin-top: 10px;
    }
}
</style>
