<template>
    <div class="card-component" :class="rootClasses">
		<h3>TODO: CardComponent</h3>
        <div><b>flat:</b>' {{ flat }}'</div>
        <div><b>color:</b>' {{ color }}'</div>
        <div><b>minWidth:</b>' {{ minWidth }}'</div>
        <div><b>maxWidth:</b>' {{ maxWidth }}'</div>

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
export default class CardComponent extends Vue {

    @Prop({ required: false, default: false })
    flat!: string | boolean;

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: 0 })
    minWidth!: number;

    @Prop({ required: false, default: 0 })
    maxWidth!: number;


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
             'flat': this.isFlat
        };
    }

    get isFlat(): boolean { return ValueUtils.IsToggleTrue(this.flat); }

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
.card-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.flat { }
}
</style>
