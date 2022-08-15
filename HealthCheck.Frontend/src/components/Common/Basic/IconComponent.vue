<template>
    <span class="icon-component" :class="rootClasses" :style="rootStyle">
        <i class="material-icons" :style="iconStyle"><slot></slot></i>
    </span>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'
import CssUtils from "@util/CssUtils";

@Options({
    components: {}
})
export default class IconComponent extends Vue {

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: false })
    small!: string | boolean;

    @Prop({ required: false, default: false })
    medium!: string | boolean;

    @Prop({ required: false, default: false })
    large!: string | boolean;

    @Prop({ required: false, default: false })
    xLarge!: string | boolean;

    @Prop({ required: false, default: false })
    help!: string | boolean;

    @Prop({ required: false, default: null })
    size!: string;

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
             'small': this.isSmall,
             'large': this.isLarge,
             'medium': this.isMedium,
             'xLarge': this.isXLarge,
             'help': this.isHelp,
             'color-f': true
        };

        CssUtils.setColorClassIfPredefined(this.color || 'def-color', classes, 'custom-color');
        if (!this.isSmall && !this.isMedium && !this.isLarge && !this.isXLarge) classes['small'] = true;
        return classes;
    }

    get rootStyle(): any {
        let style = {};
        CssUtils.setColorStyleIfNotPredefined(this.color || 'def-color', style);
        if (this.size) {
            style['width'] = this.size;
            style['height'] = this.size;
        }
        return style;
    }

    get iconStyle(): any {
        let style = {};
        if (this.size) {
            style['font-size'] = this.size;
        }
        return style;
    }

    get isSmall(): boolean { return ValueUtils.IsToggleTrue(this.small); }
    get isLarge(): boolean { return ValueUtils.IsToggleTrue(this.large); }
    get isMedium(): boolean { return ValueUtils.IsToggleTrue(this.medium); }
    get isXLarge(): boolean { return ValueUtils.IsToggleTrue(this.xLarge); }
    get isHelp(): boolean { return ValueUtils.IsToggleTrue(this.help); }

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
@import 'src/styles/core/colors_mixin.scss';

.icon-component {
    display: flex;
    user-select: none;
    align-items: center;

    @include no-color-class {
        color: var(--color--accent-darken10);
    }

    &.small { width: 24px; height: 24px; .material-icons { font-size: 24px; } }
    &.medium { width: 28px; height: 28px; .material-icons { font-size: 28px; } }
    &.large { width: 36px; height: 36px; .material-icons { font-size: 36px; } }
    &.xLarge { width: 40px; height: 40px; .material-icons { font-size: 40px; } }
    &.help { cursor: help; }
}
</style>

<style lang="scss">
.disabled {
    .icon-component {
        .material-icons {
            opacity: 0.25;
        }
    }
}
</style>
