<!-- src/components/modules/DataRepeater/DataRepeaterBatchActionComponent.vue -->
<template>
    <div class="data-repeater-batch-action">
        <h3>{{ action.Name }}</h3>
        <p v-if="action.Description">{{ action.Description }}</p>

        <div class="data-repeater-batch-action--parameters">
            <backend-input-component
                v-for="(parameterDef, pIndex) in action.ParameterDefinitions"
                :key="`action-parameter-item-${action.Id}-${pIndex}`"
                class="action-parameter-item"
                v-model:value="parameters[parameterDef.Id]"
                :config="parameterDef"
                :readonly="dataLoadStatus.inProgress"
                />
        </div>
        
        <div class="data-repeater-batch-action--actions">
            <div style="display: flex; align-items: baseline;">
                <btn-component :disabled="!allowExecute"
                    :loading="dataLoadStatus.inProgress"
                    @click="showExecuteActionDialog" class="mb-3">
                    {{ (action.ExecuteButtonLabel || 'Run') }}
                </btn-component>
                
                <span v-if="result && result.Message">{{ result.Message }}</span>
            </div>

            <!-- DATA LOAD ERROR -->
            <alert-component :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
            {{ dataLoadStatus.errorMessage }}
            </alert-component>
        </div>

        <!-- DIALOGS -->
        <dialog-component v-model:value="confirmExecuteDialogVisible"
            @keydown.esc="confirmExecuteDialogVisible = false"
            max-width="480"
            content-class="confirm-dialog"
            :persistent="dataLoadStatus.inProgress">
            <div>
                <div class="headline">Confirm execute '{{ action.Name }}'</div>
                <div>
                    Are you sure you want to execute the action '{{ action.Name }}'?
                </div>
                                <div>
                                        <btn-component color="secondary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="confirmExecuteDialogVisible = false">Cancel</btn-component>
                    <btn-component color="primary"
                        :disabled="!allowExecute"
                        :loading="dataLoadStatus.inProgress"
                        @click="executeAction()">Execute</btn-component>
                </div>
            </div>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import DataRepeaterService from '@services/DataRepeaterService';
import { HCDataRepeaterStreamViewModel } from "@generated/Models/Core/HCDataRepeaterStreamViewModel";
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import ModuleConfig from "@models/Common/ModuleConfig";
import { HCDataRepeaterStreamBatchActionResult } from "@generated/Models/Core/HCDataRepeaterStreamBatchActionResult";
import { HCDataRepeaterStreamBatchActionViewModel } from "@generated/Models/Core/HCDataRepeaterStreamBatchActionViewModel";
import { StoreUtil } from "@util/StoreUtil";

@Options({
    components: {
        BackendInputComponent
    }
})
export default class DataRepeaterBatchActionComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;

    @Prop({ required: true })
    stream!: HCDataRepeaterStreamViewModel;

    @Prop({ required: true })
    action!: HCDataRepeaterStreamBatchActionViewModel;
    
    // Service
    service: DataRepeaterService = new DataRepeaterService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    parameters: { [key:string]: string } = {};
    result: HCDataRepeaterStreamBatchActionResult | null = null;
    confirmExecuteDialogVisible: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get allowExecute(): boolean {
        return !this.dataLoadStatus.inProgress;
    }

    ////////////////
    //  METHODS  //
    //////////////
    showExecuteActionDialog(): void {
        this.confirmExecuteDialogVisible = true;
    }

    executeAction(): void {
        this.service.PerformBatchAction({
            StreamId: this.stream.Id,
            ActionId: this.action.Id,
            Parameters: this.parameters
        }, this.dataLoadStatus, {
            onSuccess: (data) => this.onActionExecutedResult(data),
            onDone: () => {
                this.confirmExecuteDialogVisible = false;
            }
        });
    }

    onActionExecutedResult(data: HCDataRepeaterStreamBatchActionResult): void {
        this.result = data;
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
.data-repeater-batch-action {
    padding: 10px;
    background-color: #f8f8f8;

    &--parameters {
        margin-left: 20px;
    }
    &--actions {
        margin-left: 20px;
    }
}
</style>