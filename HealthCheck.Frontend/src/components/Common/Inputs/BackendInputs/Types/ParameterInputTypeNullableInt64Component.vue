<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeNullableInt64Component.vue -->
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

@Component({
    components: {
    }
})
export default class ParameterInputTypeNullableInt64Component extends Vue {
    @Prop({ required: true })
    value!: string;

    localValue: string | null = '';
    
    mounted(): void {
        this.localValue = this.value;
        if (this.localValue === '') {
            this.localValue = "0";
        }
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
        if (this.localValue === '') {
            this.localValue = null;
        }
    }
    
    get placeholderText(): string {
        return this.value == null ? "null" : "";
    }
}
</script>

<style scoped>
</style>
