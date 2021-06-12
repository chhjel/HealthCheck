<!-- src/components/profile/HealthCheckProfileComponent.vue -->
<template>
    <div>
        <div v-if="profileOptions.Username">
            <div class="meta-header">Username:</div> <div class="username">{{ profileOptions.Username }}</div>
        </div>

        <div v-if="profileOptions.ShowHealthCheckRoles">
            <div class="meta-header">Roles:</div>
            <div class="userrole">{{ userRoles.join(', ') }}</div>
        </div>
        
        <div v-if="profileOptions.BodyHtml" v-html="profileOptions.BodyHtml" class="mt-2 mb-2"></div>
        
        <block-component v-if="profileOptions.TotpElevationEnabled || profileOptions.AddTotpEnabled || profileOptions.RemoveTotpEnabled"
            title="Time-based One-time Password authentication"
            class="mt-4">

            <div class="mt-4">
                <v-btn v-if="profileOptions.TotpElevationEnabled"
                    round color="primary" large
                    @click.prevent="elevateTotpDialogVisible = true"
                    :disabled="disableTotpElevate">
                    Elevate access using TOTP
                </v-btn>
                
                <v-btn v-if="profileOptions.AddTotpEnabled"
                    round color="primary" large
                    @click.prevent="addTotpDialogVisible = true"
                    :disabled="disableTotpAdd">
                    Register TOTP
                </v-btn>

                <v-btn v-if="profileOptions.RemoveTotpEnabled"
                    round color="error" large
                    @click.prevent="removeTotpDialogVisible = true"
                    :disabled="disableTotpRemove">
                    Remove TOTP
                </v-btn>
            </div>

            <p class="status-text">{{ totpStatus }}</p>
        </block-component>

        <block-component v-if="profileOptions.WebAuthnElevationEnabled || profileOptions.AddWebAuthnEnabled || profileOptions.RemoveWebAuthnEnabled"
            title="Web Authentication (WebAuthn)"
            class="mt-4">

            <div class="mt-4">
                <v-btn v-if="profileOptions.WebAuthnElevationEnabled"
                    round color="primary" large
                    @click.prevent="elevateWebAuthnDialogVisible = true"
                    :disabled="disableWebAuthnElevate">
                    Elevate access using WebAuthn
                </v-btn>
                
                <v-btn v-if="profileOptions.AddWebAuthnEnabled"
                    round color="primary" large
                    @click.prevent="addWebAuthnDialogVisible = true"
                    :disabled="disableWebAuthnAdd">
                    Register WebAuthn
                </v-btn>

                <v-btn v-if="profileOptions.RemoveWebAuthnEnabled"
                    round color="error" large
                    @click.prevent="removeWebAuthnDialogVisible = true"
                    :disabled="disableWebAuthnRemove">
                    Remove WebAuthn
                </v-btn>
            </div>

            <p class="status-text">{{ webAuthnStatus }}</p>
        </block-component>

        <!-- WEBAUTHN DIALOGS -->
        <v-dialog v-model="elevateWebAuthnDialogVisible"
            @keydown.esc="elevateWebAuthnDialogVisible = false"
            scrollable
            persistent
            max-width="500"
            content-class="profile-dialog">
            <v-card style="background-color: #f4f4f4">
                <v-toolbar class="elevation-0">
                    <v-toolbar-title>Elevate access using WebAuthn</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-btn icon @click="elevateWebAuthnDialogVisible = false">
                        <v-icon>close</v-icon>
                    </v-btn>
                </v-toolbar>

                <v-divider></v-divider>
                
                <v-card-text>
                    <v-btn
                        round color="primary" large
                        @click.prevent="elevateWebAuthn"
                        :disabled="disableWebAuthnElevate">
                        Elevate access
                    </v-btn>

                    <v-alert :value="webAuthnElevateStatus.failed" type="error">
                    {{ webAuthnElevateStatus.errorMessage }}
                    </v-alert>

                    <div class="success-result">{{ webAuthnElevationSuccessMessage }}</div>
                    <div class="error-result">{{ webAuthnElevationError }}</div>
                </v-card-text>

                <v-divider></v-divider>
                <v-card-actions >
                    <v-spacer></v-spacer>
                    <v-btn :disabled="webAuthnElevateStatus.inProgress"
                            @click="elevateWebAuthnDialogVisible = false">Close</v-btn>
                </v-card-actions>
            </v-card>
        
        </v-dialog>

        <v-dialog v-model="addWebAuthnDialogVisible"
            @keydown.esc="addWebAuthnDialogVisible = false"
            scrollable
            persistent
            max-width="500"
            content-class="profile-dialog">
            <v-card style="background-color: #f4f4f4">
                <v-toolbar class="elevation-0">
                    <v-toolbar-title>Register WebAuthn authenticator</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-btn icon @click="addWebAuthnDialogVisible = false">
                        <v-icon>close</v-icon>
                    </v-btn>
                </v-toolbar>

                <v-divider></v-divider>
                
                <v-card-text>
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
                        :disabled="disableWebAuthnAdd">
                        Register WebAuthn
                    </v-btn>

                    <v-alert :value="webAuthnAddLoadStatus.failed" type="error">
                    {{ webAuthnAddLoadStatus.errorMessage }}
                    </v-alert>

                    <div class="success-result">{{ webAuthnAddSuccessMessage }}</div>
                    <div class="error-result">{{ webAuthnAddError }}</div>
                </v-card-text>

                <v-divider></v-divider>
                <v-card-actions >
                    <v-spacer></v-spacer>
                    <v-btn :disabled="webAuthnAddLoadStatus.inProgress"
                            @click="addWebAuthnDialogVisible = false">Close</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <v-dialog v-model="removeWebAuthnDialogVisible"
            @keydown.esc="removeWebAuthnDialogVisible = false"
            scrollable
            persistent
            max-width="500"
            content-class="profile-dialog">
            <v-card style="background-color: #f4f4f4">
                <v-toolbar class="elevation-0">
                    <v-toolbar-title>Remove WebAuthn authenticator</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-btn icon @click="removeWebAuthnDialogVisible = false">
                        <v-icon>close</v-icon>
                    </v-btn>
                </v-toolbar>

                <v-divider></v-divider>
                
                <v-card-text>
                    <p>Confirm removal of WebAuthn authenticator from your account.</p>

                    <input-component
                        name="Confirm account password"
                        autocomplete="current-password"
                        v-model="removeWebAuthnPassword"
                        :disabled="disableWebAuthnAdd"
                        type="password"
                        :clearable="true"
                    ></input-component>

                    <v-btn
                        round color="primary" large
                        @click.prevent="removeWebAuthn"
                        :disabled="disableWebAuthnRemove">
                        Remove WebAuthn
                    </v-btn>

                    <v-alert :value="webAuthnRemoveStatus.failed" type="error">
                    {{ webAuthnRemoveStatus.errorMessage }}
                    </v-alert>
                    
                    <div class="success-result">{{ webAuthnRemoveSuccessMessage }}</div>
                    <div class="error-result">{{ webAuthnRemoveError }}</div>
                </v-card-text>

                <v-divider></v-divider>
                <v-card-actions >
                    <v-spacer></v-spacer>
                    <v-btn :disabled="webAuthnRemoveStatus.inProgress"
                            @click="removeWebAuthnDialogVisible = false">Close</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <!-- TOTP DIALOGS -->
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
                        :disabled="disableTotpElevate"
                        :clearable="true"
                    ></input-component>

                    <v-btn
                        round color="primary" large class="mt-4"
                        @click.prevent="elevateTotp"
                        :disabled="disableTotpElevate">
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
                        :disabled="disableTotpAdd"
                        :clearable="true"
                    ></input-component>

                    <input-component
                        name="Confirm account password"
                        autocomplete="current-password"
                        v-model="registerTotpPassword"
                        :disabled="disableTotpAdd"
                        type="password"
                        :clearable="true"
                    ></input-component>

                    <v-btn 
                        round color="primary" large
                        @click.prevent="registerTotp"
                        :disabled="disableTotpAdd">
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
                        :disabled="disableTotpRemove"
                        type="password"
                        :clearable="true"
                    ></input-component>

                    <v-btn
                        round color="error" large
                        @click.prevent="removeTotp"
                        :disabled="disableTotpRemove">
                        Remove TOTP
                    </v-btn>

                    <v-alert :value="totpRemoveLoadStatus.failed" type="error">
                    {{ totpRemoveLoadStatus.errorMessage }}
                    </v-alert>
                    
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
import BlockComponent from 'components/Common/Basic/BlockComponent.vue';
import { HCResultPageAction } from "generated/Models/WebUI/HCResultPageAction";
import { HCResultPageActionType } from "generated/Enums/WebUI/HCResultPageActionType";

@Component({
    components: {
        InputComponent,
        BlockComponent
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
    hasElevatedTotp: boolean | null = null;

    totpAddLoadStatus: FetchStatus = new FetchStatus();
    addTotpDialogVisible: boolean = false;
    totpAddError: string = '';
    totpAddSuccessMessage: string = '';
    registerTotpSecret: string = '';
    registerTotpCode: string = '';
    registerTotpPassword: string = '';
    hasRegisteredTotp: boolean | null = null;
    
    totpRemoveLoadStatus: FetchStatus = new FetchStatus();
    removeTotpDialogVisible: boolean = false;
    totpRemoveError: string = '';
    totpRemoveSuccessMessage: string = '';
    removeTotpPassword: string = '';

    // WEBAUTHN
    webAuthnAddLoadStatus: FetchStatus = new FetchStatus();
    addWebAuthnDialogVisible: boolean = false;
    webAuthnAddError: string = '';
    registerWebAuthnPassword: string = '';
    webAuthnAddSuccessMessage: string = '';

    webAuthnElevateStatus: FetchStatus = new FetchStatus();
    elevateWebAuthnDialogVisible: boolean = false;
    webAuthnElevationError: string = '';
    webAuthnElevationSuccessMessage: string = '';
    hasRegisteredWebAuthn: boolean | null = null;
    hasElevatedWebAuthn: boolean | null = null;
    
    webAuthnRemoveStatus: FetchStatus = new FetchStatus();
    removeWebAuthnDialogVisible: boolean = false;
    webAuthnRemoveError: string = '';
    removeWebAuthnPassword: string = '';
    webAuthnRemoveSuccessMessage: string = '';

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

    get disableTotpAdd(): boolean {
        return this.hasRegisteredTotp === true || this.totpAddLoadStatus.inProgress;
    }

    get disableTotpRemove(): boolean {
        return this.hasRegisteredTotp === false || this.totpRemoveLoadStatus.inProgress;
    }

    get disableTotpElevate(): boolean {
        return this.hasElevatedTotp == true || this.totpElevateLoadStatus.inProgress;
    }

    get totpStatus(): string {
        if (this.hasRegisteredTotp === true) {
            return 'TOTP authentication added.';
        }
        else if (this.hasRegisteredTotp === false) {
            return 'TOTP authentication removed.';
        }
        return '';
    }

    get disableWebAuthnAdd(): boolean {
        return this.hasRegisteredWebAuthn === true || this.webAuthnAddLoadStatus.inProgress;
    }

    get disableWebAuthnRemove(): boolean {
        return this.hasRegisteredWebAuthn === false || this.webAuthnRemoveStatus.inProgress;
    }

    get disableWebAuthnElevate(): boolean {
        return this.hasElevatedWebAuthn == true || this.webAuthnElevateStatus.inProgress;
    }

    get webAuthnStatus(): string {
        if (this.hasRegisteredWebAuthn === true) {
            return 'WebAuthn authentication added.';
        }
        else if (this.hasRegisteredWebAuthn === false) {
            return 'WebAuthn authentication removed.';
        }
        return '';
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    elevateTotp(): void {
        this.totpElevateSuccessMessage = '';
        this.service.ElevateTotp(this.totpElevateCode, this.totpElevateLoadStatus,
            {
                onSuccess: (d) => {
                    this.totpElevationError = d.Error || '';
                    if (!this.totpElevationError)
                    {
                        this.totpElevateSuccessMessage = 'Elevated successfully';
                        this.hasElevatedTotp = true;

                        const action = (d as any).Data as HCResultPageAction;
                        const actionType = action ? action.Type : HCResultPageActionType.None;
                        if (actionType === HCResultPageActionType.Refresh)
                        {
                            window.location.reload();
                        }
                        if (actionType === HCResultPageActionType.Redirect)
                        {
                            window.location.href = action.RedirectTarget;
                        }
                    }
                }
            }
        );
    }

    registerTotp(): void {
        this.totpAddSuccessMessage = '';
        this.service.RegisterTotp(this.registerTotpSecret, this.registerTotpCode, this.registerTotpPassword, this.totpAddLoadStatus,
            {
                onSuccess: (d) => {
                    this.totpAddError = d.Error || '';
                    this.registerTotpPassword = '';
                    if (!this.totpAddError)
                    {
                        this.totpAddSuccessMessage = 'TOTP authenticator added';
                        this.registerTotpSecret = '';
                        this.registerTotpCode = '';
                        this.hasRegisteredTotp = true;
                    }
                }
            });
    }

    removeTotp(): void {
        this.totpRemoveSuccessMessage = '';
        this.service.RemoveTotp(this.removeTotpPassword, this.totpRemoveLoadStatus,
            {
                onSuccess: (d) => {
                    this.totpRemoveError = d.Error || '';
                    this.removeTotpPassword = '';
                    if (!this.totpRemoveError)
                    {
                        this.totpRemoveSuccessMessage = 'TOTP authenticator removed';
                        this.hasRegisteredTotp = false;
                    }
                }
            }
        );
    }

    elevateWebAuthn(): void {
        this.webAuthnElevationSuccessMessage = '';
        this.service.CreateWebAuthnAssertionOptions(this.username, this.webAuthnElevateStatus, {
            onSuccess: (d) => {
                if (d.Success)
                {
                    this.onWebAuthnAssertionOptionsCreated(d.Data);
                }
                else {
                    this.webAuthnElevationError = d.Error;
                }
            }
        });
    }
    async onWebAuthnAssertionOptionsCreated(options: any): Promise<void> {
        if (options.status !== "ok")
        {
            this.webAuthnElevationError = 'Failed to elevate access.';
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

            this.service.ElevateWebAuthn(payload, this.webAuthnElevateStatus,
                {
                    onSuccess: (d) => {
                        this.webAuthnElevationError = d.Error || '';
                        if (!this.webAuthnElevationError)
                        {
                            this.webAuthnElevationSuccessMessage = 'Elevated successfully';
                            this.hasElevatedWebAuthn = true;
                        }
                    }
                }
            );
        } catch (e) {
            this.webAuthnElevationError = 'Assertion failed';
            console.error(e);
        }
    }

    removeWebAuthn(): void {
        this.webAuthnRemoveSuccessMessage = '';
        this.service.RemoveWebAuthn(this.removeWebAuthnPassword, this.webAuthnRemoveStatus,
            {
                onSuccess: (d) => {
                    this.webAuthnRemoveError = (d as any).error || '';
                    this.removeWebAuthnPassword = '';
                    if (!this.webAuthnRemoveError)
                    {
                        this.webAuthnRemoveSuccessMessage = 'WebAuthn authenticator removed';
                        this.hasRegisteredWebAuthn = false;
                    }
                }
            }
        );
    }

    registerWebAuthn(): void {
        this.webAuthnAddError = '';
        this.webAuthnAddSuccessMessage = '';
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
                    this.webAuthnAddError = d.Error || '';
                    if (!this.webAuthnAddError)
                    {
                        this.webAuthnAddSuccessMessage = 'WebAuthn registered successfully.';
                        this.hasRegisteredWebAuthn = true;
                    }
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
.meta-header {
    display: inline-block;
    margin-right: 5px;
    font-size: 16px;
}
.username {
    display: inline-block;
    font-size: 16px;
}
.userrole {
    display: inline-block;
    font-size: 16px;
}
.status-text {
    text-align: center;
    margin-top: 20px;
    margin-bottom: -20px;
}
</style>

<style lang="scss">
.profile-dialog {
    text-align: center;
}
</style>