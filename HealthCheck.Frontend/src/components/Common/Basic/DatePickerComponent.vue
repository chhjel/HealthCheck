<template>
    <div class="date-picker-component" :class="rootClasses">
		<h3>TODO: DatePickerComponent</h3>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>allowedDates:</b>' {{ allowedDates }}'</div>
        <div><b>scrollable:</b>' {{ scrollable }}'</div>

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
export default class DatePickerComponent extends Vue {

    @Prop({ required: true })
    value!: string;

    @Prop({ required: false, default: null })
    allowedDates!: string;

    @Prop({ required: false, default: false })
    scrollable!: string | boolean;

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
             'scrollable': this.isScrollable
        };
    }

    get isScrollable(): boolean { return ValueUtils.IsToggleTrue(this.scrollable); }

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
.date-picker-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.scrollable { }
}
</style>
