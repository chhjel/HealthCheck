<!-- src/components/modules/DataRepeater/DataRepeaterItemActionComponent.vue -->
<template>
    <div>
        <h3>{{ action.Name }}</h3>
        <p v-if="action.Description">{{ action.Description }}</p>

        <backend-input-component
            v-for="(parameterDef, pIndex) in action.ParameterDefinitions"
            :key="`action-parameter-item-${action.Id}-${pIndex}`"
            class="action-parameter-item"
            v-model:value="parameters[parameterDef.Id]"
            :config="parameterDef"
            :readonly="dataLoadStatus.inProgress"
            />
        
        <div v-if="disabledReason"><b>{{ disabledReason }}</b></div>

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
import DataRepeaterService, { HCDataRepeaterResultWithItem } from '@services/DataRepeaterService';
import { HCDataRepeaterStreamViewModel } from "@generated/Models/Core/HCDataRepeaterStreamViewModel";
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import { HCDataRepeaterStreamItemViewModel } from "@generated/Models/Core/HCDataRepeaterStreamItemViewModel";
import { HCDataRepeaterStreamItemDetails } from "@generated/Models/Core/HCDataRepeaterStreamItemDetails";
import ModuleConfig from "@models/Common/ModuleConfig";
import { HCDataRepeaterStreamActionViewModel } from "@generated/Models/Core/HCDataRepeaterStreamActionViewModel";
import { HCDataRepeaterStreamItemActionResult } from "@generated/Models/Core/HCDataRepeaterStreamItemActionResult";
import { HCDataRepeaterStreamItemActionAllowedViewModel } from "@generated/Models/Core/HCDataRepeaterStreamItemActionAllowedViewModel";
import { StoreUtil } from "@util/StoreUtil";

@Options({
    components: {
        BackendInputComponent
    }
})
export default class DataRepeaterItemActionComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    item!: HCDataRepeaterStreamItemViewModel;

    @Prop({ required: true })
    stream!: HCDataRepeaterStreamViewModel;

    @Prop({ required: true })
    action!: HCDataRepeaterStreamActionViewModel;
    
    // Service
    service: DataRepeaterService = new DataRepeaterService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    details: HCDataRepeaterStreamItemDetails | null = null;
    parameters: { [key:string]: string } = {};
    result: HCDataRepeaterStreamItemActionResult | null = null;
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
        return !this.dataLoadStatus.inProgress && this.actionValidationResult.Allowed;
    }

    get disabledReason(): string | null {
        if (!this.actionValidationResult.Allowed)
        {
            return this.actionValidationResult.Reason || 'Not allowed to be executed for this item in its current state.';
        }
        return null;
    }

    get actionValidationResult(): HCDataRepeaterStreamItemActionAllowedViewModel
    {
        const fallback: HCDataRepeaterStreamItemActionAllowedViewModel = {
            ActionId: this.action.Id,
            Allowed: true,
            Reason: ''
        };
        return this.item.ActionValidationResults.filter(x => x.ActionId == this.action.Id)[0] || fallback;
    }

    ////////////////
    //  METHODS  //
    //////////////
    showExecuteActionDialog(): void {
        this.confirmExecuteDialogVisible = true;
    }

    executeAction(): void {
        this.service.PerformItemAction({
            StreamId: this.stream.Id,
            ItemId: this.item.Id,
            ActionId: this.action.Id,
            Parameters: this.parameters
        }, this.dataLoadStatus, {
            onSuccess: (data) => this.onActionExecutedResult(data),
            onDone: () => {
                this.confirmExecuteDialogVisible = false;
            }
        });
    }

    onActionExecutedResult(data: HCDataRepeaterResultWithItem<HCDataRepeaterStreamItemActionResult> | null): void {
        this.result = data?.Data || null;
        if (data?.Item)
        {
            this.notifyItemUpdated(data.Item);
        }
    }

    notifyItemUpdated(item: HCDataRepeaterStreamItemViewModel): void {
        this.$emit('change', item);
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
</style>