<!-- src/components/modules/SecureFileDownload/DownloadPageComponent.vue -->
<template>
    <div>
    <v-app light class="approot">
    <div class="secure-file-download-page">
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            <v-container>
                
                <!-- AFTER DOWNLOAD HAS STARTED -->
                <block-component class="mb-4" v-if="isDownloading">
                    <div class="downloading-content">
                        <h3 class="heading">Downloading '{{data.download.filename}}'</h3>
                        <v-icon large color="green darken-2">done</v-icon> 
                    </div>
                </block-component>
                
                <!-- BEFORE STARTING DOWNLOAD -->
                <block-component class="mb-4" v-if="!isDownloading">
                    <div>
                        <!-- LOAD STATUS -->
                        <v-alert :value="data.definitionValidationError !== null" type="error">
                            {{ data.definitionValidationError }}
                        </v-alert>
                        <v-alert
                            :value="loadStatus.failed"
                            type="error">
                        {{ loadStatus.errorMessage }}
                        </v-alert>
                        <v-progress-linear color="primary" indeterminate v-if="loadStatus.inProgress"></v-progress-linear>

                        <!-- PASSWORD INPUT -->
                        <div v-if="data.download.protected">
                            <strong>A password is required to download this file</strong>
                            <v-text-field 
                                box
                                hide-details single-line
                                v-model="currentPassword"
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
                        <v-btn color="primary" large
                            class="mt-3"
                            @click.prevent="onDownloadClicked"
                            :disabled="isDownloadButtonDisabled"
                            >
                            <span style="white-space: normal;">
                            Download '{{data.download.filename}}'
                            </span>
                            <v-icon dark right>cloud_download</v-icon>
                        </v-btn>
                    </div>
                </block-component>

                <!-- NOTES -->
                <block-component class="mb-4"
                    v-if="data.download.note !== null">
                    {{ data.download.note }}
                </block-component>

                <!-- EXPIRATION -->
                <block-component class="mb-4"
                    v-if="data.download.expiresAt !== null || data.download.downloadsRemaining !== null">
                    <div v-if="data.download.expiresAt !== null">
                        Expires {{ formatedExpirationDate }}.
                    </div>
                    <div v-if="data.download.downloadsRemaining !== null">
                        <b>{{ data.download.downloadsRemaining }}</b> downloads remaining.
                    </div>
                </block-component>

            </v-container>
          <!-- CONTENT END -->
        </v-flex>
        </v-layout>
        </v-container>
        </v-content>
    </div>
    </v-app>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import DateUtils from  '../../../util/DateUtils';
import LinqUtils from  '../../../util/LinqUtils';
import HCServiceBase, { FetchStatus,  } from  '../../../services/abstractions/HCServiceBase';
import BlockComponent from '../../Common/Basic/BlockComponent.vue';
import UrlUtils from "../../../util/UrlUtils";

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

@Component({
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

    data: ConfigFromWindow = {
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

        if (!this.data.download.protected && this.data.download.downloadsRemaining == null)
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
        // else if (this.data.download.protected && this.currentPassword.trim().length == 0)
        // {
        //     return true;
        // }

        return false;
    }

    get formatedExpirationDate(): string {
        if (this.data.download.expiresAt == null)
        {
            return '';
        }

        return DateUtils.FormatDate(this.data.download.expiresAt, 'd. MMM yyyy')
            + ' at '
            + DateUtils.FormatDate(this.data.download.expiresAt, 'HH:mm:ss');
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

        this.data = {
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
        if (this.data.download.downloadsRemaining != null)
        {
            this.data.download.downloadsRemaining = this.data.download.downloadsRemaining - 1;
        }
    }

    validatePassword(password: string): void
    {
        if (this.loadStatus.inProgress) {
            return;
        }

        let url = UrlUtils.makeAbsolute(document.baseURI, `../SFDValidatePassword`);
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
                'x-id': this.data.download.name,
                'x-pwd': password
            });
    }

    startDownloadingCurrentFile(token: string = ''): void {
        let relativeUrl = this.data.download.downloadLink;
        let url = UrlUtils.makeAbsolute(document.baseURI, `../${relativeUrl}`);

        // Not protected => download at once
        if (!this.data.download.protected)
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
        max-width: 600px;
    }

    .downloading-content {
        display: flex;
        justify-content: center;
        align-items: center;
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
