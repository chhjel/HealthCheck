<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeFlaggedEnumComponent.vue -->
<template>
    <div>
        <parameter-input-type-enum-component
            v-model="localValue"
            :config="config"
            :multiple="true" />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import BackendInputConfig from "../BackendInputConfig";
import ParameterInputTypeEnumComponent from './ParameterInputTypeEnumComponent.vue';

@Component({
    components: {
        ParameterInputTypeEnumComponent
    }
})
export default class ParameterInputTypeFlaggedEnumComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    config!: BackendInputConfig;

    localValue: any = "";

    mounted(): void {
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
        this.$emit('input', this.localValue);
    }
}
</script>

<style>
</style>
