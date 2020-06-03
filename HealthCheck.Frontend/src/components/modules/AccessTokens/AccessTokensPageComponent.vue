<!-- src/components/modules/AccessTokens/AccessTokensPageComponent.vue -->
<template>
    <div class="access-manager-page">
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            <v-container>
                <h1 class="mb-4">Access Tokens</h1>

                <!-- LOAD PROGRESS -->
                <v-progress-linear 
                    v-if="loadStatus.inProgress"
                    indeterminate color="green"></v-progress-linear>

                <!-- DATA LOAD ERROR -->
                <v-alert :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
                </v-alert>

                <!-- DATA LOAD ERROR -->
                <v-alert :value="true" v-if="lastCreatedTokenData != null" type="info">
                New token '{{ lastCreatedTokenData.Name }}' created: <code>{{ lastCreatedTokenData.Token }}</code><br />
                Copy it now, it will never be shown again.
                </v-alert>

                <v-btn
                    v-if="canCreateNewTokens"
                    @click="onAddNewTokenClicked"
                    class="mb-3">
                    <v-icon size="20px" class="mr-2">add</v-icon>
                    Add new
                </v-btn>

                <block-component
                    v-if="canViewTokenData"
                    class="mt-4"
                    title="Generated tokens">

                    <div class="token-item"
                        v-for="(token, tokenIndex) in tokens"
                        :key="`token-${tokenIndex}`">
                        {{ token.Name }}

                        <div class="token-item--roles">
                            <span class="token-item--roles--item"
                                v-for="(role, roleIndex) in token.Roles"
                                :key="`token-${tokenIndex}-role-${roleIndex}`">
                                {{ role }}
                            </span>
                        </div>

                        <div class="token-item--modules">
                            <span class="token-item--modules--item"
                                v-for="(module, moduleIndex) in token.Modules"
                                :key="`token-${tokenIndex}-module-${moduleIndex}`">
                                {{ module.ModuleId }}
                                
                                <span class="token-item--modules--item--option"
                                    v-for="(option, optionIndex) in module.Options"
                                    :key="`token-${tokenIndex}-module-${moduleIndex}-option-${optionIndex}`">
                                    <code>{{ option }}</code>
                                </span>
                            </span>
                        </div>
                        
                        <div class="token-item--last-updated-at" v-if="token.LastUsedAt != null">
                            {{ token.LastUsedAtSummary }} @ {{ token.LastUsedAt }}
                        </div>
                        <div class="token-item--expires-at" v-if="token.ExpiresAt != null">
                            {{ token.ExpiresAtSummary }} @ {{ token.ExpiresAt }}
                        </div>
                        
                        <v-btn color="error"
                            v-if="canDeleteToken"
                            :loading="loadStatus.inProgress"
                            :disabled="loadStatus.inProgress"
                            @click="deleteToken(token.Id)">Delete</v-btn>
                    </div>
                </block-component>

            </v-container>
        </v-flex>
        </v-layout>
        </v-container>
        
        <v-dialog v-model="createNewTokenDialogVisible"
            @keydown.esc="createNewTokenDialogVisible = false"
            scrollable
            :persistent="loadStatus.inProgress"
            max-width="1200"
            content-class="create-access-token-dialog">
            <v-card style="background-color: #f4f4f4">
                <v-toolbar class="elevation-0">
                    <v-toolbar-title class="current-config-dialog__title">Create new access token</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-btn icon
                        @click="createNewTokenDialogVisible = false">
                        <v-icon>close</v-icon>
                    </v-btn>
                </v-toolbar>
                <v-divider></v-divider>
                
                <v-card-text>
                    <access-grid-component
                        :access-data="accessData"
                        :read-only="loadStatus.inProgress"
                        v-model="accessDataInEdit" />
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions >
                    <v-spacer></v-spacer>
                    <v-btn
                        color="primary"
                        :loading="loadStatus.inProgress"
                        :disabled="loadStatus.inProgress"
                        @click="onCreateNewTokenClicked">
                        Create token
                    </v-btn>
                    <v-btn color="secondary"
                        :loading="loadStatus.inProgress"
                        :disabled="loadStatus.inProgress"
                        @click="createNewTokenDialogVisible = false">Close</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import DateUtils from  '../../../util/DateUtils';
import LinqUtils from  '../../../util/LinqUtils';
import SettingInputComponent from '../Settings/SettingInputComponent.vue';
import AccessTokensService, { AccessData, CreatedAccessData, CreateNewTokenResponse, TokenData } from  '../../../services/AccessTokensService';
import { FetchStatus,  } from  '../../../services/abstractions/HCServiceBase';
import BlockComponent from '../../Common/Basic/BlockComponent.vue';
import ModuleConfig from  '../../../models/Common/ModuleConfig';
import ModuleOptions from  '../../../models/Common/ModuleOptions';
import AccessGridComponent from './AccessGridComponent.vue';

@Component({
    components: {
        SettingInputComponent,
        BlockComponent,
        AccessGridComponent
    }
})
export default class AccessTokensPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;

    service: AccessTokensService = new AccessTokensService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    loadStatus: FetchStatus = new FetchStatus();

    tokens: Array<TokenData> = [];
    accessData: AccessData = {
        Roles: [],
        ModuleOptions: []
    };

    createNewTokenDialogVisible: boolean = false;
    accessDataInEdit: CreatedAccessData = this.defaultNewTokenData();
    lastCreatedTokenData: CreateNewTokenResponse | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.loadData();
    }

    created(): void {
    }

    beforeDestroy(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }

    get canViewTokenData(): boolean {
        return this.hasAccess('ViewToken');
    }

    get canCreateNewTokens(): boolean {
        return this.hasAccess('CreateNewToken');
    }

    get canDeleteToken(): boolean {
        return this.hasAccess('DeleteToken');
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.service.GetTokens(this.loadStatus, { onSuccess: (data) => this.onTokensRetrieved(data) });
        this.service.GetAccessData(this.loadStatus, { onSuccess: (data) => this.accessData = data });
    }

    onTokensRetrieved(data: Array<TokenData>): void {
        this.tokens = data.map(x => {
            if (x.LastUsedAt != null)
            {
                x.LastUsedAt = new Date(x.LastUsedAt);
            }
            return x;
        });
    }

    hasAccess(option: string): boolean {
        return this.options.AccessOptions.indexOf(option) != -1;
    }

    onAddNewTokenClicked(): void {
        this.createNewTokenDialogVisible = true;
    }

    onCreateNewTokenClicked(): void {
        this.service.CreateNewToken(this.accessDataInEdit, this.loadStatus, { onSuccess: (data) => this.onNewTokenCreated(data) });
    }

    onNewTokenCreated(createdToken: CreateNewTokenResponse): void {
        this.lastCreatedTokenData = createdToken;
        this.createNewTokenDialogVisible = false;

        this.tokens.push({
            Id: createdToken.Id,
            Name: createdToken.Name,
            LastUsedAt: null,
            LastUsedAtSummary: null,
            ExpiresAt: null,
            ExpiresAtSummary: null,
            Roles: this.accessDataInEdit.Roles.map(x => x),
            Modules: this.accessDataInEdit.Modules.map(x => x)
        });

        this.accessDataInEdit = this.defaultNewTokenData();
    }

    defaultNewTokenData(): CreatedAccessData {
        return {
            Name: 'New Token',
            Roles: [],
            Modules: []
        };
    }

    deleteToken(id: string): void {
        this.service.DeleteToken(id, this.loadStatus, { onSuccess: (data) => {
            const index = this.tokens.findIndex(x => x.Id == id);
            Vue.delete(this.tokens, index);
        }});
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    
}
</script>

<style scoped lang="scss">
/* .access-manager-page {
} */
</style>
