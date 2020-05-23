<!-- src/components/DynamicCodeExecution/DynamicCodeExecutionPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
            <v-container fluid fill-height class="content-root">
            <v-layout>
            <v-flex>
            <v-container>
                <h1 class="mb-1">ToDo</h1>

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
import LoggedEndpointDefinitionViewModel from '../../models/RequestLog/LoggedEndpointDefinitionViewModel';
import LoggedEndpointRequestViewModel from '../../models/RequestLog/LoggedEndpointRequestViewModel';
import { EntryState } from '../../models/RequestLog/EntryState';
import DateUtils from "../../util/DateUtils";
import LinqUtils from "../../util/LinqUtils";
import KeyArray from "../../util/models/KeyArray";
import KeyValuePair from "../../models/Common/KeyValuePair";
import { GetEventNotificationConfigsViewModel, IEventNotifier, EventSinkNotificationConfig, FilterMatchType, NotifierConfig, Dictionary, NotifierConfigOptionsItem, EventSinkNotificationConfigFilter, KnownEventDefinition } from "../../models/EventNotifications/EventNotificationModels";
import '@lazy-copilot/datetimepicker/dist/datetimepicker.css'
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";
import FilterInputComponent from '.././Common/FilterInputComponent.vue';
import DataTableComponent, { DataTableGroup } from '.././Common/DataTableComponent.vue';
import SimpleDateTimeComponent from '.././Common/SimpleDateTimeComponent.vue';
import FilterableListComponent, { FilterableListItem } from '.././Common/FilterableListComponent.vue';
import ConfigFilterComponent from '.././EventNotifications/ConfigFilterComponent.vue';
import EventNotificationConfigComponent from '.././EventNotifications/EventNotificationConfigComponent.vue';
import IdUtils from "../../util/IdUtils";
import EventSinkNotificationConfigUtils, { ConfigDescription, ConfigFilterDescription, ConfigActionDescription } from "../../util/EventNotifications/EventSinkNotificationConfigUtils";
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
export default class EventNotificationsPageComponent extends Vue {
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