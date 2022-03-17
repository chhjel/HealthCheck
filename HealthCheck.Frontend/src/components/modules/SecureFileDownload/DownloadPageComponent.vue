<!-- src/components/modules/SecureFileDownload/DownloadPageComponent.vue -->
<template>
    <div>
    <div class="approot">
    <div class="secure-file-download-page">
        <content-component class="pl-0">
        <div fluid fill-height class="content-root">
        <div>
        <div class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            <div>
                
                <!-- AFTER DOWNLOAD HAS STARTED -->
                <block-component class="mb-4" v-if="isDownloading">
                    <div class="downloading-content">
                        <h3 class="heading">Downloading '{{datax.download.filename}}'</h3>
                        <icon-component large color="green darken-2">done</icon-component> 
                    </div>
                </block-component>
                
                <!-- BEFORE STARTING DOWNLOAD -->
                <block-component class="mb-4" v-if="!isDownloading">
                    <div>
                        <!-- LOAD STATUS -->
                        <alert-component :value="datax.definitionValidationError !== null" type="error">
                            {{ datax.definitionValidationError }}
                        </alert-component>
                        <alert-component
                            :value="loadStatus.failed"
                            type="error">
                        {{ loadStatus.errorMessage }}
                        </alert-component>

                        <!-- PASSWORD INPUT -->
                        <div v-if="datax.download.protected && !isExpired">
                            <strong>A password is required to download this file</strong>
                            <text-field-component 
                                box
                                hide-details single-line
                                v-model:value="currentPassword"
                                :disabled="isPasswordFieldDisabled"
                                :type="showPassword ? 'text' : 'password'"
                                :append-icon="showPassword ? 'visibility' : 'visibility_off'"
                                @click:append="showPassword = !showPassword"
                                class="pt-0 mt-2" />
                        
                            <div v-if="passwordValidationResult != null && !passwordValidationResult.success"
                                class="error--text mt-2">
                                <b>{{ passwordValidationResult.message }}</b>
                            </div>
                        </div>

                        <!-- DOWNLOAD LINK -->
                        <btn-component color="primary" large
                            class="mt-3"
                            v-if="!isExpired"
                            @click.prevent="onDownloadClicked"
                            :disabled="isDownloadButtonDisabled"
                            >
                            <span style="white-space: normal;">
                            {{ downloadButtonText }}
                            </span>
                            <icon-component dark right>cloud_download</icon-component>
                        </btn-component>
                        <progress-linear-component color="primary" indeterminate v-if="loadStatus.inProgress"></progress-linear-component>
                    </div>
                </block-component>

                <!-- NOTES -->
                <block-component class="mb-4"
                    v-if="datax.download.note !== null">
                    <div class="download-note">{{ datax.download.note }}</div>
                </block-component>

                <!-- EXPIRATION -->
                <block-component class="mb-4"
                    v-if="(datax.download.expiresAt !== null || datax.download.downloadsRemaining !== null) && !isExpired">
                    <div v-if="datax.download.expiresAt !== null">
                        Expires {{ formatedExpirationDate }}.
                    </div>
                    <div v-if="datax.download.downloadsRemaining !== null">
                        <b>{{ datax.download.downloadsRemaining }}</b> downloads remaining.
                    </div>
                </block-component>

            </div>
          <!-- CONTENT END -->
        </div>
        </div>
        </div>
        </content-component>
    </div>
    </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import DateUtils from '@util/DateUtils';
import LinqUtils from '@util/LinqUtils';
import HCServiceBase, { FetchStatus,  } from '@services/abstractions/HCServiceBase';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import UrlUtils from '@util/UrlUtils';

interface ConfigFromWindow
{
    definitionValidationError: string,
    download: DownloadConfigFromWindow;
}
interface DownloadConfigFromWindow
{
    name: string;
    filename: string,
    downloadLink: string,
    note: string | null,
    protected: boolean,
    expiresAt: Date | null,
    downloadsRemaining: number | null
}
interface ValidatePasswordResult
{
    success: string;
    message: string;
    token: string;
}

@Options({
    components: {
        BlockComponent,
    }
})
export default class DownloadPageComponent extends Vue {

    loadStatus: FetchStatus = new FetchStatus();
    passwordValidationResult: ValidatePasswordResult | null = null;
    isDownloading: boolean = false;
    currentPassword: string = '';
    showPassword: boolean = false;

    datax: ConfigFromWindow = {
        definitionValidationError: '',
        download: {
            name: '',
            filename: '',
            downloadLink: '',
            note: null,
            protected: true,
            expiresAt: null,
            downloadsRemaining: null
        }
    };

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.loadData();

        if (!this.datax.download.protected && this.datax.download.downloadsRemaining == null && !this.datax.definitionValidationError)
        {
            this.startDownloadingCurrentFile();
        }
    }

    created(): void {
    }

    beforeDestroy(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get downloadButtonText(): string {
        if (this.loadStatus.inProgress) {
            return 'Authenticating..';
        } else {
            return `Download '${this.datax.download.filename}'`;
        }
    }

    get isExpired(): boolean {
        if (this.datax.download.expiresAt != null && this.datax.download.expiresAt.getTime() < new Date().getTime())
        {
            return true;
        }
        else if (this.datax.download.downloadsRemaining != null && this.datax.download.downloadsRemaining <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    get isPasswordFieldDisabled(): boolean {
        if (this.loadStatus.inProgress)
        {
            return true;
        }

        return false;
    }

    get isDownloadButtonDisabled(): boolean {
        if (this.loadStatus.inProgress)
        {
            return true;
        }
        // else if (this.datax.download.protected && this.currentPassword.trim().length == 0)
        // {
        //     return true;
        // }

        return false;
    }

    get formatedExpirationDate(): string {
        if (this.datax.download.expiresAt == null)
        {
            return '';
        }

        return DateUtils.FormatDate(this.datax.download.expiresAt, 'd. MMM yyyy')
            + ' at '
            + DateUtils.FormatDate(this.datax.download.expiresAt, 'HH:mm:ss');
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        const windowData = (<any>window).__hc_data;

        const expiresAt = (windowData.download.expiresAt === '') ? null 
            : new Date(Number(windowData.download.expiresAt) * 1000);
        const downloadsRemaining = (windowData.download.downloadsRemaining === '') ? null 
            : Number(windowData.download.downloadsRemaining);

        this.datax = {
            definitionValidationError: windowData.definitionValidationError,
            download: {
                name: windowData.download.name,
                filename: windowData.download.filename,
                note: windowData.download.note,
                downloadLink: windowData.download.downloadLink,
                protected: windowData.download.protected,
                expiresAt: expiresAt,
                downloadsRemaining: downloadsRemaining
            }
        }
    }

    startDownload(url: string): void
    {
        const link = document.createElement("a");
        document.body.appendChild(link);
        link.href = url;
        link.setAttribute("type", "hidden");
        // link.setAttribute("download", "true");
        link.click();
        
        this.isDownloading = true;
        if (this.datax.download.downloadsRemaining != null)
        {
            this.datax.download.downloadsRemaining = this.datax.download.downloadsRemaining - 1;
        }
    }

    validatePassword(password: string): void
    {
        if (this.loadStatus.inProgress) {
            return;
        }

        let url = UrlUtils.getRelativeToCurrent(`../SFDValidatePassword`);
        let payload = {};

        let service = new HCServiceBase('', false);
        service.fetchExt<ValidatePasswordResult>(url, 'POST', payload, this.loadStatus,
            {
                onSuccess: (result) => {
                    this.passwordValidationResult = result;
                    if (result.success)
                    {
                        this.startDownloadingCurrentFile(result.token);
                    }
                }
            },
            true,
            {
                'x-id': this.datax.download.name,
                'x-pwd': password
            });
    }

    startDownloadingCurrentFile(token: string = ''): void {
        let relativeUrl = this.datax.download.downloadLink;
        let url = UrlUtils.getRelativeToCurrent(`../${relativeUrl}`);

        // Not protected => download at once
        if (!this.datax.download.protected)
        {
            this.startDownload(url);
        }
        // Protected and need token => get token
        else if (token.length == 0)
        {
            this.validatePassword(this.currentPassword);
        }
        // Got a token
        else if (token.length > 0)
        {
            url = url.replace('/__', `/${token}__`);
            this.startDownload(url);
        }
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDownloadClicked(): void {
        this.startDownloadingCurrentFile();
    }
}
</script>

<style scoped lang="scss">
.secure-file-download-page {
    /* background-color: hsla(0, 0%, 16%, 1); */
    height: 100%;
    text-align: center;
    
    .content-root {
        max-width: 800px;
    }

    .downloading-content {
        display: flex;
        justify-content: center;
        align-items: center;
    }

    .download-note {
        white-space: pre-wrap;
    }
}
</style>

<style lang="scss">
.secure-file-download-page {
    button {
        max-width: 100%;

        .v-btn__content {
            max-width: 100%;
        }
    }
}
</style>
