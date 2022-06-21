<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputPickReferenceComponent.vue -->
<template>
    <div>
        <div class="pick-ref-button-wrapper">
            
            <tooltip-component :disabled="!tooltip" :tooltip="tooltip">
                <btn-component @click="showDialog" :disabled="readonly" class="pick-ref-button ml-0 mr-0">{{ selectedChoiceLabel }}</btn-component>
            </tooltip-component>
        
            <tooltip-component v-if="localValue" tooltip="Copy to clipboard">
                <btn-component flat small icon color="primary" v-if="localValue" class="mr-0" @click="copyToClipboard">
                    <icon-component small>content_copy</icon-component>
                </btn-component>
            </tooltip-component>
        </div>

        <textarea style="display:none;" ref="copyValue" :value="localValue" />
        <snackbar-component v-model:value="showCopyAlert" :timeout="5000" :color="copyAlertColor">
          {{ copyAlertText }}
          <btn-component flat @click="showCopyAlert = false">Close</btn-component>
        </snackbar-component>
        
        <dialog-component v-model:value="choicesDialogVisible" max-width="600">
            <template #header>{{ dialogTitle }}</template>
            <template #footer>
                <btn-component color="primary"
                    @click="choicesDialogVisible = false">Cancel</btn-component>
            </template>

            <div>
                <p v-if="dialogDescription">{{ dialogDescription }}</p>
                <div row>
                    <div xs9>
                        <text-field-component
                            class="pb-1"
                            v-model:value="choicesFilterText"
                            placeholder="Filter.." />
                    </div>
                    <div xs3>
                        <btn-component @click="loadChoices" :disabled="loadingChoicesStatus.inProgress">{{ dialogSearchButtonText }}</btn-component>
                    </div>
                </div>
                <small>{{ choices.length - 1 }} results</small>
                <div
                    v-for="(choice, cindex) in choices"
                    :key="`${config.parameterIndex}-choices-${cindex}`"
                    class="mb-2">
                    <btn-component class="select-reference-item"
                        @click="selectChoice(choice)"
                        :color="choiceColor(choice)"
                        :disabled="readonly"
                        >
                        <div class="select-reference-item__name">{{ choice.Name }}</div>
                        <div class="select-reference-item__desc" v-if="choice.Description">{{ choice.Description }}</div>
                    </btn-component>
                </div>
                <progress-linear-component
                    v-if="loadingChoicesStatus.inProgress"
                    indeterminate color="primary"></progress-linear-component>
            </div>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestParameterReferenceChoiceViewModel } from '@models/modules/TestSuite/TestParameterViewModel';
import { FetchStatus, ServiceFetchCallbacks } from '@services/abstractions/HCServiceBase';
import { HCBackendInputConfig } from '@generated/Models/Core/HCBackendInputConfig';
import TestsUtils from "@util/TestsModule/TestsUtils";
import ClipboardUtil from "@util/ClipboardUtil";
import { ReferenceValueFactoryConfigViewModel } from "@generated/Models/Core/ReferenceValueFactoryConfigViewModel";
import { StoreUtil } from "@util/StoreUtil";
import EventBus from "@util/EventBus";

@Options({
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

    @Prop({ required: false, default: null })
    referenceValueFactoryConfig!: ReferenceValueFactoryConfigViewModel | null;

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
    
    created(): void {
        const loadedValue = this.getParameterDetail('choice');
        if (loadedValue) { this.selectedChoice = loadedValue as TestParameterReferenceChoiceViewModel; }

        if (this.localValue == null) {
            this.localValue = "";
        }
    }

    get tooltip(): string {
        if (!this.localValue || !this.selectedChoice || !this.selectedChoice.Name) return '';
        else return this.selectedChoice.Name;
    }

    get nullName(): string {
        return this.config.NullName || '[null]';
    }

    get selectedChoiceLabel(): string {
        if (this.selectedChoice == null) return this.nullName;
        else return this.selectedChoice.Name;
    }

    get choices(): Array<TestParameterReferenceChoiceViewModel>
    {
        let values = Array<TestParameterReferenceChoiceViewModel>();
        values.push({
            Id: '',
            Name: this.nullName,
            Description: ''
        });

        if (this.loadedChoices != null)
        {
            this.loadedChoices.forEach(choice => {
                values.push(choice);
            }); 
        }

        return values;
    }

    get dialogTitle(): string {
        if (this.referenceValueFactoryConfig && this.referenceValueFactoryConfig.Title)
        {
            return this.referenceValueFactoryConfig.Title;
        }
        return "Select value";
    }

    get dialogDescription(): string {
        if (this.referenceValueFactoryConfig && this.referenceValueFactoryConfig.Description)
        {
            return this.referenceValueFactoryConfig.Description;
        }
        return "";
    }

    get dialogSearchButtonText(): string {
        if (this.referenceValueFactoryConfig && this.referenceValueFactoryConfig.SearchButtonText)
        {
            return this.referenceValueFactoryConfig.SearchButtonText;
        }
        return "Search";
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

        EventBus.notify("loadTestParameterChoices", 
            {
                'component': this,
                'loadStatus' : this.loadingChoicesStatus,
                'callbacks': callbacks,
                'parameterIndex': this.config.ParameterIndex,
                'filter': this.choicesFilterText ?? ''
            });
        // this.$root?.$emit('hc__loadTestParameterChoices', 
        //     {
        //         'component': this,
        //         'loadStatus' : this.loadingChoicesStatus,
        //         'callbacks': callbacks,
        //         'parameterIndex': this.config.ParameterIndex,
        //         'filter': this.choicesFilterText ?? ''
        //     }
        // );
    }

    selectChoice(choice: TestParameterReferenceChoiceViewModel): void
    {
        if (this.readonly) return;
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
        return TestsUtils.setParameterDetail<T>(StoreUtil.store, this.createParameterDetailKey(), key, value);
    }
    getParameterDetail<T>(key: string): T | null {
        return TestsUtils.getParameterDetail<T>(StoreUtil.store, this.createParameterDetailKey(), key);
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
        this.$emit('update:value', this.localValue);
    }
}
</script>

<style scoped lang="scss">
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

<style lang="scss">
.select-reference-item {
    height: inherit;
    padding: 10px 10px;
    min-width: inherit;
    
    .select-reference-item__name {
        max-width: 524px;
        text-align: left;
    }
    .select-reference-item__desc {
        color: #e7e7e7;
        font-size: 13px;
        text-transform: none;
        white-space: pre-wrap;
        max-width: 524px;
        text-align: left;
    }
}
</style>
