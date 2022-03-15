<template>
    <div class="dialog-component" :class="rootClasses">
		<h3>TODO: DialogComponent</h3>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>fullscreen:</b>' {{ fullscreen }}'</div>
        <div><b>hideOverlay:</b>' {{ hideOverlay }}'</div>
        <div><b>scrollable:</b>' {{ scrollable }}'</div>
        <div><b>persistent:</b>' {{ persistent }}'</div>
        <div><b>maxWidth:</b>' {{ maxWidth }}'</div>
        <div><b>contentClass:</b>' {{ contentClass }}'</div>
        <div><b>fullWidth:</b>' {{ fullWidth }}'</div>
        <div><b>width:</b>' {{ width }}'</div>

		<div v-if="value">
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
export default class DialogComponent extends Vue {

    @Prop({ required: true })
    value!: string;

    @Prop({ required: false, default: false })
    fullscreen!: string | boolean;

    @Prop({ required: false, default: false })
    hideOverlay!: string | boolean;

    @Prop({ required: false, default: false })
    scrollable!: string | boolean;

    @Prop({ required: false, default: false })
    persistent!: string | boolean;

    @Prop({ required: false, default: 0 })
    maxWidth!: number;

    @Prop({ required: false, default: null })
    contentClass!: string;

    @Prop({ required: false, default: false })
    fullWidth!: string | boolean;

    @Prop({ required: false, default: 0 })
    width!: number;

    localValue: string = "";

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
        this.updateLocalValue();
        this.emitLocalValue();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get rootClasses(): any {
        return {
             'fullscreen': this.isFullscreen,
             'hide-overlay': this.isHideOverlay,
             'scrollable': this.isScrollable,
             'persistent': this.isPersistent,
             'full-width': this.isFullWidth
        };
    }

    get isFullscreen(): boolean { return ValueUtils.IsToggleTrue(this.fullscreen); }
    get isHideOverlay(): boolean { return ValueUtils.IsToggleTrue(this.hideOverlay); }
    get isScrollable(): boolean { return ValueUtils.IsToggleTrue(this.scrollable); }
    get isPersistent(): boolean { return ValueUtils.IsToggleTrue(this.persistent); }
    get isFullWidth(): boolean { return ValueUtils.IsToggleTrue(this.fullWidth); }

    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
	
    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
		this.localValue = this.value;
    }

    @Watch('localValue')
    emitLocalValue(): void
    {
		this.$emit('update:input', this.localValue);
    }
}
</script>

<style scoped lang="scss">
.dialog-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.fullscreen { }
    &.hide-overlay { }
    &.scrollable { }
    &.persistent { }
    &.full-width { }
}
</style>
