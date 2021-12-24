<!-- src/components/modules/AccessTokens/AccessTokensPageComponent.vue -->
<template>
    <div class="access-tokens-page">
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            <v-container>
                <h1 class="mb-1">Access Tokens</h1>

                <!-- LOAD PROGRESS -->
                <v-progress-linear 
                    v-if="loadStatus.inProgress"
                    indeterminate color="green"></v-progress-linear>

                <!-- DATA LOAD ERROR -->
                <v-alert :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
                </v-alert>

                <!-- CEATED TOKEN DETAILS -->
                <v-alert :value="true" v-if="lastCreatedTokenData != null" type="info">
                New token '{{ lastCreatedTokenData.Name }}' created: <code>{{ lastCreatedTokenData.Token }}</code><br />
                Copy it now, it will never be shown again.
                </v-alert>
                
                <!-- TOKEN USAGE INFO -->
                <v-alert :value="true" v-if="lastCreatedTokenData != null" outline type="info" elevation="2">
                Tokens can be consumed either through query string <code>?x-token=...</code> or header <code>x-token: ...</code>
                </v-alert>

                <v-btn
                    v-if="canCreateNewTokens"
                    @click="onAddNewTokenClicked"
                    class="mb-3">
                    <v-icon size="20px" class="mr-2">add</v-icon>
                    Add new
                </v-btn>

                <div v-if="canViewTokenData"
                    class="token-items">
                    <block-component
                        class="token-item mb-3"
                        v-for="(token, tokenIndex) in tokens"
                        :key="`token-${tokenIndex}`"
                        :class="{ 'token-item--expired': token.IsExpired }">

                        <div class="token-item--title">{{ token.Name }}</div>

                        <div class="token-item--metadata">
                            <v-tooltip bottom>
                                <template v-slot:activator="{ on }">
                                    <div class="token-item--created-at" v-on="on">
                                        <v-icon>vpn_key</v-icon>
                                        {{ token.CreatedAtSummary }}
                                    </div>
                                </template>
                                Created {{ formatDate(token.CreatedAt) }}
                            </v-tooltip>
                            
                            <v-tooltip bottom>
                                <template v-slot:activator="{ on }">
                                    <div class="token-item--last-used-at" v-on="on">
                                        <v-icon v-if="token.LastUsedAt != null">visibility</v-icon>
                                        <v-icon v-else>visibility_off</v-icon>
                                        {{ token.LastUsedAtSummary }}
                                    </div>
                                </template>
                                <span v-if="token.LastUsedAt != null">Last used {{ formatDate(token.LastUsedAt) }}</span>
                                <span v-else>Not used yet</span>
                            </v-tooltip>

                            <v-tooltip bottom v-if="token.ExpiresAt != null">
                                <template v-slot:activator="{ on }">
                                    <div class="token-item--expires-at" v-on="on">
                                        <v-icon v-if="token.IsExpired">timer_off</v-icon>
                                        <v-icon v-else>timer</v-icon>
                                        {{ token.ExpiresAtSummary }}
                                    </div>
                                </template>
                                <span v-if="token.IsExpired">This token expired {{ formatDate(token.ExpiresAt) }}</span>
                                <span v-else>Expires {{ formatDate(token.ExpiresAt) }}</span>
                            </v-tooltip>
                        </div>

                        <div class="token-item--roles">
                            <span class="token-item--roles--header">Roles:</span>
                            <span class="token-item--roles--item"
                                v-for="(role, roleIndex) in token.Roles"
                                :key="`token-${tokenIndex}-role-${roleIndex}`">
                                {{ role }}
                            </span>
                            <span v-if="token.Roles.length == 0">
                                None
                            </span>
                        </div>

                        <div class="token-item--modules">
                            <span class="token-item--modules--header">Modules:</span>
                            <span class="token-item--modules--item"
                                v-for="(module, moduleIndex) in token.Modules"
                                :key="`token-${tokenIndex}-module-${moduleIndex}`">
                                <span class="token-item--modules--item--header">{{ module.ModuleId }}</span>
                                
                                <span v-if="module.Options.length > 0">
                                    with access to
                                    <span class="token-item--modules--item--option"
                                        v-for="(option, optionIndex) in module.Options"
                                        :key="`token-${tokenIndex}-module-${moduleIndex}-option-${optionIndex}`">
                                        <code>{{ option }}</code>
                                        <span v-if="module.Options.length >= 2 && optionIndex == module.Options.length - 2">and</span>
                                        <span v-else-if="module.Options.length >= 2 && optionIndex < module.Options.length - 2">,</span>
                                    </span>
                                </span>
                                <span v-if="module.Options.length == 0">
                                    without any specific access options
                                </span>

                                <span v-if="module.Categories.length > 0">
                                    <br />
                                    * Access limited to the following categories:
                                    <span class="token-item--modules--item--option"
                                        v-for="(cat, catIndex) in module.Categories"
                                        :key="`token-${tokenIndex}-module-${moduleIndex}-cat-${catIndex}`">
                                        <code>{{ cat }}</code>
                                        <span v-if="module.Categories.length >= 2 && catIndex == module.Categories.length - 2">and</span>
                                        <span v-else-if="module.Categories.length >= 2 && catIndex < module.Categories.length - 2">,</span>
                                    </span>
                                </span>

                                <span v-if="module.Ids && module.Ids.length > 0">
                                    <br />
                                    * Access limited to the following:
                                    <span class="token-item--modules--item--option"
                                        v-for="(id, idIndex) in module.Ids"
                                        :key="`token-${tokenIndex}-module-${moduleIndex}-id-${idIndex}`">
                                        <code>{{ id }}</code>
                                        <span v-if="module.Ids.length >= 2 && idIndex == module.Ids.length - 2">and</span>
                                        <span v-else-if="module.Ids.length >= 2 && idIndex < module.Ids.length - 2">,</span>
                                    </span>
                                </span>
                            </span>
                            <span v-if="token.Modules.length == 0">
                                None
                            </span>
                        </div>
                        
                        <v-btn color="error" small
                            v-if="canDeleteToken"
                            :loading="loadStatus.inProgress"
                            :disabled="loadStatus.inProgress"
                            @click="deleteToken(token)">Delete</v-btn>
                    </block-component>
                </div>

            </v-container>
        </v-flex>
        </v-layout>
        </v-container>
        
        
        <!-- ##################### -->
        <!-- ###### DIALOGS ######-->
        <v-dialog v-model="deleteTokenDialogVisible"
            @keydown.esc="deleteTokenDialogVisible = false"
            max-width="350"
            content-class="delete-token-dialog">
            <v-card style="background-color: #f4f4f4">
                <v-card-title class="headline">Confirm deletion</v-card-title>
                <v-card-text>
                    {{ deleteTokenDialogText }}
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="primary" @click="deleteTokenDialogVisible = false">Cancel</v-btn>
                    <v-btn color="error" @click="confirmDeleteToken(tokenToBeDeleted)">Delete it</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <!-- ##################### -->
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

                    <edit-access-token-component
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
                        :disabled="loadStatus.inProgress || !enableCreateTokenButton"
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
        <!-- ##################### -->
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import DateUtils from  '../../../util/DateUtils';
import LinqUtils from  '../../../util/LinqUtils';
import AccessTokensService, { AccessData, CreatedAccessData, CreateNewTokenResponse, TokenData } from  '../../../services/AccessTokensService';
import { FetchStatus,  } from  '../../../services/abstractions/HCServiceBase';
import BlockComponent from '../../Common/Basic/BlockComponent.vue';
import ModuleConfig from  '../../../models/Common/ModuleConfig';
import ModuleOptions from  '../../../models/Common/ModuleOptions';
import EditAccessTokenComponent from './EditAccessTokenComponent.vue';

@Component({
    components: {
        BlockComponent,
        EditAccessTokenComponent
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

    deleteTokenDialogVisible: boolean = false;
    deleteTokenDialogText: string = '';
    tokenToBeDeleted: TokenData | null = null;

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

    get enableCreateTokenButton(): boolean {
        return this.accessDataInEdit.Modules.length > 0;
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
            if (x.CreatedAt != null)
            {
                x.CreatedAt = new Date(x.CreatedAt);
            }
            if (x.ExpiresAt != null)
            {
                x.ExpiresAt = new Date(x.ExpiresAt);
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

        const isExpired = (this.accessDataInEdit.ExpiresAt != null && this.accessDataInEdit.ExpiresAt < new Date());
        const expirationSummary = (this.accessDataInEdit.ExpiresAt == null)
            ? ''
            : (isExpired) 
                ? 'Expired'
                : `Expires at ${this.formatDate(this.accessDataInEdit.ExpiresAt)}`;
        this.tokens.push({
            Id: createdToken.Id,
            Name: createdToken.Name,
            CreatedAt: new Date(),
            CreatedAtSummary: 'Created just now',
            LastUsedAt: null,
            LastUsedAtSummary: 'Not used yet',
            ExpiresAt: this.accessDataInEdit.ExpiresAt,
            ExpiresAtSummary: expirationSummary,
            IsExpired: isExpired,
            Roles: this.accessDataInEdit.Roles.map(x => x),
            Modules: this.accessDataInEdit.Modules.map(x => x)
        });

        this.accessDataInEdit = this.defaultNewTokenData();
    }

    formatDate(date: Date): string {
        return DateUtils.FormatDate(date, "dddd dd. MMMM yyyy HH:mm:ss");
    }

    defaultNewTokenData(): CreatedAccessData {
        return {
            Name: 'New Token',
            Roles: [],
            Modules: [],
            ExpiresAt: null
        };
    }

    deleteToken(token: TokenData): void {
        this.tokenToBeDeleted = token;
        this.deleteTokenDialogText = `Are your sure you want to delete the token '${token.Name}'?`;
        this.deleteTokenDialogVisible = true;
    }

    confirmDeleteToken(token: TokenData): void {
        this.service.DeleteToken(token.Id, this.loadStatus, { onSuccess: (data) => {
            const index = this.tokens.findIndex(x => x.Id == token.Id);
            Vue.delete(this.tokens, index);
        }});
        this.deleteTokenDialogVisible = false;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    
}
</script>

<style scoped lang="scss">
.access-tokens-page {
    .token-items {
        .token-item {
            &.token-item--expired {
                background-color: #fdd;;
            }

            .token-item--title {
                font-size: 18px;
                font-weight: 600;
            }
            .token-item--roles {
                margin-bottom: 5px;

                .token-item--roles--item {
                    display: inline-block;
                    padding: 0px 2px;
                    margin-right: 4px;
                    box-shadow: 0 0 3px 0px #b5b5b5;
                    background-color: #eef;
                    margin-right: 10px;
                }
            }
            .token-item--modules {
                .token-item--modules--item {
                    display: block;
                    margin-left: 10px;
                    padding: 2px;

                    .token-item--modules--item--header {
                        font-weight: 600;
                    }

                    .token-item--modules--item--option {
                        margin-right: 7px;
                    }
                }
            }
            .token-item--metadata {
                margin-top: 5px;
                margin-bottom: 5px;

                .token-item--created-at {
                    display: inline-flex;
                    align-items: flex-end;
                    cursor: help;
                    margin-right: 10px;
                }
                .token-item--last-used-at {
                    display: inline-flex;
                    align-items: flex-end;
                    cursor: help;
                    margin-right: 10px;
                }
                .token-item--expires-at {
                    display: inline-flex;
                    align-items: flex-end;
                    cursor: help;
                }
            }
        }
    }
} 
</style>
