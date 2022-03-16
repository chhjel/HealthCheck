<template>
    <div class="tooltip-component" :class="rootClasses" :title="tooltip">
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
export default class TooltipComponent extends Vue {
    @Prop({ required: false, default: '' })
    tooltip!: string;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

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
             'disabled': this.isDisabled
        };
    }

    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }

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
.tooltip-component {
	/* border: 2px solid red;
	padding: 5px;
	margin: 5px; */
    &.disabled { }
}
</style>
