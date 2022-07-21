<template>
    <div class="checkbox-component" :class="rootClasses" tabindex="0" @click="onClick">
        <span class='checkbox-component__indicator color-f color-border' :class="colorClasses" :style="indicatorStyle">
            <div v-if="isChecked" class="checkbox-component__indicator_bg" :class="colorClasses"></div>
            <icon-component v-if="isChecked" :color="iconColor" :size="size" style="position:absolute">check</icon-component>
            <icon-component v-if="currentValueIsIndeterminate" :color="iconColor" :size="size" style="position:absolute">horizontal_rule</icon-component>
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
    value!: boolean | null;

    @Prop({ required: false, default: null })
    label!: string;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: false })
    readonly!: string | boolean;

    @Prop({ required: false, default: false })
    allowIndeterminate!: string | boolean;

    @Prop({ required: false, default: false })
    inline!: string | boolean;
    
    @Prop({ required: false, default: null })
    inputValue!: string;

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: null })
    toggle!: any;

    @Prop({ required: false, default: '18px' })
    size!: string;

    id: string = IdUtils.generateId();
    localValue: boolean | Array<any> | null = false;

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
             'indeterminate': this.currentValueIsIndeterminate,
             'checked': this.isChecked,
             'inline': this.isInline == true
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

    get currentValueIsIndeterminate(): boolean {
        return !this.localValue && this.localValue !== false;
    }

    get isChecked(): boolean {
        return this.localValue === true 
            || (Array.isArray(this.localValue) && this.localValue.includes(this.toggle));
    }
    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled) || ValueUtils.IsToggleTrue(this.readonly); }
    get isReadOnly(): boolean { return ValueUtils.IsToggleTrue(this.readonly) || ValueUtils.IsToggleTrue(this.disabled); }
    get isInline(): boolean { return ValueUtils.IsToggleTrue(this.inline); }
    get isAllowIndeterminate(): boolean { return ValueUtils.IsToggleTrue(this.allowIndeterminate); }

    ////////////////
    //  METHODS  //
    //////////////
    onClick(): void {
        if (this.isDisabled || this.isReadOnly) {
            return;
        }
        this.setNextState();
    }

    setNextState(): void {
        if (this.readonly)
        {
            return;
        }

        if (Array.isArray(this.value) && this.toggle != null) {
            let newArray = Array.from(this.value);
            if (!newArray.includes(this.toggle)) newArray.push(this.toggle);
            else newArray = newArray.filter(x => x != this.toggle);
            this.localValue = newArray;
            return;
        }
        
        // null => true
        if (this.localValue == null) {
            this.localValue = true;
        }
        // true => false
        else if (this.localValue === true) {
            this.localValue = false;
        }
        // false => null if indeterminate allowed
        else if (this.localValue === false && this.isAllowIndeterminate) {
            this.localValue = null
        }
        // false => true if indeterminate not allowed
        else if (this.localValue === false && !this.isAllowIndeterminate) {
            this.localValue = true
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
    min-height: 36px;
    
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
        left: -1px;
        right: -1px;
        top: -1px;
        bottom: -1px;
    }

    &.disabled { }
    &.indeterminate { }

    &.inline {
        display: inline-flex;
    }
}
</style>
