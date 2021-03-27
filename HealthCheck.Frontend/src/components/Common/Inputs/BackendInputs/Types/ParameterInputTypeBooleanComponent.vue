<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeBooleanComponent.vue -->
<template>
    <div>
        <v-switch 
            v-model="localValue" 
            :label="label"
            color="secondary"
            class="parameter-checkbox pt-0 mt-2"
        ></v-switch>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";

@Component({
    components: {
    }
})
export default class ParameterInputTypeBooleanComponent extends Vue {
    @Prop({ required: true })
    value!: any;

    localValue: any = '';
    
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
            this.localValue = (this.value != null && this.value.toLowerCase() == "true"));
        }
    }

    get label(): string {
        return (typeof this.value === "boolean" && this.value)
            || (typeof this.value === "string" && this.value.toLowerCase() == "true")
             ? "Yes" : "No";
    }
}
</script>

<style>
.parameter-checkbox label {
    color: #000 !important;
}
</style>
