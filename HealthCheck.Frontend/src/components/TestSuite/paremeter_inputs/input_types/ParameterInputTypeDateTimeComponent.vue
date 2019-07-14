<!-- src/components/paremeter_inputs/input_types/ParameterInputTypeDateTimeComponent.vue -->
<template>
    <div>
        <v-text-field
            class="pt-0"
            v-model="parameter.Value"
            v-on:change="onValueChanged"
            required />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from '../../../../models/TestSuite/TestParameterViewModel';
import DateUtils from "../../../../util/DateUtils";

@Component({
    components: {
    }
})
export default class ParameterInputTypeDateTimeComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;
    
    mounted(): void {
        if (this.parameter.Value == null || this.parameter.Value === '') {
            this.setValueToNow();
        }
    }

    onValueChanged(): void {
        if (this.parameter.Value == null || this.parameter.Value === '') {
            this.setValueToNow();
        }
    }

    setValueToNow(): void {
        this.parameter.Value = DateUtils.FormatDate(new Date(), 'dd-MM-yy HH:mm:ss');
    }
}
</script>

<style scoped>
</style>
