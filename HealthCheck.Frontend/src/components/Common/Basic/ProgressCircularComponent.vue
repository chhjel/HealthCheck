<template>
    <div class="progress-circular-component" :class="rootClasses" :style="rootStyle">
        <svg :width="size" :height="size" viewPort="0 0 100 100" version="1.1" xmlns="http://www.w3.org/2000/svg">
            <circle :r="size/2" :cx="size/2" :cy="size/2" fill="transparent" stroke-dasharray="565.48" stroke-dashoffset="0"></circle>
            <circle class="progress-circular-component_bar" :r="size/2" :cx="size/2" :cy="size/2" fill="transparent" stroke-dasharray="565.48" stroke-dashoffset="0"></circle>
        </svg>
        <div class="progress-circular-component_content">
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
export default class ProgressCircularComponent extends Vue {

    @Prop({ required: false, default: 0 })
    value!: number;

    @Prop({ required: false, default: 32 })
    size!: number;

    @Prop({ required: false, default: 4 })
    width!: number;

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: false })
    indeterminate!: string | boolean;

    localValue: number = 0;

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
    get resolvedColorName(): string {
        return this.color || 'primary';
    }

    get rootClasses(): any {
        let classes = {
             'indeterminate': this.isIndeterminate
        };
        // classes[this.resolvedColorName] = true;
        return classes;
    }

    get rootStyle(): any {
        const colorVarName = `--color--${this.resolvedColorName}-lighten7`;
        return {
            'height': `${this.size}px`,
            'width': `${this.size}px`,
            // 'background-color': `var(${colorVarName})`
        };
    }

    // get barClasses(): any {
    //     let classes = {};
    //     classes[this.resolvedColorName] = true;
    //     return classes;
    // }

    // get barStyle(): any {
    //     const colorVarName = `--color--${this.resolvedColorName}-base`;
    //     const barSize = this.size - this.width;
    //     let style = {
    //         // 'height': `${barSize}px`,
    //         // 'width': `${barSize}px`,
    //         'background-color': `var(${colorVarName})`
    //     };
    //     if (!this.isIndeterminate)
    //     {
    //         // style['width'] = `${Math.max(0, Math.min(100, this.value))}%`;
    //     }
    //     return style;
    // }

    // get centerClasses(): any {
    //     let classes = {};
    //     classes[this.resolvedColorName] = true;
    //     return classes;
    // }

    // get centerStyle(): any {
    //     const colorVarName = `--color--${this.resolvedColorName}-base`;
    //     const barSize = this.size - this.width;
    //     let style = {
    //         'height': `${barSize}px`,
    //         'width': `${barSize}px`,
    //         'background-color': `var(${colorVarName})`
    //     };
    //     if (!this.isIndeterminate)
    //     {
    //         // style['width'] = `${Math.max(0, Math.min(100, this.value))}%`;
    //     }
    //     return style;
    // }

    get isIndeterminate(): boolean { return ValueUtils.IsToggleTrue(this.indeterminate); }

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
		this.$emit('update:value', this.localValue);
    }
}
</script>

<style scoped lang="scss">
.progress-circular-component {

    svg circle {
        stroke-dashoffset: 424;
        transition: stroke-dashoffset 1s linear;
        stroke: #666;
        stroke-width: 8px;
    }

    &_bar {
        /* transition: width 0.2s; */
        stroke: #FF9F1E;
    }

    &_center {
    }

    &.indeterminate {
        /* .progress-linear-component_bar {
            width: 100%;
            animation: indeterminateAnimation 1s infinite linear;
            transform-origin: 0% 50%;
        } */
    }
}
</style>
