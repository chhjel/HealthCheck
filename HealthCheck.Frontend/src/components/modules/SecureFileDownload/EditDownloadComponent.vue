<!-- src/components/modules/SecureFileDownload/EditDownloadComponent.vue -->
<template>
    <div class="root">
        <alert-component
            :value="serverInteractionError != null && serverInteractionError.length > 0"
            type="error" >
            {{ serverInteractionError }}
        </alert-component>

        <!-- ###### FILE ###### -->
        <block-component class="mb-5">
            <div class="mb-4" style="font-weight: 600"
                v-if="absoluteDownloadUrl != null"
                >Download link: <a :href="absoluteDownloadUrl">{{ absoluteDownloadUrl }}</a> </div>

            <input-component
                class="mt-2"
                v-model:value="internalDownload.FileName"
                :disabled="!allowChanges"
                name="Filename"
                description="Name of the file when downloading."
                type="text"
                :error="validateFileName"
                />
                
            <input-component
                class="mt-2"
                v-model:value="internalDownload.UrlSegmentText"
                :disabled="!allowChanges"
                name="Url segment text"
                description="Text in the url after '/download/'."
                type="text"
                :error="validateUrlSegmentText"
                />
            
            <select-component
                class="mt-2"
                v-model:value="internalDownload.StorageId"
                :items="storageOptions"
                :disabled="!allowChanges || internalDownload.HasUploadedFile"
                name="Where to get file from"
                @input="onStorageChanged()"
                />
            
            <div class="file-selection-method" v-if="showFileIdSelect || showFileIdInput">
                <select-component
                    v-if="showFileIdSelect"
                    v-model:value="selectedFileName"
                    @change="onFileSelectorChanged"
                    class="mt-2"
                    :error="validateFileId"
                    :items="fileIdOptions"
                    :disabled="!allowChanges"
                    :loading="fileIdOptionsLoadStatus.inProgress"
                    :name="getStorageFileIdLabel(internalDownload.StorageId)"
                    :description="getStorageFileIdInfo(internalDownload.StorageId)"
                    show-description-on-start="true"
                    />
                
                <input-component
                    v-if="showFileIdInput"
                    class="mt-2"
                    v-model:value="internalDownload.FileId"
                    :error="validateFileId"
                    :disabled="!allowChanges"
                    :name="getStorageFileIdLabel(internalDownload.StorageId)"
                    :description="getStorageFileIdInfo(internalDownload.StorageId)"
                    show-description-on-start="true"
                    type="text"
                    />
            </div>

            <h3 v-if="(showFileIdSelect || showFileIdInput) && selectedStorageSupportsUploads">Or</h3>

            <div v-if="selectedStorageSupportsUploads" class="file-selection-method">
                <div class="input-label-d">Upload a file</div>
                <br />

                <parameter-input-type-http-posted-file-base-component :value="null" :config="emptyObj"
                    class="upload-file-component"
                    ref="filepicker"
                    @onFileChanged="onFileChanged" 
                    :readonly="!allowChanges || !isEditing"
                    :allowClear="false" />
                <br />

                <div v-if="selectedFile != null">
                    <btn-component small color="primary"
                        @click="uploadSelectedFile"
                        :loading="uploadResult && uploadResult.inProgress"
                        :disabled="!allowChanges">Upload selected file</btn-component>
                    <span v-if="uploadResult && !uploadResult.success">{{ uploadResult.message }}</span>
                    <span v-if="uploadResult && uploadResult.success">File uploaded successfully.</span>
                    <fetch-status-progress-component :status="uploadLoadStatus" class="ml-2 mr-2" style="margin-top: -4px; width: 178px;" />
                </div>
                
                <btn-component small class="error"
                    v-if="internalDownload.HasUploadedFile"
                    @click="removeUploadedFile"
                    :disabled="!allowChanges">Delete uploaded file '{{ internalDownload.OriginalFileName }}'</btn-component>
                <b v-if="!isEditing">Save first to upload files.</b>
                <span v-if="deleteFileStatus && !deleteFileStatus.success">{{ deleteFileStatus.message }}</span>
                <span v-if="deleteFileStatus && deleteFileStatus.success">File deleted successfully.</span>
            </div>
            
            <input-component
                class="mt-2"
                v-model:value="internalDownload.Note"
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
                v-model:value="internalDownload.DownloadCountLimit"
                :disabled="!allowChanges"
                name="Number of allowed downloads"
                :description="`Total number of times the file can be downloaded. It has been downloaded ${internalDownload.DownloadCount} times so far.`"
                type="number"
                />
            
            <simple-date-time-component
                v-model:value="internalDownload.ExpiresAt"
                :readonly="!allowChanges"
                name="Expires at"
                description="After this time, the file will not be available for download."
                />
                
            <input-component
                class="mt-2"
                v-model:value="internalDownload.Password"
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
        <dialog-component v-model:value="deleteDialogVisible"
            @keydown.esc="deleteDialogVisible = false"
            max-width="290"
            content-class="confirm-dialog">
            <card-component>
                <div class="headline">Confirm deletion</div>
                <div>
                    Are you sure you want to delete this download?
                </div>
                                <div>
                                        <btn-component color="secondary" @click="deleteDialogVisible = false">Cancel</btn-component>
                    <btn-component color="error" @click="deleteDownload()">Delete it</btn-component>
                </div>
            </card-component>
        </dialog-component>

        <dialog-component v-model:value="showSaveError"
            @keydown.esc="showSaveError = false"
            max-width="400"
            content-class="confirm-dialog">
            <card-component>
                <div class="headline">Save error</div>
                <div style="overflow: auto;">
                    {{ saveError }}
                </div>
                                <div>
                                        <btn-component color="secondary" @click="showSaveError = false">Close</btn-component>
                </div>
            </card-component>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import SimpleDateTimeComponent from '@components/Common/SimpleDateTimeComponent.vue';
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import DateUtils from '@util/DateUtils';
import IdUtils from '@util/IdUtils';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import InputComponent from '@components/Common/Basic/InputComponent.vue';
import SelectComponent from '@components/Common/Basic/SelectComponent.vue';
import SecureFileDownloadUtils from '@util/SecureFileDownload/SecureFileDownloadUtils';
import SecureFileDownloadService from '@services/SecureFileDownloadService';
import { FetchStatus, FetchStatusWithProgress } from '@services/abstractions/HCServiceBase';
import ParameterInputTypeHttpPostedFileBaseComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeHttpPostedFileBaseComponent.vue";
import FetchStatusProgressComponent from "@components/Common/Basic/FetchStatusProgressComponent.vue";
import { SecureFileDownloadDefinition, SecureFileDownloadSaveViewModel, SecureFileDownloadStorageInfo, SecureFileDownloadStorageUploadFileResult } from "@models/modules/SecureFileDownload/Models";
import { StoreUtil } from "@util/StoreUtil";

@Options({
    components: {
        SimpleDateTimeComponent,
        BlockComponent,
        InputComponent,
        SelectComponent,
        ParameterInputTypeHttpPostedFileBaseComponent,
        FetchStatusProgressComponent
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
    fileIdOptionIds: Array<string> = [];
    selectedFileName: string = '';
    fileIdOptionsLoadStatus: FetchStatus = new FetchStatus();
    deleteFileStatus: FetchStatus = new FetchStatus();
    uploadLoadStatus: FetchStatusWithProgress = new FetchStatusWithProgress();
    uploadResult: SecureFileDownloadStorageUploadFileResult | null = null;
    emptyObj: any = {};
    selectedFile: File | null = null;

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
        return StoreUtil.store.state.globalOptions;
    }

    get showFileIdInput(): boolean {
        return this.selectedStorageSupportsSelectingFile &&!this.hasFileIdOptions && !this.internalDownload.HasUploadedFile && this.selectedFile == null;
    }

    get showFileIdSelect(): boolean {
        return this.selectedStorageSupportsSelectingFile && this.hasFileIdOptions && !this.internalDownload.HasUploadedFile && this.selectedFile == null;
    }

    get hasFileIdOptions(): boolean {
        return this.fileIdOptions.length > 0;
    }

    get isEditing(): boolean {
        return this.hasDefinitionId;
    }

    get hasDefinitionId(): boolean {
        return this.internalDownload.Id != null
            && this.internalDownload.Id != '00000000-0000-0000-0000-000000000000';
    }
    
    get allowChanges(): boolean {
        return !this.readonly && !this.serverInteractionInProgress;
    }

    get absoluteDownloadUrl(): string {
        return SecureFileDownloadUtils.getAbsoluteDownloadUrl(this.globalOptions.EndpointBase, this.internalDownload.UrlSegmentText);
    }

    get selectedStorageSupportsUploads(): boolean {
        return this.storageInfos.some(x => x.StorageId == this.internalDownload.StorageId && x.SupportsUpload);
    }

    get selectedStorageSupportsSelectingFile(): boolean {
        return this.storageInfos.some(x => x.StorageId == this.internalDownload.StorageId && x.SupportsSelectingFile);
    }

    get storageOptions(): any {
        return this.storageInfos.map(x => {
                return { text: x.StorageName, value: x.StorageId }
            });
    }
    
    get validateFileId(): string | null {
        if ((this.internalDownload.FileId == null || this.internalDownload.FileId.length == 0) && !this.selectedStorageSupportsUploads)
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
            this.internalDownload = result.Definition;
        }
        else
        {
            this.saveError = result.ErrorMessage || 'Failed to save with unknown error.';
            this.showSaveError = true;
        }
    }

    updateFileIdChoices(): void {
        this.service.GetStorageFileIdOptions(this.internalDownload.StorageId, this.fileIdOptionsLoadStatus, {
            onSuccess: (items) => {
                this.fileIdOptions = items.map(x => x.Name);
                this.fileIdOptionIds = items.map(x => x.Id);
            }
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

    onFileSelectorChanged(): void {
        const index = this.fileIdOptions.indexOf(this.selectedFileName);
        const fileId = this.fileIdOptionIds[index] || '';
        this.internalDownload.FileId = fileId;
    }

    async onFileChanged(file: File | null) {
        this.selectedFile = file;
        if (!this.allowChanges || !this.hasDefinitionId) return;
    }

    async uploadSelectedFile() {
        this.uploadResult = null;
        if (this.selectedFile != null)
        {
            await this.uploadFile(this.selectedFile);
        }
    }

    async uploadFile(file: File): Promise<SecureFileDownloadStorageUploadFileResult> {
        this.setServerInteractionInProgress(true);
        const result = await this.service.UploadFile(file, this.internalDownload.Id, this.internalDownload.StorageId, this.globalOptions.EndpointBase, this.uploadLoadStatus);
        this.uploadResult = result
        if (result.success)
        {
            this.internalDownload.Id = result.defId;
            this.internalDownload.HasUploadedFile = true;
            this.internalDownload.FileId = result.fileId;
            this.internalDownload.OriginalFileName = file.name;
        }
        this.setServerInteractionInProgress(false);
        return result;
    }

    removeUploadedFile(): void {
        this.setServerInteractionInProgress(true);
        this.service.DeleteUploadedFile(this.internalDownload.Id, this.deleteFileStatus, {
            onSuccess: (success) => {
                if (success)
                {
                    this.selectedFile = null;
                    this.internalDownload.HasUploadedFile = false;
                    this.internalDownload.FileId = '';
                    this.internalDownload.OriginalFileName = '';
                    const filepicker = this.$refs.filepicker as ParameterInputTypeHttpPostedFileBaseComponent;
                    if (filepicker != null)
                    {
                        filepicker.setValueToNull();
                    }
                }
                else {
                    this.deleteFileStatus.failed = true;
                    this.deleteFileStatus.errorMessage = "Failed to delete file.";
                }
            },
            onError: (message) => this.setServerInteractionInProgress(false, message),
            onDone: () => this.setServerInteractionInProgress(false)
        });
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
.file-selection-method {
    border: 4px solid #f4f4f4;
    padding: 10px;
}
.input-label-d {
    display: inline-block;
    font-size: 16px;
    color: var(--v-secondary-base);
    font-weight: 600;
}
.upload-file-component {
    display: inline-block;
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