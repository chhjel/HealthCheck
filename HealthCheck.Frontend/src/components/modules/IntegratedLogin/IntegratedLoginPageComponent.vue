<!-- src/components/modules/IntegratedLogin/IntegratedLoginPageComponent.vue -->
<template>
    <div>
    <v-app light class="approot">
    <floating-squares-effect-component />

    <div class="integrated-login-page">
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            <v-container>
                
                <div class="mb-4 login-block">
                    <div>
                        <!-- INPUTS -->
                        <div>
                            <h1 class="login-title">{{ title }}</h1>
                            <v-text-field 
                                v-model="username"
                                :disabled="loadStatus.inProgress"
                                v-on:keyup.enter="onLoginClicked"
                                type="text"
                                label="Username"
                                placeholder=" "
                                class="pt-0 mt-5" />
                            
                            <v-text-field 
                                v-model="password"
                                :disabled="loadStatus.inProgress"
                                v-on:keyup.enter="onLoginClicked"
                                label="Password"
                                placeholder=" "
                                :type="showPassword ? 'text' : 'password'"
                                :append-icon="showPassword ? 'visibility' : 'visibility_off'"
                                @click:append="showPassword = !showPassword"
                                class="pt-0 mt-2" />
                            
                            <v-layout row class="mt-4 mb-4" v-if="show2FAInput">
                                <v-flex :xs6="show2FASendCodeButton" :xs12="!show2FASendCodeButton">
                                    <v-text-field 
                                        v-model="twoFactorCode"
                                        :disabled="loadStatus.inProgress"
                                        v-on:keyup.enter="onLoginClicked"
                                        label="Two factor code"
                                        placeholder=" "
                                        type="text"
                                        class="pt-0 mt-0"
                                        :loading="show2FACodeExpirationTime">
                                        <template v-slot:progress>
                                            <v-progress-linear
                                            :value="twoFactorInputProgress"
                                            :color="twoFactorInputColor"
                                            height="7"
                                            ></v-progress-linear>
                                        </template>
                                    </v-text-field>
                                </v-flex>
                                <v-flex xs6 v-if="show2FASendCodeButton">
                                    <v-btn round color="secondary" class="mt-0"
                                        @click.prevent="onSendCodeClicked"
                                        :disabled="loadStatus.inProgress">
                                        <span style="white-space: normal;">{{ send2FASCodeButtonText }}</span>
                                    </v-btn>
                                </v-flex>
                            </v-layout>
                        </div>

                        <v-btn round color="primary" large class="mt-4 login-button"
                            @click.prevent="onLoginClicked"
                            :disabled="loadStatus.inProgress">
                            <span style="white-space: normal;">Sign in</span>
                        </v-btn>

                        <v-progress-linear color="primary" indeterminate v-if="loadStatus.inProgress"></v-progress-linear>
                    
                        <div v-if="error != null && error.length > 0" class="error--text mt-4">
                            <b v-if="!showErrorAsHtml">{{ error }}</b>
                            <div v-if="showErrorAsHtml" v-html="error"></div>
                        </div>

                        <div v-if="codeMessage != null && codeMessage.length > 0" class="mt-4">
                            <b v-if="!showCodeMessageAsHtml">{{ codeMessage }}</b>
                            <div v-if="showCodeMessageAsHtml" v-html="codeMessage"></div>
                        </div>

                        <!-- LOAD STATUS -->
                        <v-alert
                            :value="loadStatus.failed"
                            type="error">
                        {{ loadStatus.errorMessage }}
                        </v-alert>
                    </div>
                </div>

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
import { FetchStatus,  } from  '../../../services/abstractions/HCServiceBase';
import IntegratedLoginService, { HCIntegratedLoginRequest, HCIntegratedLoginRequest2FACodeRequest, HCIntegratedLoginResult } from '../../../services/IntegratedLoginService';
import BlockComponent from '../../Common/Basic/BlockComponent.vue';
import FloatingSquaresEffectComponent from '../../Common/Effects/FloatingSquaresEffectComponent.vue';

@Component({
    components: {
        BlockComponent,
        FloatingSquaresEffectComponent
    }
})
export default class IntegratedLoginPageComponent extends Vue {

    loadStatus: FetchStatus = new FetchStatus();
    
    username: string = '';
    password: string = '';
    twoFactorCode: string = '';
    showPassword: boolean = false;
    error: string = '';
    showErrorAsHtml: boolean = false;
    codeMessage: string = '';
    showCodeMessageAsHtml: boolean = false;
    current2FACodeProgress: number = 0;
    twoFactorIntervalId: number = 0;
    codeExpirationTime: Date | null = null;
    codeExpirationDuration: number | null = null;
    allowShowProgress: boolean = true;
    isSingleProgress: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.twoFactorIntervalId = setInterval(this.update2FAProgress, 1000);
    }

    beforeDestroy(): void {
        clearInterval(this.twoFactorIntervalId);
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get title(): string {
        return this.globalOptions.ApplicationTitle;
    }

    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }

    get show2FAInput(): boolean {
        return this.globalOptions.IntegratedLoginShow2FA;
    }

    get Send2FACodeEndpoint(): string {
        return this.globalOptions.IntegratedLoginSend2FACodeEndpoint;
    }

    get show2FASendCodeButton(): boolean {
        return !!this.Send2FACodeEndpoint;
    }

    get send2FASCodeButtonText(): string {
        return this.globalOptions.IntegratedLoginSend2FACodeButtonText;
    }

    get show2FACodeExpirationTime(): boolean {
        return this.allowShowProgress && !!(this.globalOptions.IntegratedLoginCurrent2FACodeExpirationTime || this.codeExpirationTime);
    }

    get twoFactorInputProgress(): number {
        return this.current2FACodeProgress;
    }

    get twoFactorInputColor(): string {
        if (this.twoFactorInputProgress < 20)
        {
            return 'error';
        }
        else if (this.twoFactorInputProgress < 35)
        {
            return 'warning'
        }
        else {
            return 'success';
        }
    }

    ////////////////
    //  METHODS  //
    //////////////
    login(): void
    {
        if (this.loadStatus.inProgress) {
            return;
        }

        this.error = '';
        let url = this.globalOptions.IntegratedLoginEndpoint;
        let payload: HCIntegratedLoginRequest = {
            Username: this.username,
            Password: this.password,
            TwoFactorCode: this.twoFactorCode
        };

        let service = new IntegratedLoginService(true);
        service.Login(url, payload, this.loadStatus,
            {
                onSuccess: (result) => {
                    if (result.Success)
                    {
                        location.reload();
                    }
                    else
                    {
                        this.showErrorAsHtml = result.ShowErrorAsHtml;
                        this.error = result.ErrorMessage;
                    }
                }
            });
    }

    request2FACode(): void {
        if (this.loadStatus.inProgress) {
            return;
        }

        this.error = '';
        this.codeMessage = '';
        let url = this.Send2FACodeEndpoint;
        let payload: HCIntegratedLoginRequest2FACodeRequest = {
            Username: this.username
        };

        let service = new IntegratedLoginService(true);
        service.RequestCode(url, payload, this.loadStatus,
            {
                onSuccess: (result) => {
                    if (result.Success)
                    {
                        this.codeMessage = result.SuccessMessage;
                        this.showCodeMessageAsHtml = result.ShowSuccessAsHtml;

                        if (result.CodeExpiresInSeconds)
                        {
                            this.isSingleProgress = true;
                            this.allowShowProgress = true;
                            this.codeExpirationTime = new Date();
                            this.codeExpirationTime.setSeconds(this.codeExpirationTime.getSeconds() + result.CodeExpiresInSeconds);
                            this.codeExpirationDuration = result.CodeExpiresInSeconds;
                        }
                    }
                    else
                    {
                        this.showErrorAsHtml = result.ShowErrorAsHtml;
                        this.error = result.ErrorMessage;
                    }
                }
            });
    }

    update2FAProgress(): void {
        if (!this.show2FACodeExpirationTime)
        {
            return; 
        }

        const expirationTime = this.codeExpirationTime || this.globalOptions.IntegratedLoginCurrent2FACodeExpirationTime;
        const initialDate = new Date(expirationTime || '');
        const lifetime = this.codeExpirationDuration || this.globalOptions.IntegratedLogin2FACodeLifetime;
        let mod = (((new Date().getTime() - initialDate.getTime()) / 1000) % lifetime);
        if (mod < 0)
        {
            mod = mod + lifetime;
        }

        const timeLeft = lifetime - mod;
        const percentage = timeLeft / lifetime;
        this.current2FACodeProgress = percentage * 100;

        if (timeLeft <= 1.1 && this.isSingleProgress)
        {
            this.allowShowProgress = false;
        }
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onLoginClicked(): void {
        this.login();
    }

    onSendCodeClicked(): void {
        this.request2FACode();
    }
}
</script>

<style scoped lang="scss">
.integrated-login-page {
    /* background-color: hsla(0, 0%, 16%, 1); */
    height: 100%;
    text-align: center;
    font-family: 'Montserrat';
    
    .content-root {
        max-width: 500px;
        margin-top: 8vh;
    }

    .login-button {
        width: 100%;
        margin: 0;
        background-color: var(--v-primary-darken2) !important;
        text-transform: none;
        font-size: 19px;
        height: 46px;
    }

    .login-block {
        border-radius: 0;
    }

    .login-title {
        text-align: left;
    }
}
</style>

<style lang="scss">
</style>
