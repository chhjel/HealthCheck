<!-- src/components/modules/DataRepeater/DataRepeaterItemComponent.vue -->
<template>
    <div>
        <!-- LOAD PROGRESS -->
        <v-progress-linear 
            v-if="dataLoadStatus.inProgress && item == null"
            indeterminate color="green"></v-progress-linear>

        <!-- DATA LOAD ERROR -->
        <v-alert :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
        {{ dataLoadStatus.errorMessage }}
        </v-alert>

        <!-- NOT FOUND -->
        <v-alert :value="itemNotFound" v-if="itemNotFound" type="info">
        No item with the given id was found.
        </v-alert>
        
        <v-btn @click="$emit('close')" v-if="itemNotFound">Close</v-btn>
        
        <div v-if="item">
            <v-btn @click="$emit('close')" class="right">Close</v-btn>
            <h2>{{ stream.ItemIdName }}: {{ item.ItemId }}</h2>
            <p v-if="details && details.Description">{{ details.Description }}</p>

            <div v-if="item.InitialError">
                <h3 class="mt-2">Initial error</h3>
                <code>{{ item.InitialError }}</code>
            </div>

            <div>
                <h3 class="mt-2">Metadata</h3>
                <ul>
                    <li><b>Inserted: </b>{{ item.InsertedAt }}</li>
                    <li><b>Retry allowed: </b>{{ item.AllowRetry }}</li>
                    <li v-if="item.Tags && item.Tags.length > 0"><b>Tags: </b>{{ item.Tags }}</li>
                    <li v-if="item.LastRetriedAt"><b>Last retried: </b>{{ item.LastRetriedAt }}</li>
                    <li v-if="item.LastRetryWasSuccessful != null"><b>Last retry was success: </b>{{ item.LastRetryWasSuccessful }}</li>
                    <li v-if="item.LastActionAt"><b>Last action: </b>{{ item.LastActionAt }}</li>
                    <li v-if="item.LastActionWasSuccessful != null"><b>Last action was success: </b>{{ item.LastActionWasSuccessful }}</li>
                </ul>
            </div>

            <div v-if="details && details.Links && details.Links.length > 0">
                <h3 class="mt-2">Links</h3>
                <ul>
                    <li v-for="(link, lIndex) in details.Links"
                        :key="`link-${item.Id}-${lIndex}`">
                        <a :href="link.Url" target="_blank">{{ link.Text }}</a>
                    </li>
                </ul>
            </div>

            <div v-if="item.Log && item.Log.length > 0">
                <h3 class="mt-2">Log</h3>
                <ul>
                    <li v-for="(logEntry, lIndex) in item.Log"
                        :key="`log-${item.Id}-${lIndex}`">
                        <code>[{{ logEntry.Timestamp }}] {{ logEntry.Message }}</code>
                    </li>
                </ul>
            </div>

            <editor-component
                class="editor mt-2"
                :language="'json'"
                v-model="item.SerializedDataOverride"
                ref="editor" />

            <div v-if="item.SerializedDataOverride && item.SerializedDataOverride != this.item.SerializedData">
                <a href="#" @click.prevent="restoreOriginalData" class="right">Restore original data</a>
            </div>
            
            <!-- RETRY -->
            <div>
                <p v-if="stream.RetryDescription">{{ stream.RetryDescription }}</p>

                <v-btn :disabled="!retryAllowed" :loading="dataLoadStatus.inProgress"
                    @click="retry" class="mb-3">
                    {{ (stream.RetryActionName || 'Retry') }}
                </v-btn>

                <span v-if="retryResult && retryResult.Message">{{ retryResult.Message }}</span>
            </div>

            <!-- ACTION PARAMETERS -->
            <div v-for="(action, aIndex) in stream.Actions"
                :key="`action-${aIndex}-${action.Id}`">
                <data-repeater-item-action-component 
                    :item="item"
                    :stream="stream"
                    :config="config"
                    :action="action" />
            </div>
        </div>
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
import ModuleConfig from "models/Common/ModuleConfig";
import DataRepeaterItemActionComponent from "./DataRepeaterItemActionComponent.vue";
import { HCDataRepeaterRetryResult } from "generated/Models/Core/HCDataRepeaterRetryResult";
import EditorComponent from "components/Common/EditorComponent.vue";
import { HCDataRepeaterStreamItemDetailsViewModel } from "generated/Models/Core/HCDataRepeaterStreamItemDetailsViewModel";

@Component({
    components: {
        BackendInputComponent,
        DataRepeaterItemActionComponent,
        EditorComponent
    }
})
export default class DataRepeaterItemComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;

    @Prop({ required: true })
    stream!: HCDataRepeaterStreamViewModel;

    @Prop({ required: true })
    itemId!: string;
    
    // Service
    service: DataRepeaterService = new DataRepeaterService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    details: HCDataRepeaterStreamItemDetailsViewModel | null = null;
    item: HCDataRepeaterStreamItemViewModel | null = null;
    retryResult: HCDataRepeaterRetryResult | null = null;
    itemNotFound: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.loadData();
        this.onItemChanged();

        this.refreshEditorSize();
        this.$nextTick(() => this.refreshEditorSize());
        setTimeout(() => {
            this.refreshEditorSize();
        }, 100);
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }

    get retryAllowed(): boolean {
        return !this.dataLoadStatus.inProgress
            && !!this.item
            && this.item.AllowRetry;
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.service.GetItemDetails({
            StreamId: this.stream.Id,
            ItemId: this.itemId
        }, this.dataLoadStatus, {
            onSuccess: (data) => this.onItemDetailsRetrieved(data)
        });
    }

    onItemDetailsRetrieved(data: HCDataRepeaterStreamItemDetailsViewModel | null): void {
        this.details = data;
        this.item = data?.Item || null;
        if (this.item == null) {
            this.itemNotFound = true;
        }
    }

    retry(): void {
        if (!this.item) return;
        this.service.RetryItem({
            StreamId: this.stream.Id,
            ItemId: this.item.Id,
            SerializedDataOverride: this.item.SerializedDataOverride
        }, this.dataLoadStatus, {
            onSuccess: (data) => this.onRetryResult(data)
        });
    }

    onRetryResult(data: HCDataRepeaterResultWithLogMessage<HCDataRepeaterRetryResult> | null): void {
        if (!this.item) return;
        this.retryResult = data?.Data || null;
        if (data?.LogMessage)
        {
            this.item.Log.push(data.LogMessage);
            this.item.Log = this.item.Log.slice(Math.max(this.item.Log.length - 10, 0))
        }
        this.service.ApplyChanges(this.item, data?.Data || null);
    }
    
    refreshEditorSize(): void {
        const editor: EditorComponent = <EditorComponent>this.$refs.editor;
        if (editor)
        {
            editor.refreshSize();
        }
    }

    restoreOriginalData(): void {
        if (!this.item) return;
        this.item.SerializedDataOverride = this.item.SerializedData;
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch("item")
    onItemChanged(): void {
        if (!this.item) return;
        if (!this.item.SerializedDataOverride)
        {
            this.item.SerializedDataOverride = this.item.SerializedData;
        }
    }
}
</script>

<style scoped lang="scss">
.editor {
  width: 100%;
  height: 200px;
  border: 1px solid #949494;
}
</style>