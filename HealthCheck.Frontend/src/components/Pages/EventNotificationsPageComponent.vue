<!-- src/components/Pages/EventNotificationsPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
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
                    @click="onAddNewConfigClicked"
                    class="mb-3">
                    <v-icon size="20px" class="mr-2">add</v-icon>
                    Add new
                </v-btn>

                <block-component
                    v-for="(config, cindex) in configs"
                    :key="`config-${cindex}-${config.Id}`"
                    class="config-list-item"
                    >
                    <div class="config-list-item--inner">
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                            <v-switch v-on="on"
                                v-model="config.Enabled"
                                color="secondary"
                                style="flex: 0"
                                @click="setConfigEnabled(config, !config.Enabled)"
                                ></v-switch>
                                <!-- :disabled="!allowConfigChanges" -->
                            </template>
                            <span>Enable or disable this configuration</span>
                        </v-tooltip>
                        
                        <div class="config-list-item--rule"
                            @click="showConfig(config)"
                            >
                            <span class="config-list-item--operator">IF</span>
                            <span v-for="(condition, condIndex) in describeConditions(config)"
                                :key="`condition-${condIndex}`">
                                <span class="config-list-item--condition">
                                    {{ condition.description }}
                                </span>
                                <span v-if="condIndex == describeConditions(config).length - 2"> and </span>
                                <span v-else-if="condIndex < describeConditions(config).length - 1">, </span>
                            </span>

                            <br />
                            <span class="config-list-item--operator">THEN</span>
                            <span v-if="describeActions(config).length == 0">&lt;do nothing&gt;</span>
                            <span v-else>notify using</span>
                            <span v-for="(action, actIndex) in describeActions(config)"
                                :key="`action-${actIndex}`">
                                <span class="config-list-item--action">
                                    {{ action.description }}
                                </span>
                                <span v-if="actIndex == describeActions(config).length - 2"> and </span>
                                <span v-else-if="actIndex < describeActions(config).length - 1">, </span>
                            </span>
                        </div>
                        
                        <v-tooltip bottom v-if="getConfigWarning(config) != null">
                            <template v-slot:activator="{ on }">
                                <v-icon style="cursor: help;" color="warning" v-on="on">warning</v-icon>
                            </template>
                            <span>{{getConfigWarning(config)}}</span>
                        </v-tooltip>

                        <v-tooltip bottom v-if="configIsOutsideLimit(config)">
                            <template v-slot:activator="{ on }">
                                <v-icon v-on="on" style="cursor: help;">timer_off</v-icon>
                            </template>
                            <span>This configs' limits has been reached</span>
                        </v-tooltip>

                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon style="cursor: help;" v-on="on">person</v-icon>
                                <code style="color: var(--v-primary-base); cursor: help;" v-on="on">{{ config.LastChangedBy }}</code>
                            </template>
                            <span>Last modified by '{{ config.LastChangedBy }}'</span>
                        </v-tooltip>
                    </div>
                </block-component>

            </v-container>
            </v-flex>
            </v-layout>
            </v-container>
            
            <v-dialog v-model="configDialogVisible"
                scrollable
                persistent
                max-width="1200"
                content-class="current-config-dialog">
                <v-card v-if="currentConfig != null" style="background-color: #f4f4f4">
                    <v-toolbar class="elevation-0">
                        <v-toolbar-title class="current-config-dialog__title">{{ currentDialogTitle }}</v-toolbar-title>
                        <v-spacer></v-spacer>
                        <v-btn icon
                            @click="hideCurrentConfig()"
                            :disabled="serverInteractionInProgress">
                            <v-icon>close</v-icon>
                        </v-btn>
                    </v-toolbar>

                    <v-divider></v-divider>
                    
                    <v-card-text>
                        <event-notification-config-component
                            :config="currentConfig"
                            :notifiers="notifiers"
                            :eventdefinitions="eventDefinitions"
                            :placeholders="placeholders"
                            :readonly="!allowConfigChanges"
                            :options="options"
                            v-on:configDeleted="onConfigDeleted"
                            v-on:configSaved="onConfigSaved"
                            v-on:serverInteractionInProgress="setServerInteractionInProgress"
                            ref="currentConfigComponent"
                            />
                    </v-card-text>
                    <v-divider></v-divider>
                    <v-card-actions >
                        <v-spacer></v-spacer>
                        <v-btn color="error" flat
                        :disabled="serverInteractionInProgress"
                        @click="$refs.currentConfigComponent.tryDeleteConfig()">Delete</v-btn>
                        <v-btn color="success"
                        :disabled="serverInteractionInProgress"
                        @click="$refs.currentConfigComponent.saveConfig()">Save</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
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

@Component({
    components: {
        ConfigFilterComponent,
        SimpleDateTimeComponent,
        EventNotificationConfigComponent,
        BlockComponent
    }
})
export default class EventNotificationsPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // UI STATE
    dataLoadInProgress: boolean = false;
    dataLoadFailed: boolean = false;
    dataFailedErrorMessage: string = '';
    serverInteractionInProgress: boolean = false;

    data: GetEventNotificationConfigsViewModel | null = null;
    currentConfig: EventSinkNotificationConfig | null = null;

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
    get allowConfigChanges(): boolean
    {
        return !this.serverInteractionInProgress;
    };

    get currentDialogTitle(): string
    {
        return (this.currentConfig != null && this.currentConfig.Id != null)
            ? 'Edit notification config'
            : 'Create new notification config';
    }

    get configDialogVisible(): boolean
    {
        return this.currentConfig != null;
    }

    get notifiers(): Array<IEventNotifier>
    {
        return (this.data == null) ? [] : this.data.Notifiers;
    };

    get eventDefinitions(): Array<KnownEventDefinition>
    {
        return (this.data == null) ? [] : this.data.KnownEventDefinitions;
    };

    get placeholders(): Array<string>
    {
        return (this.data == null) ? [] : this.data.Placeholders;
    };

    get configs(): Array<EventSinkNotificationConfig>
    {
        let configs = (this.data == null) ? [] : this.data.Configs;
        return configs;
    };

    ////////////////
    //  METHODS  //
    //////////////
    // Invoked from parent
    public onPageShow(): void {
        const parts = (<any>window).eventnotificationsState;
        if (parts != null && parts != undefined) {
            this.updateUrl(parts);
        } else {
            this.updateUrl(['eventnotifications']);
        }
    }

    setFromUrl(forcedParts: Array<string> | null = null): void {
        const parts = forcedParts || UrlUtils.GetHashParts();
        
        let didSelectConfig = false;
        const selectedItem = parts[1];
        if (selectedItem !== undefined && selectedItem.length > 0) {
            let configFromUrl = this.configs.filter(x => x.Id != null && UrlUtils.EncodeHashPart(x.Id) == selectedItem)[0];
            if (configFromUrl != null)
            {
                didSelectConfig = true;
                this.showConfig(configFromUrl);
            }
        }
    }

    updateUrl(parts?: Array<string> | null): void {
        if (parts == null)
        {
            parts = ['eventnotifications'];

            if (this.currentConfig != null && this.currentConfig.Id != null)
            {
                parts.push(UrlUtils.EncodeHashPart(this.currentConfig.Id));
            }
        }

        UrlUtils.SetHashParts(parts);
        
        (<any>window).eventnotificationsState = parts;
    }

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
            EventSinkNotificationConfigUtils.postProcessConfig(config, this.notifiers);
        });

        this.dataLoadInProgress = false;
        
        const originalUrlHashParts = UrlUtils.GetHashParts();
        this.setFromUrl(originalUrlHashParts);
        
        this.data.Configs = this.data.Configs.sort(
            (a, b) => LinqUtils.SortByThenBy(a, b,
                x => x.Enabled ? 1 : 0,
                x => (x.LastChangedAt == null) ? 32503676400000 : x.LastChangedAt.getTime(),
                false, true)
            );
    }

    setConfigEnabled(config: EventSinkNotificationConfig, enabled: boolean): void {
        this.serverInteractionInProgress = true;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.SetEventNotificationConfigEnabledEndpoint}${queryStringIfEnabled}`;
        let payload = {
            configId: config.Id,
            enabled: enabled
        };
        fetch(url, {
            credentials: 'include',
            method: "POST",
            body: JSON.stringify(payload),
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => response.json())
        .then((data: any) => {
            this.serverInteractionInProgress = false;

            if (data.Success == true) {
                config.Enabled = enabled;
            }
        })
        .catch((e) => {
            this.serverInteractionInProgress = false;
            console.error(e);
        });
    }

    onConfigSaved(config: EventSinkNotificationConfig): void {
        if (this.data == null)
        {
            return;
        }
        EventSinkNotificationConfigUtils.postProcessConfig(config, this.notifiers);

        const position = this.data.Configs.findIndex(x => x.Id == config.Id);
        this.data.Configs = this.data.Configs.filter(x => x.Id != config.Id);

        if (position == -1)
        {
            this.data.Configs.push(config);
        }
        else {
            Vue.set(this.data.Configs, position, config);
            // this.data.Configs.unshift(config);
        }
        // this.$forceUpdate();

        this.hideCurrentConfig();
    }

    onConfigDeleted(config: EventSinkNotificationConfig): void {
        if (this.data == null)
        {
            return;
        }

        this.data.Configs = this.data.Configs.filter(x => x.Id != config.Id);
        this.hideCurrentConfig();
    }

    showConfig(config: EventSinkNotificationConfig): void {
        this.currentConfig = config;
        this.updateUrl();
    }

    hideCurrentConfig(): void {
        this.currentConfig = null;
        this.updateUrl();
    }
    
    setServerInteractionInProgress(inProgress: boolean): void
    {
        this.serverInteractionInProgress = inProgress;
    }

    describeConfig(config: EventSinkNotificationConfig): ConfigDescription
    {
        return EventSinkNotificationConfigUtils.describeConfig(config);
    }

    getConfigWarning(config: EventSinkNotificationConfig): string | null
    {
        if (config.NotifierConfigs.length == 0)
        {
            return 'Config has no effect because it has zero notifiers configured.';
        }

        return null;
    }

    configIsOutsideLimit(config: EventSinkNotificationConfig): boolean
    {
        if (config.ToTime != null && config.ToTime.getTime() > new Date().getTime())
        {
            return true;
        }
        else if (config.FromTime != null && config.FromTime.getTime() < new Date().getTime())
        {
            return true;
        }
        else if (config.NotificationCountLimit != null && config.NotificationCountLimit <= 0)
        {
            return true;
        }

        return false;
    }

    describeConditions(config: EventSinkNotificationConfig): Array<ConfigFilterDescription>
    {
        return EventSinkNotificationConfigUtils.describeConfig(config).conditions;
    }

    describeActions(config: EventSinkNotificationConfig): Array<ConfigActionDescription>
    {
        return EventSinkNotificationConfigUtils.describeConfig(config).actions;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onAddNewConfigClicked(): void {
        if (this.data == null)
        {
            return;
        }

        let config = {
            Id: null,
            LastChangedBy: 'You',
            Enabled: true,
            NotificationCountLimit: null,
            FromTime: null,
            ToTime: null,
            LastChangedAt: new Date(),
            LastNotifiedAt: null,
            NotifierConfigs: [],
            EventIdFilter: {
                PropertyName: null,
                Filter: '',
                MatchType: FilterMatchType.Matches,
                CaseSensitive: false,
                _frontendId: IdUtils.generateId()
            },
            PayloadFilters: [],
            LatestResults: []
        };

        this.showConfig(config);
    }
}
</script>

<style scoped lang="scss">
.current-config-dialog__title {
    font-size: 34px;
    font-weight: 600;
}
.config-list-item {
    margin-bottom: 20px;
    
    .config-list-item--inner {
        display: flex;
        align-items: center;
        flex-direction: row;
        flex-wrap: nowrap;

        .config-list-item--rule {
            flex: 1;
            cursor: pointer;
            font-size: 16px;
            margin-left: 20px;
            margin-right: 20px;
            
            .config-list-item--operator {
                font-weight: 600;
            }
            .config-list-item--condition {
                color: var(--v-primary-base);
            }
            .config-list-item--action {
                color: var(--v-secondary-base);
            }
            /* .config-list-item--condition,
            .config-list-item--action {
                font-weight: 600;
            } */
        }
    }
}
</style>