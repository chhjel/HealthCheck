<template>
    <div class="progress-circular-component" :class="rootClasses" :style="rootStyle">
        <svg :width="svgWidth" :height="svgHeight"
            viewPort="0 0 100 100" version="1.1" xmlns="http://www.w3.org/2000/svg">
            <circle
                :r="r" cx="50%" cy="50%"
                :style="circleBgStyle"
                fill="transparent" :stroke-dasharray="circumference" stroke-dashoffset="0"></circle>
            <circle class="progress-circular-component_bar"
                :r="r" cx="50%" cy="50%"
                :style="circleStyle"
                fill="transparent" :stroke-dasharray="circumference" stroke-dashoffset="0"></circle>
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
    get svgWidth(): number { return Number(this.size) + 2; }
    get svgHeight(): number { return Number(this.size) + 2; }
    get r(): number { return (Number(this.size) / 2) - this.width; }
    get circumference(): number { return Math.PI * (this.r * 2); }

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
        return {
            'height': `${Number(Number(this.size))+2}px`,
            'width': `${Number(this.size)+2}px`,
            // 'background-color': `var(${colorVarName})`
        };
    }

    get circleStyle(): any {
        const colorVarName = `--color--${this.resolvedColorName}-base`;
        let style = {
            'stroke': `var(${colorVarName})`,
            'stroke-width': `${this.width}px`
        };
        if (!this.isIndeterminate)
        {
            style['stroke-dashoffset'] = `${this.calcDashOffset(this.value)}px`;
        }
        return style;
    }

    get circleBgStyle(): any {
        const colorVarName = `--color--${this.resolvedColorName}-lighten7`;
        let style = {
            'stroke': `var(${colorVarName})`,
            'stroke-width': `${this.width}px`,
            'stroke-dashoffset': `${this.calcDashOffset(100)}px`
        };
        return style;
    }

    get isIndeterminate(): boolean { return ValueUtils.IsToggleTrue(this.indeterminate); }

    ////////////////
    //  METHODS  //
    //////////////
    calcDashOffset(value: number): number {
        let val: number = (value == null || isNaN(value)) ? 100 : value;
        if (val < 0) { val = 0; }
        else if (val > 100) { val = 100; }

        const percentage = ((100 - val) / 100); // alt: val / 100;
        return percentage * this.circumference;
    }

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
        stroke-dashoffset: 0;
        transition: stroke-dashoffset 0.2s;
        /* stroke-linecap: round; */
    }

    &.indeterminate {
        svg {
            animation: rotate 2s linear infinite;
        }
        .progress-circular-component_bar {
            stroke-dasharray: 100%;
            stroke-dashoffset: -55;
            animation: dash 1.5s ease-in-out infinite;
        }
    }
    @keyframes rotate {
        100%{
            transform: rotate(360deg);
        }
    }
    @keyframes dash {
        0%{
            stroke-dasharray: 100%;
            stroke-dashoffset: -55;
        }
        50%{
            stroke-dasharray: 170%;
            stroke-dashoffset: -10%;
        }
        100%{
            stroke-dasharray: 100%;
            stroke-dashoffset: -55;
        }
    }
}
</style>
