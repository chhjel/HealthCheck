<template>
    <div class="text-field-component" :class="rootClasses">
		<!-- <h3>TODO: TextFieldComponent</h3>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>type:</b>' {{ type }}'</div>
        <div><b>errorMessages:</b>' {{ errorMessages }}'</div>
        <div><b>disabled:</b>' {{ disabled }}'</div>
        <div><b>clearable:</b>' {{ clearable }}'</div>
        <div><b>label:</b>' {{ label }}'</div>
        <div><b>solo:</b>' {{ solo }}'</div>
        <div><b>placeholder:</b>' {{ placeholder }}'</div>
        <div><b>appendOuterIcon:</b>' {{ appendOuterIcon }}'</div>
        <div><b>loading:</b>' {{ loading }}'</div>
        <div><b>readonly:</b>' {{ readonly }}'</div>
        <div><b>appendIcon:</b>' {{ appendIcon }}'</div>
        <div><b>prependIcon:</b>' {{ prependIcon }}'</div>
        <div><b>box:</b>' {{ box }}'</div>
        <div><b>hideDetails:</b>' {{ hideDetails }}'</div>
        <div><b>singleLine:</b>' {{ singleLine }}'</div>
        <div><b>required:</b>' {{ required }}'</div> -->

		<input type="text" v-model="localValue" />
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'

@Options({
    components: {}
})
export default class TextFieldComponent extends Vue {

    @Prop({ required: true })
    value!: string;

    @Prop({ required: false, default: null })
    type!: string;

    @Prop({ required: false, default: null })
    errorMessages!: string;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: false })
    clearable!: string | boolean;

    @Prop({ required: false, default: null })
    label!: string;

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

    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }
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
		this.$emit('update:input', this.localValue);
    }
}
</script>

<style scoped lang="scss">
.text-field-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.disabled { }
    &.clearable { }
    &.loading { }
    &.readonly { }
    &.required { }
}
</style>
