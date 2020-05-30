<!-- src/components/modules/TestSuite/paremeter_inputs/UnknownParameterInputComponent.vue -->
<template>
    <div>
        Unknown parameter type '{{parameter.Type}}'. No input component created for this type yet.<br />
        Default value {{valueName}} will be used.
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from  '../../../../models/modules/TestSuite/TestParameterViewModel';

@Component({
    components: {
    }
})
export default class UnknownParameterInputComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;

    mounted(): void {
        if (this.parameter.Type.startsWith("Nullable<")) {
            this.parameter.Value = null;
        }
    }

    get valueName(): string {
        if (this.parameter.Value == null) return "null";
        else return `'${this.parameter.Value}'`;
    }
}
</script>

<style scoped>
</style>
