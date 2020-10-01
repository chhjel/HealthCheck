<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputPickReferenceComponent.vue -->
<template>
    <div>
        <v-btn @click="choicesDialogVisible = true">{{ selectedChoiceLabel }}</v-btn>
        
        <v-dialog v-model="choicesDialogVisible"
            @keydown.esc="choicesDialogVisible = false"
            scrollable
            max-width="600"
            content-class="select-reference-item-dialog">
            <v-card style="background-color: #f4f4f4">
                <v-toolbar class="elevation-0">
                    <v-toolbar-title>Select value</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-btn icon
                        @click="choicesDialogVisible = false">
                        <v-icon>close</v-icon>
                    </v-btn>
                </v-toolbar>

                <v-divider></v-divider>
                
                <v-card-text>
                    <v-text-field
                        class="pb-1"
                        v-model="choicesFilterText"
                        placeholder="Filter.." />
                    <small>{{ filteredChoices.length }}/{{ maxFilteredResults }} of {{ choiceCount }} total</small>
                    <div
                        v-for="(choice, cindex) in filteredChoices"
                        :key="`${parameter.index}-choices-${cindex}`"
                        class="mb-2">
                        <v-btn
                            @click="selectChoice(choice)"
                            :color="choiceColor(choice)"
                            >
                            {{ choice.Name }}
                        </v-btn>
                    </div>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions >
                    <v-spacer></v-spacer>
                    <v-btn color="primary"
                        @click="choicesDialogVisible = false">Cancel</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
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

    selectedChoice: TestParameterReferenceChoiceViewModel = this.choices[0];
    choicesDialogVisible: boolean = false;
    choicesFilterText: string = '';
    maxFilteredResults: number = 100;
    
    mounted(): void {
        if (this.parameter.Value == null) {
            this.parameter.Value = "";
        }
    }

    get selectedChoiceLabel(): string {
        if (this.selectedChoice == null) return '[null]';
        else return this.selectedChoice.Name;
    }

    get choices(): Array<TestParameterReferenceChoiceViewModel>
    {
        let values = Array<TestParameterReferenceChoiceViewModel>();
        values.push({
            Id: '',
            Name: '[null]'
        });
        if (this.parameter.ReferenceChoices != null)
        {
            this.parameter.ReferenceChoices.forEach(choice => {
                values.push(choice);
            }); 
        }

        return values;
    }

    get filteredChoices(): Array<TestParameterReferenceChoiceViewModel>
    {
        const filterText = this.choicesFilterText ?? '';
        return this.choices.filter((x, index) => {
            const idMatches = filterText.length == 0 || x.Id.toLowerCase().indexOf(filterText.toLowerCase()) != -1;
            const nameMatches = filterText.length == 0 || x.Name.toLowerCase().indexOf(filterText.toLowerCase()) != -1;
            const withinCountLimit = index < this.maxFilteredResults;
            return withinCountLimit && (idMatches || nameMatches);
        });
    }

    get choiceCount(): number {
        return this.choices.length;
    }

    selectChoice(choice: TestParameterReferenceChoiceViewModel): void
    {
        this.selectedChoice = choice;
        this.parameter.Value = choice.Id;
        this.choicesDialogVisible = false;
    }

    choiceColor(choice: TestParameterReferenceChoiceViewModel): string
    {
        return (choice.Id == null || choice.Id.length == 0) ? 'secondary' : 'primary';
    }
}
</script>

<style scoped>
</style>
