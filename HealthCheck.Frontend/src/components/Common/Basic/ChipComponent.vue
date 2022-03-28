<template>
    <div class="chip-component" :class="rootClasses">
		<!-- <h3>TODO: ChipComponent</h3>
        <div><b>color:</b>' {{ color }}'</div>
        <div><b>outline:</b>' {{ outline }}'</div>
        <div><b>disabled:</b>' {{ disabled }}'</div> -->
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
export default class ChipComponent extends Vue {

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: false })
    outline!: string | boolean;

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
        let classes = {
             'outline': this.isOutline,
             'disabled': this.isDisabled
        };
        classes[this.color || 'accent'] = true;
        classes['hoverable'] = this.hasClickListener;
        classes['clickable'] = this.hasClickListener;
        return classes;
    }

    get isOutline(): boolean { return ValueUtils.IsToggleTrue(this.outline); }
    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }
    get hasClickListener(): boolean {
        return this.$attrs && this.$attrs.onClick != null;
    }

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
.chip-component {
    padding: 2px 10px;
    border-radius: 20px;
    display: inline-block;
    font-size: 13px;
    display: inline-flex;
    align-items: center;
    flex-wrap: nowrap;
    min-height: 25px;
    &.outline { }
    &.disabled { }
}
</style>
