<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeGuidComponent.vue -->
<template>
    <div>
        <v-text-field
            class="pt-0"
            v-model="parameter.Value"
            :placeholder="placeholderText"
            required />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from  '../../../../../models/modules/TestSuite/TestParameterViewModel';

@Component({
    components: {
    }
})
export default class ParameterInputTypeGuidComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;

    @Prop({ required: false })
    isListItem!: boolean;
    
    mounted(): void {
        if (this.parameter.Value == null && this.parameter.NotNull) {
            this.parameter.Value = "";
        }
    }

    get placeholderText(): string {
        return (this.parameter.Value == null || this.parameter.Value.length == 0)
            ? "00000000-0000-0000-0000-000000000000" : "";
    }
}
</script>

<style scoped>
</style>
