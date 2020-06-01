<!-- src/components/modules/AccessManager/AccessManagerPageComponent.vue -->
<template>
    <div class="access-manager-page">
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            <v-container>
                <h1 class="mb-4">Access Manager</h1>

                <!-- LOAD PROGRESS -->
                <v-progress-linear 
                    v-if="loadStatus.inProgress"
                    indeterminate color="green"></v-progress-linear>

                <!-- DATA LOAD ERROR -->
                <v-alert :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
                </v-alert>

                <v-btn
                    @click="onAddNewTokenClicked"
                    class="mb-3">
                    <v-icon size="20px" class="mr-2">add</v-icon>
                    Add new
                </v-btn>

                <block-component
                    class="mt-4"
                    title="Generated tokens">
                    List of <br />
                    name | roles | modules |
                    last used some time ago | delete
                </block-component>

            </v-container>
        </v-flex>
        </v-layout>
        </v-container>
        
        <v-dialog v-model="createNewTokenDialogVisible"
            @keydown.esc="createNewTokenDialogVisible = false"
            scrollable
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
import AccessManagerService, { AccessData, CreatedAccessData } from  '../../../services/AccessManagerService';
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
export default class AccessManagerPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;

    service: AccessManagerService = new AccessManagerService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    loadStatus: FetchStatus = new FetchStatus();

    accessData: AccessData = {
        Roles: [],
        ModuleOptions: []
    };

    createNewTokenDialogVisible: boolean = true;
    accessDataInEdit: CreatedAccessData = this.defaultNewTokenData();

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
    
    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.service.GetAccessData(this.loadStatus, { onSuccess: (data) => this.onDataRetrieved(data) });
    }

    onDataRetrieved(data: AccessData): void {
        this.accessData = data;
    }

    onAddNewTokenClicked(): void {
        this.createNewTokenDialogVisible = true;
    }

    onCreateNewTokenClicked(): void {
        this.service.CreateNewToken(this.accessDataInEdit, this.loadStatus, { onSuccess: (data) => this.onNewTokenCreated(data) });
    }

    onNewTokenCreated(createdToken: any): void {
        this.accessDataInEdit = this.defaultNewTokenData();
    }

    defaultNewTokenData(): CreatedAccessData {
        return {
            name: 'New Token',
            roles: [],
            modules: []
        };
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    
}
</script>

<style scoped lang="scss">
.access-manager-page {
}
</style>
