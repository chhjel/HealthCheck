<template>
    <div class="textarea-component" :class="rootClasses">
		<!-- <h3>TODO: TextareaComponent</h3>
        <div><b>disabled:</b>' {{ disabled }}'</div>
        <div><b>clearable:</b>' {{ clearable }}'</div>
        <div><b>placeholder:</b>' {{ placeholder }}'</div>
        <div><b>required:</b>' {{ required }}'</div>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>readonly:</b>' {{ readonly }}'</div>
        <div><b>rows:</b>' {{ rows }}'</div>
        <div><b>autoGrow:</b>' {{ autoGrow }}'</div> -->

		<textarea v-model="localValue"></textarea>
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

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: false })
    clearable!: string | boolean;

    @Prop({ required: false, default: null })
    placeholder!: string;

    @Prop({ required: false, default: false })
    required!: string | boolean;

    @Prop({ required: false, default: false })
    readonly!: string | boolean;

    @Prop({ required: false, default: null })
    rows!: string;

    @Prop({ required: false, default: false })
    autoGrow!: string | boolean;

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
             'required': this.isRequired,
             'readonly': this.isReadonly,
             'auto-grow': this.isAutoGrow
        };
    }

    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }
    get isClearable(): boolean { return ValueUtils.IsToggleTrue(this.clearable); }
    get isRequired(): boolean { return ValueUtils.IsToggleTrue(this.required); }
    get isReadonly(): boolean { return ValueUtils.IsToggleTrue(this.readonly); }
    get isAutoGrow(): boolean { return ValueUtils.IsToggleTrue(this.autoGrow); }

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
.textarea-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.disabled { }
    &.clearable { }
    &.required { }
    &.readonly { }
    &.auto-grow { }
}
</style>
