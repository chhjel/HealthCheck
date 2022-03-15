<template>
    <div class="toolbar-component" :class="rootClasses">
		<h3>TODO: ToolbarComponent</h3>
        <div><b>clippedLeft:</b>' {{ clippedLeft }}'</div>
        <div><b>color:</b>' {{ color }}'</div>

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
export default class ToolbarComponent extends Vue {

    @Prop({ required: false, default: false })
    clippedLeft!: string | boolean;

    @Prop({ required: false, default: null })
    color!: string;


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
             'clipped-left': this.isClippedLeft
        };
    }

    get isClippedLeft(): boolean { return ValueUtils.IsToggleTrue(this.clippedLeft); }

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
.toolbar-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.clipped-left { }
}
</style>
