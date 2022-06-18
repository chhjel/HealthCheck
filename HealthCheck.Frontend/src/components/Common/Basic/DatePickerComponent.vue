<template>
    <div class="date-picker-component" :class="rootClasses">
        <datepicker 
            v-model="localValue"
            :range="isRange"
            :disabled="isDisabled" 
            :clearable="isClearable"
            :presetRanges="internalRangePresets"
            :disabledDates="disabledDates" />
    </div>
</template>

<script lang="ts">
// https://vue3datepicker.com/api/props/#inline
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'
import { DatePickerPresetDateRange, presetRangesBackInTime } from "./DatePickerComponent.vue.models";

@Options({
    components: {}
})
export default class DatePickerComponent extends Vue {
    @Prop({ required: true })
    value!: Date[];

    @Prop({ required: false, default: null })
    allowedDates!: ((date: Date) => boolean) | null;

    @Prop({ required: false, default: false })
    range!: string | boolean;

    @Prop({ required: false, default: false })
    clearable!: string | boolean;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: '' })
    rangePresets!: 'past' | DatePickerPresetDateRange[];

    localValue: Date[] = [];
    presetItems: DatePickerPresetDateRange[] = [];

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
        };
    }

    get isClearable(): boolean { return ValueUtils.IsToggleTrue(this.clearable); }
    get isRange(): boolean { return ValueUtils.IsToggleTrue(this.range); }
    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }

    get presets(): Array<DatePickerPresetDateRange> {
        if (!this.isRange) return [];
        if (this.rangePresets == "past") return presetRangesBackInTime();
        else if (this.rangePresets && Array.isArray(this.rangePresets) && this.rangePresets.length > 0) return this.rangePresets;
        else return [];
    }

    //#region Proxy methods consumed by internal datepicker
    get internalRangePresets(): Array<any> {
        return this.presets.map(x => ({
            label: x.name,
            range: [ x.from, x.to ]
        }));
    }
    disabledDates(date: Date): boolean {
        if (this.allowedDates == null) return false;
        else return !this.allowedDates(date);
    }
    //#endregion

    ////////////////
    //  METHODS  //
    //////////////
    setDatePickerPresetRange(preset: DatePickerPresetDateRange): void {
        this.localValue = [ preset.from, preset.to ];
    }

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
/* .date-picker-component {
} */
</style>
