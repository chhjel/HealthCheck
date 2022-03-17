<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeDateTimeOffsetComponent.vue -->
<template>
    <div>
        <text-field-component
            class="pt-0"
            v-model:value="localValue"
            :placeholder="placeholderText"
            :disabled="readonly"
            type="datetime-local"
            required>
            <tooltip-component v-if="isNullable" tooltip="Set value to null">
                <icon-component @click="clearValue">clear</icon-component>
            </tooltip-component>
        </text-field-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import DateUtils from '@util/DateUtils';
import { HCBackendInputConfig } from '@generated/Models/Core/HCBackendInputConfig';

@Options({
    components: {
    }
})
export default class ParameterInputTypeDateTimeOffsetComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    config!: HCBackendInputConfig;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    localValue: string | null = '';
    
    mounted(): void {
        this.updateLocalValue();
    }

    public formatDefaultValue(val: string): string | null {
        return DateUtils.FormatDate(new Date(val), 'yyyy-MM-ddThh:mm:ss');;
    }

    validateValue(): void {
        if (this.localValue == null || this.localValue === '') {
            if (this.isNullable)
            {
                this.localValue = null;
            }
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
        this.localValue = DateUtils.FormatDate(new Date(), 'yyyy-MM-ddThh:mm:ss');
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

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
        this.localValue = this.value;
        this.validateValue();
    }

    @Watch('localValue')
    onLocalValueChanged(): void
    {
        this.validateValue();
        this.$emit('update:value', this.localValue);
    }
}
</script>

<style scoped lang="scss">
</style>
