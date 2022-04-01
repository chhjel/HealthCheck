<template>
    <div class="text-field-component" :class="rootClasses">
        <input-header-component :name="label" :description="description" />
		<!-- <h3>TODO: TextFieldComponent</h3>
        <div><b>errorMessages:</b>' {{ errorMessages }}'</div>

        <div><b>disabled:</b>' {{ disabled }}'</div>
        <div><b>clearable:</b>' {{ clearable }}'</div>
        <div><b>readonly:</b>' {{ readonly }}'</div>
        <div><b>solo:</b>' {{ solo }}'</div>

        <div><b>appendOuterIcon:</b>' {{ appendOuterIcon }}'</div> => click:append-outer
        <div><b>appendIcon:</b>' {{ appendIcon }}'</div> => @click:append
        <div><b>prependIcon:</b>' {{ prependIcon }}'</div> => @click:prepend

        <div><b>singleLine:</b>' {{ singleLine }}'</div>
        <div><b>hideDetails:</b>' {{ hideDetails }}'</div>
        <div><b>required:</b>' {{ required }}'</div>
        <div><b>box:</b>' {{ box }}'</div>
        -->

		<input :type="type" v-model="localValue"
            :placeholder="placeholder" :disabled="isDisabled"
            class="text-field-component__input" />

        <progress-linear-component v-if="isLoading" indeterminate height="3" />
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

    @Prop({ required: false, default: null })
    appendOuterIcon!: string;

    @Prop({ required: false, default: false })
    loading!: string | boolean;

    @Prop({ required: false, default: false })
    readonly!: string | boolean;

    @Prop({ required: false, default: null })
    appendIcon!: string;

    @Prop({ required: false, default: null })
    prependIcon!: string;

    @Prop({ required: false, default: null })
    box!: string;

    @Prop({ required: false, default: null })
    hideDetails!: string;

    @Prop({ required: false, default: null })
    singleLine!: string;

    @Prop({ required: false, default: false })
    required!: string | boolean;

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
             'required': this.isRequired
        };
    }

    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled) || this.isReadonly; }
    get isClearable(): boolean { return ValueUtils.IsToggleTrue(this.clearable); }
    get isLoading(): boolean { return ValueUtils.IsToggleTrue(this.loading); }
    get isReadonly(): boolean { return ValueUtils.IsToggleTrue(this.readonly); }
    get isRequired(): boolean { return ValueUtils.IsToggleTrue(this.required); }

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
.text-field-component {
    /* display: inline-block; */
	padding: 5px;

    &.clearable { }
    &.loading { }
    
    .text-field-component__input {
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
}
</style>
