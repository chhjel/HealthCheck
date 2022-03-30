<template>
    <div class="progress-linear-component" :class="rootClasses" :style="rootStyle">
        <div class="progress-linear-component_bar" :class="barClasses" :style="barStyle"></div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'

@Options({
    components: {}
})
export default class ProgressLinearComponent extends Vue {

    @Prop({ required: false, default: 0 })
    value!: number;

    @Prop({ required: false, default: false })
    indeterminate!: string | boolean;

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: 7 })
    height!: number;

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
            'height': `${this.height}px`,
            'background-color': `var(${colorVarName})`
        };
    }

    get barClasses(): any {
        let classes = {};
        classes[this.resolvedColorName] = true;
        return classes;
    }

    get barStyle(): any {
        let style = {};
        if (!this.isIndeterminate)
        {
            style['width'] = `${Math.max(0, Math.min(100, this.value))}%`;
        }
        return style;
    }

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
.progress-linear-component {
    overflow: hidden;
    &_bar {
        height: 100%;
        transition: width 0.2s;
    }

    &.indeterminate {
        .progress-linear-component_bar {
            width: 100%;
            animation: indeterminateAnimation 1s infinite linear;
            transform-origin: 0% 50%;
        }
    }
}
@keyframes indeterminateAnimation {
  0% {
    transform:  translateX(0) scaleX(0);
  }
  40% {
    transform:  translateX(0) scaleX(0.4);
  }
  100% {
    transform:  translateX(100%) scaleX(0.5);
  }
}
</style>
