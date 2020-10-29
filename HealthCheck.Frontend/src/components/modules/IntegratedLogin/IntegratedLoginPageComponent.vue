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
                                type="text"
                                label="Username"
                                class="pt-0 mt-5" />
                            <v-text-field 
                                v-model="password"
                                :disabled="loadStatus.inProgress"
                                label="Password"
                                :type="showPassword ? 'text' : 'password'"
                                :append-icon="showPassword ? 'visibility' : 'visibility_off'"
                                @click:append="showPassword = !showPassword"
                                class="pt-0 mt-2" />
                        
                            <div v-if="error != null && error.length > 0" class="error--text mt-4">
                                <b>{{ error }}</b>
                            </div>
                        </div>

                        <!-- LOAD STATUS -->
                        <v-alert
                            :value="loadStatus.failed"
                            type="error">
                        {{ loadStatus.errorMessage }}
                        </v-alert>

                        <v-btn round color="primary" large class="mt-4 login-button"
                            @click.prevent="onLoginClicked"
                            :disabled="loadStatus.inProgress">
                            <!-- <v-icon dark left>login</v-icon> -->
                            <span style="white-space: normal;">Sign in</span>
                        </v-btn>

                        <v-progress-linear color="primary" indeterminate v-if="loadStatus.inProgress"></v-progress-linear>
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
import DateUtils from  '../../../util/DateUtils';
import LinqUtils from  '../../../util/LinqUtils';
import { FetchStatus,  } from  '../../../services/abstractions/HCServiceBase';
import IntegratedLoginService, { HCIntegratedLoginRequest, HCIntegratedLoginResult } from '../../../services/IntegratedLoginService';
import BlockComponent from '../../Common/Basic/BlockComponent.vue';
import UrlUtils from "../../../util/UrlUtils";
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
    showPassword: boolean = false;
    error: string = '';

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
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

    ////////////////
    //  METHODS  //
    //////////////
    login(): void
    {
        if (this.loadStatus.inProgress) {
            return;
        }

        this.error = '';
        let url = UrlUtils.makeAbsolute(document.baseURI, `../login`);
        let payload: HCIntegratedLoginRequest = {
            username: this.username,
            password: this.password,
            twoFactorCode: ''
        };

        let service = new IntegratedLoginService(true);
        service.Login(url, payload, this.loadStatus,
            {
                onSuccess: (result) => {
                    if (result.success)
                    {
                        location.reload();
                    }
                    else
                    {
                        this.error = result.errorMessage;
                    }
                }
            });
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
