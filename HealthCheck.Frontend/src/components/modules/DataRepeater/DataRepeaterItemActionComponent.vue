<!-- src/components/modules/DataRepeater/DataRepeaterItemActionComponent.vue -->
<template>
    <div>
        <h3>{{ action.Name }}</h3>
        <p v-if="action.Description">{{ action.Description }}</p>

        <backend-input-component
            v-for="(parameterDef, pIndex) in action.ParameterDefinitions"
            :key="`action-parameter-item-${action.Id}-${pIndex}`"
            class="action-parameter-item"
            v-model="parameters[parameterDef.Id]"
            :config="parameterDef"
            :readonly="dataLoadStatus.inProgress"
            />
        
        <div v-if="disabledReason"><b>{{ disabledReason }}</b></div>

        <div style="display: flex; align-items: baseline;">
            <v-btn :disabled="!allowExecute"
                :loading="dataLoadStatus.inProgress"
                @click="showExecuteActionDialog" class="mb-3">
                {{ (action.ExecuteButtonLabel || 'Run') }}
            </v-btn>
            
            <span v-if="result && result.Message">{{ result.Message }}</span>
        </div>

        <!-- DATA LOAD ERROR -->
        <v-alert :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
        {{ dataLoadStatus.errorMessage }}
        </v-alert>

        <!-- DIALOGS -->
        <v-dialog v-model="confirmExecuteDialogVisible"
            @keydown.esc="confirmExecuteDialogVisible = false"
            max-width="480"
            content-class="confirm-dialog"
            :persistent="dataLoadStatus.inProgress">
            <v-card>
                <v-card-title class="headline">Confirm execute '{{ action.Name }}'</v-card-title>
                <v-card-text>
                    Are you sure you want to execute the action '{{ action.Name }}'?
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="confirmExecuteDialogVisible = false">Cancel</v-btn>
                    <v-btn color="primary"
                        :disabled="!allowExecute"
                        :loading="dataLoadStatus.inProgress"
                        @click="executeAction()">Execute</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import { FetchStatus } from  '../../../services/abstractions/HCServiceBase';
import DataRepeaterService, { HCDataRepeaterResultWithItem } from  '../../../services/DataRepeaterService';
import { HCDataRepeaterStreamViewModel } from "generated/Models/Core/HCDataRepeaterStreamViewModel";
import BackendInputComponent from "components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import { HCDataRepeaterStreamItemViewModel } from "generated/Models/Core/HCDataRepeaterStreamItemViewModel";
import { HCDataRepeaterStreamItemDetails } from "generated/Models/Core/HCDataRepeaterStreamItemDetails";
import ModuleConfig from "models/Common/ModuleConfig";
import { HCDataRepeaterStreamActionViewModel } from "generated/Models/Core/HCDataRepeaterStreamActionViewModel";
import { HCDataRepeaterStreamItemActionResult } from "generated/Models/Core/HCDataRepeaterStreamItemActionResult";

@Component({
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
        return this.$store.state.globalOptions;
    }

    get allowExecute(): boolean {
        return !this.dataLoadStatus.inProgress
            && (this.action.AllowedOnItemsWithTags.length == 0 || this.action.AllowedOnItemsWithTags.some(t => this.item.Tags.includes(t)));
    }

    get disabledReason(): string | null {
        if (this.action.AllowedOnItemsWithTags.length > 0 && !this.action.AllowedOnItemsWithTags.some(t => this.item.Tags.includes(t)))
        {
            const tags = this.action.AllowedOnItemsWithTags.joinForSentence(', ', ' and ');
            return `Requires any of the following tags: ${tags}`;
        }
        return null;
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