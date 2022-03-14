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
        
        <div v-if="item" class="data-repeater-item">
            <v-btn @click="$emit('close')" class="right">Close</v-btn>
            <v-btn @click="loadData" class="right">
                <v-icon size="20px" class="mr-2">refresh</v-icon>Refresh
            </v-btn>

            <h1>{{ stream.ItemIdName }}: {{ item.ItemId }}</h1>
            <p v-if="item.Summary">{{ item.Summary }}</p>
            <p v-if="details && details.Description" v-html="details.Description"></p>

            <div class="data-repeater-item--detail-blocks">
                <div class="data-repeater-item--block">
                    <div class="data-repeater-item--metadata">
                        <h3 class="mt-0">Summary</h3>
                        <ul>
                            <li v-if="item.Tags && item.Tags.length > 0">
                                <div class="data-repeater-item--tags">
                                    <b>Tags: </b>
                                    <div class="data-repeater-item--tag"
                                        v-for="(tag, tIndex) in item.Tags"
                                        :key="`item-d-${item.Id}-tag-${tIndex}`">{{ tag }}</div>
                                </div>
                            </li>
                            <li><b>Inserted: </b>{{ formatDate(item.InsertedAt) }}</li>
                            <li  v-if="item.LastUpdatedAt"><b>Updated: </b>{{ formatDate(item.LastUpdatedAt) }}</li>
                            <li><b>Retry allowed: </b>{{ item.AllowRetry }}</li>
                            <li v-if="item.LastRetriedAt"><b>Last manually retried: </b>{{ formatDate(item.LastRetriedAt) }}</li>
                            <li v-if="item.LastRetryWasSuccessful != null"><b>Last retry was success: </b>{{ item.LastRetryWasSuccessful }}</li>
                            <li v-if="item.LastActionAt"><b>Last action: </b>{{ formatDate(item.LastActionAt) }}</li>
                            <li v-if="item.ExpiresAt"><b>Expires after: </b>{{ formatDate(item.ExpiresAt) }}</li>
                        </ul>
                    </div>
                </div>
                <div class="data-repeater-item--block" v-if="details && details.Links && details.Links.length > 0">
                    <div>
                        <h3 class="mt-0">Relevant links</h3>
                        <ul>
                            <li v-for="(link, lIndex) in details.Links"
                                :key="`link-${item.Id}-${lIndex}`">
                                <a :href="link.Url" target="_blank">{{ link.Text }}</a>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>

            <div v-if="item.FirstError" class="data-repeater-item--block mt-2">
                <small class="right" v-if="item.FirstErrorAt">{{ formatDate(item.FirstErrorAt) }}</small>
                <h3 class="mt-0">First error</h3>
                <code class="pa-2 repeater-error">{{ item.FirstError }}</code>
            </div>

            <div v-if="item.Error && item.Error != item.FirstError" class="data-repeater-item--block mt-2">
                <small class="right" v-if="item.LastErrorAt">{{ formatDate(item.LastErrorAt) }}</small>
                <h3 class="mt-0">Latest error</h3>
                <code class="pa-2 repeater-error">{{ item.Error }}</code>
            </div>

            <div v-if="item.Log && item.Log.length > 0" class="data-repeater-item--block mt-2">
                <h3 class="mt-0">Log</h3>
                <ul>
                    <li v-for="(logEntry, lIndex) in item.Log"
                        :key="`log-${item.Id}-${lIndex}`">
                        <div class="data-repeater-item--logentry"><b>{{ formatDate(logEntry.Timestamp) }}:</b> {{ logEntry.Message }}</div>
                    </li>
                </ul>
            </div>
            
            <!-- RETRY -->
            <div class="data-repeater-item--block mt-2">
                <h3 class="mt-0">{{ (stream.RetryActionName || 'Retry') }}</h3>
                <p v-if="stream.RetryDescription">{{ stream.RetryDescription }}</p>

                <editor-component
                    class="editor mt-2"
                    :language="'json'"
                    v-model="item.SerializedData"
                    :allowFullscreen="true"
                    ref="editor" />

                <div v-if="item.SerializedData && item.SerializedData != this.item.FirstSerializedData && hasAccessToRetry">
                    <v-tooltip bottom>
                        <template v-slot:activator="{ on }">
                            <a href="#" @click.prevent="restoreOriginalData" class="right" style="cursor: help;" v-on="on">Revert to original data</a>
                        </template>
                        <span>Reverts the content above to the first data that was stored on {{ formatDate(item.InsertedAt) }}.</span>
                    </v-tooltip>
                </div>

                <div v-if="!hasAccessToRetry" class="not-allowed-text">
                    You do not have access to retry this data.
                </div>

                <v-btn :disabled="!retryAllowed"
                    :loading="dataLoadStatus.inProgress"
                    v-if="hasAccessToRetry"
                    @click="showRetryDialog" class="ml-0 mr-2 mt-2">
                    {{ (stream.RetryActionName || 'Retry') }}
                </v-btn>

                <v-btn :disabled="!analyzeAllowed"
                    :loading="dataLoadStatus.inProgress"
                    v-if="showManualAnalysis"
                    @click="showAnalyzeDialog" class="ml-0 mr-2 mt-2">
                    {{ ( stream.AnalyzeActionName || 'Analyze') }}
                </v-btn>

                <code v-if="quickStatus" class="quickstatus">{{ quickStatus }}</code>
            </div>

            <!-- ACTION PARAMETERS -->
            <div v-if="stream.Actions && stream.Actions.length > 0 && hasAccessToExecuteItemActions">
                <h2 class="mt-4">Actions</h2>
                <div v-for="(action, aIndex) in stream.Actions"
                    :key="`action-${aIndex}-${action.Id}`"
                    class="data-repeater-item--block mt-2">
                    <data-repeater-item-action-component 
                        :item="item"
                        :stream="stream"
                        :config="config"
                        :action="action"
                        @change="onItemUpdatedFromAction" />
                </div>
            </div>
        </div>

        <!-- DIALOGS -->
        <v-dialog v-model="confirmRetryDialogVisible"
            @keydown.esc="confirmRetryDialogVisible = false"
            max-width="480"
            content-class="confirm-dialog"
            :persistent="dataLoadStatus.inProgress">
            <v-card>
                <v-card-title class="headline">Confirm {{ (stream.RetryActionName || 'Retry') }}</v-card-title>
                <v-card-text>
                    Are you sure you want to {{ (stream.RetryActionName || 'Retry') }}?
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="confirmRetryDialogVisible = false">Cancel</v-btn>
                    <v-btn color="primary"
                        :disabled="!retryAllowed"
                        :loading="dataLoadStatus.inProgress"
                        @click="retry()">{{ (stream.RetryActionName || 'Retry') }}</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <v-dialog v-model="confirmRunAnalysisDialogVisible"
            @keydown.esc="confirmRunAnalysisDialogVisible = false"
            max-width="480"
            content-class="confirm-dialog"
            :persistent="dataLoadStatus.inProgress">
            <v-card>
                <v-card-title class="headline">Confirm run analysis</v-card-title>
                <v-card-text>
                    Are you sure you want to run analysis?
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="confirmRunAnalysisDialogVisible = false">Cancel</v-btn>
                    <v-btn color="primary"
                        :disabled="!analyzeAllowed"
                        :loading="dataLoadStatus.inProgress"
                        @click="analyze()">Run analysis</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
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
import ModuleConfig from "@models/Common/ModuleConfig";
import DataRepeaterItemActionComponent from '@components/modules/DataRepeater/DataRepeaterItemActionComponent.vue';
import { HCDataRepeaterRetryResult } from "@generated/Models/Core/HCDataRepeaterRetryResult";
import EditorComponent from "@components/Common/EditorComponent.vue";
import { HCDataRepeaterStreamItemDetailsViewModel } from "@generated/Models/Core/HCDataRepeaterStreamItemDetailsViewModel";
import DateUtils from "@util/DateUtils";
import ModuleOptions from "@models/Common/ModuleOptions";
import { HCDataRepeaterItemAnalysisResult } from "@generated/Models/Core/HCDataRepeaterItemAnalysisResult";
import { StoreUtil } from "@util/StoreUtil";

@Options({
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
    options!: ModuleOptions<any>;

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
    analyzeResult: HCDataRepeaterItemAnalysisResult | null = null;
    itemNotFound: boolean = false;
    confirmRunAnalysisDialogVisible: boolean = false;
    confirmRetryDialogVisible: boolean = false;
    quickStatus: string = '';

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
        return StoreUtil.store.state.globalOptions;
    }

    get retryAllowed(): boolean {
        return !this.dataLoadStatus.inProgress
            && !!this.item
            && this.item.AllowRetry
            && this.hasAccessToRetry;
    }

    get analyzeAllowed(): boolean {
        return !this.dataLoadStatus.inProgress
            && !!this.item
            && this.hasAccessToManualAnalysis;
    }

    get showManualAnalysis(): boolean {
        return this.hasAccessToManualAnalysis && this.stream.ManualAnalyzeEnabled;
    }

    get hasAccessToRetry(): boolean {
        return this.options.AccessOptions.indexOf("RetryItems") != -1;
    }

    get hasAccessToManualAnalysis(): boolean {
        return this.options.AccessOptions.indexOf("ManualAnalysis") != -1;
    }

    get hasAccessToExecuteItemActions(): boolean {
        return this.options.AccessOptions.indexOf("ExecuteItemActions") != -1;
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
        this.onItemChanged();
    }

    showAnalyzeDialog(): void {
        this.confirmRunAnalysisDialogVisible = true;
    }

    analyze(): void {
        if (!this.item) return;
        this.service.AnalyseItem({
            StreamId: this.stream.Id,
            ItemId: this.item.Id
        }, this.dataLoadStatus, {
            onSuccess: (data) => this.onAnalyzeResult(data),
            onDone: () => {
                this.confirmRunAnalysisDialogVisible = false;
            }
        });
    }

    onAnalyzeResult(data: HCDataRepeaterResultWithItem<HCDataRepeaterItemAnalysisResult> | null): void {
        if (!this.item) return;
        this.analyzeResult = data?.Data || null;
        this.quickStatus = data?.Data?.Message || '';

        if (data?.Item)
        {
            this.item = data.Item;
            this.notifyItemUpdated(data.Item);
        }
    }

    showRetryDialog(): void {
        this.confirmRetryDialogVisible = true;
    }

    retry(): void {
        if (!this.item) return;
        this.service.RetryItem({
            StreamId: this.stream.Id,
            ItemId: this.item.Id,
            SerializedDataOverride: this.item.SerializedData
        }, this.dataLoadStatus, {
            onSuccess: (data) => this.onRetryResult(data),
            onDone: () => {
                this.confirmRetryDialogVisible = false;
            }
        });
    }

    onRetryResult(data: HCDataRepeaterResultWithItem<HCDataRepeaterRetryResult> | null): void {
        if (!this.item) return;
        this.retryResult = data?.Data || null;
        this.quickStatus = data?.Data?.Message || '';
        
        if (data?.Item)
        {
            this.item = data.Item;
            this.notifyItemUpdated(data.Item);
        }
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
        this.item.SerializedData = this.item.FirstSerializedData;
    }

    notifyItemUpdated(item: HCDataRepeaterStreamItemViewModel): void {
        this.$emit('change', item);
    }

    formatDate(date: Date): string {
        return DateUtils.FormatDate(date, "dddd dd. MMMM yyyy HH:mm:ss");
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch("item")
    onItemChanged(): void {
        if (!this.item) return;
    }

    onItemUpdatedFromAction(item: HCDataRepeaterStreamItemViewModel): void {
        this.item = item;
        this.notifyItemUpdated(item);
    }
}
</script>

<style scoped lang="scss">
.editor {
  width: 100%;
  height: 400px;
  border: 1px solid #949494;
}
.data-repeater-item {
    /* &--metadata
    {

    } */
    &--tags {
        display: flex;
        flex-wrap: wrap;
        align-items: baseline;
    }
    &--tag {
        padding: 3px 6px;
        background-color: #dcdcdc;
        border-radius: 3px;
        margin-left: 5px;
        font-size: 12px;
        margin-bottom: 1px;
    }
    &--logentry {
        font-size: 12px;
    }
    &--detail-blocks {
        display: flex;
        flex-wrap: wrap;
        align-content: space-between;
        margin: -10px;

        .data-repeater-item--block {
            margin: 10px;
        }
    }
    &--block {
        border: 2px solid #d6d6d6;
        padding: 10px;
        flex: 1;
    }
}
.quickstatus {
    display: block;
    box-shadow: none;

    &:not(.failed) {
        color: #333;
    }
}
code {
    width: 100%;
    overflow-x: auto;
}
.repeater-error:before {
    content: "";
}
.not-allowed-text {
    font-size: 12px;
    font-weight: bold;
    padding: 5px;
}
</style>