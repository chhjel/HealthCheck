<template>
    <!-- @click.stop.prevent.capture="onClick" -->
    <div class="btn-component" :class="rootClasses" :disabled="isDisabled" :style="rootStyle">
        <a v-if="href" :href="(href || '')" :target="target">
		    <span class="btn-component__contents"><slot></slot></span>
        </a>
        <span v-else class="btn-component__contents"><slot></slot></span>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'
import CssUtils from "@util/CssUtils";

@Options({
    components: {}
})
export default class BtnComponent extends Vue {

    @Prop({ required: false, default: false })
    flat!: string | boolean;

    @Prop({ required: false, default: null })
    href!: string;

    @Prop({ required: false, default: false })
    small!: string | boolean;

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: null })
    title!: string;

    @Prop({ required: false, default: false })
    icon!: string | boolean;

    @Prop({ required: false, default: false })
    round!: string | boolean;

    @Prop({ required: false, default: false })
    large!: string | boolean;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: false })
    loading!: string | boolean;

    @Prop({ required: false, default: false })
    depressed!: string | boolean;

    @Prop({ required: false, default: false })
    outline!: string | boolean;

    @Prop({ required: false, default: false })
    absolute!: string | boolean;

    @Prop({ required: false, default: false })
    xSmall!: string | boolean;

    @Prop({ required: false, default: null })
    target!: string;


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
             'hoverable': !this.isDepressed && !this.isDisabled,
             'flat': this.isFlat,
             'small': this.isSmall,
             'icon': this.isIcon,
             'round': this.isRound,
             'large': this.isLarge,
             'disabled': this.isDisabled,
             'loading': this.isLoading,
             'depressed': this.isDepressed,
             'outline': this.isOutline,
             'x-small': this.isXSmall,
             'absolute': this.isAbsolute
        };

        if (!this.color) {
            classes['accent'] = true;
        } else {
            CssUtils.setColorClassIfPredefined(this.color, classes);
        }
        return classes;
    }

    get rootStyle(): any {
        let style = {};
        if (!this.isDisabled) {
            CssUtils.setColorStyleIfNotPredefined(this.color || 'def-color', style);
        }
        return style;
    }

    get isAbsolute(): boolean { return ValueUtils.IsToggleTrue(this.absolute); }
    get isFlat(): boolean { return ValueUtils.IsToggleTrue(this.flat); }
    get isSmall(): boolean { return ValueUtils.IsToggleTrue(this.small); }
    get isIcon(): boolean { return ValueUtils.IsToggleTrue(this.icon); }
    get isRound(): boolean { return ValueUtils.IsToggleTrue(this.round); }
    get isLarge(): boolean { return ValueUtils.IsToggleTrue(this.large); }
    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }
    get isLoading(): boolean { return ValueUtils.IsToggleTrue(this.loading); }
    get isDepressed(): boolean { return ValueUtils.IsToggleTrue(this.depressed); }
    get isOutline(): boolean { return ValueUtils.IsToggleTrue(this.outline); }
    get isXSmall(): boolean { return ValueUtils.IsToggleTrue(this.xSmall); }

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
.btn-component {
    font-size: 14px;
    font-weight: 500;
    display: inline-flex;
    flex-direction: row;
    align-content: center;
    justify-content: center;
    align-items: center;
    vertical-align: middle;
    text-transform: uppercase;
    text-decoration: none;
    cursor: pointer;
    border-radius: 2px;
    user-select: none;
    min-width: 88px;
    min-height: 36px;
    margin: 6px 8px;
    &:not(.flat) {
        box-shadow: 0 3px 1px -2px rgba(0,0,0,.2),0 2px 2px 0 rgba(0,0,0,.14),0 1px 5px 0 rgba(0,0,0,.12);
    }
    a {
        color: var(--color--text);
        text-decoration: none;
        &:hover { text-decoration: none; }
        &:visited { color: var(--color--text); }
    }

    &__contents {
        display: flex;
        align-content: center;
        /* justify-content: center; */
        justify-content: flex-start;
        align-items: center;
        flex-direction: row;
        white-space: nowrap;
        padding: 5px 10px;
        text-overflow: ellipsis;
        overflow: hidden;
    }

    &.icon {
        border-radius: 50%;
        min-width: 0;
        .btn-component__contents {
            padding: 5px;
        }
    }

    &.disabled {
        cursor: default;
        opacity: 0.8;
        pointer-events: none;
    }
    &.loading { }

    // Styles
    &.outline {
        box-shadow: none !important;
    }
    &.round {
        border-radius: 50vh;
    }
    &.flat {
        background-color: transparent;
        box-shadow: none !important;
    }
    &.depressed { }

    // Sizes
    &.large { }
    &.small {
        min-width: 40px;
        min-height: 24px;
        .btn-component__contents {
            padding: 5px;
            font-size: 11px;
        }
    }
    &.x-small { }
}
</style>
