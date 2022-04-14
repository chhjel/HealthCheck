<template>
    <div class="dialog-component" :class="rootClasses" v-show="localValue">
        <!-- <h3>TODO: DialogComponent</h3>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>fullscreen:</b>' {{ fullscreen }}'</div>
        <div><b>hideOverlay:</b>' {{ hideOverlay }}'</div>
        <div><b>scrollable:</b>' {{ scrollable }}'</div>
        <div><b>persistent:</b>' {{ persistent }}'</div>
        <div><b>maxWidth:</b>' {{ maxWidth }}'</div>
        <div><b>contentClass:</b>' {{ contentClass }}'</div>
        <div><b>fullWidth:</b>' {{ fullWidth }}'</div>
        <div><b>width:</b>' {{ width }}'</div> -->
        <div class="dialog-component_modal_wrapper" @click.self.stop.prevent="onClickOutside">
            <div class="dialog-component_modal">
                <div class="dialog-component_modal_cross" @click.self.stop.prevent="onClickClose">X</div>
                <div class="dialog-component_modal_content" :style="contentStyle">
                    <slot></slot>
                </div>
            </div>
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
    value!: boolean;

    @Prop({ required: false, default: false })
    fullscreen!: string | boolean;

    @Prop({ required: false, default: false })
    hideOverlay!: string | boolean;

    @Prop({ required: false, default: false })
    scrollable!: string | boolean;

    @Prop({ required: false, default: false })
    persistent!: string | boolean;

    @Prop({ required: false, default: 800 })
    maxWidth!: number;

    @Prop({ required: false, default: null })
    contentClass!: string;

    @Prop({ required: false, default: false })
    fullWidth!: string | boolean;

    @Prop({ required: false, default: null })
    width!: number | null;

    localValue: boolean = false;

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

    get contentStyle(): any {
        let style = {
            maxWidth: this.maxWidth
        };
        if (this.width) style['width'] = this.width;
        return style;
    }

    get isFullscreen(): boolean { return ValueUtils.IsToggleTrue(this.fullscreen); }
    get isHideOverlay(): boolean { return ValueUtils.IsToggleTrue(this.hideOverlay); }
    get isScrollable(): boolean { return ValueUtils.IsToggleTrue(this.scrollable); }
    get isPersistent(): boolean { return ValueUtils.IsToggleTrue(this.persistent); }
    get isFullWidth(): boolean { return ValueUtils.IsToggleTrue(this.fullWidth); }

    ////////////////
    //  METHODS  //
    //////////////
    public close(): void {
        this.localValue = false;
        this.$emit("update:value", false);
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onClickOutside(): void {
        if (!this.persistent) this.close();
    }

    onClickClose(): void {
        this.close();
    }
	
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
		this.$emit('update:value', this.localValue);
    }
}
</script>

<style scoped lang="scss">
.dialog-component {
    position: fixed;
    left: 0;
    top: 0;
    bottom: 0;
    right: 0;
    background-color: #0000005c;
    z-index: 1000;

    &.hide-overlay {
        background-color: inherit;
    }
    &.scrollable { }
    &.persistent { }
    &.full-width { }
    
    .dialog-component_modal_wrapper {
        height: 100%;
        display: flex;
        flex-direction: column;
        justify-content: center;
        padding: 20px;
        
        .dialog-component_modal {
            margin: 0 auto;
            padding: 30px;
            background-color: #fff;
            border: 2px solid #dfdfdf;
            overflow-y: auto;
            overflow-x: hidden;
            position: relative;
            max-width: 100%;
        }
        .dialog-component_modal_cross {
            cursor: pointer;
            position: absolute;
            top: -6px;
            right: -7px;
            background-color: #dfdfdf;
            font-weight: 600;
            font-size: 20px;
            padding: 3px 8px;
            border-radius: 50%;
        }
    }

    &.fullscreen {
        .dialog-component_modal_wrapper {
            position: fixed;
            left: 0;
            top: 0;
            bottom: 0;
            right: 0;
            padding: 0;
            justify-content: inherit;

            .dialog-component_modal {
            }
            .dialog-component_modal_cross {
            }
        }
    }
}
</style>
