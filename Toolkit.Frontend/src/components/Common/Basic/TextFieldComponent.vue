<template>
    <div class="text-field-component" :class="rootClasses">
        <input-header-component :name="label" :description="description" :showDescriptionOnStart="showDescriptionOnStart" />

        <div class="text-field-component__input-wrapper input-wrapper" :class="inputWrapperClasses">
            <icon-component v-if="prependIcon" class="input-icon" :class="prependedIconClasses"
                :title="prependIconTooltip"
                @click="onPrependedIconClicked">{{ prependIcon }}</icon-component>

            <input :type="type" v-model="localValue"
                :placeholder="placeholder" :disabled="isDisabled"
                @input="onInput"
                @blur="onBlur"
                :min="min"
                :max="max"
                ref="inputElement"
                class="text-field-component__input input" />

            <icon-component v-if="appendIcon" class="input-icon" :class="appendedIconClasses"
                :title="appendIconTooltip"
                @click="onApendedIconClicked">{{ appendIcon }}</icon-component>
            <icon-component v-if="isClearable" class="input-icon" :class="clearableIconClasses"
                title="Clear"
                @click="clear">clear</icon-component>
        </div>

        <progress-linear-component
            v-if="isLoading"
            :indeterminate="loadingIsIndeterminate"
            :value="loadingValue"
            :color="loadingColor"
            :height="loadingHeight" />

        <div class="input-error mt-1" v-if="errorMessages">
            {{ errorMessages }}
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'
import InputHeaderComponent from "./InputHeaderComponent.vue";

@Options({
    components: {
        InputHeaderComponent
    }
})
export default class TextFieldComponent extends Vue {

    @Prop({ required: true })
    value!: string;

    @Prop({ required: false, default: null })
    label!: string;

    @Prop({ required: false, default: null })
    description!: string;
    
    @Prop({ required: false, default: false })
    showDescriptionOnStart!: boolean;

    @Prop({ required: false, default: null })
    type!: string;

    @Prop({ required: false, default: null })
    errorMessages!: string;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: false })
    clearable!: string | boolean;

    @Prop({ required: false, default: false })
    center!: string | boolean;

    @Prop({ required: false, default: null })
    solo!: string;

    @Prop({ required: false, default: null })
    placeholder!: string;

    @Prop({ required: false, default: false })
    readonly!: string | boolean;

    @Prop({ required: false, default: null })
    appendIcon!: string;

    @Prop({ required: false, default: null })
    prependIcon!: string;

    @Prop({ required: false, default: null })
    appendIconTooltip!: string;

    @Prop({ required: false, default: null })
    prependIconTooltip!: string;

    @Prop({ required: false, default: null })
    box!: string;

    @Prop({ required: false, default: null })
    hideDetails!: string;

    @Prop({ required: false, default: null })
    singleLine!: string;

    @Prop({ required: false, default: false })
    undefinedWhenEmpty!: string | boolean;

    @Prop({ required: false, default: false })
    loading!: string | boolean;

    @Prop({ required: false, default: null })
    loadingValue!: number | null;

    @Prop({ required: false, default: null })
    loadingColor!: string | null;

    @Prop({ required: false, default: 3 })
    loadingHeight!: number;

    @Prop({ required: false, default: null })
    min!: number | null;

    @Prop({ required: false, default: null })
    max!: number | null;

    @Ref() readonly inputElement!: HTMLInputElement;

    localValue: string = "";

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
        this.updateLocalValue();
        this.emitLocalValue();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get rootClasses(): any {
        return {
             'disabled': this.isDisabled,
             'clearable': this.isClearable,
             'loading': this.isLoading,
             'readonly': this.isReadonly
        };
    }

    get inputWrapperClasses(): any {
        return {
             'solo': this.isSolo,
             'center': this.isCenter
        };
    }

    get isCenter(): boolean { return ValueUtils.IsToggleTrue(this.center); }
    get isSolo(): boolean { return ValueUtils.IsToggleTrue(this.solo); }
    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }
    get isClearable(): boolean { return ValueUtils.IsToggleTrue(this.clearable); }
    get isLoading(): boolean { return ValueUtils.IsToggleTrue(this.loading); }
    get isReadonly(): boolean { return ValueUtils.IsToggleTrue(this.readonly); }
    get isUndefinedWhenEmpty(): boolean { return ValueUtils.IsToggleTrue(this.undefinedWhenEmpty); }

    get loadingIsIndeterminate(): boolean { return this.loadingValue == null; }

    get appendedIconClasses(): any {
        const hasAppendedIconClickListener = this.$attrs && this.$attrs["onClick:append"] != null;
        return {
            'clickable': hasAppendedIconClickListener,
            'disabled': this.isDisabled
        };
    }
    get prependedIconClasses(): any {
        const hasPrependedIconClickListener = this.$attrs && this.$attrs["onClick:prepend"] != null;
        return {
            'clickable': hasPrependedIconClickListener,
            'disabled': this.isDisabled
        };
    }
    get clearableIconClasses(): any {
        return {
            'clickable': !this.isDisabled
        };
    }

    ////////////////
    //  METHODS  //
    //////////////
    clear(): void {
        if (this.isReadonly || this.isDisabled) return;
        this.localValue = null;
        this.$emit('click:clear');
    }

    focus(): void {
        this.inputElement.focus();
        this.inputElement.select();
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onPrependedIconClicked(): void {
        if (this.isReadonly || this.isDisabled) return;
        this.$emit('click:prepend');
    }
    onApendedIconClicked(): void {
        if (this.isReadonly || this.isDisabled) return;
        this.$emit('click:append');
    }
    onInput(): void {
		this.$emit('update:value', this.localValue);
		this.$emit('input', this.localValue);
    }
    onBlur(): void {
		this.$emit('blur', this.localValue);
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
        if (this.isReadonly) {
            this.localValue = this.value;
            return;
        }
        let valueToEmit = this.localValue;
        if (this.isUndefinedWhenEmpty && !this.localValue) {
            valueToEmit = undefined;
        }
		this.$emit('update:value', valueToEmit);
		this.$emit('change', valueToEmit);
    }
}
</script>

<style scoped lang="scss">
.text-field-component {
    /* display: inline-block; */
	/* padding: 5px; */
    
    /* &__input {} */
    /* &.disabled { } */
    /* &.readonly { } */
    /* &.clearable { } */
    /* &.loading { } */
}
</style>
