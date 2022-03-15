<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeInt64Component.vue -->
<template>
    <div>
        <text-field-component
            type="number"
            class="pt-0"
            v-model:value="localValue"
            :placeholder="placeholderText"
            :disabled="readonly"
            required />
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { HCBackendInputConfig } from '@generated/Models/Core/HCBackendInputConfig';

@Options({
    components: {
    }
})
export default class ParameterInputTypeInt64Component extends Vue {
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

    validateValue(): void {
        if (this.localValue == null || this.localValue === '') {
            this.localValue = this.isNullable ? null : "0";
        }
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

<style scoped>
</style>
