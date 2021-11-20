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

        <v-btn :disabled="!allowExecute"
            :loading="dataLoadStatus.inProgress"
            @click="executeAction" class="mb-3">
            {{ (action.ExecuteButtonLabel || 'Run') }}
        </v-btn>
        
        <span v-if="result && result.Message">{{ result.Message }}</span>

        <!-- DATA LOAD ERROR -->
        <v-alert :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
        {{ dataLoadStatus.errorMessage }}
        </v-alert>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import { FetchStatus } from  '../../../services/abstractions/HCServiceBase';
import DataRepeaterService, { HCDataRepeaterResultWithLogMessage } from  '../../../services/DataRepeaterService';
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
    executeAction(): void {
        this.service.PerformItemAction({
            StreamId: this.stream.Id,
            ItemId: this.item.Id,
            ActionId: this.action.Id,
            Parameters: this.parameters
        }, this.dataLoadStatus, {
            onSuccess: (data) => this.onActionExecutedResult(data)
        });
    }

    onActionExecutedResult(data: HCDataRepeaterResultWithLogMessage<HCDataRepeaterStreamItemActionResult> | null): void {
        this.result = data?.Data || null;
        if (data?.LogMessage)
        {
            this.item.Log.push(data.LogMessage);
            this.item.Log = this.item.Log.slice(Math.max(this.item.Log.length - 10, 0))
        }
        this.service.ApplyChanges(this.item, data?.Data || null);
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
</style>