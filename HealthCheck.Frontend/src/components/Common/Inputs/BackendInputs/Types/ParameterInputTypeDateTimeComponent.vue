<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeDateTimeComponent.vue -->
<template>
    <div>
        <v-text-field
            class="pt-0"
            v-model="localValue"
            :placeholder="placeholderText"
            :disabled="readonly"
            type="datetime-local"
            required> 
            <v-tooltip slot="append" bottom v-if="isNullable">
                <v-icon slot="activator" @click="clearValue">clear</v-icon>
                Set value to null
            </v-tooltip>
        </v-text-field>
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
export default class ParameterInputTypeDateTimeComponent extends Vue {
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
        this.$emit('input', this.localValue);
    }
}
</script>

<style scoped lang="scss">
</style>
