<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeDoubleComponent.vue -->
<template>
    <div>
        <v-text-field
            type="number"
            class="pt-0"
            v-model="localValue"
            :placeholder="placeholderText"
            required />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import BackendInputConfig from "../BackendInputConfig";

@Component({
    components: {
    }
})
export default class ParameterInputTypeDoubleComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    config!: BackendInputConfig;

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
        return this.config.nullable;
    }
    
    get placeholderText(): string {
        return (this.isNullable && this.localValue == null) ? "null" : "";
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

<style scoped>
</style>
