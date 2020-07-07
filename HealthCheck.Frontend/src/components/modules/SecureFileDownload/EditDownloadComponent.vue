<!-- src/components/modules/SecureFileDownload/EditDownloadComponent.vue -->
<template>
    <div class="root">
        <v-alert
            :value="serverInteractionError != null && serverInteractionError.length > 0"
            type="error" >
            {{ serverInteractionError }}
        </v-alert>

        <!-- ###### FILE ###### -->
        <block-component class="mb-5">
            <div class="mb-4" style="font-weight: 600"
                v-if="absoluteDownloadUrl != null"
                >Download link: <a :href="absoluteDownloadUrl">{{ absoluteDownloadUrl }}</a> </div>

            <input-component
                class="mt-2"
                v-model="internalDownload.FileName"
                :disabled="!allowChanges"
                name="Filename"
                description="Name of the file when downloading."
                type="text"
                :error="validateFileName"
                />
                
            <input-component
                class="mt-2"
                v-model="internalDownload.UrlSegmentText"
                :disabled="!allowChanges"
                name="Url segment text"
                description="Text in the url after '/downloads/'."
                type="text"
                :error="validateUrlSegmentText"
                />
            
            <select-component
                class="mt-2"
                v-model="internalDownload.StorageId"
                :items="storageOptions"
                :disabled="!allowChanges"
                name="Where to get file from"
                @input="onStorageChanged()"
                />
            
            <select-component
                v-if="hasFileIdOptions"
                class="mt-2"
                v-model="internalDownload.FileId"
                :error="validateFileId"
                :items="fileIdOptions"
                :disabled="!allowChanges"
                :loading="fileIdOptionsLoadStatus.inProgress"
                :name="getStorageFileIdLabel(internalDownload.StorageId)"
                :description="getStorageFileIdInfo(internalDownload.StorageId)"
                show-description-on-start="true"
                />
            
            <input-component
                v-if="!hasFileIdOptions"
                class="mt-2"
                v-model="internalDownload.FileId"
                :error="validateFileId"
                :disabled="!allowChanges"
                :name="getStorageFileIdLabel(internalDownload.StorageId)"
                :description="getStorageFileIdInfo(internalDownload.StorageId)"
                show-description-on-start="true"
                type="text"
                />
            
            <input-component
                class="mt-2"
                v-model="internalDownload.Note"
                :disabled="!allowChanges"
                name="Note"
                description="An optional note that will be displayed on the download page."
                show-description-on-start="true"
                type="text"
                ui-hints="TextArea"
                />
                
        </block-component>

        <!-- ###### LIMITS ###### -->
        <block-component class="mb-5" title="Limits &amp; password">
            <input-component
                class="mt-2"
                v-model="internalDownload.DownloadCountLimit"
                :disabled="!allowChanges"
                name="Number of allowed downloads"
                :description="`Total number of times the file can be downloaded. It has been downloaded ${internalDownload.DownloadCount} times so far.`"
                type="number"
                />
            
            <simple-date-time-component
                v-model="internalDownload.ExpiresAt"
                :readonly="!allowChanges"
                name="Expires at"
                description="After this time, the file will not be available for download."
                />
                
            <input-component
                class="mt-2"
                v-model="internalDownload.Password"
                :disabled="!allowChanges"
                name="Password (will always be visible here)"
                description="If a password is set, it must be entered to download the file."
                type="text"
                :error="validatePassword"
                />
        </block-component>

        <block-component class="mb-5">
            <ul>
                <li class="metadata">
                    Downloaded {{ internalDownload.DownloadCount }} times
                </li>
                <li class="metadata"
                    v-if="internalDownload.LastDownloadedAt != null">
                    Last downloaded at {{ formatDate(internalDownload.LastDownloadedAt) }}
                </li>
                <li class="metadata"
                    v-if="internalDownload.LastModifiedByUsername != null && internalDownload.LastModifiedByUsername.length > 0">
                    Last changed at {{ formatDate(internalDownload.LastModifiedAt) }} by '{{ internalDownload.LastModifiedByUsername }}'
                </li>
            </ul>
        </block-component>

        <!-- ###### DIALOGS ###### -->
        <v-dialog v-model="deleteDialogVisible"
            @keydown.esc="deleteDialogVisible = false"
            max-width="290"
            content-class="confirm-dialog">
            <v-card>
                <v-card-title class="headline">Confirm deletion</v-card-title>
                <v-card-text>
                    Are you sure you want to delete this download?
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary" @click="deleteDialogVisible = false">Cancel</v-btn>
                    <v-btn color="error" @click="deleteDownload()">Delete it</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <v-dialog v-model="showSaveError"
            @keydown.esc="showSaveError = false"
            max-width="400"
            content-class="confirm-dialog">
            <v-card>
                <v-card-title class="headline">Save error</v-card-title>
                <v-card-text style="overflow: auto;">
                    {{ saveError }}
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary" @click="showSaveError = false">Close</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import SimpleDateTimeComponent from  '../../Common/SimpleDateTimeComponent.vue';
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import DateUtils from  '../../../util/DateUtils';
import IdUtils from  '../../../util/IdUtils';
import BlockComponent from  '../../Common/Basic/BlockComponent.vue';
import InputComponent from  '../../Common/Basic/InputComponent.vue';
import SelectComponent from  '../../Common/Basic/SelectComponent.vue';
import { SecureFileDownloadDefinition, SecureFileDownloadSaveViewModel, SecureFileDownloadStorageInfo } from "../../../models/modules/SecureFileDownload/Models";
import SecureFileDownloadUtils from "../../../util/SecureFileDownload/SecureFileDownloadUtils";
import SecureFileDownloadService from "../../../services/SecureFileDownloadService";
import { FetchStatus } from "../../../services/abstractions/HCServiceBase";

@Component({
    components: {
        SimpleDateTimeComponent,
        BlockComponent,
        InputComponent,
        SelectComponent
    }
})
export default class EditDownloadComponent extends Vue {
    @Prop({ required: true })
    moduleId!: string;

    @Prop({ required: true })
    download!: SecureFileDownloadDefinition;

    @Prop({ required: true })
    storageInfos!: Array<SecureFileDownloadStorageInfo>;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    // @ts-ignore
    internalDownload: SecureFileDownloadDefinition = null;
    service: SecureFileDownloadService = new SecureFileDownloadService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.moduleId);
    deleteDialogVisible: boolean = false;
    isSaving: boolean = false;
    isDeleting: boolean = false;
    serverInteractionError: string | null = null;
    serverInteractionInProgress: boolean = false;
    saveError: string = '';
    showSaveError: boolean = false;
    fileIdOptions: Array<string> = [];
    fileIdOptionsLoadStatus: FetchStatus = new FetchStatus();

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
        this.onDownloadChanged();
    }

    mounted(): void {
        this.updateFileIdChoices();
    }

    @Watch("download")
    onDownloadChanged(): void {
        let intDownload = JSON.parse(JSON.stringify(this.download));
        SecureFileDownloadUtils.postProcessDownload(intDownload);
        this.internalDownload = intDownload;
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }

    get hasFileIdOptions(): boolean {
        return this.fileIdOptions.length > 0;
    }
    
    get allowChanges(): boolean {
        return !this.readonly && !this.serverInteractionInProgress;
    }

    get absoluteDownloadUrl(): string {
        return SecureFileDownloadUtils.getAbsoluteDownloadUrl(this.internalDownload.UrlSegmentText);
    }

    get storageOptions(): any {
        return this.storageInfos.map(x => {
                return { text: x.StorageName, value: x.StorageId }
            });
    }
    
    get validateFileId(): string | null {
        if (this.internalDownload.FileId == null || this.internalDownload.FileId.length == 0)
        {
            const label = this.getStorageFileIdLabel(this.internalDownload.StorageId);
            return `${label} is required.`;
        }
        return null;
    }
    
    get validateFileName(): string | null {
        return (this.internalDownload.FileName == null || this.internalDownload.FileName.length == 0)
            ? 'A filename is required.'
            : null;
    }
    
    get validateUrlSegmentText(): string | null {
        return (this.internalDownload.UrlSegmentText == null || this.internalDownload.UrlSegmentText.length == 0)
            ? 'An url segment text is required.'
            : null;
    }
    
    get validatePassword(): string | null {
        if (this.internalDownload.Password == null || this.internalDownload.Password.length == 0)
        {
            return null;
        }

        if (this.internalDownload.Password.length < 6) {
            return "Password must be at least 6 characters long.";
        }
        
        return null;
    }

    ////////////////
    //  METHODS  //
    //////////////
    setServerInteractionInProgress(inProgress: boolean, err: string | null = null): void
    {
        this.serverInteractionError = err;
        this.serverInteractionInProgress = inProgress;
        this.$emit('serverInteractionInProgress', inProgress);
    }

    public saveDownload(): void {
        this.isSaving = true;
        this.setServerInteractionInProgress(true);

        // Need timeout to first apply any changes from currently selected field.
        setTimeout(() => {
            this.saveDownloadInternal();
        }, 50);
    }

    saveDownloadInternal(): void {
        this.isSaving = true;
        this.setServerInteractionInProgress(true);

        this.service.SaveDefinition(this.internalDownload, null, {
            onSuccess: (data) => this.onDownloadSaved(data),
            onError: (message) => this.setServerInteractionInProgress(false, message),
            onDone: () => { this.isSaving = false }
        });
    }

    onDownloadSaved(result: SecureFileDownloadSaveViewModel): void {
        this.isSaving = false;
        this.setServerInteractionInProgress(false);

        if (result.Success)
        {
            this.$emit('downloadSaved', result.Definition);
        }
        else
        {
            this.saveError = result.ErrorMessage || 'Failed to save with unknown error.';
            this.showSaveError = true;
        }
    }

    updateFileIdChoices(): void {
        this.service.GetStorageFileIdOptions(this.internalDownload.StorageId, this.fileIdOptionsLoadStatus, {
            onSuccess: (items) => this.fileIdOptions = items
        });
    }

    getStorageFileIdInfo(id: string): string {
        const info = this.storageInfos.filter(x => x.StorageId == id)[0];
        if (info == null) return '';
        else return info.FileIdInfo;
    }

    getStorageFileIdLabel(id: string): string {
        const info = this.storageInfos.filter(x => x.StorageId == id)[0];
        if (info == null) return 'File id';
        else return info.FileIdLabel || 'File id';
    }

    public tryDeleteDownload(): void {
        this.deleteDialogVisible = true;
    }

    public deleteDownload(): void {
        if (this.internalDownload.Id == null)
        {
            return;
        }

        this.deleteDialogVisible = false;
        this.isDeleting = true;
        this.setServerInteractionInProgress(true);

        this.service.DeleteDefinition(this.internalDownload.Id, null, {
            onSuccess: (success) => this.onDownloadDeleted(success),
            onError: (message) => this.setServerInteractionInProgress(false, message),
            onDone: () => { this.isDeleting = false }
        });
    }

    onDownloadDeleted(success: boolean): void {
        this.isDeleting = false;
        this.setServerInteractionInProgress(false);
        this.$emit('downloadDeleted', this.download);
    }

    formatDate(date: Date): string
    {
        return DateUtils.FormatDate(date, 'yyyy MMM d HH:mm:ss');
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onStorageChanged(): void {
        this.updateFileIdChoices();
    }
}
</script>

<style scoped lang="scss">
.without-label {
    margin-top: 0;
    padding-top: 0;
}
.payload-filter {
    border-bottom: solid 1px #ccc;
}
.download-summary {
    padding: 10px;
    margin-top: 20px;
    margin-bottom: 20px;
    text-align: center;
    font-size: 26px;

    @media (max-width: 900px) {
        font-size: 22px;
    }

    a {
        text-decoration: underline;
    }
}
.header-data {
    display: flex;
    align-items: center;
    flex-direction: row;
    flex-wrap: wrap-reverse;
}
.notifier-title {
    font-size: 18px;
    margin-bottom: 10px;
    margin-right: 10px;
}
</style>

<style lang="scss">
.possible-notifiers-dialog {
    max-width: 700px;

    .possible-notifiers-list-item {
        margin-bottom: 10px;
    }
}
.possible-placeholders-dialog {
    max-width: 700px;

    .possible-placeholder-list-item {
        margin-bottom: 10px;
    }
}
</style>