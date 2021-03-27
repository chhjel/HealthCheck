<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeDecimalComponent.vue -->
<template>
    <div>
        <v-text-field
            type="number"
            class="pt-0"
            v-model="localValue"
            required />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";

@Component({
    components: {
    }
})
export default class ParameterInputTypeDecimalComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    localValue: string = '';
    
    mounted(): void {
        this.updateLocalValue();
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
        if (this.localValue == null || this.localValue === '') {
            this.localValue = "0";
        }
    }
}
</script>

<style scoped>
</style>
