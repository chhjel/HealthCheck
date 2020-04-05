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

                <div v-for="(config, cindex) in configs"
                    :key="`config-${cindex}`">

                    <v-switch 
                        v-model="config.Enabled" 
                        label="Enabled"
                        color="secondary"
                    ></v-switch>

                    <v-btn color="error" @click="deleteConfig(config)"
                        :disabled="deleteInProgress">
                        <v-icon size="20px" class="mr-2">delete</v-icon>
                        Delete
                    </v-btn>

                    <pre>Created by: {{ config.CreatedBy }}</pre>

                    <h3>Limits</h3>

                    <v-text-field
                        type="number"
                        label="Notification count limit"
                        v-model="config.NotificationCountLimit"
                        required clearable />
                        <!-- v-on:change="onValueChanged" -->
                    
                    <simple-date-time-component
                        v-model="config.FromTime"
                        label="From"
                        />
                    
                    <simple-date-time-component
                        v-model="config.ToTime"
                        label="To"
                        />

                    <h3>Event id filter</h3>
                    <config-filter-component
                        v-model="config.EventIdFilter"
                        :allow-property-name="false"
                        />
                    
                    <h3>Payload filters</h3>
                    <config-filter-component
                        v-for="(payloadFilter, pfindex) in config.PayloadFilters"
                        :key="`payloadFilter-${pfindex}`"
                        v-model="config.PayloadFilters[pfindex]"
                        :allow-property-name="true"
                        />

                    <h3>Notify using</h3>
                    <div v-for="(notifierConfig, ncindex) in getValidNotifierConfigs(config)"
                        :key="`notifierConfig-${ncindex}`">
                        <b>{{ notifierConfig.Notifier.Name }}</b>
                        
                        <div v-for="(notifierConfigOption, ncoindex) in getNotifierConfigOptions(notifierConfig.Notifier, notifierConfig.Options)"
                            :key="`notifierConfig-${ncindex}-option-${ncoindex}`"
                            style="margin-left:20px">

                            <v-text-field
                                :label="notifierConfigOption.definition.Name"
                                v-model="notifierConfigOption.value"
                                v-on:input="notifierConfig.Options[notifierConfigOption.key] = $event"
                                :hint="notifierConfigOption.definition.Description"
                                persistent-hint
                                required clearable />
                        </div>
                    </div>
                    <v-btn>
                        <v-icon size="20px" class="mr-2">add</v-icon>
                        Add notifier
                    </v-btn>

                    <h3>10 last notification results</h3>
                    <ul>
                        <li v-if="config.LatestResults.length == 0">No results yet</li>
                        <li
                            v-for="(result, rindex) in config.LatestResults"
                            :key="`LatestResults-${rindex}`">
                            {{ result }}
                        </li>
                    </ul>
                    
                    <small>{{ config }}</small>
                </div>

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
import { GetEventNotificationConfigsViewModel, IEventNotifier, EventSinkNotificationConfig, FilterMatchType, NotifierConfig, Dictionary, NotifierConfigOptionsItem } from "../../models/EventNotifications/EventNotificationModels";
import '@lazy-copilot/datetimepicker/dist/datetimepicker.css'
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";
import FilterInputComponent from '.././Common/FilterInputComponent.vue';
import DataTableComponent, { DataTableGroup } from '.././Common/DataTableComponent.vue';
import SimpleDateTimeComponent from '.././Common/SimpleDateTimeComponent.vue';
import FilterableListComponent, { FilterableListItem } from '.././Common/FilterableListComponent.vue';
import ConfigFilterComponent from '.././EventNotifications/ConfigFilterComponent.vue';

@Component({
    components: {
        DataTableComponent,
        ConfigFilterComponent,
        SimpleDateTimeComponent
    }
})
export default class EventNotificationsPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // UI STATE
    dataLoadInProgress: boolean = false;
    dataLoadFailed: boolean = false;
    dataFailedErrorMessage: string = '';
    deleteInProgress: boolean = false;

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
        configs = configs.sort((a, b) => LinqUtils.SortBy(a, b, x => x.Enabled ? 1 : 0));

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
            config.FromTime = (config.FromTime == null) ? null : new Date(config.FromTime);
            config.ToTime = (config.ToTime == null) ? null : new Date(config.ToTime);
            
            config.NotifierConfigs.forEach(nconfig => {
                nconfig.Notifier = data.Notifiers.filter(x => x.Id == nconfig.NotifierId)[0];
            });
        });

        this.dataLoadInProgress = false;
    }

    deleteConfig(config: EventSinkNotificationConfig): void {
        this.deleteInProgress = true;
        // this.deleteInProgress = false;
    }

    getValidNotifierConfigs(config: EventSinkNotificationConfig): Array<NotifierConfig> {
        return config.NotifierConfigs.filter(x => x.Notifier != null);
    }

    getNotifierConfigOptions(notifier: IEventNotifier, options: Dictionary<string>): Array<NotifierConfigOptionsItem> {
        let keys = Object.keys(options);
        return <any>keys
            .map(key => {
                let def = notifier.Options.filter(x => x.Id == key)[0];
                if (def == null)
                {
                    return null;
                }
                return {
                    key: key,
                    definition: def,
                    value: options[key]
                };
            })
            .filter(x => x != null);
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
</style>