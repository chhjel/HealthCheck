<template>
    <div class="date-picker-component" :class="rootClasses">
        <datepicker 
            v-model="localValue"
            :range="isRange"
            :partialRange="partialRange"
            :disabled="isDisabled" 
            :clearable="isClearable"
            :presetRanges="internalRangePresets"
            :disabledDates="disabledDates"
            :placeholder="placeholder"
            :is24="true"
            :enableSeconds="true"
            :format="format"
            :altPosition="dropdownPosition"
            inputClassName="input input-date"
            ref="pickerElement" />
    </div>
</template>

<script lang="ts">
// https://vue3datepicker.com/api/props/#inline
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'
import { DatePickerPresetDateRange, presetRangesBackInTime } from "./DatePickerComponent.vue.models";
import ElementUtils from "@util/ElementUtils";

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
    partialRange!: string | boolean;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: null })
    placeholder!: string | null;

    @Prop({ required: false, default: '' })
    rangePresets!: 'past' | DatePickerPresetDateRange[];

    @Ref() readonly pickerElement!: Vue;

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

    get format(): string {
        return 'dd/MM/yyyy HH:mm:ss';
        // return this.isRange
        //     ? 'dd/MM/yyyy HH:mm:ss - dd/MM/yyyy HH:mm:ss'
        //     : 'dd/MM/yyyy HH:mm:ss';
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

    dropdownPosition(el: HTMLElement | undefined): { top: string; left: string; transform: string } {
        if (this.pickerElement == null) return { top: '', left: '', transform: ''}

        const input = this.pickerElement.$el;
        const dropdown = document.getElementsByClassName("dp__menu")[0] as HTMLElement;
        const pos = ElementUtils.calcDropdownPosition(input, dropdown, 4);
        return {
            top: pos.top,
            left: pos.left,
            transform: ''
        };
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
