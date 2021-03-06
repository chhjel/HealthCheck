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
                            
                            <v-text-field 
                                v-if="show2FAInput"
                                v-model="twoFactorCode"
                                :disabled="loadStatus.inProgress"
                                v-on:keyup.enter="onLoginClicked"
                                label="Two factor code"
                                placeholder=" "
                                type="text"
                                class="pt-0 mt-2"
                                loading>
                                <template v-slot:progress>
                                    <v-progress-linear
                                    :value="twoFactorInputProgress"
                                    :color="twoFactorInputColor"
                                    height="7"
                                    ></v-progress-linear>
                                </template>
                            </v-text-field>
                        </div>

                        <v-btn round color="primary" large class="mt-4 login-button"
                            @click.prevent="onLoginClicked"
                            :disabled="loadStatus.inProgress">
                            <!-- <v-icon dark left>login</v-icon> -->
                            <span style="white-space: normal;">Sign in</span>
                        </v-btn>

                        <v-progress-linear color="primary" indeterminate v-if="loadStatus.inProgress"></v-progress-linear>
                    
                        <div v-if="error != null && error.length > 0" class="error--text mt-4">
                            <b v-if="!showErrorAsHtml">{{ error }}</b>
                            <div v-if="showErrorAsHtml" v-html="error"></div>
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
import IntegratedLoginService, { HCIntegratedLoginRequest, HCIntegratedLoginResult } from '../../../services/IntegratedLoginService';
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
    current2FACodeProgress: number = 0;
    twoFactorIntervalId: number = 0;

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

    get show2FACodeExpirationTime(): boolean {
        return !!this.globalOptions.IntegratedLoginCurrent2FACodeExpirationTime;
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

    update2FAProgress(): void {
        if (!this.show2FACodeExpirationTime)
        {
            return; 
        }

        const initialDate = new Date(this.globalOptions.IntegratedLoginCurrent2FACodeExpirationTime || '');
        const lifetime = this.globalOptions.IntegratedLogin2FACodeLifetime;
        let mod = (((new Date().getTime() - initialDate.getTime()) / 1000) % lifetime);
        if (mod < 0)
        {
            mod = mod + lifetime;
        }

        const timeLeft = lifetime - mod;
        const percentage = timeLeft / lifetime;
        this.current2FACodeProgress = percentage * 100;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onLoginClicked(): void {
        this.login();
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
