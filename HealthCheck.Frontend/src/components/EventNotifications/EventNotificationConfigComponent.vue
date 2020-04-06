<!-- src/components/Common/EventNotificationConfigComponent.vue -->
<template>
    <div class="root">

        {{ description }}

        <v-switch 
            v-model="internalConfig.Enabled" 
            :disabled="allowChanges"
            label="Enabled"
            color="secondary"
        ></v-switch>

        <!-- <v-btn color="success"
            @click="onSaveConfigClicked()"
            :loading="isSaving"
            :disabled="allowChanges">
            <v-icon size="20px" class="mr-2">save</v-icon>
            {{ (internalConfig.Id == null ? 'Save new notification config' : 'Save changes') }}
        </v-btn>

        <v-btn color="error"
            @click="onDeleteConfigClicked()"
            :loading="isDeleting"
            :disabled="allowChanges">
            <v-icon size="20px" class="mr-2">delete</v-icon>
            Delete
        </v-btn> -->
        
        <v-alert
            :value="serverInteractionError != null && serverInteractionError.length > 0"
            type="error" >
            {{ serverInteractionError }}
        </v-alert>

        <pre v-if="internalConfig.LastChangedBy != null && internalConfig.LastChangedBy.length > 0">
            Last changed at {{ formatDate(internalConfig.LastChangedAt) }} by '{{ internalConfig.LastChangedBy }}' 
        </pre>
        <pre v-if="internalConfig.LastNotifiedAt != null">
            Last notified at {{ formatDate(internalConfig.LastNotifiedAt) }}
        </pre>

        <h3>Limits</h3>

        <v-text-field
            type="number"
            label="Notification count remaining"
            v-model="internalConfig.NotificationCountLimit"
            :disabled="allowChanges"
            required clearable />
            <!-- v-on:change="onValueChanged" -->
        
        <simple-date-time-component
            v-model="internalConfig.FromTime"
            :readonly="allowChanges"
            label="From"
            />
        
        <simple-date-time-component
            v-model="internalConfig.ToTime"
            :readonly="allowChanges"
            label="To"
            />

        <h3>Event id filter</h3>
        <config-filter-component
            :config="internalConfig.EventIdFilter"
            :allow-property-name="false"
            :readonly="allowChanges"
            @change="internalConfig.EventIdFilter = $event"
            />
        
        <h3>Payload filters</h3>
        <config-filter-component
            v-for="(payloadFilter, pfindex) in internalConfig.PayloadFilters"
            :key="`payloadFilter-${pfindex}-${payloadFilter._frontendId}`"
            :config="payloadFilter"
            :readonly="allowChanges"
            :allow-property-name="true"
            :allow-delete="true"
            @delete="onConfigFilterDelete(pfindex)"
            @change="internalConfig.PayloadFilters[pfindex] = $event"
            />
        <small v-if="internalConfig.PayloadFilters.length == 0">No payload filters added.</small>
        <v-btn :disabled="allowChanges"
            @click="onAddPayloadFilterClicked">
            <v-icon size="20px" class="mr-2">add</v-icon>
            Add payload filter
        </v-btn>

        <h3>Notify using</h3>
        <div v-for="(notifierConfig, ncindex) in getValidNotifierConfigs()"
            :key="`notifierConfig-${ncindex}`">

            <b>{{ notifierConfig.Notifier.Name }}</b>
            <v-btn color="error"
                @click="removeValidNotifierConfig(ncindex)"
                :disabled="allowChanges">
                <v-icon size="20px" class="mr-2">delete</v-icon>
            </v-btn>
            
            <div v-for="(notifierConfigOption, ncoindex) in getNotifierConfigOptions(notifierConfig.Notifier, notifierConfig.Options)"
                :key="`notifierConfig-${ncindex}-option-${ncoindex}`"
                style="margin-left:20px">

                <v-text-field
                    :label="notifierConfigOption.definition.Name"
                    v-model="notifierConfigOption.value"
                    v-on:input="notifierConfig.Options[notifierConfigOption.key] = $event"
                    :hint="notifierConfigOption.definition.Description"
                    :disabled="allowChanges"
                    persistent-hint
                    required clearable />
            </div>
        </div>
        <small v-if="getValidNotifierConfigs(config).length == 0">No notifiers added, config will be disabled.</small>
        <v-btn :disabled="allowChanges" @click.stop="notifierDialogVisible = true" v-if="notifiers != null">
            <v-icon size="20px" class="mr-2">add</v-icon>
            Add notifier
        </v-btn>

        <h3>10 last notification results</h3>
        <ul>
            <li v-if="internalConfig.LatestResults.length == 0">No results yet</li>
            <li
                v-for="(result, rindex) in internalConfig.LatestResults"
                :key="`LatestResults-${rindex}`">
                {{ result }}
            </li>
        </ul>

        <v-dialog v-model="notifierDialogVisible"
            scrollable
            v-if="notifiers != null"
            content-class="possible-notifiers-dialog">
            <v-card>
                <v-card-title>Select type of notifier to add</v-card-title>
                <v-divider></v-divider>
                <v-card-text style="max-height: 500px;">
                    <v-list class="possible-notifiers-list">
                        <v-list-tile v-for="(notifier, nindex) in notifiers"
                            :key="`possible-notifier-${nindex}`"
                            @click="onAddNotifierClicked(notifier)"
                            class="possible-notifiers-list-item">
                            <v-list-tile-action>
                                <v-icon>add</v-icon>
                            </v-list-tile-action>

                            <v-list-tile-content>
                                <v-list-tile-title class="possible-notifier-item-title">{{ notifier.Name }}</v-list-tile-title>
                                <v-list-tile-sub-title class="possible-notifier-item-description">{{ notifier.Description }}</v-list-tile-sub-title>
                            </v-list-tile-content>
                        </v-list-tile>
                    </v-list>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary" flat @click="notifierDialogVisible = false">Cancel</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <v-dialog v-model="deleteDialogVisible"
            max-width="290"
            content-class="confirm-dialog">
            <v-card>
                <v-card-title class="headline">Confirm deletion</v-card-title>
                <v-card-text>
                    Are you sure you want to delete this configuration?
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary" @click="deleteDialogVisible = false">Cancel</v-btn>
                    <v-btn color="error" @click="deleteConfig()">Delete it</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import { EventSinkNotificationConfigFilter, FilterMatchType, EventSinkNotificationConfig, NotifierConfig, Dictionary, IEventNotifier, NotifierConfigOptionsItem, KnownEventDefinition } from "../../models/EventNotifications/EventNotificationModels";
import SimpleDateTimeComponent from '.././Common/SimpleDateTimeComponent.vue';
import ConfigFilterComponent from '.././EventNotifications/ConfigFilterComponent.vue';
import FrontEndOptionsViewModel from "../../models/Page/FrontEndOptionsViewModel";
import DateUtils from "../../util/DateUtils";
import IdUtils from "../../util/IdUtils";
import EventSinkNotificationConfigUtils from "../../util/EventNotifications/EventSinkNotificationConfigUtils";

@Component({
    components: {
        ConfigFilterComponent,
        SimpleDateTimeComponent
    }
})
export default class EventNotificationConfigComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;
    
    @Prop({ required: true })
    config!: EventSinkNotificationConfig;

    @Prop({ required: false, default: null })
    notifiers!: Array<IEventNotifier> | null;

    @Prop({ required: false, default: null})
    eventdefinitions!: Array<KnownEventDefinition> | null;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    // @ts-ignore
    internalConfig: EventSinkNotificationConfig = null;
    ASD!: EventSinkNotificationConfig;
    notifierDialogVisible: boolean = false;
    deleteDialogVisible: boolean = false;
    isSaving: boolean = false;
    isDeleting: boolean = false;
    serverInteractionError: string | null = null;
    serverInteractionInProgress: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
        this.onConfigChanged();
    }

    @Watch("config")
    onConfigChanged(): void {
        let intConfig = JSON.parse(JSON.stringify(this.config));
        EventSinkNotificationConfigUtils.postProcessConfig(intConfig, this.notifiers);
        this.internalConfig = intConfig;
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get allowChanges(): boolean {
        return this.readonly || this.serverInteractionInProgress;
    }

    get description(): string
    {
        return EventSinkNotificationConfigUtils.describeConfig(this.internalConfig).description;
    }

    ////////////////
    //  METHODS  //
    //////////////
    removeValidNotifierConfig(visibleIndex: number): void {
        let index = -1;
        for(let i=0;i<this.internalConfig.NotifierConfigs.length;i++)
        {
            if (this.internalConfig.NotifierConfigs[i].Notifier == null)
            {
                continue;
            }
            index++;

            if (index == visibleIndex)
            {
                this.internalConfig.NotifierConfigs.splice(i, 1);
                return;
            }
        }
    }

    getValidNotifierConfigs(): Array<NotifierConfig> {
        return this.internalConfig.NotifierConfigs.filter(x => x.Notifier != null);
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
    
    setServerInteractionInProgress(inProgress: boolean, err: string | null = null): void
    {
        this.serverInteractionError = err;
        this.serverInteractionInProgress = inProgress;
        this.$emit('serverInteractionInProgress', inProgress);
    }

    public saveConfig(): void {
        this.isSaving = true;
        this.setServerInteractionInProgress(true);

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.SaveEventNotificationConfigEndpoint}${queryStringIfEnabled}`;
        let payload = {
            config: this.internalConfig
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
        .then((data: EventSinkNotificationConfig) => this.onConfigSaved(data))
        .catch((e) => {
            this.isSaving = false;
            this.setServerInteractionInProgress(false, e);
            console.error(e);
        });
    }

    onConfigSaved(newConfig: EventSinkNotificationConfig): void {
        this.isSaving = false;
        this.setServerInteractionInProgress(false);
        this.$emit('configSaved', newConfig);
    }

    public tryDeleteConfig(): void {
        this.deleteDialogVisible = true;
    }

    public deleteConfig(): void {
        this.deleteDialogVisible = false;
        this.isDeleting = true;
        this.setServerInteractionInProgress(true);

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.DeleteEventNotificationConfigEndpoint}${queryStringIfEnabled}`;
        let payload = {
            configId: this.internalConfig.Id
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
        .then((data: EventSinkNotificationConfig) => this.onConfigDeleted(data))
        .catch((e) => {
            this.isDeleting = false;
            this.setServerInteractionInProgress(false, e);
            console.error(e);
        });
    }

    onConfigDeleted(config: EventSinkNotificationConfig): void {
        this.isDeleting = false;
        this.setServerInteractionInProgress(false);
        this.$emit('configDeleted', this.config);
    }

    formatDate(date: Date): string
    {
        return DateUtils.FormatDate(date, 'yyyy MMM d HH:mm:ss');
    }

    createOptionsObjectFor(notifier: IEventNotifier): Dictionary<string> {
        return notifier.Options
            .map(x => x.Id)
            .reduce((a: any, b: any) => { a[b] = ""; return a; }, {});
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onSaveConfigClicked(): void {
        this.saveConfig();
    }

    onDeleteConfigClicked(): void {
        this.deleteConfig();
    }

    onAddNotifierClicked(notifier: IEventNotifier): void {
        this.notifierDialogVisible = false;
        this.internalConfig.NotifierConfigs.push({
            NotifierId: notifier.Id,
            Notifier: notifier,
            Options: this.createOptionsObjectFor(notifier)
        });
    }

    onAddPayloadFilterClicked(): void {
        this.internalConfig.PayloadFilters.push({
            PropertyName: null,
            Filter: '',
            MatchType: FilterMatchType.Contains,
            CaseSensitive: false,
            _frontendId: IdUtils.generateId()
        });
    }

    onConfigFilterDelete(filterIndex: number): void {
        this.internalConfig.PayloadFilters.splice(filterIndex, 1);
    }
}
</script>

<style scoped lang="scss">
/* .root {
} */
</style>

<style lang="scss">
.possible-notifiers-dialog {
    max-width: 700px;

    .possible-notifiers-list-item {
        margin-bottom: 10px;
    }
}
</style>