<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputPickReferenceComponent.vue -->
<template>
    <div>
        <v-text-field
            type="text"
            class="pt-0"
            v-model="parameter.Value"
            required />
        
        {{ choices }}
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel, { TestParameterReferenceChoiceViewModel } from  '../../../../../models/modules/TestSuite/TestParameterViewModel';

@Component({
    components: {
    }
})
export default class ParameterInputPickReferenceComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;

    @Prop({ required: false })
    isListItem!: boolean;
    
    mounted(): void {
        if (this.parameter.Value == null) {
            this.parameter.Value = "";
        }
    }

    get choices(): Array<TestParameterReferenceChoiceViewModel>
    {
        let values = Array<TestParameterReferenceChoiceViewModel>();
        values.push({
            id: '',
            name: '[null]'
        });
        if (this.parameter.ReferenceChoices != null)
        {
            this.parameter.ReferenceChoices.forEach(choice => {
                values.push(choice);
            }); 
        }
        return values;
    }
}
</script>

<style scoped>
</style>
