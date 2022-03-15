<!-- src/components/modules/SecureFileDownload/SecureFileDownloadPageComponent.vue -->
<template>
    <div>
        <content-component class="pl-0">
            <v-container fluid fill-height class="content-root">
            <v-layout>
            <v-flex>
            <v-container>
                <h1 class="mb-1">Downloads</h1>

                <!-- LOAD PROGRESS -->
                <progress-linear-component
                    v-if="loadStatus.inProgress"
                    indeterminate color="green"></progress-linear-component>

                <!-- DATA LOAD ERROR -->
                <alert-component :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
                </alert-component>

                <btn-component
                    v-if="canCreateNewDownloads"
                    :disabled="!allowDownloadChanges"
                    @click="onAddNewDownloadClicked"
                    class="mb-3">
                    <icon-component size="20px" class="mr-2">add</icon-component>
                    Add new
                </btn-component>

                <block-component
                    v-for="(download, cindex) in downloads"
                    :key="`download-${cindex}-${download.Id}`"
                    class="download-list-item"
                    >
                    <div>
                        <div class="download-list-item--inner">
                            <div class="download-list-item--rule"
                                @click="showDownload(download)">
                                <icon-component>description</icon-component>
                                {{ download.FileName }}
                            </div>
                            
                            <tooltip-component bottom v-if="download.Password != null">
                                <template v-slot:activator="{ on }">
                                    <icon-component style="cursor: help;" v-on="on">lock</icon-component>
                                </template>
                                <span>This download is protected by a password</span>
                            </tooltip-component>

                            <tooltip-component bottom v-if="getDownloadWarning(download) != null">
                                <template v-slot:activator="{ on }">
                                    <icon-component style="cursor: help;" color="warning" v-on="on">warning</icon-component>
                                </template>
                                <span>{{getDownloadWarning(download)}}</span>
                            </tooltip-component>

                            <tooltip-component bottom v-if="downloadIsOutsideLimit(download)">
                                <template v-slot:activator="{ on }">
                                    <icon-component v-on="on" style="cursor: help;">timer_off</icon-component>
                                </template>
                                <span>This downloads' limits has been reached</span>
                            </tooltip-component>

                            <tooltip-component bottom>
                                <template v-slot:activator="{ on }">
                                    <icon-component style="cursor: help;" v-on="on">person</icon-component>
                                    <code style="color: var(--v-primary-base); cursor: help;" v-on="on">{{ download.LastModifiedByUsername }}</code>
                                </template>
                                <span>Last modified by '{{ download.LastModifiedByUsername }}'</span>
                            </tooltip-component>
                        </div>
                            
                        <div class="download-link">
                            Download link: <a :href="getAbsoluteDownloadUrl(download)">{{ getAbsoluteDownloadUrl(download) }}</a>
                        </div>
                    </div>
                </block-component>

            </v-container>
            </v-flex>
            </v-layout>
            </v-container>
            
            <dialog-component v-model:value="downloadDialogVisible"
                scrollable
                persistent
                max-width="1200"
                content-class="current-download-dialog">
                <card-component v-if="currentDownload != null" style="background-color: #f4f4f4">
                    <toolbar-component class="elevation-0">
                        <v-toolbar-title class="current-download-dialog__title">{{ currentDialogTitle }}</v-toolbar-title>
                        <v-spacer></v-spacer>
                        <btn-component icon
                            @click="hideCurrentDownload()"
                            :disabled="serverInteractionInProgress">
                            <icon-component>close</icon-component>
                        </btn-component>
                    </toolbar-component>

                    <v-divider></v-divider>
                    
                    <v-card-text>
                        <edit-download-component
                            :module-id="config.Id"
                            :download="currentDownload"
                            :storage-infos="options.Options.StorageInfos"
                            :readonly="!allowDownloadChanges"
                            v-on:downloadDeleted="onDownloadDeleted"
                            v-on:downloadSaved="onDownloadSaved"
                            v-on:serverInteractionInProgress="setServerInteractionInProgress"
                            ref="currentDownloadComponent"
                            />
                    </v-card-text>
                    <v-divider></v-divider>
                    <v-card-actions >
                        <v-spacer></v-spacer>
                        <btn-component color="error" flat
                            v-if="showDeleteDownload"
                            :disabled="serverInteractionInProgress"
                            @click="$refs.currentDownloadComponent.tryDeleteDownload()">Delete</btn-component>
                        <btn-component color="success"
                            v-if="showSaveDownload"
                            :disabled="serverInteractionInProgress"
                            @click="$refs.currentDownloadComponent.saveDownload()">Save</btn-component>
                    </v-card-actions>
                </card-component>
            </dialog-component>
        </content-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { EntryState } from '@models/modules/RequestLog/EntryState';
import DateUtils from '@util/DateUtils';
import LinqUtils from '@util/LinqUtils';
import KeyArray from '@util/models/KeyArray';
import KeyValuePair from '@models/Common/KeyValuePair';
import '@lazy-copilot/datetimepicker/dist/datetimepicker.css'
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";
import FilterInputComponent from '@components/Common/FilterInputComponent.vue';
import DataTableComponent from '@components/Common/DataTableComponent.vue';
import { DataTableGroup } from '@components/Common/DataTableComponent.vue.models';
import SimpleDateTimeComponent from '@components/Common/SimpleDateTimeComponent.vue';
import FilterableListComponent from '@components/Common/FilterableListComponent.vue';
import { FilterableListItem } from '@components/Common/FilterableListComponent.vue.models';
import EditDownloadComponent from '@components/modules/SecureFileDownload/EditDownloadComponent.vue';
import IdUtils from '@util/IdUtils';
import SecureFileDownloadUtils from '@util/SecureFileDownload/SecureFileDownloadUtils';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import SecureFileDownloadService from '@services/SecureFileDownloadService';
import { SecureFileDownloadsViewModel, SecureFileDownloadDefinition, SecureFileDownloadFrontendOptionsModel } from "@models/modules/SecureFileDownload/Models";
import { StoreUtil } from "@util/StoreUtil";

@Options({
    components: {
        SimpleDateTimeComponent,
        EditDownloadComponent,
        BlockComponent
    }
})
export default class SecureFileDownloadPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<SecureFileDownloadFrontendOptionsModel>;

    service: SecureFileDownloadService = new SecureFileDownloadService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);

    // UI STATE
    loadStatus: FetchStatus = new FetchStatus();
    serverInteractionInProgress: boolean = false;

    datax: Array<SecureFileDownloadDefinition> = [];
    currentDownload: SecureFileDownloadDefinition | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.loadData();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get canViewDownload(): boolean {
        return this.hasAccess('ViewDefinitions');
    }

    get canCreateNewDownloads(): boolean {
        return this.hasAccess('CreateDefinition');
    }

    get canEditDownloads(): boolean {
        return this.hasAccess('EditDefinition');
    }

    get canDeleteDownload(): boolean {
        return this.hasAccess('DeleteDefinition');
    }
    
    get showDeleteDownload(): boolean
    {
        return this.currentDownload != null
            && this.currentDownload.Id != null
            && this.canDeleteDownload;
    }
    
    get showSaveDownload(): boolean
    {
        var defaultId = this.getDefaultNewDownloadData().Id;

        if (this.currentDownload == null) return false;
        else if (this.currentDownload.Id == defaultId && !this.canCreateNewDownloads) return false;
        else if (this.currentDownload.Id != defaultId && !this.canEditDownloads) return false;
        else return true;
    }

    get allowDownloadChanges(): boolean
    {
        return !this.serverInteractionInProgress;
    };

    get currentDialogTitle(): string
    {
        return (this.currentDownload != null && this.currentDownload.Id != null)
            ? 'Edit download'
            : 'Create new download';
    }

    get downloadDialogVisible(): boolean
    {
        return this.currentDownload != null;
    }

    get downloads(): Array<SecureFileDownloadDefinition>
    {
        let downloads = (this.datax == null) ? [] : this.datax;
        return downloads;
    };

    ////////////////
    //  METHODS  //
    //////////////
    updateUrl(): void {
        let routeParams: any = {};
        if (this.currentDownload != null && this.currentDownload.Id != null)
        {
            routeParams['id'] = this.currentDownload.Id;
        }

        this.$router.push({ name: this.config.Id, params: routeParams })
    }

    updateSelectionFromUrl(): void {
        const idFromHash = Array.isArray(this.$route.params.id) ? this.$route.params.id[0] : this.$route.params.id;
        
        if (idFromHash) {
            let downloadFromUrl = this.downloads.filter(x => x.Id != null && x.Id == idFromHash)[0];
            if (downloadFromUrl != null)
            {
                this.showDownload(downloadFromUrl, false);
            }
        }
    }

    getAbsoluteDownloadUrl(download: SecureFileDownloadDefinition): string {
        return SecureFileDownloadUtils.getAbsoluteDownloadUrl(this.globalOptions.EndpointBase, download.UrlSegmentText);
    }

    loadData(): void {
        if (this.canViewDownload)
        {
            this.service.GetDownloads(this.loadStatus, { onSuccess: (data) => this.onDataRetrieved(data) });
        }
    }

    onDataRetrieved(data: SecureFileDownloadsViewModel): void {
        this.datax = data.Definitions.map(x => {
            SecureFileDownloadUtils.postProcessDownload(x);
            return x;
        });

        this.updateSelectionFromUrl();
    }

    hasAccess(option: string): boolean {
        return this.options.AccessOptions.indexOf(option) != -1;
    }

    onDownloadSaved(download: SecureFileDownloadDefinition): void {
        if (this.datax == null)
        {
            return;
        }
        SecureFileDownloadUtils.postProcessDownload(download);

        const position = this.datax.findIndex(x => x.Id == download.Id);

        if (position == -1)
        {
            this.datax.push(download);
        }
        else {
            this.data[position] = download;
        }

        if (download.FileId)
        {
            this.hideCurrentDownload();
        }
    }

    onDownloadDeleted(download: SecureFileDownloadDefinition): void {
        if (this.datax == null)
        {
            return;
        }

        this.datax = this.datax.filter(x => x.Id != download.Id);
        this.hideCurrentDownload();
    }

    showDownload(download: SecureFileDownloadDefinition, updateRoute: boolean = true): void {
        this.currentDownload = download;

        if (updateRoute)
        {
            this.updateUrl();
        }
    }

    hideCurrentDownload(): void {
        this.currentDownload = null;
        this.updateUrl();
    }
    
    setServerInteractionInProgress(inProgress: boolean): void
    {
        this.serverInteractionInProgress = inProgress;
    }

    getDownloadWarning(download: SecureFileDownloadDefinition): string | null
    {
        // if (download.NotifierDownloads.length == 0)
        // {
        //     return 'Download has no effect because it has zero notifiers downloadured.';
        // }

        return null;
    }

    downloadIsOutsideLimit(download: SecureFileDownloadDefinition): boolean
    {
        if (download.ExpiresAt != null && download.ExpiresAt.getTime() < new Date().getTime())
        {
            return true;
        }
        else if (download.DownloadCountLimit != null
            && download.DownloadCount >= download.DownloadCountLimit)
        {
            return true;
        }

        return false;
    }

    getDefaultNewDownloadData(): SecureFileDownloadDefinition {
        return {
            Id: '00000000-0000-0000-0000-000000000000',
            CreatedAt: new Date(),
            CreatedByUsername: '',
            CreatedByUserId: '',
            LastModifiedAt: new Date(),
            LastModifiedByUsername: '',
            LastModifiedByUserId: '',
            FileName: '',
            StorageId: '',
            FileId: '',
            UrlSegmentText: '',
            Password: null,
            DownloadCount: 0,
            LastDownloadedAt: null,
            DownloadCountLimit: null,
            ExpiresAt: null,
            Note: null,
            IsExpired: false,
            HasUploadedFile: false,
            OriginalFileName: ''
        };
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////    
    onAddNewDownloadClicked(): void {
        if (this.datax == null)
        {
            return;
        }

        let download = this.getDefaultNewDownloadData();
        this.showDownload(download);
    }
}
</script>

<style scoped lang="scss">
.current-download-dialog__title {
    font-size: 34px;
    font-weight: 600;
}
.download-link {
    margin-left: 28px;
}
.download-list-item {
    margin-bottom: 20px;
    
    .download-list-item--inner {
        display: flex;
        align-items: center;
        flex-direction: row;
        flex-wrap: nowrap;

        .download-list-item--rule {
            flex: 1;
            cursor: pointer;
            font-size: 16px;
            margin-right: 20px;
            
            .download-list-item--operator {
                font-weight: 600;
            }
            .download-list-item--condition {
                color: var(--v-primary-base);
            }
            .download-list-item--action {
                color: var(--v-secondary-base);
            }
            /* .download-list-item--condition,
            .download-list-item--action {
                font-weight: 600;
            } */
        }
    }
}
</style>