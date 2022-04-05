<template>
    <div class="textarea-component" :class="rootClasses">
        <input-header-component :name="label" :description="description" :showDescriptionOnStart="showDescriptionOnStart" />

        <div class="textarea-component__input-wrapper input-wrapper">
            <icon-component v-if="prependIcon" class="input-icon" :class="prependedIconClasses"
                :title="prependIconTooltip"
                @click="onPrependedIconClicked">{{ prependIcon }}</icon-component>

            <textarea v-model="localValue"
                :placeholder="placeholder" :disabled="isDisabled"
                :rows="rows"
                @input="onInput"
                ref="input"
                class="textarea-component__input input"></textarea>

            <icon-component v-if="appendIcon" class="input-icon" :class="appendedIconClasses"
                :title="appendIconTooltip"
                @click="onApendedIconClicked">{{ appendIcon }}</icon-component>
            <icon-component v-if="isClearable" class="input-icon" :class="clearableIconClasses"
                title="Clear"
                @click="clear">clear</icon-component>
        </div>

        <progress-linear-component v-if="isLoading" indeterminate height="3" />

        <div class="input-error mt-1" v-if="errorMessages">
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
    components: { InputHeaderComponent }
})
export default class TextareaComponent extends Vue {

    @Prop({ required: true })
    value!: string;

    @Prop({ required: false, default: null })
    label!: string;

    @Prop({ required: false, default: null })
    description!: string;
    
    @Prop({ required: false, default: false })
    showDescriptionOnStart!: boolean;

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
        this.$emit('click:clear');
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
		this.$emit('input', this.localValue);
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
		this.$emit('change', this.localValue);
    }
}
</script>

<style scoped lang="scss">
.textarea-component {
    /* display: inline-block; */
	padding: 5px;

    &__input-wrapper {
        align-items: flex-start;
    }
    
    /* &__input {} */
    /* &.disabled { } */
    /* &.readonly { } */
    /* &.clearable { } */
    /* &.loading { } */
    /* &.auto-grow { } */

    &.autogrow {
        .textarea-component__input {
            resize: none;
            overflow: hidden;
        }
    }
}
</style>
