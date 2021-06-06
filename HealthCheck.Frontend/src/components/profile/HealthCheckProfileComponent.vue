<!-- src/components/profile/HealthCheckProfileComponent.vue -->
<template>
    <div>
        <div style="margin-top: 100px;"></div>

        <div v-if="profileOptions.Username">
            Username: <code>{{ profileOptions.Username }}</code>
        </div>

        <div v-if="profileOptions.ShowHealthCheckRoles">
            User roles: <code>{{ userRoles }}</code>
        </div>

        <div v-if="profileOptions.TotpElevationEnabled">
            <input-component
                name="TOTP code"
                v-model="totpCode"
                :disabled="totpLoadStatus.inProgress"
                :clearable="true"
            ></input-component>

            <v-btn
                round color="primary" large class="mt-4"
                @click.prevent="elevateTotp"
                :disabled="totpLoadStatus.inProgress">
                Elevate access using TOTP
            </v-btn>
        </div>
        
        <div v-if="profileOptions.AddTotpEnabled">
            <p>Scan the QR code with an authenticator app</p>
            <canvas ref="qrCodeCanvas"></canvas>
            <p>Optionally enter secret manually in your app of choice: <code>{{ registerTotpSecret }}</code>.</p>

            <input-component
                name="TOTP code from authenticator"
                v-model="registerTotpCode"
                :disabled="totpAddLoadStatus.inProgress"
                :clearable="true"
            ></input-component>

            <v-btn 
                round color="primary" large
                @click.prevent="registerTotp"
                :disabled="totpAddLoadStatus.inProgress">
                Register TOTP
            </v-btn>

            <v-alert :value="totpAddLoadStatus.failed" type="error">
            {{ totpAddLoadStatus.errorMessage }}
            </v-alert>
        </div>

        <v-btn v-if="profileOptions.RemoveTotpEnabled"
            round color="primary" large
            @click.prevent="removeTotp"
            :disabled="totpLoadStatus.inProgress">
            Remove TOTP
        </v-btn>

        <v-alert :value="totpLoadStatus.failed" type="error">
        {{ totpLoadStatus.errorMessage }}
        </v-alert>
        <br />

        <div style="hide">
            <v-btn v-if="profileOptions.WebAuthnElevationEnabled"
                round color="primary" large
                @click.prevent="elevateWebAuthn"
                :disabled="webAuthnLoadStatus.inProgress">
                Elevate access using WebAuthn
            </v-btn>

            <v-btn v-if="profileOptions.AddWebAuthnEnabled"
                round color="primary" large
                @click.prevent="registerWebAuthn"
                :disabled="webAuthnLoadStatus.inProgress">
                Register WebAuthn
            </v-btn>

            <v-btn v-if="profileOptions.RemoveWebAuthnEnabled"
                round color="primary" large
                @click.prevent="removeWebAuthn"
                :disabled="webAuthnLoadStatus.inProgress">
                Remove WebAuthn
            </v-btn>

            <v-alert :value="webAuthnLoadStatus.failed" type="error">
            {{ webAuthnLoadStatus.errorMessage }}
            </v-alert>
        </div>

        <br />
        <code>{{ globalOptions.IntegratedProfileConfig }}</code>
    </div>
</template>

<script lang="ts">
import { Vue, Component } from "vue-property-decorator";
import IntegratedProfileService from 'services/IntegratedProfileService';
import { HCFrontEndOptions } from "generated/Models/WebUI/HCFrontEndOptions";
import { HCIntegratedProfileConfig } from "generated/Models/WebUI/HCIntegratedProfileConfig";
import { FetchStatus } from "services/abstractions/HCServiceBase";
import InputComponent from "components/Common/Basic/InputComponent.vue"
import WebAuthnUtil from "util/WebAuthnUtil";
import Base32Util from "util/Base32Util";
import { VerifyWebAuthnAssertionModel } from "generated/Models/WebUI/VerifyWebAuthnAssertionModel";
import { toCanvas } from 'QRCode';

@Component({
    components: {
        InputComponent
    }
})
export default class HealthCheckProfileComponent extends Vue
{
    service: IntegratedProfileService = new IntegratedProfileService(this.globalOptions.EndpointBase, this.globalOptions.InludeQueryStringInApiCalls);

    totpLoadStatus: FetchStatus = new FetchStatus();
    totpAddLoadStatus: FetchStatus = new FetchStatus();
    webAuthnLoadStatus: FetchStatus = new FetchStatus();
    totpCode: string = '';
    registerTotpSecret: string = '';
    registerTotpCode: string = '';
    registerTotpPassword: string = '';
    removeTotpPassword: string = '';
    registerWebAuthnPassword: string = '';
    removeWebAuthnPassword: string = '';

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.generateQrCode();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): HCFrontEndOptions {
        return this.$store.state.globalOptions;
    }

    get profileOptions(): HCIntegratedProfileConfig {
        return this.globalOptions.IntegratedProfileConfig;
    }

    get userRoles(): Array<string> {
        return this.globalOptions.UserRoles;
    }

    get username(): string {
        return this.profileOptions.Username;
    }

    get totpQrCodeIssuer(): string {
        return 'Issuer';
    }

    get totpQrCodeLabel(): string {
        return 'Label';
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    elevateTotp(): void {
        this.service.ElevateTotp(this.totpCode, this.totpLoadStatus);
    }

    registerTotp(): void {
        this.service.RegisterTotp(this.registerTotpSecret, this.registerTotpCode, this.registerTotpPassword, this.totpLoadStatus);
    }

    removeTotp(): void {
        this.service.RemoveTotp(this.removeTotpPassword, this.totpLoadStatus);
    }

    elevateWebAuthn(): void {
        const payload = {} as VerifyWebAuthnAssertionModel;
        this.service.ElevateWebAuthn(payload, this.webAuthnLoadStatus);
    }

    removeWebAuthn(): void {
        this.service.RemoveWebAuthn(this.removeWebAuthnPassword, this.webAuthnLoadStatus);
    }

    registerWebAuthn(): void {
        this.service.CreateWebAuthnRegistrationOptions(this.username, this.webAuthnLoadStatus, {
            onSuccess: (options) => {
                this.onWebAuthnRegistrationOptionsCreated(options);
            },
            onError: (e) => console.error(e)
        });
    }

    async onWebAuthnRegistrationOptionsCreated(options: any): Promise<void> {
        if (options.status !== "ok")
        {
            alert('Status not ok, check log.');
            console.error(options);
            return;
        }

        // Turn the challenge back into the accepted format of padded base64
        options.challenge = WebAuthnUtil.coerceToArrayBuffer(options.challenge);
        // Turn ID into a UInt8Array Buffer for some reason
        options.user.id = WebAuthnUtil.coerceToArrayBuffer(options.user.id);
        options.excludeCredentials = options.excludeCredentials.map((c: any) => {
            c.id = WebAuthnUtil.coerceToArrayBuffer(c.id);
            return c;
        });
        if (options.authenticatorSelection.authenticatorAttachment === null) options.authenticatorSelection.authenticatorAttachment = undefined;

        let newCredential;
        try {
            newCredential = await navigator.credentials.create({
                publicKey: options
            }) as any;
        } catch (e) {
            var msg = "Could not create credentials in browser. Probably because the username is already registered with your authenticator. Please change username or authenticator."
            console.error(msg, e);
            alert(msg);
            return;
        }

        try {
            let attestationObject = new Uint8Array(newCredential.response.attestationObject);
            let clientDataJSON = new Uint8Array(newCredential.response.clientDataJSON);
            let rawId = new Uint8Array(newCredential.rawId);

            const registerPayload = {
                id: newCredential.id,
                rawId: WebAuthnUtil.coerceToBase64Url(rawId),
                type: newCredential.type,
                extensions: newCredential.getClientExtensionResults(),
                response: {
                    AttestationObject: WebAuthnUtil.coerceToBase64Url(attestationObject),
                    clientDataJson: WebAuthnUtil.coerceToBase64Url(clientDataJSON)
                }
            };

            this.service.RegisterWebAuthn(this.registerWebAuthnPassword, registerPayload, this.webAuthnLoadStatus);
        } catch (e) {
            console.error('RegisterWebAuthn failed');
            console.error(e);
            alert(e);
        }
    }

    generateQrCode(): void {
        const data = this.generateTotpQrCodeData();
        const canvas = this.$refs.qrCodeCanvas as HTMLCanvasElement;
        toCanvas(canvas, data);
    }

    generateTotpQrCodeData(): string {
        const randomStr = Math.random().toString(36);
        const secret = Base32Util.encode(randomStr).substr(0, 20);
        this.registerTotpSecret = secret;
        return `otpauth://totp/${this.totpQrCodeLabel}?secret=${secret}&issuer=${this.totpQrCodeIssuer}`;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
</style>