<template>
    <div class="combobox-component" :class="rootClasses">
		<h3>TODO: ComboboxComponent</h3>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>items:</b>' {{ items }}'</div>
        <div><b>label:</b>' {{ label }}'</div>
        <div><b>multiple:</b>' {{ multiple }}'</div>
        <div><b>chips:</b>' {{ chips }}'</div>
        <div><b>clearable:</b>' {{ clearable }}'</div>
        <div><b>readonly:</b>' {{ readonly }}'</div>
        <div><b>noDataText:</b>' {{ noDataText }}'</div>
        <div><b>placeholder:</b>' {{ placeholder }}'</div>
        <div><b>disabled:</b>' {{ disabled }}'</div>

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
export default class ComboboxComponent extends Vue {

    @Prop({ required: true })
    value!: string;

    @Prop({ required: false, default: null })
    items!: string;

    @Prop({ required: false, default: null })
    label!: string;

    @Prop({ required: false, default: false })
    multiple!: string | boolean;

    @Prop({ required: false, default: false })
    chips!: string | boolean;

    @Prop({ required: false, default: false })
    clearable!: string | boolean;

    @Prop({ required: false, default: false })
    readonly!: string | boolean;

    @Prop({ required: false, default: false })
    noDataText!: string | boolean;

    @Prop({ required: false, default: null })
    placeholder!: string;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

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
             'no-data-text': this.isNoDataText,
             'disabled': this.isDisabled
        };
    }

    get isMultiple(): boolean { return ValueUtils.IsToggleTrue(this.multiple); }
    get isChips(): boolean { return ValueUtils.IsToggleTrue(this.chips); }
    get isClearable(): boolean { return ValueUtils.IsToggleTrue(this.clearable); }
    get isReadonly(): boolean { return ValueUtils.IsToggleTrue(this.readonly); }
    get isNoDataText(): boolean { return ValueUtils.IsToggleTrue(this.noDataText); }
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
.combobox-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.multiple { }
    &.chips { }
    &.clearable { }
    &.readonly { }
    &.no-data-text { }
    &.disabled { }
}
</style>
