<!-- src/components/paremeter_inputs/input_types/ParameterInputTypeInt32Component.vue -->
<template>
    <div>
        <v-text-field
            type="number"
            class="pt-0"
            v-model="parameter.Value"
            required />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from '../../../../models/TestParameterViewModel';

@Component({
    components: {
    }
})
export default class ParameterInputTypeInt32Component extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;
    
    mounted(): void {
    }
}
</script>

<style scoped>
</style>
