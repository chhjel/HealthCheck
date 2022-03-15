<template>
    <div class="toolbar-component" :class="rootClasses">
		<!-- <h3>TODO: ToolbarComponent</h3>
        <div><b>clippedLeft:</b>' {{ clippedLeft }}'</div>
        <div><b>color:</b>' {{ color }}'</div> -->
        <div class="toolbar-component__items">
		    <slot></slot>
        </div>
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
    background-color: #fff;
    box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.02), 0 3px 2px 0 rgba(0, 0, 0, 0.02), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
    z-index: 99;
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.clipped-left { }

    &__items {
        display: flex;
        flex-direction: row;
        flex-wrap: nowrap;
        overflow-y: hidden;
        overflow-x: auto;
        overflow: overlay hidden;
        -ms-overflow-style: none;

        &::-webkit-scrollbar {
            display: none;
        }
    }
}
</style>

<style lang="scss">
.toolbar-component {
    .button-component {

    }
}
</style>
