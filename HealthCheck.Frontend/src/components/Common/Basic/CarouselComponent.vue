<template>
    <div class="carousel-component" :class="rootClasses">
		<h3>TODO: CarouselComponent</h3>
        <div><b>height:</b>' {{ height }}'</div>
        <div><b>cycle:</b>' {{ cycle }}'</div>
        <div><b>hideDelimiters:</b>' {{ hideDelimiters }}'</div>
        <div><b>hideControls:</b>' {{ hideControls }}'</div>

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
export default class CarouselComponent extends Vue {

    @Prop({ required: false, default: 0 })
    height!: number;

    @Prop({ required: false, default: false })
    cycle!: string | boolean;

    @Prop({ required: false, default: false })
    hideDelimiters!: string | boolean;

    @Prop({ required: false, default: false })
    hideControls!: string | boolean;


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
             'cycle': this.isCycle,
             'hide-delimiters': this.isHideDelimiters,
             'hide-controls': this.isHideControls
        };
    }

    get isCycle(): boolean { return ValueUtils.IsToggleTrue(this.cycle); }
    get isHideDelimiters(): boolean { return ValueUtils.IsToggleTrue(this.hideDelimiters); }
    get isHideControls(): boolean { return ValueUtils.IsToggleTrue(this.hideControls); }

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
.carousel-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.cycle { }
    &.hide-delimiters { }
    &.hide-controls { }
}
</style>
