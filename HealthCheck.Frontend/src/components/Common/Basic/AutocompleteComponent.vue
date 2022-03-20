<template>
    <div class="autocomplete-component" :class="rootClasses">
		<h3>TODO: AutocompleteComponent</h3>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>items:</b>' {{ items }}'</div>
        <div><b>itemValue:</b>' {{ itemValue }}'</div>
        <div><b>itemText:</b>' {{ itemText }}'</div>
        <div><b>multiple:</b>' {{ multiple }}'</div>
        <div><b>chips:</b>' {{ chips }}'</div>
        <div><b>clearable:</b>' {{ clearable }}'</div>
        <div><b>readonly:</b>' {{ readonly }}'</div>
        <div><b>input:</b>' {{ input }}'</div>
        <div><b>disabled:</b>' {{ disabled }}'</div>
        <div><b>label:</b>' {{ label }}'</div>
        <div><b>properties:</b>' {{ properties }}'</div>

		<slot></slot>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'

@Options({
    components: {}
})
export default class AutocompleteComponent extends Vue {

    @Prop({ required: true })
    value!: string;

    @Prop({ required: false, default: null })
    items!: string;

    @Prop({ required: false, default: null })
    itemValue!: string;

    @Prop({ required: false, default: null })
    itemText!: string;

    @Prop({ required: false, default: false })
    multiple!: string | boolean;

    @Prop({ required: false, default: false })
    chips!: string | boolean;

    @Prop({ required: false, default: false })
    clearable!: string | boolean;

    @Prop({ required: false, default: false })
    readonly!: string | boolean;

    @Prop({ required: false, default: null })
    input!: string;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: null })
    label!: string;

    @Prop({ required: false, default: null })
    properties!: string;

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
             'multiple': this.isMultiple,
             'chips': this.isChips,
             'clearable': this.isClearable,
             'readonly': this.isReadonly,
             'disabled': this.isDisabled
        };
    }

    get isMultiple(): boolean { return ValueUtils.IsToggleTrue(this.multiple); }
    get isChips(): boolean { return ValueUtils.IsToggleTrue(this.chips); }
    get isClearable(): boolean { return ValueUtils.IsToggleTrue(this.clearable); }
    get isReadonly(): boolean { return ValueUtils.IsToggleTrue(this.readonly); }
    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }

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
.autocomplete-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.multiple { }
    &.chips { }
    &.clearable { }
    &.readonly { }
    &.disabled { }
}
</style>
