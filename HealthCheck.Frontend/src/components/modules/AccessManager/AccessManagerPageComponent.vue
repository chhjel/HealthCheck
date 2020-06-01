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

                <block-component
                    class="mt-4"
                    title="Generate new access token">

                    <access-grid-component
                        class="mt-2"
                        :access-data="accessData"
                        v-model="accessDataInEdit" />
                </block-component>

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
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import DateUtils from  '../../../util/DateUtils';
import LinqUtils from  '../../../util/LinqUtils';
import SettingInputComponent from '../Settings/SettingInputComponent.vue';
import AccessManagerService, { AccessData } from  '../../../services/AccessManagerService';
import { FetchStatus,  } from  '../../../services/abstractions/HCServiceBase';
import BlockComponent from '../../Common/Basic/BlockComponent.vue';
import ModuleConfig from  '../../../models/Common/ModuleConfig';
import ModuleOptions from  '../../../models/Common/ModuleOptions';
import AccessGridComponent, { CreatedAccessData } from './AccessGridComponent.vue';

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

    accessDataInEdit: CreatedAccessData = {
        roles: [],
        modules: []
    };

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

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    
}
</script>

<style scoped lang="scss">
.access-manager-page {
}
</style>
