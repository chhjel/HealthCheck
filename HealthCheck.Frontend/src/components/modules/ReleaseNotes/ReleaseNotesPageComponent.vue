<!-- src/components/modules/ReleaseNotes/ReleaseNotesPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
            <!-- CONTENT -->
            <v-container fluid fill-height class="content-root">
            <v-layout>
            <v-flex>
            <v-container>
                <!-- LOAD PROGRESS -->
                <v-progress-linear
                    v-if="loadStatus.inProgress"
                    indeterminate color="green"></v-progress-linear>

                <!-- DATA LOAD ERROR -->
                <v-alert :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
                </v-alert>

                <div v-if="!data && loadStatus.inProgress">
                    No release notes data was found.
                </div>
                <v-alert :value="data && data.ErrorMessage" v-if="data && data.ErrorMessage" type="error">
                    {{ data.ErrorMessage }}
                </v-alert>

                <div v-if="data">
                    <h1 v-if="data.Title">{{ data.Title }}</h1>
                    <p v-if="data.Description">{{ data.Description }}</p>
                    
                    <h2 v-if="data.Version">Version {{ data.Version }}</h2>
                    <div v-if="data.BuiltAt">Built: {{ data.BuiltAt }}</div>
                    <div v-if="data.DeployedAt">Deployed: {{ data.DeployedAt }}</div>
                    <div v-if="data.BuiltCommitHash">BuiltCommitHash: {{ data.BuiltCommitHash }}</div>

                    <div v-if="data.Changes">
                        <h3>Changes</h3>
                        <div v-for="(change, cindex) in data.Changes"
                            :key="`change-${cindex}`"
                            style="border-left: 3px solid blue; margin-bottom: 10px; padding: 10px;">
                            <div>Title: {{ change.Title }}</div>
                            <div>Description: {{ change.Description }}</div>
                            <div>Icon: {{ change.Icon }}</div>
                            <div>Timestamp: {{ change.Timestamp }}</div>
                            <div>CommitHash: {{ change.CommitHash }}</div>
                            <div>Author: {{ change.AuthorName }}</div>
                            <div>MainLink: {{ change.MainLink }}</div>
                            <div>Links: {{ change.Links }}</div>
                        </div>
                    </div>
                </div>

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
import BlockComponent from  '../../Common/Basic/BlockComponent.vue';
import { FetchStatus } from  '../../../services/abstractions/HCServiceBase';
import ReleaseNotesService from  '../../../services/ReleaseNotesService';
import ModuleOptions from  '../../../models/Common/ModuleOptions';
import ModuleConfig from "../../../models/Common/ModuleConfig";
import { HCReleaseNotesViewModel } from "generated/Models/Core/HCReleaseNotesViewModel";

export interface ModuleFrontendOptions {
}

@Component({
    components: {
        BlockComponent
    }
})
export default class EndpointControlPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<ModuleFrontendOptions>;

    service: ReleaseNotesService = new ReleaseNotesService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    data: HCReleaseNotesViewModel | null = null;

    // UI STATE
    loadStatus: FetchStatus = new FetchStatus();

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.loadData();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }
    get HasAccessToDevDetails(): boolean {
        return this.options.AccessOptions.indexOf("DeveloperDetails") != -1;
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.service.GetReleaseNotes(this.HasAccessToDevDetails, this.loadStatus, { onSuccess: (data) => this.onDataRetrieved(data) });
    }

    onDataRetrieved(data: HCReleaseNotesViewModel | null): void {
        this.data = data;
    }
}
</script>

<style scoped lang="scss">
</style>