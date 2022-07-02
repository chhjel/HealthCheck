<!-- src/components/modules/EventNotifications/EventNotificationsPageComponent.vue -->
<template>
    <div>
        
        <div class="content-root">
            <h1 class="mb-1">Configured notifications</h1>

            <!-- LOAD PROGRESS -->
            <progress-linear-component
                class="ma-0"
                v-if="loadStatus.inProgress"
                indeterminate color="success"></progress-linear-component>
            <div style="height: 7px" v-else></div>

            <!-- DATA LOAD ERROR -->
            <alert-component :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
            {{ loadStatus.errorMessage }}
            </alert-component>

            <div class="top-actions">
                <btn-component :disabled="!allowConfigChanges"
                    @click="onAddNewConfigClicked"
                    class="mb-3 mr-2">
                    <icon-component size="20px" class="mr-2">add</icon-component>
                    Add new
                </btn-component>

                                    <btn-component v-if="HasAccessToEditEventDefinitions"
                    @click="editDefinitionsDialogVisible = true"
                    class="mb-3">
                    Edit payload definitions
                </btn-component>
            </div>

            <block-component
                v-for="(config, cindex) in configs"
                :key="`config-${cindex}-${config.Id}`"
                class="config-list-item"
                >
                <div class="config-list-item--inner flex layout">
                    <div class="config-list-item--switch-and-summary">
                        <tooltip-component tooltip="Enable or disable this configuration">
                            <switch-component
                                :value="config.Enabled"
                                color="secondary"
                                style="flex: 0"
                                @click="setConfigEnabled(config, !config.Enabled)"
                                ></switch-component>
                        </tooltip-component>
                        
                        <div class="config-list-item--rule" @click="showConfig(config)">
                            <span class="config-list-item--operator">IF </span>
                            <span v-for="(condition, condIndex) in describeConditions(config)"
                                :key="`condition-${condIndex}`">
                                <span class="config-list-item--condition">
                                    {{ condition.description }}
                                </span>
                                <span v-if="condIndex == describeConditions(config).length - 2"> and </span>
                                <span v-else-if="condIndex < describeConditions(config).length - 1">, </span>
                            </span>

                            <br />
                            <span class="config-list-item--operator">THEN </span>
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
                    </div>
                    
                    <div class="config-list-item--metadata">
                        <tooltip-component v-if="getConfigWarning(config) != null" :tooltip="getConfigWarning(config)">
                            <icon-component style="cursor: help;" color="warning">warning</icon-component>
                        </tooltip-component>

                        <tooltip-component v-if="configIsOutsideLimit(config)" tooltip="This configs' limits has been reached">
                            <icon-component style="cursor: help;">timer_off</icon-component>
                        </tooltip-component>

                        <tooltip-component :tooltip="`Last modified by '${config.LastChangedBy}'`">
                            <icon-component style="cursor: help;">person</icon-component>
                            <code style="color: var(--color--primary-base); cursor: help;">{{ config.LastChangedBy }}</code>
                        </tooltip-component>
                    </div>
                </div>
            </block-component>
        </div>
        
        <!-- DIALOGS -->
        <dialog-component v-model:value="configDialogVisible" persistent max-width="1200" @close="hideCurrentConfig">
            <template #header>{{ currentDialogTitle }}</template>
            <template #footer>
                <btn-component color="error"
                    v-if="showDeleteConfig"
                    :disabled="serverInteractionInProgress"
                    @click="$refs.currentConfigComponent.tryDeleteConfig()">Delete</btn-component>
                <btn-component color="primary"
                    :disabled="serverInteractionInProgress"
                    @click="$refs.currentConfigComponent.saveConfig()">Save</btn-component>
            </template>
            <div v-if="currentConfig != null">                 
                <event-notification-config-component
                    :module-id="config.Id"
                    :config="currentConfig"
                    :notifiers="notifiers"
                    :eventdefinitions="eventDefinitions"
                    :placeholders="placeholders"
                    :readonly="!allowConfigChanges"
                    v-on:configDeleted="onConfigDeleted"
                    v-on:configSaved="onConfigSaved"
                    v-on:serverInteractionInProgress="setServerInteractionInProgress"
                    ref="currentConfigComponent"
                    />
            </div>
        </dialog-component>

        <dialog-component v-model:value="deleteDefinitionDialogVisible" max-width="290">
            <template #header>Confirm deletion</template>
            <template #footer>
                <btn-component color="secondary" @click="deleteDefinitionDialogVisible = false">Cancel</btn-component>
                <btn-component color="error"
                    :loading="loadStatus.inProgress"
                    :disabled="loadStatus.inProgress"
                    @click="confirmDeleteEventDefinition()">Delete</btn-component>
            </template>
            <div>
                {{ deleteDefinitionDialogText }}
            </div>
        </dialog-component>
        
        <dialog-component v-model:value="editDefinitionsDialogVisible" max-width="1200">
            <template #header>Edit event payload definitions</template>
            <template #footer>
                <btn-component
                    :loading="loadStatus.inProgress"
                    :disabled="loadStatus.inProgress"
                    color="error"
                    @click="showDeleteDefinitionDialog(null)">
                    <icon-component size="20px" class="mr-2">delete_forever</icon-component>
                    Delete all definitions
                </btn-component>
                <btn-component color="secondary"
                    @click="editDefinitionsDialogVisible = false">Close</btn-component>
            </template>

            <div>
                <block-component
                    v-for="(def, dindex) in eventDefinitions"
                    :key="`eventdef-${dindex}-${def.EventId}`"
                    class="definition-list-item mb-2">
                    <btn-component
                        :loading="loadStatus.inProgress"
                        :disabled="loadStatus.inProgress"
                        color="error" class="right"
                        @click="showDeleteDefinitionDialog(def.EventId)">
                        <icon-component size="20px" class="mr-2">delete</icon-component>
                        Delete
                    </btn-component>

                    <h3>{{ def.EventId }}</h3>

                    <div v-if="!def.IsStringified">
                        <h4 class="mt-2 mr-1" style="display:inline-block">Properties:</h4>
                        <code
                            v-for="(defProp, dpindex) in def.PayloadProperties"
                            :key="`eventdefprop-${dindex}-${dpindex}`"
                            class="mr-2">{{ defProp }}</code>
                    </div>
                    <div style="clear:both;"></div>
                </block-component>
            </div>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import LinqUtils from '@util/LinqUtils';
import SimpleDateTimeComponent from '@components/Common/SimpleDateTimeComponent.vue';
import ConfigFilterComponent from '@components/modules/EventNotifications/ConfigFilterComponent.vue';
import EventNotificationConfigComponent from '@components/modules/EventNotifications/EventNotificationConfigComponent.vue';
import IdUtils from '@util/IdUtils';
import EventSinkNotificationConfigUtils, { ConfigDescription, ConfigFilterDescription, ConfigActionDescription } from '@util/EventNotifications/EventSinkNotificationConfigUtils';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import EventNotificationService from '@services/EventNotificationService';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import { EventSinkNotificationConfig, FilterMatchType, GetEventNotificationConfigsViewModel, IEventNotifier, KnownEventDefinition } from "@models/modules/EventNotifications/EventNotificationModels";
import { StoreUtil } from "@util/StoreUtil";
import StringUtils from "@util/StringUtils";

@Options({
    components: {
        ConfigFilterComponent,
        SimpleDateTimeComponent,
        EventNotificationConfigComponent,
        BlockComponent
    }
})
export default class EventNotificationsPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;

    service: EventNotificationService = new EventNotificationService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);

    // UI STATE
    loadStatus: FetchStatus = new FetchStatus();
    serverInteractionInProgress: boolean = false;
    editDefinitionsDialogVisible: boolean = false;
    deleteDefinitionDialogVisible: boolean = false;
    configDialogVisible: boolean = false;
    deleteDefinitionDialogText: string = "";
    eventDefinitionIdToDelete: string | null = null;

    datax: GetEventNotificationConfigsViewModel | null = null;
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
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get HasAccessToEditEventDefinitions(): boolean {
        return this.options.AccessOptions.indexOf("EditEventDefinitions") != -1;
    }
    
    get showDeleteConfig(): boolean
    {
        return this.currentConfig != null && this.currentConfig.Id != null;
    }

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

    get notifiers(): Array<IEventNotifier>
    {
        return (this.datax == null) ? [] : this.datax.Notifiers;
    };

    get eventDefinitions(): Array<KnownEventDefinition>
    {
        return (this.datax == null) ? [] : this.datax.KnownEventDefinitions;
    };

    get placeholders(): Array<string>
    {
        return (this.datax == null) ? [] : this.datax.Placeholders;
    };

    get configs(): Array<EventSinkNotificationConfig>
    {
        let configs = (this.datax == null) ? [] : this.datax.Configs;
        return configs;
    };

    ////////////////
    //  METHODS  //
    //////////////
    updateUrl(): void {
        let routeParams: any = {};
        if (this.currentConfig != null && this.currentConfig.Id != null)
        {
            routeParams['id'] = this.currentConfig.Id;
        }

        this.$router.push({ name: this.config.Id, params: routeParams })
    }

    updateSelectionFromUrl(): void {
        const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.id) || null;
        
        if (idFromHash) {
            let configFromUrl = this.configs.filter(x => x.Id != null && x.Id == idFromHash)[0];
            if (configFromUrl != null)
            {
                this.showConfig(configFromUrl, false);
            }
        }
    }

    loadData(): void {
        this.service.GetEventNotifications(this.loadStatus, { onSuccess: (data) => this.onDataRetrieved(data) });
    }

    onDataRetrieved(data: GetEventNotificationConfigsViewModel): void {
        this.datax = data;
        this.datax.Configs.forEach(config => {
            EventSinkNotificationConfigUtils.postProcessConfig(config, this.notifiers);
        });

        this.updateSelectionFromUrl();
        
        this.datax.Configs = this.datax.Configs.sort(
            (a, b) => LinqUtils.SortByThenBy(a, b,
                x => x.Enabled ? 1 : 0,
                x => (x.LastChangedAt == null) ? 32503676400000 : x.LastChangedAt.getTime(),
                false, false)
            );
    }

    setConfigEnabled(config: EventSinkNotificationConfig, enabled: boolean): void {
        if (this.serverInteractionInProgress)
        {
            return;
        }

        this.serverInteractionInProgress = true;
        
        this.service.SetConfigEnabled(config, enabled, this.loadStatus, {
            onSuccess: (data) => {
                if (data.Success == true) {
                    config.Enabled = enabled;
                }
            },
            onDone: () => this.serverInteractionInProgress = false
        });
    }

    onConfigSaved(config: EventSinkNotificationConfig): void {
        if (this.datax == null)
        {
            return;
        }
        EventSinkNotificationConfigUtils.postProcessConfig(config, this.notifiers);

        const position = this.datax.Configs.findIndex(x => x.Id == config.Id);
        //this.datax.Configs = this.datax.Configs.filter(x => x.Id != config.Id);

        if (position == -1)
        {
            this.datax.Configs.push(config);
        }
        else {
            this.datax.Configs[position] = config;
            // this.datax.Configs.unshift(config);
        }
        // this.$forceUpdate();

        this.hideCurrentConfig();
    }

    onConfigDeleted(config: EventSinkNotificationConfig): void {
        if (this.datax == null)
        {
            return;
        }

        this.datax.Configs = this.datax.Configs.filter(x => x.Id != config.Id);
        this.hideCurrentConfig();
    }

    showConfig(config: EventSinkNotificationConfig, updateRoute: boolean = true): void {
        this.currentConfig = config;
        this.configDialogVisible = config != null;

        if (updateRoute)
        {
            this.updateUrl();
        }
    }

    hideCurrentConfig(): void {
        this.currentConfig = null;
        this.configDialogVisible = false;
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

    showDeleteDefinitionDialog(eventId: string | null): void
    {
        this.deleteDefinitionDialogVisible = true;
        this.eventDefinitionIdToDelete = eventId;
        this.deleteDefinitionDialogText = (eventId == null)
            ? `Delete all event definitions?`
            : `Delete event definition '${eventId}'?`;
    }

    confirmDeleteEventDefinition(): void {
        this.deleteDefinitionDialogVisible = false;

        if (this.eventDefinitionIdToDelete != null)
        {
            this.service.DeleteDefintion(this.eventDefinitionIdToDelete, this.loadStatus, {
                onSuccess: (r) => {
                    if (this.datax != null)
                    {
                        this.datax.KnownEventDefinitions = this.datax.KnownEventDefinitions
                            .filter(x => x.EventId != this.eventDefinitionIdToDelete);
                    }
                }
            });
        }
        else
        {
            this.service.DeleteDefintions(this.loadStatus, {
                onSuccess: (r) => {
                    if (this.datax != null)
                    {
                        this.datax.KnownEventDefinitions = [];
                    }
                }
            });
        }
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////    
    onAddNewConfigClicked(): void {
        if (this.datax == null)
        {
            return;
        }

        let config: EventSinkNotificationConfig = {
            Id: '00000000-0000-0000-0000-000000000000',
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
            LatestResults: [],
            DistinctNotificationKey: '',
            DistinctNotificationCacheDuration: '0:0:0'
        };

        this.showConfig(config);
    }
}
</script>
<style lang="scss">
.config-list-item {
    .block-component--content {
        @media (max-width: 960px) {
            padding-right: 12px !important;
        }
    }
}
</style>
<style scoped lang="scss">
.current-config-dialog__title {
    font-size: 34px;
    font-weight: 600;
}
.config-list-item {
    margin-bottom: 20px;
    
    .config-list-item--inner {
        display: flex;
        flex-wrap: nowrap;
        flex-direction: column;
        @media (min-width: 960px) {
            flex-direction: row;
            align-items: center;
        }

        .config-list-item--switch-and-summary {
            display: flex;
            align-items: center;
            flex-direction: row;
            flex-wrap: nowrap;
            flex: 1;
        }

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
                color: var(--color--primary-base);
            }
            .config-list-item--action {
                color: var(--color--secondary-base);
            }
        }

        .config-list-item--metadata {
            display: flex;
            align-items: center;
            flex-direction: row;
            flex-wrap: nowrap;

            @media (max-width: 960px) {
                margin-top: 12px;
            }
        }
    }
}
.top-actions {
    display: flex;
    flex-wrap: wrap;
}
</style>