<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputPickReferenceComponent.vue -->
<template>
    <div>
        <div class="pick-ref-button-wrapper">
            <v-btn @click="showDialog" :disabled="readonly" class="pick-ref-button ml-0 mr-0">{{ selectedChoiceLabel }}</v-btn>
        
            <v-tooltip bottom v-if="localValue" >
                <template v-slot:activator="{ on }">
                    <v-btn flat small icon color="primary" v-if="localValue" class="mr-0" @click="copyToClipboard" v-on="on">
                        <v-icon small>content_copy</v-icon>
                    </v-btn>
                </template>
                <span>Copy to clipboard</span>
            </v-tooltip>
        </div>

        <textarea style="display:none;" ref="copyValue" :value="localValue" />
        <v-snackbar v-model="showCopyAlert" :timeout="5000" :color="copyAlertColor" :bottom="true">
          {{ copyAlertText }}
          <v-btn flat @click="showCopyAlert = false">Close</v-btn>
        </v-snackbar>
        
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
                    <v-layout row>
                        <v-flex xs9>
                            <v-text-field
                                class="pb-1"
                                v-model="choicesFilterText"
                                placeholder="Filter.." />
                        </v-flex>
                        <v-flex xs3>
                            <v-btn @click="loadChoices" :disabled="loadingChoicesStatus.inProgress">Search</v-btn>
                        </v-flex>
                    </v-layout>
                    <small>{{ choices.length - 1 }} results</small>
                    <div
                        v-for="(choice, cindex) in choices"
                        :key="`${config.parameterIndex}-choices-${cindex}`"
                        class="mb-2">
                        <v-btn
                            @click="selectChoice(choice)"
                            :color="choiceColor(choice)"
                            :disabled="readonly"
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
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import { TestParameterReferenceChoiceViewModel } from  '../../../../../models/modules/TestSuite/TestParameterViewModel';
import { FetchStatus, ServiceFetchCallbacks } from "../../../../../services/abstractions/HCServiceBase";
import { HCBackendInputConfig } from 'generated/Models/Core/HCBackendInputConfig';
import TestsUtils from "util/TestsModule/TestsUtils";
import ClipboardUtil from "util/ClipboardUtil";

@Component({
    components: {
    }
})
export default class ParameterInputPickReferenceComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    config!: HCBackendInputConfig;

    localValue: string | null = '';

    @Prop({ required: false })
    isListItem!: boolean;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    @Prop({ required: false, default: '' })
    parameterDetailContext!: string;

    // Service
    loadingChoicesStatus: FetchStatus = new FetchStatus();
    hasLoadedChoices: boolean = false;
    loadedChoices: Array<TestParameterReferenceChoiceViewModel> = [];

    selectedChoice: TestParameterReferenceChoiceViewModel = this.choices[0];
    choicesDialogVisible: boolean = false;
    choicesFilterText: string = '';
    
    showCopyAlert: boolean = false;
    copyAlertText: string = "";
    copyAlertColor: string = "success";
    
    mounted(): void {
        const loadedValue = this.getParameterDetail('choice');
        if (loadedValue) { this.selectedChoice = loadedValue as TestParameterReferenceChoiceViewModel; }

        if (this.localValue == null) {
            this.localValue = "";
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

    showDialog(): void {
        this.choicesDialogVisible = true;

        if (!this.hasLoadedChoices && !this.loadingChoicesStatus.inProgress)
        {
            this.loadChoices();
        }
    }

    loadChoices(): void {
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
                'parameterIndex': this.config.ParameterIndex,
                'filter': this.choicesFilterText ?? ''
            }
        );
    }

    selectChoice(choice: TestParameterReferenceChoiceViewModel): void
    {
        this.selectedChoice = choice;
        this.localValue = choice.Id;
        this.choicesDialogVisible = false;
        this.setParameterDetail('choice', this.selectedChoice);
    }

    choiceColor(choice: TestParameterReferenceChoiceViewModel): string
    {
        return (choice.Id == null || choice.Id.length == 0) ? 'secondary' : 'primary';
    }

    copyToClipboard(): void {
        let copySourceElement = this.$refs.copyValue as HTMLTextAreaElement;
        const err = ClipboardUtil.putDataOnClipboard(copySourceElement);
        if (err)
        {
            this.ShowCopyAlert(err, true);
        }
        else {
            this.ShowCopyAlert("Data successfully put on clipboard.", true);
        }
    }

    ShowCopyAlert(msg: string, isSuccess: boolean): void {
      this.showCopyAlert = true;
      this.copyAlertText = msg;
      this.copyAlertColor = (isSuccess) ? "success" : "error";
    }

    //////////////////////////
    //  PARAMETER DETAILS  //
    ////////////////////////
    createParameterDetailKey(): string {
        return this.parameterDetailContext + "_" + this.config.Id;
    }
    setParameterDetail<T>(key: string, value: T): void {
        return TestsUtils.setParameterDetail<T>(this.$store, this.createParameterDetailKey(), key, value);
    }
    getParameterDetail<T>(key: string): T | null {
        return TestsUtils.getParameterDetail<T>(this.$store, this.createParameterDetailKey(), key);
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
        this.localValue = this.value;
    }

    @Watch('localValue')
    onLocalValueChanged(): void
    {
        this.$emit('input', this.localValue);
    }
}
</script>

<style scoped>
.pick-ref-button-wrapper {
    max-width: 100%;
    overflow: hidden;
    display: flex;
    flex-wrap: nowrap;
    align-items: center;
}
.pick-ref-button {
    max-width: 100%;
    overflow: hidden;
    justify-content: flex-start;
    flex: 1;
}
</style>
