<template>
    <div class="dialog-component" :class="rootClasses" v-show="localValue">
        <div class="dialog-component_modal_wrapper" @click.self.stop.prevent="onClickOutside">
            <div class="dialog-component_modal" :style="dialogStyle">
                <div class="dialog-component_modal_header">
                    <div class="dialog-component_modal_cross" @click.self.stop.prevent="onClickClose">X</div>
                    <slot name="header"></slot>
                </div>
                <div class="dialog-component_modal_content" :style="contentStyle">
                    <slot></slot>
                </div>
                <div class="dialog-component_modal_footer" v-if="hasFooterSlot">
                    <slot name="footer"></slot>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'
import EventBus, { CallbackUnregisterShortcut } from "@util/EventBus";

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
    callbacks: Array<CallbackUnregisterShortcut> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
        this.updateLocalValue();
        this.emitLocalValue();

        this.callbacks = [
            EventBus.on("onEscapeClicked", this.onEscapeClicked.bind(this))
        ];
    }

    beforeUnmount(): void {
      this.callbacks.forEach(x => x.unregister());
    }

    ////////////////
    //  METHODS  //
    //////////////
    hasFooterSlot() { return !!this.$slots.footer; }

    ////////////////
    //  GETTERS  //
    //////////////
    get rootClasses(): any {
        return {
             'fullscreen': this.isFullscreen,
             'hide-overlay': this.isHideOverlay,
             'persistent': this.isPersistent,
             'full-width': this.isFullWidth
        };
    }

    get dialogStyle(): any {
        let maxWidthValue = this.maxWidth?.toString() || '';
        if (this.maxWidth && maxWidthValue && !isNaN(Number(maxWidthValue))) {
            maxWidthValue = `${maxWidthValue}px`;
        }
        let widthValue = this.width?.toString() || '';
        if (this.width && widthValue && !isNaN(Number(widthValue))) {
            widthValue = `${widthValue}px`;
        }

        if (this.isFullscreen) {
            maxWidthValue = null;
            widthValue = null;
        }

        let style = {
            maxWidth: maxWidthValue
        };
        if (widthValue) style['width'] = widthValue;
        return style;
    }

    get contentStyle(): any {
        // let maxWidthValue = this.maxWidth?.toString() || '';
        // if (this.maxWidth && maxWidthValue && !isNaN(Number(maxWidthValue))) {
        //     maxWidthValue = `${maxWidthValue}px`;
        // }
        // let widthValue = this.width?.toString() || '';
        // if (this.width && widthValue && !isNaN(Number(widthValue))) {
        //     widthValue = `${widthValue}px`;
        // }

        let style = {
            // maxWidth: maxWidthValue
        };
        // if (widthValue) style['width'] = widthValue;
        return style;
    }

    get isFullscreen(): boolean { return ValueUtils.IsToggleTrue(this.fullscreen); }
    get isHideOverlay(): boolean { return ValueUtils.IsToggleTrue(this.hideOverlay); }
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
    onEscapeClicked(): void {
        if (!this.isPersistent) this.close();
    }

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
            margin-bottom: 40px;
            padding: 30px 0;
            background-color: #fff;
            border: 2px solid #dfdfdf;
            position: relative;
            max-width: 100%;
            /* max-height: calc(100vh - 40px - 40px); */
            overflow: hidden;
            display: flex;
            flex-direction: column;

            &_header {
                display: flex;
                align-items: center;
                margin-top: -30px;
                padding: 10px 30px;
                font-size: 30px;
                font-weight: 600;
                /* border-bottom: 2px solid var(--color--accent-lighten1); */
                background-color: var(--color--accent-lighten1);
            }
            &_footer {
                margin-bottom: -30px;
                padding: 10px 30px;
                border-top: 2px solid var(--color--accent-lighten1);
                /* background-color: var(--color--accent-base); */
            }
            &_content {
                overflow-y: auto;
                overflow-x: hidden;
                max-height: calc(100vh - 190px);
                padding: 15px 30px;
                flex: 1;
            }
        }
        .dialog-component_modal_cross {
            cursor: pointer;
            position: absolute;
            top: -1px;
            right: -1px;
            height: 19px;
            width: 19px;
            background-color: #dfdfdf;
            border: 1px solid #dfdfdf;
            font-weight: 600;
            font-size: 17px;
            display: flex;
            align-items: center;
            justify-content: center;
            &:hover {
                background-color: #e8e8e8;
            }
        }
    }

    &.fullscreen {
        .dialog-component_modal {
            margin-bottom: 0;
            width: 100%;
            height: 100%;
        }

        .dialog-component_modal_wrapper {
            position: fixed;
            left: 0;
            top: 0;
            bottom: 0;
            right: 0;
            padding: 0;
            justify-content: inherit;

            .dialog-component_modal {
                &_content {
                max-height: calc(100vh - 140px);
                }
            }
        }
    }
}
</style>
