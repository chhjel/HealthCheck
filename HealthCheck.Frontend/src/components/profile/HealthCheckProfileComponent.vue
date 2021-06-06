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
        
        <v-btn v-if="profileOptions.TotpElevationEnabled"
            round color="primary" large
            @click.prevent="elevateTotpDialogVisible = true"
            :disabled="totpElevateLoadStatus.inProgress">
            Elevate access using TOTP
        </v-btn>
        
        <v-btn v-if="profileOptions.AddTotpEnabled"
            round color="primary" large
            @click.prevent="addTotpDialogVisible = true"
            :disabled="totpAddLoadStatus.inProgress">
            Register TOTP
        </v-btn>

        <v-btn v-if="profileOptions.RemoveTotpEnabled"
            round color="error" large
            @click.prevent="removeTotpDialogVisible = true"
            :disabled="totpRemoveLoadStatus.inProgress">
            Remove TOTP
        </v-btn>

        <div v-if="profileOptions.WebAuthnElevationEnabled" style="border: 2px solid gray; margin: 20px; padding: 10px;">
            <h2>WebAuthn elevation</h2>
            <v-btn
                round color="primary" large
                @click.prevent="elevateWebAuthn"
                :disabled="webAuthnLoadStatus.inProgress">
                Elevate access
            </v-btn>
            <div class="error-result">{{ webAuthnElevationError }}</div>
        </div>

        <div v-if="profileOptions.AddWebAuthnEnabled" style="border: 2px solid gray; margin: 20px; padding: 10px;">
            <!--
            addWebAuthnDialogVisible: boolean = false;
            -->
            <h2>Add WebAuthn</h2>
            <input-component
                name="Confirm account password"
                autocomplete="current-password"
                v-model="registerWebAuthnPassword"
                :disabled="webAuthnAddLoadStatus.inProgress"
                type="password"
                :clearable="true"
            ></input-component>

            <v-btn
                round color="primary" large
                @click.prevent="registerWebAuthn"
                :disabled="webAuthnLoadStatus.inProgress">
                Register WebAuthn
            </v-btn>

            <v-alert :value="webAuthnAddLoadStatus.failed" type="error">
            {{ webAuthnAddLoadStatus.errorMessage }}
            </v-alert>

            <div class="success-result">{{ webAuthnAddSuccessMessage }}</div>
            <div class="error-result">{{ webAuthnAddError }}</div>
        </div>

        <div v-if="profileOptions.RemoveWebAuthnEnabled" style="border: 2px solid gray; margin: 20px; padding: 10px;">
            <h2>Remove WebAuthn</h2>
            <input-component
                name="Confirm account password"
                autocomplete="current-password"
                v-model="removeWebAuthnPassword"
                :disabled="totpAddLoadStatus.inProgress"
                type="password"
                :clearable="true"
            ></input-component>

            <v-btn
                round color="primary" large
                @click.prevent="removeWebAuthn"
                :disabled="webAuthnLoadStatus.inProgress">
                Remove WebAuthn
            </v-btn>
            <div class="error-result">{{ webAuthnRemoveError }}</div>

            <v-alert :value="webAuthnLoadStatus.failed" type="error">
            {{ webAuthnLoadStatus.errorMessage }}
            </v-alert>
        </div>

        <v-dialog v-model="elevateTotpDialogVisible"
            @keydown.esc="elevateTotpDialogVisible = false"
            scrollable
            persistent
            max-width="500"
            content-class="profile-dialog">
            <v-card style="background-color: #f4f4f4">
                <v-toolbar class="elevation-0">
                    <v-toolbar-title>Elevate access using TOTP</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-btn icon @click="elevateTotpDialogVisible = false">
                        <v-icon>close</v-icon>
                    </v-btn>
                </v-toolbar>

                <v-divider></v-divider>
                
                <v-card-text>
                    <input-component
                        name="TOTP code"
                        v-model="totpElevateCode"
                        :disabled="totpElevateLoadStatus.inProgress"
                        :clearable="true"
                    ></input-component>

                    <v-btn
                        round color="primary" large class="mt-4"
                        @click.prevent="elevateTotp"
                        :disabled="totpElevateLoadStatus.inProgress">
                        Elevate access
                    </v-btn>

                    <v-alert :value="totpElevateLoadStatus.failed" type="error">
                    {{ totpElevateLoadStatus.errorMessage }}
                    </v-alert>

                    <div class="success-result">{{ totpElevateSuccessMessage }}</div>
                    <div class="error-result">{{ totpElevationError }}</div>
                </v-card-text>

                <v-divider></v-divider>
                <v-card-actions >
                    <v-spacer></v-spacer>
                    <v-btn :disabled="totpElevateLoadStatus.inProgress"
                            @click="elevateTotpDialogVisible = false">Close</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <v-dialog v-model="addTotpDialogVisible"
            @keydown.esc="addTotpDialogVisible = false"
            scrollable
            persistent
            max-width="500"
            content-class="profile-dialog">
            <v-card style="background-color: #f4f4f4">
                <v-toolbar class="elevation-0">
                    <v-toolbar-title>Register TOTP authenticator</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-btn icon @click="addTotpDialogVisible = false">
                        <v-icon>close</v-icon>
                    </v-btn>
                </v-toolbar>

                <v-divider></v-divider>
                
                <v-card-text>
                    <p>Scan the QR code with an authenticator app</p>
                    <canvas ref="qrCodeCanvas"></canvas>
                    <p>Or optionally enter this secret manually in your app of choice: <code>{{ registerTotpSecret }}</code></p>

                    <input-component
                        name="Code from authenticator"
                        autocomplete="one-time-code"
                        v-model="registerTotpCode"
                        :disabled="totpAddLoadStatus.inProgress"
                        :clearable="true"
                    ></input-component>

                    <input-component
                        name="Confirm account password"
                        autocomplete="current-password"
                        v-model="registerTotpPassword"
                        :disabled="totpAddLoadStatus.inProgress"
                        type="password"
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

                    <div class="success-result">{{ totpAddSuccessMessage }}</div>
                    <div class="error-result">{{ totpAddError }}</div>
                </v-card-text>

                <v-divider></v-divider>
                <v-card-actions >
                    <v-spacer></v-spacer>
                    <v-btn :disabled="totpAddLoadStatus.inProgress"
                            @click="addTotpDialogVisible = false">Close</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <v-dialog v-model="removeTotpDialogVisible"
            @keydown.esc="removeTotpDialogVisible = false"
            scrollable
            persistent
            max-width="500"
            content-class="profile-dialog">
            <v-card style="background-color: #f4f4f4">
                <v-toolbar class="elevation-0">
                    <v-toolbar-title>Remove TOTP authenticator</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-btn icon @click="removeTotpDialogVisible = false">
                        <v-icon>close</v-icon>
                    </v-btn>
                </v-toolbar>

                <v-divider></v-divider>
                
                <v-card-text>
                    <p>Confirm removal of TOTP authenticator from your account.</p>

                    <input-component
                        name="Confirm account password"
                        autocomplete="current-password"
                        v-model="removeTotpPassword"
                        :disabled="totpRemoveLoadStatus.inProgress"
                        type="password"
                        :clearable="true"
                    ></input-component>

                    <v-btn
                        round color="error" large
                        @click.prevent="removeTotp"
                        :disabled="totpRemoveLoadStatus.inProgress">
                        Remove TOTP
                    </v-btn>
                    
                    <div class="success-result">{{ totpRemoveSuccessMessage }}</div>
                    <div class="error-result">{{ totpRemoveError }}</div>
                </v-card-text>

                <v-divider></v-divider>
                <v-card-actions >
                    <v-spacer></v-spacer>
                    <v-btn :disabled="totpRemoveLoadStatus.inProgress"
                            @click="removeTotpDialogVisible = false">Close</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
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
import { HCVerifyWebAuthnAssertionModel } from "generated/Models/WebUI/HCVerifyWebAuthnAssertionModel";
import { Ecc, QrCode } from 'util/QRCodeUtil';

@Component({
    components: {
        InputComponent
    }
})
export default class HealthCheckProfileComponent extends Vue
{
    service: IntegratedProfileService = new IntegratedProfileService(this.globalOptions.EndpointBase, this.globalOptions.InludeQueryStringInApiCalls);

    // TOTP
    totpElevateLoadStatus: FetchStatus = new FetchStatus();
    elevateTotpDialogVisible: boolean = false;
    totpElevationError: string = '';
    totpElevateSuccessMessage: string = '';
    totpElevateCode: string = '';

    totpAddLoadStatus: FetchStatus = new FetchStatus();
    addTotpDialogVisible: boolean = false;
    totpAddError: string = '';
    totpAddSuccessMessage: string = '';
    registerTotpSecret: string = '';
    registerTotpCode: string = '';
    registerTotpPassword: string = '';
    
    totpRemoveLoadStatus: FetchStatus = new FetchStatus();
    removeTotpDialogVisible: boolean = false;
    totpRemoveError: string = '';
    totpRemoveSuccessMessage: string = '';
    removeTotpPassword: string = '';

    // WEBAUTHN
    webAuthnAddLoadStatus: FetchStatus = new FetchStatus();
    addWebAuthnDialogVisible: boolean = false;
    webAuthnAddError: string = '';
    webAuthnAddSuccessMessage: string = '';
    registerWebAuthnPassword: string = '';

    // Other
    webAuthnLoadStatus: FetchStatus = new FetchStatus();
    webAuthnElevationError: string = '';
    webAuthnRemoveError: string = '';
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
        this.service.ElevateTotp(this.totpElevateCode, this.totpElevateLoadStatus,
            { onSuccess: (d) => this.totpElevationError = (d as any).error || '' });
    }

    registerTotp(): void {
        this.totpAddSuccessMessage = '';
        this.service.RegisterTotp(this.registerTotpSecret, this.registerTotpCode, this.registerTotpPassword, this.totpAddLoadStatus,
            {
                onSuccess: (d) => {
                    this.totpAddError = (d as any).error || '';
                    this.registerTotpPassword = '';
                    if (!this.totpAddError)
                    {
                        this.totpAddSuccessMessage = 'TOTP authenticator added';
                        this.registerTotpSecret = '';
                        this.registerTotpCode = '';
                    }
                }
            });
    }

    removeTotp(): void {
        this.totpRemoveSuccessMessage = '';
        this.service.RemoveTotp(this.removeTotpPassword, this.totpRemoveLoadStatus,
            {
                onSuccess: (d) => {
                    this.totpRemoveError = (d as any).error || '';
                    this.removeTotpPassword = '';
                    if (!this.totpRemoveError)
                    {
                        this.totpRemoveSuccessMessage = 'TOTP authenticator removed';
                    }
                }
            });
    }

    elevateWebAuthn(): void {
        const payload = {} as HCVerifyWebAuthnAssertionModel;
        this.service.ElevateWebAuthn(payload, this.webAuthnLoadStatus,
            { onSuccess: (d) => this.webAuthnElevationError = (d as any).error || '' });
    }

    removeWebAuthn(): void {
        this.service.RemoveWebAuthn(this.removeWebAuthnPassword, this.webAuthnLoadStatus,
            { onSuccess: (d) => this.webAuthnRemoveError = (d as any).error || '' });
    }

    registerWebAuthn(): void {
        this.webAuthnAddError = '';
        this.service.CreateWebAuthnRegistrationOptions(this.username, this.registerWebAuthnPassword, this.webAuthnAddLoadStatus, {
            onSuccess: (d) => {
                if (d.Success)
                {
                    this.onWebAuthnRegistrationOptionsCreated(d.Data);
                }
                else {
                    this.webAuthnAddError = d.Error;
                }
            },
            onError: (e) => this.webAuthnAddError = e
        });
    }

    async onWebAuthnRegistrationOptionsCreated(options: any): Promise<void> {
        if (options.status !== "ok")
        {
            this.webAuthnAddError = 'Failed to register WebAuthn.';
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
            var msg = "Could not create credentials in browser. Probably either because the username is already registered with your authenticator, or you cancelled the process. Please change username or authenticator, or try again."
            this.webAuthnAddError = msg;
            console.error(msg, e);
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

            this.service.RegisterWebAuthn(this.registerWebAuthnPassword, registerPayload, this.webAuthnAddLoadStatus, {
                onSuccess: (d) => {
                    this.registerWebAuthnPassword = '';
                }
            });
        } catch (e) {
            this.webAuthnAddError = e;
            console.error('RegisterWebAuthn failed');
            console.error(e);
        }
    }

    generateQrCode(): void {
        const data = this.generateTotpQrCodeData();
        const canvas = this.$refs.qrCodeCanvas as HTMLCanvasElement;
        
        const qr = QrCode.encodeText(data, Ecc.MEDIUM);
        qr.drawCanvas(10, 2, canvas);
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
.error-result {
    font-weight: 600;
    color: #8a2222;
    min-height: 21px;
}
</style>

<style lang="scss">
.profile-dialog {
    text-align: center;
}
</style>