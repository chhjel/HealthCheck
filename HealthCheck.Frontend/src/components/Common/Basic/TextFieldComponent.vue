<template>
    <div class="text-field-component" :class="rootClasses">
        <input-header-component :name="label" :description="description" />
		<!-- <h3>TODO: TextFieldComponent</h3>
        <div><b>errorMessages:</b>' {{ errorMessages }}'</div>

        <div><b>solo:</b>' {{ solo }}'</div>
        <div><b>clearable:</b>' {{ clearable }}'</div>

        <div><b>singleLine:</b>' {{ singleLine }}'</div>
        <div><b>hideDetails:</b>' {{ hideDetails }}'</div>
        <div><b>required:</b>' {{ required }}'</div>
        <div><b>box:</b>' {{ box }}'</div>
        -->

        <div class="text-field-component__input-wrapper">
            <icon-component v-if="prependIcon" class="text-field-component__icon" :class="prependedIconClasses"
                :title="prependIconTooltip"
                @click="onPrependedIconClicked">{{ prependIcon }}</icon-component>
            <input :type="type" v-model="localValue"
                :placeholder="placeholder" :disabled="isDisabled"
                class="text-field-component__input" />
            <icon-component v-if="appendIcon" class="text-field-component__icon" :class="appendedIconClasses"
                :title="appendIconTooltip"
                @click="onApendedIconClicked">{{ appendIcon }}</icon-component>
            <icon-component v-if="isClearable" class="text-field-component__icon" :class="clearableIconClasses"
                title="Clear"
                @click="clear">clear</icon-component>
        </div>

        <progress-linear-component v-if="isLoading" indeterminate height="3" />

        <div class="text-field-component__error mt-1" v-if="errorMessages">
            {{ errorMessages }}
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
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

    @Prop({ required: false, default: null })
    type!: string;

    @Prop({ required: false, default: null })
    errorMessages!: string;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: false })
    clearable!: string | boolean;

    @Prop({ required: false, default: null })
    solo!: string;

    @Prop({ required: false, default: null })
    placeholder!: string;

    @Prop({ required: false, default: false })
    loading!: string | boolean;

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

    localValue: string = "";

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
             'clearable': this.isClearable,
             'loading': this.isLoading,
             'readonly': this.isReadonly
        };
    }

    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }
    get isClearable(): boolean { return ValueUtils.IsToggleTrue(this.clearable); }
    get isLoading(): boolean { return ValueUtils.IsToggleTrue(this.loading); }
    get isReadonly(): boolean { return ValueUtils.IsToggleTrue(this.readonly); }

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
		this.$emit('update:value', this.localValue);
    }
}
</script>

<style scoped lang="scss">
.text-field-component {
    /* display: inline-block; */
	padding: 5px;

    &.clearable { }
    &.loading { }

    &__input-wrapper {
        display: flex;
        flex-flow: nowrap;
        align-items: center;
    }

    &__icon {
        transition: background-color 0.2s;
        padding: 2px;
        border-radius: 50%;
    }
    &:not(.disabled)
    {
        .text-field-component__icon {
            &.clickable 
            {
                cursor: pointer;
                &:hover {
                    background-color: var(--color--accent-base);
                }
            }
        }
    }
    
    &__input {
        border: 0;
        outline: 0;
        width: 100%;
        border-bottom: 2px solid var(--color--primary-base);
        font-size: 16px;
        padding: 6px 0;
        background: transparent;
        transition: border-color 0.2s;

        &:focus {
            padding-bottom: 5px; 
            border-bottom: 3px solid var(--color--primary-lighten3);
        }
    }
    
    &.loading {
        .text-field-component__input {
            padding-bottom: 5px;
            border-bottom: none;
        }
    }

    &.disabled {
        .text-field-component__input {
            color: var(--color--accent-darken6);
            border-color: var(--color--accent-darken6);
        }
    }
    &.readonly { }

    &__error {
        font-size: 12px;
        color: var(--color--error-darken2);
        font-weight: 600;
    }
}
</style>
