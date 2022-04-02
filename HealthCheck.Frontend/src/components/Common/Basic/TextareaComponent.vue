<template>
    <div class="textarea-component" :class="rootClasses">
        <input-header-component :name="label" :description="description" />

        <div class="textarea-component__input-wrapper">
            <icon-component v-if="prependIcon" class="textarea-component__icon" :class="prependedIconClasses"
                :title="prependIconTooltip"
                @click="onPrependedIconClicked">{{ prependIcon }}</icon-component>

            <textarea v-model="localValue"
                :placeholder="placeholder" :disabled="isDisabled"
                :rows="rows"
                @input="onInput"
                ref="input"
                class="textarea-component__input"></textarea>

            <icon-component v-if="appendIcon" class="textarea-component__icon" :class="appendedIconClasses"
                :title="appendIconTooltip"
                @click="onApendedIconClicked">{{ appendIcon }}</icon-component>
            <icon-component v-if="isClearable" class="textarea-component__icon" :class="clearableIconClasses"
                title="Clear"
                @click="clear">clear</icon-component>
        </div>

        <progress-linear-component v-if="isLoading" indeterminate height="3" />

        <div class="textarea-component__error mt-1" v-if="errorMessages">
            {{ errorMessages }}
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
export default class TextareaComponent extends Vue {

    @Prop({ required: true })
    value!: string;

    @Prop({ required: false, default: null })
    label!: string;

    @Prop({ required: false, default: null })
    description!: string;

    @Prop({ required: false, default: null })
    errorMessages!: string;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: false })
    clearable!: string | boolean;

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
    rows!: string;

    @Prop({ required: false, default: false })
    autoGrow!: string | boolean;

    @Prop({ required: false, default: 999 })
    autoGrowMaxLines!: number;

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
             'readonly': this.isReadonly,
             'autogrow': this.isAutoGrow
        };
    }

    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }
    get isClearable(): boolean { return ValueUtils.IsToggleTrue(this.clearable); }
    get isLoading(): boolean { return ValueUtils.IsToggleTrue(this.loading); }
    get isReadonly(): boolean { return ValueUtils.IsToggleTrue(this.readonly); }
    get isAutoGrow(): boolean { return ValueUtils.IsToggleTrue(this.autoGrow); }

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

    getInputOffset(): number {
        const textarea = (<HTMLElement>this.$refs.input);
        var style = window.getComputedStyle(textarea, null),
            props = ['paddingTop', 'paddingBottom'],
            offset = 0;

        for(var i=0; i<props.length; i++){
            offset += parseInt(style[props[i]]);
        }
        return offset;
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
        if (!this.isAutoGrow) return;
        const textarea = (<HTMLInputElement>this.$refs.input);
        
        let newHeight = 0, hasGrown = false;
        const offset = this.getInputOffset();
        let rows = 1;
        if (this.rows) rows = Number(this.rows);
        const lineHeight = (textarea.scrollHeight / rows) - (offset / rows);
        const maxAllowedHeight = (lineHeight * this.autoGrowMaxLines) - offset;
        if((textarea.scrollHeight - offset) > maxAllowedHeight){
            textarea.style.overflowY = 'scroll';
            newHeight = maxAllowedHeight;
        }
        else
        {
            textarea.style.overflowY = 'hidden';
            textarea.style.height = 'auto';
            newHeight = textarea.scrollHeight - offset;
        }
        textarea.style.height = newHeight + 'px';
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
.textarea-component {
    /* display: inline-block; */
	padding: 5px;

    &.clearable { }
    &.loading { }
    &.auto-grow { }

    &__input-wrapper {
        display: flex;
        flex-flow: nowrap;
        align-items: flex-start;
    }

    &__icon {
        transition: background-color 0.2s;
        padding: 2px;
        border-radius: 50%;
    }
    &:not(.disabled)
    {
        .textarea-component__icon {
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
        box-sizing: border-box;

        &:focus {
            padding-bottom: 5px; 
            border-bottom: 3px solid var(--color--primary-lighten3);
        }
    }

    &.autogrow {
        .textarea-component__input {
            resize: none;
            overflow: hidden;
        }
    }
    
    &.loading {
        .textarea-component__input {
            padding-bottom: 5px;
            border-bottom: none;
        }
    }

    &.disabled {
        .textarea-component__input {
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
