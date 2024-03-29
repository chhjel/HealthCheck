<!-- src/components/modules/SecureFileDownload/SecureFileDownloadPageComponent.vue -->
<template>
    <div>
        <div class="content-root">
            <h1 class="mb-1">Downloads</h1>

            <!-- LOAD PROGRESS -->
            <progress-linear-component
                v-if="loadStatus.inProgress"
                indeterminate color="success"></progress-linear-component>

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
                @click="showDownload(download)"
                >
                <div>
                    <div class="download-list-item--inner">
                        <div class="download-list-item--rule">
                            <icon-component class="download-list-item--rule-icon">description</icon-component>
                            {{ download.FileName }}
                        </div>
                        
                        <tooltip-component tooltip="This download is protected by a password"
                            v-if="download.Password != null" >
                            <icon-component help>lock</icon-component>
                        </tooltip-component>

                        <tooltip-component :tooltip="getDownloadWarning(download)" 
                            v-if="getDownloadWarning(download) != null">
                            <icon-component help color="warning">warning</icon-component>
                        </tooltip-component>

                        <tooltip-component tooltip="This downloads' limits has been reached"
                            v-if="downloadIsOutsideLimit(download)">
                            <icon-component help >timer_off</icon-component>
                        </tooltip-component>

                        <tooltip-component :tooltip="`Last modified by '${download.LastModifiedByUsername}'`" bottom>
                            <icon-component help>person</icon-component>
                            <code style="color: var(--color--primary-base); cursor: help;">{{ download.LastModifiedByUsername }}</code>
                        </tooltip-component>
                    </div>
                        
                    <div class="download-link">
                        Download link: <a :href="getAbsoluteDownloadUrl(download)" @click.stop="" target="_blank">{{ getAbsoluteDownloadUrl(download) }}</a>
                    </div>
                </div>
            </block-component>

        </div>
        
        <dialog-component v-model:value="downloadDialogVisible" persistent max-width="1200" @close="hideCurrentDownload">
            <template #header>{{ currentDialogTitle }}</template>
            <template #footer>
                <btn-component color="primary"
                    v-if="showSaveDownload"
                    :disabled="serverInteractionInProgress"
                    @click="$refs.currentDownloadComponent.saveDownload()">Save</btn-component>
                <btn-component color="error"
                    v-if="showDeleteDownload"
                    :disabled="serverInteractionInProgress"
                    @click="$refs.currentDownloadComponent.tryDeleteDownload()">Delete</btn-component>
                <btn-component color="secondary" @click="hideCurrentDownload">Cancel</btn-component>
            </template>

            <div v-if="currentDownload != null">
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
            </div>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import SimpleDateTimeComponent from '@components/Common/SimpleDateTimeComponent.vue';
import EditDownloadComponent from '@components/modules/SecureFileDownload/EditDownloadComponent.vue';
import SecureFileDownloadUtils from '@util/SecureFileDownload/SecureFileDownloadUtils';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import { FetchStatus } from '@services/abstractions/TKServiceBase';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import SecureFileDownloadService from '@services/SecureFileDownloadService';
import { SecureFileDownloadsViewModel, SecureFileDownloadDefinition, SecureFileDownloadFrontendOptionsModel } from "@models/modules/SecureFileDownload/Models";
import { StoreUtil } from "@util/StoreUtil";
import StringUtils from "@util/StringUtils";

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
    downloadDialogVisible: boolean = false;

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
        const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.id) || null;
        
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
            this.datax[position] = download;
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
        this.downloadDialogVisible = download != null;

        if (updateRoute)
        {
            this.updateUrl();
        }
    }

    hideCurrentDownload(): void {
        this.currentDownload = null;
        this.downloadDialogVisible = false;
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
    cursor: pointer;
    
    .download-list-item--inner {
        display: flex;
        align-items: center;
        flex-direction: row;
        flex-wrap: nowrap;

        .download-list-item--rule {
            flex: 1;
            font-size: 16px;
            margin-right: 20px;
            display: flex;
            align-items: center;

            .download-list-item--operator {
                font-weight: 600;
            }
            .download-list-item--condition {
                color: var(--color--primary-base);
            }
            .download-list-item--action {
                color: var(--color--secondary-base);
            }
            /* .download-list-item--condition,
            .download-list-item--action {
                font-weight: 600;
            } */
        }

        .download-list-item--rule-icon {
            margin-right: 5px;
        }
    }
}
</style>