<template>
    <div class="timeline-component" :class="rootClasses">
		<h3>TODO: TimelineComponent</h3>
        <div><b>alignTop:</b>' {{ alignTop }}'</div>
        <div><b>dense:</b>' {{ dense }}'</div>

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
export default class TimelineComponent extends Vue {

    @Prop({ required: false, default: false })
    alignTop!: string | boolean;

    @Prop({ required: false, default: false })
    dense!: string | boolean;


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
             'align-top': this.isAlignTop,
             'dense': this.isDense
        };
    }

    get isAlignTop(): boolean { return ValueUtils.IsToggleTrue(this.alignTop); }
    get isDense(): boolean { return ValueUtils.IsToggleTrue(this.dense); }

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
.timeline-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.align-top { }
    &.dense { }
}
</style>
