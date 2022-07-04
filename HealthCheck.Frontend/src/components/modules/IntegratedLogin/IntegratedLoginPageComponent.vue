<!-- src/components/modules/IntegratedLogin/IntegratedLoginPageComponent.vue -->
<template>
    <div>
        <floating-squares-effect-component /> 

        <div class="integrated-login-page">
            <div class="content-root">
                <div class="mb-4 login-block">
                    <!-- INPUTS -->
                    <div>
                        <h1 class="login-title">{{ title }}</h1>
                        <text-field-component
                            v-model:value="username"
                            :disabled="loadStatus.inProgress"
                            v-on:keyup.enter="onLoginClicked"
                            type="text"
                            label="Username"
                            placeholder=" "
                            class="pt-0 mt-5" />

                        <text-field-component
                            v-model:value="password"
                            :disabled="loadStatus.inProgress"
                            v-on:keyup.enter="onLoginClicked"
                            label="Password"
                            placeholder=" "
                            :type="showPassword ? 'text' : 'password'"
                            :append-icon="showPassword ? 'visibility' : 'visibility_off'"
                            @click:append="showPassword = !showPassword"
                            class="pt-0 mt-2" />

                        <div v-if="showTwoFactorCodeInput" class="mb-4 mt-2">
                            <div class="flex layout">
                                <div :class="mfaInputClasses">
                                    <text-field-component
                                        v-model:value="twoFactorCode"
                                        :disabled="loadStatus.inProgress"
                                        v-on:keyup.enter="onLoginClicked"
                                        label="Two factor code"
                                        placeholder=" "
                                        type="text"
                                        class="pt-0 mt-0"
                                        :loading="show2FACodeExpirationTime">
                                        <template v-slot:progress>
                                            <progress-linear-component
                                            :value="twoFactorInputProgress"
                                            :color="twoFactorInputColor"
                                            height="7"
                                            ></progress-linear-component>
                                        </template>
                                    </text-field-component>
                                </div>
                                <div class="xs6" v-if="show2FASendCodeButton">
                                    <btn-component round color="secondary" class="mt-0"
                                        @click.prevent="onSendCodeClicked"
                                        :disabled="loadStatus.inProgress">
                                        <span style="white-space: normal;">{{ send2FASCodeButtonText }}</span>
                                    </btn-component>
                                </div>
                            </div>
                            <div v-if="note2FA" class="mfa-note tfa">{{note2FA}}</div>
                        </div>
                    </div>

                    <div v-if="showWebAuthnInput">
                        <btn-component round outline  color="primary" 
                            class="webauthn-button"
                            @click.prevent="verifyWebAuthn"
                            :disabled="loadStatus.inProgress">
                            <span style="white-space: normal;">Verify WebAuthn</span>
                        </btn-component>
                        <div v-if="noteWebAuthn" class="mfa-note mt-1">{{noteWebAuthn}}</div>
                    </div>

                    <btn-component round color="primary" large class="mt-4 login-button"
                        @click.prevent="onLoginClicked"
                        :disabled="loadStatus.inProgress">
                        <span style="white-space: normal;">Sign in</span>
                    </btn-component>

                    <progress-linear-component color="primary" class="mt-1" indeterminate v-if="loadStatus.inProgress"></progress-linear-component>

                    <div v-if="error != null && error.length > 0" class="error--text mt-4">
                        <b v-if="!showErrorAsHtml">{{ error }}</b>
                        <div v-if="showErrorAsHtml" v-html="error"></div>
                    </div>

                    <div v-if="codeMessage != null && codeMessage.length > 0" class="mt-4">
                        <b v-if="!showCodeMessageAsHtml">{{ codeMessage }}</b>
                        <div v-if="showCodeMessageAsHtml" v-html="codeMessage"></div>
                    </div>

                    <!-- LOAD STATUS -->
                    <alert-component
                        :value="loadStatus.failed"
                        type="error">
                    {{ loadStatus.errorMessage }}
                    </alert-component>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { HCLoginTwoFactorCodeInputMode } from "@generated/Enums/WebUI/HCLoginTwoFactorCodeInputMode";
import { HCLoginWebAuthnMode } from "@generated/Enums/WebUI/HCLoginWebAuthnMode";
import { HCFrontEndOptions } from "@generated/Models/WebUI/HCFrontEndOptions";
import { HCIntegratedLoginRequest } from "@generated/Models/WebUI/HCIntegratedLoginRequest";
import { Vue,  } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { FetchStatus,  } from '@services/abstractions/HCServiceBase';
import IntegratedLoginService, { HCIntegratedLoginRequest2FACodeRequest, HCIntegratedLoginResult } from '@services/IntegratedLoginService';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import FloatingSquaresEffectComponent from '@components/Common/Effects/FloatingSquaresEffectComponent.vue';
import WebAuthnUtil from '@util/WebAuthnUtil';
import { HCVerifyWebAuthnAssertionModel } from "@generated/Models/WebUI/HCVerifyWebAuthnAssertionModel";
import { StoreUtil } from "@util/StoreUtil";

@Options({
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
    webAuthnLoginPayload: HCVerifyWebAuthnAssertionModel | null = null;
    showPassword: boolean = false;
    error: string = '';
    showErrorAsHtml: boolean = false;
    codeMessage: string = '';
    showCodeMessageAsHtml: boolean = false;
    current2FACodeProgress: number = 0;
    twoFactorIntervalId: any = 0;
    codeExpirationTime: Date | null = null;
    codeExpirationDuration: number | null = null;
    allowShowProgress: boolean = true;
    isSingleProgress: boolean = false;
    autoCallLoginAfterNextWebAuthn: boolean = false;

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

    get globalOptions(): HCFrontEndOptions {
        return StoreUtil.store.state.globalOptions;
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

    get note2FA(): string {
        return this.globalOptions.IntegratedLogin2FANote;
    }

    get noteWebAuthn(): string {
        return this.globalOptions.IntegratedLoginWebAuthnNote;
    }

    get show2FACodeExpirationTime(): boolean {
        return this.allowShowProgress && !!(this.globalOptions.IntegratedLoginCurrent2FACodeExpirationTime || this.codeExpirationTime);
    }

    get webAuthnMode(): HCLoginWebAuthnMode {
        return this.globalOptions.IntegratedLoginWebAuthnMode;
    }
    get requireWebAuthn(): boolean
    {
        return this.webAuthnMode == HCLoginWebAuthnMode.Required;
    }
    get showWebAuthnInput(): boolean
    {
        return this.webAuthnMode != HCLoginWebAuthnMode.Off;
    }

    get twoFactorCodeInputMode(): HCLoginTwoFactorCodeInputMode {
        return this.globalOptions.IntegratedLoginTwoFactorCodeInputMode;
    }
    get requireTwoFactorCode(): boolean
    {
        return this.twoFactorCodeInputMode == HCLoginTwoFactorCodeInputMode.Required;
    }
    get showTwoFactorCodeInput(): boolean
    {
        return this.twoFactorCodeInputMode != HCLoginTwoFactorCodeInputMode.Off;
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

    get mfaInputClasses(): any {
        return {
            xs6: this.show2FASendCodeButton,
            xs12: !this.show2FASendCodeButton,
        };
    }

    ////////////////
    //  METHODS  //
    //////////////
    login(): void
    {
        this.error = '';
        
        if (this.loadStatus.inProgress) {
            return;
        }
        else if (this.requireWebAuthn && !this.webAuthnLoginPayload) {
            this.verifyWebAuthn();
            this.autoCallLoginAfterNextWebAuthn = true;
            return;
        }
        else if (this.requireTwoFactorCode && !this.twoFactorCode) {
            this.error = 'Multi-factor code required.';
            return;
        }

        let url = this.globalOptions.IntegratedLoginEndpoint;
        let payload: HCIntegratedLoginRequest = {
            Username: this.username,
            Password: this.password,
            TwoFactorCode: this.twoFactorCode,
            WebAuthnPayload: (this.webAuthnLoginPayload || {}) as HCVerifyWebAuthnAssertionModel
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

    //////////////////////
    //  MFA: WebAuthn  //
    ////////////////////
    verifyWebAuthn(): void {
        let service = new IntegratedLoginService(true);
        service.CreateWebAuthnAssertionOptions(this.username, this.loadStatus, {
            onSuccess: (options) => {
                // console.log(options);
                this.onWebAuthnAssertionOptionsCreated(options);
            },
            onError: (e) => console.error(e),
            onDone: () => this.autoCallLoginAfterNextWebAuthn = false
        });
    }

    async onWebAuthnAssertionOptionsCreated(options: any): Promise<void> {
        if (options.status !== "ok")
        {
            this.error = options.error || 'Assertion options creation failed.';
            console.error(options);
            return;
        }

        options.challenge = WebAuthnUtil.coerceToArrayBuffer(options.challenge);
        options.allowCredentials.forEach((item: any) => {
            item.id = WebAuthnUtil.coerceToArrayBuffer(item.id);
        });

        try {
            const assertedCredential = (await navigator.credentials.get({ publicKey: options })) as any;
            
            let authData = new Uint8Array(assertedCredential.response.authenticatorData);
            let clientDataJSON = new Uint8Array(assertedCredential.response.clientDataJSON);
            let rawId = new Uint8Array(assertedCredential.rawId);
            let sig = new Uint8Array(assertedCredential.response.signature);
            const payload: HCVerifyWebAuthnAssertionModel = {
                Id: assertedCredential.id,
                RawId: WebAuthnUtil.coerceToBase64Url(rawId),
                Extensions: assertedCredential.getClientExtensionResults(),
                Response: {
                    AuthenticatorData: WebAuthnUtil.coerceToBase64Url(authData),
                    ClientDataJson: WebAuthnUtil.coerceToBase64Url(clientDataJSON),
                    Signature: WebAuthnUtil.coerceToBase64Url(sig)
                }
            };

            this.webAuthnLoginPayload = payload;
            if (this.autoCallLoginAfterNextWebAuthn)
            {
                this.login();
            }
        } catch (e) {
            this.error = 'Assertion failed.';
            console.error(e);
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
    height: 100%;
    text-align: center;
    font-family: 'Montserrat';
    padding: 5px 20px;
    margin: 0 auto;
    max-width: 1280px;
    width: calc(100% - 40px); // - padding (20+20)

    .content-root {
        max-width: 370px;
        margin: 0 auto;
        margin-top: 8vh;
        background: #ffffff94;
    }

    .login-button {
        width: 100%;
        margin: 0;
        background-color: var(--color--primary-darken1) !important;
        text-transform: none;
        font-size: 19px;
        height: 46px;
    }

    .webauthn-button {
        margin: 0;
        text-transform: none;
        font-size: 19px;
    }

    .login-block {
        border-radius: 0;
    }

    .login-title {
        text-align: left;
    }

    .mfa-note {
        font-size: small;
/* 
        &.tfa { 
            margin-top: -8px;
        } */
    }
}
</style>

<style lang="scss">
</style>
