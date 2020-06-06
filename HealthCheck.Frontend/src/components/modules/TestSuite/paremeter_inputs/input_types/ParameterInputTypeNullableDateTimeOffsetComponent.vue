<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeNullableDateTimeOffsetComponent.vue -->
<template>
    <div>
        <v-text-field
            class="pt-0"
            v-model="parameter.Value"
            v-on:change="onValueChanged"
            :placeholder="placeholderText"
            required />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from  '../../../../../models/modules/TestSuite/TestParameterViewModel';
import DateUtils from  '../../../../../util/DateUtils';

@Component({
    components: {
    }
})
export default class ParameterInputTypeNullableDateTimeOffsetComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;

    mounted(): void {
        if (this.parameter.Value === null) {
            this.parameter.Value = DateUtils.FormatDate(new Date(), 'dd-MM-yy HH:mm:ss');
        }
    }

    onValueChanged(): void {
        if (this.parameter.Value === '') {
            this.parameter.Value = null;
        }
    }
    
    get placeholderText(): string {
        return this.parameter.Value == null ? "null" : "";
    }
}
</script>

<style scoped>
</style>
