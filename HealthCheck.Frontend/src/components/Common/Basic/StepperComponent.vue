<template>
    <div class="stepper-component" :class="rootClasses">
		<h3>TODO: StepperComponent</h3>
        <div><b>altLabels:</b>' {{ altLabels }}'</div>
        <div><b>nonLinear:</b>' {{ nonLinear }}'</div>

		<slot></slot>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'

@Options({
    components: {}
})
export default class StepperComponent extends Vue {

    @Prop({ required: false, default: false })
    altLabels!: string | boolean;

    @Prop({ required: false, default: false })
    nonLinear!: string | boolean;


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
             'alt-labels': this.isAltLabels,
             'non-linear': this.isNonLinear
        };
    }

    get isAltLabels(): boolean { return ValueUtils.IsToggleTrue(this.altLabels); }
    get isNonLinear(): boolean { return ValueUtils.IsToggleTrue(this.nonLinear); }

    ////////////////
    //  METHODS  //
    //////////////

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
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.alt-labels { }
    &.non-linear { }
}
</style>
