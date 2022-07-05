<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeDateTimeComponent.vue -->
<template>
    <div>
        <date-picker-component
            v-model:value="localValueDates"
            :placeholder="placeholderText"
            :disabled="readonly"
            :clearable="isNullable"
        ></date-picker-component>
    </div>
</template>

<script lang="ts">
// Old:
// <text-field-component
//     class="pt-0"
//     v-model:value="localValue"
//     :placeholder="placeholderText"
//     :disabled="readonly"
//     type="datetime-local"> 
//     <tooltip-component v-if="isNullable" tooltip="Set value to null">
//         <icon-component @click="clearValue">clear</icon-component>
//     </tooltip-component>
// </text-field-component>
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import DateUtils from '@util/DateUtils';
import { HCBackendInputConfig } from '@generated/Models/Core/HCBackendInputConfig';

@Options({
    components: {
    }
})
export default class ParameterInputTypeDateTimeComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    config!: HCBackendInputConfig;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    localValue: string | null = '';
    localValueDates: Date[] | null = null;
    isCreated: boolean = false;
    
    created(): void {
        this.updateLocalValue();
        this.isCreated = true;
    }

    public formatDefaultValue(val: string): string | null {
        return DateUtils.FormatDate(new Date(val), 'yyyy-MM-ddTHH:mm:ss');;
    }

    validateValue(): void {
        if (this.localValue == null || this.localValue === '') {
            if (this.isNullable)
            {
                this.localValue = null;
            }
            // Ignore on create if we have default value
            else if (!this.isCreated && this.config.DefaultValue && !this.config.DefaultValue.startsWith('01-Jan-01 00:00:00')) { }
            else
            {
                this.setValueToNow();
            }
        }
    }

    clearValue(): void {
        this.localValue = null;
    }

    setValueToNow(): void {
        this.localValue = DateUtils.FormatDate(new Date(), 'yyyy-MM-ddTHH:mm:ss');
    }

    get isNullable(): boolean {
        return this.config.Nullable;
    }
    
    get placeholderText(): string {
        return (this.isNullable && this.localValue == null) ? this.nullName : "";
    }

    get nullName(): string {
        return this.config.NullName || 'null';
    }

    setLocalValueFromDate(date: Date | Date[] | null): void {
        if (date == null) {
            this.clearValue();
        } else if (Array.isArray(date) && (date.length == 0 || date[0] == null)) {
            this.clearValue();
        } else if (Array.isArray(date) && date.length > 0) {
            this.localValue = DateUtils.FormatDate(date[0], 'yyyy-MM-ddTHH:mm:ss');
        } else if (!Array.isArray(date)) {
            this.localValue = DateUtils.FormatDate(date, 'yyyy-MM-ddTHH:mm:ss');
        }
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
        this.localValue = this.value;
        this.localValueDates = !!this.localValue ? [new Date(this.localValue)] : null;
        this.validateValue();
    }

    @Watch('localValue')
    onLocalValueChanged(): void
    {
        this.validateValue();
        this.$emit('update:value', this.localValue);
    }

    @Watch('localValueDates', { deep: true })
    onLocalValueDatesChanged(): void
    {
        this.setLocalValueFromDate(this.localValueDates);
    }
}
</script>

<style scoped lang="scss">
</style>
