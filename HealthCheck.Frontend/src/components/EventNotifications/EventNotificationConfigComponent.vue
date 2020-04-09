<!-- src/components/Common/EventNotificationConfigComponent.vue -->
<template>
    <div class="root">
        <v-alert
            :value="serverInteractionError != null && serverInteractionError.length > 0"
            type="error" >
            {{ serverInteractionError }}
        </v-alert>

        <div class="header-data">
            <v-switch
                v-model="internalConfig.Enabled" 
                :disabled="!allowChanges"
                label="Enabled"
                color="secondary"
                class="left mr-2"
                style="flex: 1"
            ></v-switch>
            <div>
                <div class="metadata-chip"
                    v-if="internalConfig.LastChangedBy != null && internalConfig.LastChangedBy.length > 0">
                    Last changed at {{ formatDate(internalConfig.LastChangedAt) }} by '{{ internalConfig.LastChangedBy }}'
                </div>
                <div class="metadata-chip"
                    v-if="internalConfig.LastNotifiedAt != null">
                    Last notified at {{ formatDate(internalConfig.LastNotifiedAt) }}
                </div>
            </div>
        </div>

        <div class="config-summary">
            <b>IF</b>
            <span v-for="(condition, condIndex) in descriptionConditions"
                :key="`condition-${condIndex}`">
                <a @click.prevent.stop="onSummaryConditionClicked(condition)"
                    >{{ condition.description }}</a>
                <span v-if="condIndex == descriptionConditions.length - 2"> and </span>
                <span v-else-if="condIndex < descriptionConditions.length - 1">, </span>
            </span>

            <br />
            <b>THEN</b>
            <span v-if="descriptionActions.length == 0">&lt;do nothing&gt;</span>
            <span v-else>notify using</span>
            <span v-for="(action, actIndex) in descriptionActions"
                :key="`action-${actIndex}`">
                <a @click.prevent.stop="onSummaryActionClicked(action)"
                    >{{ action.description }}</a>
                <span v-if="actIndex == descriptionActions.length - 2"> and </span>
                <span v-else-if="actIndex < descriptionActions.length - 1">, </span>
            </span>
        </div>

        <!-- ###### EVENT ID ###### -->
        <block-component class="mb-4">
            <h3>Event id to listen for</h3>
            <v-combobox
                v-model="internalConfig.EventIdFilter.Filter"
                :items="knownEventIds"
                :readonly="!allowChanges"
                no-data-text="Unknown event id."
                placeholder="Event id"
                class="without-label"
                ref="eventIdFilter"
                >
            </v-combobox>
            <!-- label="Event id" -->
            <!-- </div> -->
            <!-- prepend-icon="mdi-city" -->
            <!-- :hint="!isEditing ? 'Click the icon to edit' : 'Click the icon to save'" -->
            <!-- <config-filter-component
                :config="internalConfig.EventIdFilter"
                :allow-property-name="false"
                :readonly="!allowChanges"
                @change="internalConfig.EventIdFilter = $event"
                /> -->
            
            <!-- ###### PAYLOAD FILTERS ###### -->
            <h3>Filters</h3>
            <config-filter-component
                v-for="(payloadFilter, pfindex) in internalConfig.PayloadFilters"
                :key="`payloadFilter-${pfindex}-${payloadFilter._frontendId}`"
                ref="payloadFilters"
                class="payload-filter"
                :config="payloadFilter"
                :readonly="!allowChanges"
                :allow-property-name="true"
                :allow-delete="true"
                @delete="onConfigFilterDelete(pfindex)"
                @change="onConfigFilterChanged(pfindex, $event)"
                />
            <small v-if="internalConfig.PayloadFilters.length == 0">No event payload filters added.</small>

            <div class="mt-4">
                <v-btn :disabled="!allowChanges"
                    @click="onAddPayloadFilterClicked(null)">
                    <v-icon size="20px" class="mr-2">add</v-icon>
                    Add payload filter
                </v-btn>

                <div v-if="suggestedPayloadProperties.length > 0" style="display: inline-block;">
                    <small>Suggested for selected event id:</small>
                    <v-chip
                        v-for="(suggestedPayloadProperty, sppIndex) in suggestedPayloadProperties"
                        :key="`suggested-payload-property-${sppIndex}`"
                        color="primary" outline
                        class="suggested-payload-property-choice"
                        @click="onSuggestedPayloadFilterClicked(suggestedPayloadProperty)">
                        {{ suggestedPayloadProperty }}
                        <v-icon right>add</v-icon>
                    </v-chip>
                    <!-- <v-chip
                        color="primary" outline
                        class="suggested-payload-property-choice"
                        @click="onAddPayloadFilterClicked">
                        Other
                        <v-icon right>add</v-icon>
                    </v-chip> -->
                </div>
            </div>
        </block-component>

        <!-- ###### NOTIFICATION ###### -->
        <block-component class="mb-4" title="Notifications">
            <div v-for="(notifierConfig, ncindex) in getValidNotifierConfigs()"
                :key="`notifierConfig-${ncindex}`"
                ref="notifiers"
                class="mb-3">

                <b>{{ notifierConfig.Notifier.Name }}</b>
                <v-btn color="error"
                    @click="removeValidNotifierConfig(ncindex)"
                    :disabled="!allowChanges">
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
                        :disabled="!allowChanges"
                        :append-outer-icon="getPlaceholdersFor(notifierConfig.Notifier, notifierConfigOption).length == 0 ? '' : 'insert_link'"
                        @click:append-outer="showPlaceholdersFor(notifierConfig, notifierConfigOption.key, notifierConfigOption)"
                        persistent-hint
                        required clearable />
                </div>
            </div>
            
            <small v-if="getValidNotifierConfigs(config).length == 0">No notifiers added, config will be disabled.</small>

            <v-btn
                :disabled="!allowChanges" 
                @click.stop="notifierDialogVisible = true" 
                v-if="notifiers != null">
                <v-icon size="20px" class="mr-2">add</v-icon>
                Add notifier
            </v-btn>
        </block-component>

        <!-- ###### LIMITS ###### -->
        <block-component class="mb-4" title="Limits">
            <v-text-field
                type="number"
                label="Notification count remaining"
                v-model="internalConfig.NotificationCountLimit"
                :disabled="!allowChanges"
                required clearable />
                <!-- v-on:change="onValueChanged" -->
            
            <simple-date-time-component
                v-model="internalConfig.FromTime"
                :readonly="!allowChanges"
                label="From"
                />
            
            <simple-date-time-component
                v-model="internalConfig.ToTime"
                :readonly="!allowChanges"
                label="To"
                />
        </block-component>

        <!-- ###### LOG ###### -->
        <block-component class="mb-5" title="Last 10 notification results">
            <ul>
                <li v-if="internalConfig.LatestResults.length == 0">No results yet</li>
                <li
                    v-for="(result, rindex) in internalConfig.LatestResults"
                    :key="`LatestResults-${rindex}`">
                    {{ result }}
                </li>
            </ul>
        </block-component>

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

        <v-dialog v-model="placeholderDialogVisible"
            scrollable
            content-class="possible-placeholders-dialog">
            <v-card>
                <v-card-title>Select placeholder to add</v-card-title>
                <v-divider></v-divider>
                <v-card-text style="max-height: 500px;">
                    <v-list class="possible-placeholders-list">
                        <v-list-tile v-for="(placeholder, placeholderIndex) in getPlaceholdersFor((currentPlaceholderDialogTargetConfig == null ? null :currentPlaceholderDialogTargetConfig.Notifier), currentPlaceholderDialogTarget)"
                            :key="`possible-placeholder-${placeholderIndex}`"
                            @click="onAddPlaceholderClicked(placeholder, currentPlaceholderDialogTarget)"
                            class="possible-placeholder-list-item">
                            <v-list-tile-action>
                                <v-icon>add</v-icon>
                            </v-list-tile-action>

                            <v-list-tile-content>
                                <v-list-tile-title class="possible-placeholder-item-title">{{ `\{${placeholder.toUpperCase()}\}` }}</v-list-tile-title>
                            </v-list-tile-content>
                        </v-list-tile>
                    </v-list>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary" flat @click="hidePlaceholderDialog()">Cancel</v-btn>
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
import EventSinkNotificationConfigUtils, { ConfigFilterDescription, ConfigDescription, ConfigActionDescription } from "../../util/EventNotifications/EventSinkNotificationConfigUtils";
import BlockComponent from '../../components/Common/Basic/BlockComponent.vue';

@Component({
    components: {
        ConfigFilterComponent,
        SimpleDateTimeComponent,
        BlockComponent
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

    @Prop({ required: false, default: [] })
    placeholders!: Array<string>;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    // @ts-ignore
    internalConfig: EventSinkNotificationConfig = null;
    ASD!: EventSinkNotificationConfig;
    notifierDialogVisible: boolean = false;
    deleteDialogVisible: boolean = false;
    placeholderDialogVisible: boolean = false;
    currentPlaceholderDialogTarget: NotifierConfigOptionsItem | null = null;
    currentPlaceholderDialogTargetConfig: NotifierConfig | null = null;
    currentPlaceholderDialogTargetOptionKey: string | null = null;
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
        return !this.readonly && !this.serverInteractionInProgress;
    }

    get description(): string
    {
        return EventSinkNotificationConfigUtils.describeConfig(this.internalConfig).description;
    }

    get descriptionConditions(): Array<ConfigFilterDescription>
    {
        return EventSinkNotificationConfigUtils.describeConfig(this.internalConfig).conditions;
    }

    get descriptionActions(): Array<ConfigActionDescription>
    {
        return EventSinkNotificationConfigUtils.describeConfig(this.internalConfig).actions;
    }

    get knownEventIds(): Array<string> {
        if (this.eventdefinitions == null) return [];
        else return this.eventdefinitions.map(x => x.EventId);
    }

    get possiblePlaceholders(): Array<string> {
        return this.currentEventDefinitionProperties;
    }

    get currentEventDefinition(): KnownEventDefinition | null {
        if (this.eventdefinitions == null) return null;
        
        return this.eventdefinitions.filter(x => 
            x.EventId != null && this.internalConfig.EventIdFilter.Filter != null
            && x.EventId.toLowerCase().trim() == this.internalConfig.EventIdFilter.Filter.toLowerCase().trim()
        )[0];
    }

    get currentEventDefinitionProperties(): Array<string> {
        if (this.currentEventDefinition == null) return [];
        else return this.currentEventDefinition.PayloadProperties || [];
    }

    get suggestedPayloadProperties(): Array<string> {
        if (this.eventdefinitions == null) return [];
        
        const currentEventDefinition = this.eventdefinitions.filter(x => 
            x.EventId != null && this.internalConfig.EventIdFilter.Filter != null
            && x.EventId.toLowerCase().trim() == this.internalConfig.EventIdFilter.Filter.toLowerCase().trim()
        )[0];
        if (currentEventDefinition == null) return [];
        
        const currentPayloadFilterProps = this.internalConfig.PayloadFilters
            .map(x => (x.PropertyName || '').toLowerCase().trim());
        
        const suggestions: Array<string>
            = currentEventDefinition.PayloadProperties
            .filter(x => x != null && currentPayloadFilterProps.indexOf(x.toLowerCase().trim()) == -1)
        
        return suggestions;
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

    showPlaceholdersFor(config: NotifierConfig, optionKey: string, option: NotifierConfigOptionsItem): void
    {
        this.currentPlaceholderDialogTarget = option;
        this.currentPlaceholderDialogTargetConfig = config;
        this.currentPlaceholderDialogTargetOptionKey = optionKey;
        this.placeholderDialogVisible = true;
    }

    hidePlaceholderDialog(): void {
        this.placeholderDialogVisible = false;
        this.currentPlaceholderDialogTarget = null;
        this.currentPlaceholderDialogTargetConfig = null;
        this.currentPlaceholderDialogTargetOptionKey = null;
    }

    onAddPlaceholderClicked(placeholder: string, option: NotifierConfigOptionsItem): void {
        if (this.currentPlaceholderDialogTargetConfig == null
            || this.currentPlaceholderDialogTargetOptionKey == null)
        {
            return;
        }

        let value = this.currentPlaceholderDialogTargetConfig.Options[this.currentPlaceholderDialogTargetOptionKey];
        value = `${value}\{${placeholder.toUpperCase()}\}`;
        this.currentPlaceholderDialogTargetConfig.Options[this.currentPlaceholderDialogTargetOptionKey] = value;

        this.hidePlaceholderDialog();
    }

    getPlaceholdersFor(notifier: IEventNotifier, option: NotifierConfigOptionsItem): Array<string>
    {
        if (notifier == null || option == null || !option.definition.SupportsPlaceholders)
        {
            return [];
        }

        const customPlaceholders = notifier.Placeholders || [];
        const currentEventPlaceholders = this.currentEventDefinitionProperties;

        return customPlaceholders
            .concat(currentEventPlaceholders)
            .concat(this.placeholders)
            .filter(x => x != null)
            .map(x => x.toUpperCase());
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

    onSuggestedPayloadFilterClicked(propName: string): void {
        this.onAddPayloadFilterClicked(propName);
    }

    onAddPayloadFilterClicked(propName: string | null = null): void {
        if (propName == null && this.currentEventDefinition != null)
        {
            if (!this.currentEventDefinition.IsStringified)
            {
                propName = '';
            }
        }

        this.internalConfig.PayloadFilters.push({
            PropertyName: propName,
            Filter: '',
            MatchType: FilterMatchType.Contains,
            CaseSensitive: false,
            _frontendId: IdUtils.generateId()
        });
    }

    onConfigFilterDelete(filterIndex: number): void {
        this.internalConfig.PayloadFilters.splice(filterIndex, 1);
    }

    onConfigFilterChanged(index: number, filter: EventSinkNotificationConfigFilter): void {
        Vue.set(this.internalConfig.PayloadFilters, index, filter);
    }

    onSummaryConditionClicked(condition: ConfigFilterDescription): void {
        let targetElement: HTMLElement | null = null;
        if (condition.id == this.internalConfig.EventIdFilter._frontendId) 
        {
            targetElement = this.$refs.eventIdFilter as HTMLElement;
        }
        else
        {
            const index = this.internalConfig.PayloadFilters.findIndex(x => x._frontendId == condition.id);
            const comp = (<Vue[]>this.$refs.payloadFilters)[index] as ConfigFilterComponent;
            targetElement = (comp.$el.querySelectorAll("input")[0]) as HTMLElement;
        }

        if (targetElement != null)
        {
            targetElement.focus();
        }
    }

    onSummaryActionClicked(action: ConfigActionDescription): void
    {
        const index = this.internalConfig.NotifierConfigs
            .filter(x => x.Notifier != null)
            .findIndex(x => x.NotifierId == action.id);
        const el = (<Element[]>this.$refs.notifiers)[index];
        let targetElement = (el.querySelectorAll("input")[0]) as HTMLElement;
        
        if (targetElement != null)
        {
            targetElement.focus();
        }
    }
}
</script>

<style scoped lang="scss">
.metadata-chip {
    display: inline-block;
    border: 1px solid gray;
    padding: 5px;
    margin: 5px;
    font-size: 12px;
}
.without-label {
    margin-top: 0;
    padding-top: 0;
}
.payload-filter {
    border-bottom: solid 1px #ccc;
}
.config-summary {
    padding: 10px;
    margin-top: 20px;
    margin-bottom: 20px;
    text-align: center;
    font-size: 26px;

    a {
        text-decoration: underline;
    }
}
.header-data {
    display: flex;
    align-items: center;
    flex-direction: row;
    flex-wrap: wrap-reverse;
}
</style>

<style lang="scss">
.possible-notifiers-dialog {
    max-width: 700px;

    .possible-notifiers-list-item {
        margin-bottom: 10px;
    }
}
.possible-placeholders-dialog {
    max-width: 700px;

    .possible-placeholder-list-item {
        margin-bottom: 10px;
    }
}
</style>