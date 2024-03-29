<!-- src/components/modules/DataRepeater/DataRepeaterItemComponent.vue -->
<template>
    <div>
        <!-- LOAD PROGRESS -->
        <progress-linear-component 
            v-if="dataLoadStatus.inProgress && item == null"
            indeterminate color="success"></progress-linear-component>

        <!-- DATA LOAD ERROR -->
        <alert-component :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
        {{ dataLoadStatus.errorMessage }}
        </alert-component>

        <!-- NOT FOUND -->
        <alert-component :value="itemNotFound" v-if="itemNotFound" type="info">
        No item with the given id was found.
        </alert-component>
        
        <btn-component @click="$emit('close')" v-if="itemNotFound">Close</btn-component>
        
        <div v-if="item" class="data-repeater-item">
            <div class="header-layout">
                <h1 class="header-layout__title">{{ stream.ItemIdName }}: {{ item.ItemId }}</h1>
                <div class="header-layout__actions">
                    <btn-component @click="$emit('close')">Close</btn-component>
                    <btn-component @click="loadData">
                        <icon-component size="20px" class="mr-2">refresh</icon-component>Refresh
                    </btn-component>
                </div>
            </div>

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
                    v-model:value="item.SerializedData"
                    :allowFullscreen="true"
                    ref="editor" />

                <div v-if="item.SerializedData && item.SerializedData != this.item.FirstSerializedData && hasAccessToRetry">
                    <tooltip-component :tooltip="`Reverts the content above to the first data that was stored on ${formatDate(item.InsertedAt)}.`">
                        <a href="#" @click.prevent="restoreOriginalData" class="right" style="cursor: help;">Revert to original data</a>
                    </tooltip-component>
                </div>

                <div v-if="!hasAccessToRetry" class="not-allowed-text">
                    You do not have access to retry this data.
                </div>

                <btn-component :disabled="!retryAllowed"
                    :loading="dataLoadStatus.inProgress"
                    v-if="hasAccessToRetry"
                    @click="showRetryDialog" class="ml-0 mr-2 mt-2"
                    color="primary">
                    {{ (stream.RetryActionName || 'Retry') }}
                </btn-component>

                <btn-component :disabled="!analyzeAllowed"
                    :loading="dataLoadStatus.inProgress"
                    v-if="showManualAnalysis"
                    @click="showAnalyzeDialog" class="ml-0 mr-2 mt-2">
                    {{ ( stream.AnalyzeActionName || 'Analyze') }}
                </btn-component>

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
        <dialog-component v-model:value="confirmRetryDialogVisible"
            max-width="480"
            :persistent="dataLoadStatus.inProgress">
            <template #header>Confirm {{ (stream.RetryActionName || 'Retry') }}</template>
            <template #footer>
                <btn-component color="primary"
                    :disabled="!retryAllowed"
                    :loading="dataLoadStatus.inProgress"
                    @click="retry()">{{ (stream.RetryActionName || 'Retry') }}</btn-component>
                <btn-component color="secondary"
                    :disabled="dataLoadStatus.inProgress"
                    :loading="dataLoadStatus.inProgress"
                    @click="confirmRetryDialogVisible = false">Cancel</btn-component>
            </template>
            <div>
                Are you sure you want to {{ (stream.RetryActionName || 'Retry') }}?
            </div>
        </dialog-component>
        <dialog-component v-model:value="confirmRunAnalysisDialogVisible"
            max-width="480"
            :persistent="dataLoadStatus.inProgress">
            <template #header>Confirm run analysis</template>
            <template #footer>
                <btn-component color="primary"
                    :disabled="!analyzeAllowed"
                    :loading="dataLoadStatus.inProgress"
                    @click="analyze()">Run analysis</btn-component>
                <btn-component color="secondary"
                    :disabled="dataLoadStatus.inProgress"
                    :loading="dataLoadStatus.inProgress"
                    @click="confirmRunAnalysisDialogVisible = false">Cancel</btn-component>
            </template>
            <div>
                Are you sure you want to run analysis?
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
import ModuleConfig from "@models/Common/ModuleConfig";
import DataRepeaterItemActionComponent from '@components/modules/DataRepeater/DataRepeaterItemActionComponent.vue';
import { TKDataRepeaterRetryResult } from "@generated/Models/Core/TKDataRepeaterRetryResult";
import EditorComponent from "@components/Common/EditorComponent.vue";
import { TKDataRepeaterStreamItemDetailsViewModel } from "@generated/Models/Core/TKDataRepeaterStreamItemDetailsViewModel";
import DateUtils from "@util/DateUtils";
import ModuleOptions from "@models/Common/ModuleOptions";
import { TKDataRepeaterItemAnalysisResult } from "@generated/Models/Core/TKDataRepeaterItemAnalysisResult";
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
    stream!: TKDataRepeaterStreamViewModel;

    @Prop({ required: true })
    itemId!: string;
    
    // Service
    service: DataRepeaterService = new DataRepeaterService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    details: TKDataRepeaterStreamItemDetailsViewModel | null = null;
    item: TKDataRepeaterStreamItemViewModel | null = null;
    retryResult: TKDataRepeaterRetryResult | null = null;
    analyzeResult: TKDataRepeaterItemAnalysisResult | null = null;
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

    onItemDetailsRetrieved(data: TKDataRepeaterStreamItemDetailsViewModel | null): void {
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

    onAnalyzeResult(data: TKDataRepeaterResultWithItem<TKDataRepeaterItemAnalysisResult> | null): void {
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

    onRetryResult(data: TKDataRepeaterResultWithItem<TKDataRepeaterRetryResult> | null): void {
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

    notifyItemUpdated(item: TKDataRepeaterStreamItemViewModel): void {
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

    onItemUpdatedFromAction(item: TKDataRepeaterStreamItemViewModel): void {
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
        /* color: #333; */
    }
}
code {
    width: calc(100% - 16px);
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