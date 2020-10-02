<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputPickReferenceComponent.vue -->
<template>
    <div>
        <v-btn @click="showDialog">{{ selectedChoiceLabel }}</v-btn>
        
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
                    <v-progress-linear
                        v-if="loadingChoicesStatus.inProgress"
                        indeterminate color="primary"></v-progress-linear>
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
import { FetchStatus, ServiceFetchCallbacks } from "../../../../../services/abstractions/HCServiceBase";

@Component({
    components: {
    }
})
export default class ParameterInputPickReferenceComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;

    @Prop({ required: false })
    isListItem!: boolean;

    // Service
    loadingChoicesStatus: FetchStatus = new FetchStatus();
    hasLoadedChoices: boolean = false;
    loadedChoices: Array<TestParameterReferenceChoiceViewModel> = [];

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

        if (this.loadedChoices != null)
        {
            this.loadedChoices.forEach(choice => {
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

    showDialog(): void {
        this.choicesDialogVisible = true;

        if (!this.hasLoadedChoices && !this.loadingChoicesStatus.inProgress)
        {
            const callbacks: ServiceFetchCallbacks<Array<TestParameterReferenceChoiceViewModel>> = {
                onSuccess: (data) => {
                    this.hasLoadedChoices = true;
                    this.loadedChoices = data;
                }
            };

            this.$root.$emit('hc__loadTestParameterChoices', 
                {
                    'component': this,
                    'loadStatus' : this.loadingChoicesStatus,
                    'callbacks': callbacks,
                    'parameterIndex': this.parameter.Index
                }
            );
        }
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
