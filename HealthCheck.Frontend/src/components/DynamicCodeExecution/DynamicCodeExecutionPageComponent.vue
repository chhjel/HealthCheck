<!-- src/components/DynamicCodeExecution/DynamicCodeExecutionPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
            <v-container fluid fill-height class="content-root">
            <v-layout>
            <v-flex>
            <v-container>
                <h1 class="mb-1">ToDo</h1>

                <code>{{ options }}</code>

                <code>{{ config }}</code>

                <!-- LOAD PROGRESS -->
                <!-- <v-progress-linear
                    v-if="loadStatus.inProgress"
                    indeterminate color="green"></v-progress-linear> -->

                <!-- DATA LOAD ERROR -->
                <!-- <v-alert :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
                </v-alert> -->

            </v-container>
            </v-flex>
            </v-layout>
            </v-container>
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Common/FrontEndOptionsViewModel';
import DateUtils from "../../util/DateUtils";
import LinqUtils from "../../util/LinqUtils";
import KeyArray from "../../util/models/KeyArray";
import KeyValuePair from "../../models/Common/KeyValuePair";
import BlockComponent from '../../components/Common/Basic/BlockComponent.vue';
import { FetchStatus } from "../../services/abstractions/HCServiceBase";
import EventNotificationService from "../../services/EventNotificationService";
import ModuleConfig from "../../models/Common/ModuleConfig";
import ModuleOptions from "../../models/Common/ModuleOptions";

@Component({
    components: {
        BlockComponent
    }
})
export default class DynamicCodeExecutionPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;

    // service: EventNotificationService = new EventNotificationService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);

    // UI STATE
    loadStatus: FetchStatus = new FetchStatus();

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        // this.loadData();
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
    // loadData(): void {
    //     this.service.GetEventNotifications(this.loadStatus, { onSuccess: (data) => this.onDataRetrieved(data) });
    // }

    // onDataRetrieved(data: GetEventNotificationConfigsViewModel): void {
    //     this.data = data;
    // }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
</style>