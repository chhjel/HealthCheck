<!-- src/components/paremeter_inputs/UnknownParameterInputComponent.vue -->
<template>
    <div>
        Unknown parameter type '{{parameter.Type}}'. No input component created for this type yet.<br />
        Default value '{{parameter.Value}}' will be used.
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from '../../models/TestParameterViewModel';

@Component({
    components: {
    }
})
export default class UnknownParameterInputComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;

    mounted(): void {
    }
}
</script>

<style scoped>
</style>
