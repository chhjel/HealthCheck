<template>
    <div class="checkbox-component" :class="rootClasses" tabindex="0">
        <input type="checkbox" :id="`cb-${id}`"
            v-model="localValue"
            :disabled="disabled"
            :indeterminate="indeterminate" />
        <span class='checkbox-component__indicator color-f color-border' :class="colorClasses" :style="indicatorStyle">
            <div v-if="isChecked" class="checkbox-component__indicator_bg" :class="colorClasses"></div>
            <icon-component v-if="localValue == true" :color="iconColor" :size="size" style="position:absolute">check</icon-component>
            <icon-component v-if="isIndeterminate" :color="iconColor" :size="size" style="position:absolute">horizontal_rule</icon-component>
        </span>
        <label v-if="label">{{ label }}</label>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'
import IdUtils from "@util/IdUtils";

@Options({
    components: {}
})
export default class CheckboxComponent extends Vue {

    @Prop({ required: true })
    value!: boolean;

    @Prop({ required: false, default: null })
    label!: string;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: false })
    indeterminate!: string | boolean;

    @Prop({ required: false, default: null })
    inputValue!: string;

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: '18px' })
    size!: string;

    id: string = IdUtils.generateId();
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
             'disabled': this.isDisabled,
             'indeterminate': this.isIndeterminate,
             'checked': this.localValue == true
        };
    }

    get indicatorStyle(): any {
        let style = {};
        if (this.size) {
            style['width'] = this.size;
            style['height'] = this.size;
        }
        return style;
    }

    get colorClasses(): any {
        let classes = {
        };
        classes[this.color || 'primary'] = true;
        return classes;
    }

    get iconColor(): string {
        if (this.isChecked) return '#fff';
        else return this.color;
    }

    get isChecked(): boolean { return this.localValue === true; }
    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }
    get isIndeterminate(): boolean { return ValueUtils.IsToggleTrue(this.indeterminate); }

    ////////////////
    //  METHODS  //
    //////////////
    tryToggle(): void {
        if (!this.isDisabled) {
            this.localValue = !this.localValue;
        }
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
		this.$emit('change', this.localValue);
    }
}
</script>

<style scoped lang="scss">
.checkbox-component {
    display: flex;
    align-items: center;
    
    &:not(.disabled) {
        cursor: pointer;
        input, label {
            cursor: pointer;
        }
    }
    input, label {
        user-select: none;
    }
    input {
        display: none;
    }
    label {
        padding-left: 5px;
        flex: 1;
    }

    &__indicator {
        width: 24px;
        height: 24px;
        border-width: 2px;
        border-radius: 4px;
        border-style: solid;
        position: relative;
    }

    .checkbox-component__indicator_bg {
        position: absolute;
        left: 0;
        right: 0;
        top: 0;
        bottom: 0;
    }

    &.disabled { }
    &.indeterminate { }
}
</style>
