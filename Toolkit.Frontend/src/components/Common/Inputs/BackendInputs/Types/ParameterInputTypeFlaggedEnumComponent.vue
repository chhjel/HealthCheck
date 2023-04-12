<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeFlaggedEnumComponent.vue -->
<template>
    <div>
        <parameter-input-type-enum-component
            v-model:value="localValue"
            :config="config"
            :readonly="readonly"
            :multiple="true" />
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TKBackendInputConfig } from '@generated/Models/Core/TKBackendInputConfig';
import ParameterInputTypeEnumComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeEnumComponent.vue';

@Options({
    components: {
        ParameterInputTypeEnumComponent
    }
})
export default class ParameterInputTypeFlaggedEnumComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    config!: TKBackendInputConfig;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    localValue: any = "";

    created(): void {
        this.localValue = this.value;
    }
    
    @Watch('value')
    updateLocalValue(): void
    {
        this.localValue = this.value;
    }

    @Watch('localValue')
    onLocalValueChanged(): void
    {
        this.$emit('update:value', this.localValue);
    }
}
</script>

<style>
</style>
