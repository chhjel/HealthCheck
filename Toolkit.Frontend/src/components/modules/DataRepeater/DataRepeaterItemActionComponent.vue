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

        <div style="display: flex; align-items: baseline;" class="mt-2">
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
            max-width="480"
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
import DataRepeaterService, { TKDataRepeaterResultWithItem } from '@services/DataRepeaterService';
import { TKDataRepeaterStreamViewModel } from "@generated/Models/Core/TKDataRepeaterStreamViewModel";
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import { TKDataRepeaterStreamItemViewModel } from "@generated/Models/Core/TKDataRepeaterStreamItemViewModel";
import { TKDataRepeaterStreamItemDetails } from "@generated/Models/Core/TKDataRepeaterStreamItemDetails";
import ModuleConfig from "@models/Common/ModuleConfig";
import { TKDataRepeaterStreamActionViewModel } from "@generated/Models/Core/TKDataRepeaterStreamActionViewModel";
import { TKDataRepeaterStreamItemActionResult } from "@generated/Models/Core/TKDataRepeaterStreamItemActionResult";
import { TKDataRepeaterStreamItemActionAllowedViewModel } from "@generated/Models/Core/TKDataRepeaterStreamItemActionAllowedViewModel";
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
    item!: TKDataRepeaterStreamItemViewModel;

    @Prop({ required: true })
    stream!: TKDataRepeaterStreamViewModel;

    @Prop({ required: true })
    action!: TKDataRepeaterStreamActionViewModel;
    
    // Service
    service: DataRepeaterService = new DataRepeaterService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    details: TKDataRepeaterStreamItemDetails | null = null;
    parameters: { [key:string]: string } = {};
    result: TKDataRepeaterStreamItemActionResult | null = null;
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

    get actionValidationResult(): TKDataRepeaterStreamItemActionAllowedViewModel
    {
        const fallback: TKDataRepeaterStreamItemActionAllowedViewModel = {
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

    onActionExecutedResult(data: TKDataRepeaterResultWithItem<TKDataRepeaterStreamItemActionResult> | null): void {
        this.result = data?.Data || null;
        if (data?.Item)
        {
            this.notifyItemUpdated(data.Item);
        }
    }

    notifyItemUpdated(item: TKDataRepeaterStreamItemViewModel): void {
        this.$emit('change', item);
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
</style>