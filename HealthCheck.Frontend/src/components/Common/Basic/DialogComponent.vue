<template>
    <TeleportFix to="body">
    <div class="dialog-component" :class="rootClasses" :style="rootStyle" v-show="localValue">
        <div class="dialog-component_modal_wrapper" @click.self.prevent="onClickOutside" ref="modalWrapper">
            <div class="dialog-component_modal" :style="dialogStyle">
                <div class="dialog-component_modal_header" :class="headerColor">
                    <slot name="header"></slot>
                    <div class="spacer01"></div>
                    <slot name="headerRight"></slot>
                    <btn-component flat @click="onClickClose">
                        <icon-component large class="dialog-component_modal_header__closer">close</icon-component>
                    </btn-component>
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
    </TeleportFix>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { Teleport as teleport_, TeleportProps, VNodeProps } from 'vue';
import ValueUtils from '@util/ValueUtils'
import EventBus, { CallbackUnregisterShortcut } from "@util/EventBus";

const TeleportFix = teleport_ as {
  new (): {
    $props: VNodeProps & TeleportProps
  }
}
@Options({
    components: { TeleportFix },
    emits: ['close']
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

    @Prop({ required: false, default: false })
    fullWidth!: string | boolean;

    @Prop({ required: false, default: false })
    dark!: string | boolean;

    @Prop({ required: false, default: false })
    scrollableX!: string | boolean;

    @Prop({ required: false, default: null })
    width!: number | null;

    @Prop({ required: false, default: null })
    headerColor!: string | null;

    @Prop({ required: false, default: null })
    class!: string | null;

    @Ref() readonly modalWrapper!: HTMLElement;

    localValue: boolean = false;
    callbacks: Array<CallbackUnregisterShortcut> = [];
    static zIndexCounter: number = 1001;
    zIndex: number = 1001;
    static activeDialogCount: number = 0;

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
        let classes: any = {
             'fullscreen': this.isFullscreen,
             'hide-overlay': this.isHideOverlay,
             'persistent': this.isPersistent,
             'full-width': this.isFullWidth,
             'dark': this.isDark,
             'has-color': !!this.headerColor
        };
        if (this.class) classes[this.class] = true;
        return classes;
    }

    get rootStyle(): any {
        let style: any = {
            'z-index': this.zIndex
        };
        return style;
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

        let style: any = {
            maxWidth: maxWidthValue ? `min(${maxWidthValue}, calc(100vw - 40px))` : null
        };
        if (widthValue) style['width'] = `min(${widthValue}, calc(100vw - 40px))`;
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

        let style: any = {
            // maxWidth: maxWidthValue
        };
        if (this.isScrollableX) style['overflow-x'] = 'auto';

        return style;
    }

    get isFullscreen(): boolean { return ValueUtils.IsToggleTrue(this.fullscreen); }
    get isHideOverlay(): boolean { return ValueUtils.IsToggleTrue(this.hideOverlay); }
    get isPersistent(): boolean { return ValueUtils.IsToggleTrue(this.persistent); }
    get isFullWidth(): boolean { return ValueUtils.IsToggleTrue(this.fullWidth); }
    get isDark(): boolean { return ValueUtils.IsToggleTrue(this.dark); }
    get isScrollableX(): boolean { return ValueUtils.IsToggleTrue(this.scrollableX); }

    ////////////////
    //  METHODS  //
    //////////////
    public close(): void {
        const changed = this.localValue == true;
        this.localValue = false;
        if (changed) {
            this.onVisibilityChanged(this.localValue);
        }
        this.$emit("update:value", false);
        this.$emit("close", true);
    }

    shake(): void {
        this.clearShake();
        void this.modalWrapper.offsetWidth; // trigger reflow
        this.modalWrapper.classList.add('persistent-shake'); // start animation
    }

    clearShake(): void {
        this.modalWrapper.classList.remove('persistent-shake'); // reset animation
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onEscapeClicked(): void {
        if (this.isPersistent
            || document.activeElement?.classList?.contains('monaco-mouse-cursor-text') == true) {
            this.shake();
            return;
        }
        this.close();
    }

    onClickOutside(): void {
        if (!this.persistent) this.close();
        else this.shake();
    }

    onClickClose(): void {
        this.close();
    }

    onVisibilityChanged(shown: boolean): void {
        DialogComponent.activeDialogCount = DialogComponent.activeDialogCount + (shown ? 1 : -1);
        document.body.style.overflow = DialogComponent.activeDialogCount == 0 ? null : 'hidden';
        this.clearShake();
    }
	
    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
        const changed = this.localValue != this.value;

		this.localValue = this.value;
        if (this.localValue) {
            DialogComponent.zIndexCounter = DialogComponent.zIndexCounter + 1;
            this.zIndex = DialogComponent.zIndexCounter;
        }

        if (changed) {
            this.onVisibilityChanged(this.localValue);
        }
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
    animation: dialog-open-bg .15s ease-in-out;

    &.hide-overlay {
        background-color: inherit;
    }
    &.persistent { }
    &.full-width { }
    &:not(.has-color) {
        .dialog-component_modal_header {
            /* background-color: var(--color--accent-lighten1); */
        }
    }
    
    .dialog-component_modal_wrapper {
        height: 100%;
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: center;
        padding: 20px;
        animation: dialog-open .15s ease-in-out;

        &.persistent-shake {
            animation: dialog-persistent-shake 0.35s; // ease-in-out;
        }
        
        .dialog-component_modal {
            margin: 0 auto;
            margin-bottom: 40px;
            padding: 30px 0;
            background-color: #fff;
            transition: all 0.2s;
            box-shadow: 0 0 13px 9px #33333340;
            position: relative;
            max-width: 100%;
            overflow: hidden;
            display: flex;
            flex-direction: column;

            &_header {
                display: flex;
                align-items: center;
                justify-content: space-between;
                margin-top: -30px;
                padding: 10px 30px;
                font-size: 30px;
                font-weight: 600;
                min-height: 18px;
                box-sizing: border-box;
                border-bottom: 2px solid var(--color--accent-lighten1);

                @media (max-width: 961px) {
                    padding: 5px;
                }

                &:empty {
                    display: none !important;
                }
                &__closer {
                    font-size: 30px;
                }
            }
            &_footer {
                margin-bottom: -30px;
                padding: 10px 30px;
                border: 2px solid #dfdfdf;
                border-top: 2px solid var(--color--accent-lighten1);
            }
            &_content {
                overflow-y: auto;
                overflow-x: hidden;
                max-height: calc(100vh - 190px);
                padding: 15px 30px;
                flex: 1;
                border-left: 2px solid #dfdfdf;
                border-right: 2px solid #dfdfdf;

                @media (max-width: 961px) {
                    padding: 5px;
                }
            }
        }
    }

    &.dark {
        .dialog-component_modal {
            background: var(--color--background-dark) !important;
            color: #fff;
            margin: 0 !important;
            border: none !important;
        }
        .dialog-component_modal_header {
            background: var(--color--background-dark) !important;
            border: none !important;
        }
        .dialog-component_modal_content {
            border: none !important;
        }
        .dialog-component_modal_footer {
            border: none !important;
            border-top: 2px solid var(--color--accent-darken9);
        }
    }

    &.fullscreen {
        .dialog-component_modal {
            width: calc(100% - 16px);
            height: 100%;
            
            background: #fff;
            box-shadow: 0 0 4px 3px #33333321;
            box-sizing: border-box;
            margin: 8px;
            border: 2px solid #dfdfdf;
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
                    border: none;
                    max-height: inherit;
                    /* max-height: calc(100vh - 140px); */
                }
                &_footer {
                    border: none;
                    border-top: 2px solid var(--color--accent-lighten1);
                }
            }
        }
    }
}

@keyframes dialog-open-bg {
  0% {
    opacity: 0;
  }
  100% {
    opacity: 1;
  }
}

@keyframes dialog-open {
  0% {
    opacity: 0;
    transform: translateY(50px);
  }
  100% {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes dialog-persistent-shake {
    0% { transform: translateX(0) }
    25% { transform: translateX(5px) }
    50% { transform: translateX(-5px) }
    75% { transform: translateX(5px) }
    100% { transform: translateX(0) }
}
</style>
