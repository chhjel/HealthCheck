<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeGuidComponent.vue -->
<template>
    <div>
        <v-text-field
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
export default class ParameterInputTypeGuidComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: false })
    isListItem!: boolean;

    @Prop({ required: true })
    config!: BackendInputConfig;

    localValue: string | null = '';
    
    mounted(): void {
        this.updateLocalValue();
    }
    
    setValueToNull(): void {
        this.localValue = null;
    }

    get placeholderText(): string {
        // return this.localValue == null ? "null" : "";
        return (this.localValue == null || this.localValue.length == 0)
            ? "00000000-0000-0000-0000-000000000000" : "";
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

    validateValue(): void {
        if (this.localValue == null && this.config.notNull) {
            this.localValue = "";
        }
    }
}
</script>

<style scoped>
</style>
