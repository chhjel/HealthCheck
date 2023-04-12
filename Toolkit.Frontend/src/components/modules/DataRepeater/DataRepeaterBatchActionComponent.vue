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
            max-width="520"
            :persistent="dataLoadStatus.inProgress">
            <template #header>Confirm execute '{{ action.Name }}'</template>
            <template #footer>
                <btn-component color="primary"
                    :disabled="!allowExecute"
                    :loading="dataLoadStatus.inProgress"
                    @click="executeAction()">Execute</btn-component>
                <btn-component color="secondary"
                    :disabled="dataLoadStatus.inProgress"
                    :loading="dataLoadStatus.inProgress"
                    @click="confirmExecuteDialogVisible = false">Cancel</btn-component>
            </template>
            <div>
                Are you sure you want to execute the action '{{ action.Name }}'?
            </div>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { FetchStatus } from '@services/abstractions/TKServiceBase';
import DataRepeaterService from '@services/DataRepeaterService';
import { TKDataRepeaterStreamViewModel } from "@generated/Models/Core/TKDataRepeaterStreamViewModel";
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import ModuleConfig from "@models/Common/ModuleConfig";
import { TKDataRepeaterStreamBatchActionResult } from "@generated/Models/Core/TKDataRepeaterStreamBatchActionResult";
import { TKDataRepeaterStreamBatchActionViewModel } from "@generated/Models/Core/TKDataRepeaterStreamBatchActionViewModel";
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
    stream!: TKDataRepeaterStreamViewModel;

    @Prop({ required: true })
    action!: TKDataRepeaterStreamBatchActionViewModel;
    
    // Service
    service: DataRepeaterService = new DataRepeaterService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    parameters: { [key:string]: string } = {};
    result: TKDataRepeaterStreamBatchActionResult | null = null;
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

    onActionExecutedResult(data: TKDataRepeaterStreamBatchActionResult): void {
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