<!-- src/components/paremeter_inputs/input_types/ParameterInputTypeStringComponent.vue -->
<template>
    <div>
        <v-text-field
            class="pt-0"
            v-model="parameter.Value"
            required />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from '../../../models/TestParameterViewModel';

@Component({
    components: {
    }
})
export default class ParameterInputTypeStringComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;
    
    mounted(): void {
    }
}
</script>

<style scoped>
</style>
