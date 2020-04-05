<!-- src/components/Pages/EventNotificationsPageComponent.vue -->
<template>
    <div>
        <v-content>
            <v-container fluid fill-height class="content-root">
            <v-layout>
            <v-flex>
            <v-container>
                <!-- LOAD PROGRESS -->
                <v-progress-linear 
                    v-if="dataLoadInProgress"
                    indeterminate color="green"></v-progress-linear>

                <!-- DATA LOAD ERROR -->
                <v-alert :value="dataLoadFailed" v-if="dataLoadFailed" type="error">
                {{ dataFailedErrorMessage }}
                </v-alert>

                <h2>Configured notifications</h2>
               
                <v-btn :disabled="!allowConfigChanges"
                    @click="onAddNewConfigClicked">
                    <v-icon size="20px" class="mr-2">add</v-icon>
                    Add new config
                </v-btn>

                <event-notification-config-component
                    v-for="(config, cindex) in configs"
                    :key="`config-${cindex}-${config.Id}`"
                    :config="config"
                    :readonly="!allowConfigChanges"
                    :options="options"
                    v-on:configDeleted="onConfigDeleted"
                    v-on:configSaved="onConfigSaved"
                    />

                <br /><br />
                <hr />

                <div v-for="(notifier, nindex) in notifiers"
                    :key="`notifier-${nindex}`">
                    <b>{{ notifier.Name }}</b>: {{ notifier.Description }}<br />
                    <small>{{ notifier }}</small>
                </div>

                {{ data }}

            </v-container>
            </v-flex>
            </v-layout>
            </v-container>
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Page/FrontEndOptionsViewModel';
import LoggedEndpointDefinitionViewModel from '../../models/RequestLog/LoggedEndpointDefinitionViewModel';
import LoggedEndpointRequestViewModel from '../../models/RequestLog/LoggedEndpointRequestViewModel';
import { EntryState } from '../../models/RequestLog/EntryState';
import DateUtils from "../../util/DateUtils";
import LinqUtils from "../../util/LinqUtils";
import UrlUtils from "../../util/UrlUtils";
import KeyArray from "../../util/models/KeyArray";
import KeyValuePair from "../../models/Common/KeyValuePair";
import { GetEventNotificationConfigsViewModel, IEventNotifier, EventSinkNotificationConfig, FilterMatchType, NotifierConfig, Dictionary, NotifierConfigOptionsItem, EventSinkNotificationConfigFilter } from "../../models/EventNotifications/EventNotificationModels";
import '@lazy-copilot/datetimepicker/dist/datetimepicker.css'
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";
import FilterInputComponent from '.././Common/FilterInputComponent.vue';
import DataTableComponent, { DataTableGroup } from '.././Common/DataTableComponent.vue';
import SimpleDateTimeComponent from '.././Common/SimpleDateTimeComponent.vue';
import FilterableListComponent, { FilterableListItem } from '.././Common/FilterableListComponent.vue';
import ConfigFilterComponent from '.././EventNotifications/ConfigFilterComponent.vue';
import EventNotificationConfigComponent from '.././EventNotifications/EventNotificationConfigComponent.vue';

@Component({
    components: {
        ConfigFilterComponent,
        SimpleDateTimeComponent,
        EventNotificationConfigComponent
    }
})
export default class EventNotificationsPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // UI STATE
    dataLoadInProgress: boolean = false;
    dataLoadFailed: boolean = false;
    dataFailedErrorMessage: string = '';
    allowConfigChanges: boolean = true;

    data: GetEventNotificationConfigsViewModel | null = null;

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
    get notifiers(): Array<IEventNotifier>
    {
        return (this.data == null) ? [] : this.data.Notifiers;
    };

    get configs(): Array<EventSinkNotificationConfig>
    {
        let configs = (this.data == null) ? [] : this.data.Configs;
        configs = configs.sort(
            (a, b) => LinqUtils.SortByThenBy(a, b,
                x => x.Enabled ? 1 : 0,
                x => (x.LastNotifiedAt == null) ? 32503676400000 : x.LastNotifiedAt.getTime(),
                false, true)
            );

        return configs;
    };

    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.dataLoadInProgress = true;
        this.dataLoadFailed = false;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.GetEventNotificationConfigsEndpoint}${queryStringIfEnabled}`;
        fetch(url, {
            credentials: 'include',
            method: "GET",
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => response.json())
        .then((data: GetEventNotificationConfigsViewModel) => this.onDataRetrieved(data))
        .catch((e) => {
            this.dataLoadFailed = true;
            this.dataLoadInProgress = false;
            this.dataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    onDataRetrieved(data: GetEventNotificationConfigsViewModel): void {
        this.data = data;
        this.data.Configs.forEach(config => {
            this.postProcessConfig(config);
        });

        this.dataLoadInProgress = false;
    }

    postProcessConfig(config: EventSinkNotificationConfig): void {
        config.FromTime = (config.FromTime == null) ? null : new Date(config.FromTime);
        config.ToTime = (config.ToTime == null) ? null : new Date(config.ToTime);
        config.LastNotifiedAt = (config.LastNotifiedAt == null) ? null : new Date(config.LastNotifiedAt);
        config.LastChangedAt = new Date(config.LastChangedAt);
        
        config.NotifierConfigs.forEach(nconfig => {
            if (this.data != null)
            {
                nconfig.Notifier = this.data.Notifiers.filter(x => x.Id == nconfig.NotifierId)[0];
            }
        });
    }

    onConfigSaved(config: EventSinkNotificationConfig): void {
        if (this.data == null)
        {
            return;
        }
        this.postProcessConfig(config);

        console.log("Saved");
        console.log(config);

        this.data.Configs = this.data.Configs.filter(x => x.Id != config.Id);
        this.data.Configs.push(config);
        this.$forceUpdate();
    }

    onConfigDeleted(config: EventSinkNotificationConfig): void {
        if (this.data == null)
        {
            return;
        }

        console.log("Deleted");
        console.log(config);
        
        this.data.Configs = this.data.Configs.filter(x => x.Id != config.Id);
    }

    createNewEventSinkNotificationConfigFilter(): EventSinkNotificationConfigFilter
    {
        return {
            PropertyName: null,
            Filter: '',
            MatchType: FilterMatchType.Contains,
            CaseSensitive: false
        };
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onAddNewConfigClicked(): void {
        // Todo: dont add to data.Configs until it has been saved. id == null wont be removed.
        if (this.data == null)
        {
            return;
        }

        this.data.Configs.push({
            Id: null,
            LastChangedBy: 'You',
            Enabled: true,
            NotificationCountLimit: null,
            FromTime: null,
            ToTime: null,
            LastChangedAt: new Date(),
            LastNotifiedAt: null,
            NotifierConfigs: [],
            EventIdFilter: this.createNewEventSinkNotificationConfigFilter(),
            PayloadFilters: [],
            LatestResults: []
        });
    }
}
</script>

<style scoped lang="scss">
</style>