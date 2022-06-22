<!-- src/components/modules/EventNotifications/EventNotificationConfigComponent.vue -->
<template>
    <div class="root">
        <alert-component
            :value="serverInteractionError != null && serverInteractionError.length > 0"
            type="error" >
            {{ serverInteractionError }}
        </alert-component>

        <div class="header-data">
            <switch-component
                v-model:value="internalConfig.Enabled" 
                :disabled="!allowChanges"
                label="Enabled"
                color="secondary"
                class="left mr-2"
                style="flex: 1"
            ></switch-component>
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
            <select-component
                v-model:value="internalConfig.EventIdFilter.Filter"
                :items="knownEventIds"
                :readonly="!allowChanges"
                no-data-text="Unknown event id."
                placeholder="Event id"
                class="without-label"
                ref="eventIdFilter"
                allowInput allowCustom
                >
            </select-component>
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
                @change.self="onConfigFilterChanged(pfindex, $event)"
                />
            <small v-if="internalConfig.PayloadFilters.length == 0">No event payload filters added.</small>

            <div class="mt-4">
                <btn-component :disabled="!allowChanges"
                    @click="onAddPayloadFilterClicked(null)">
                    <icon-component size="20px" class="mr-2">add</icon-component>
                    Add payload filter
                </btn-component>

                <div v-if="suggestedPayloadProperties.length > 0" style="display: inline-block;">
                    <small>Suggested for selected event id:</small>
                    <chip-component
                        v-for="(suggestedPayloadProperty, sppIndex) in suggestedPayloadProperties"
                        :key="`suggested-payload-property-${sppIndex}`"
                        color="primary" outline
                        class="suggested-payload-property-choice"
                        @click="onSuggestedPayloadFilterClicked(suggestedPayloadProperty)">
                        {{ suggestedPayloadProperty }}
                        <icon-component right>add</icon-component>
                    </chip-component>
                    <!-- <chip-component
                        color="primary" outline
                        class="suggested-payload-property-choice"
                        @click="onAddPayloadFilterClicked">
                        Other
                        <icon-component right>add</icon-component>
                    </chip-component> -->
                </div>
            </div>
        </block-component>

        <!-- ###### NOTIFICATION ###### -->
        <block-component class="mb-4" title="Notifications">
            <div v-for="(notifierConfig, ncindex) in getValidNotifierConfigs()"
                :key="`notifierConfig-${ncindex}`"
                ref="notifiers"
                class="mb-3">

                <div style="display: flex; align-items: baseline; flex-direction: row; flex-wrap: nowrap;">
                    <h3 class="notifier-title">{{ notifierConfig.Notifier.Name }}</h3>
                    <btn-component color="error" right flat small class="ml-3"
                        @click="removeValidNotifierConfig(ncindex)"
                        :disabled="!allowChanges">
                        Remove
                    </btn-component>
                    <btn-component color="secondary" right flat small class="ml-1"
                        @click="showNotifierTest(ncindex)"
                        :disabled="!allowChanges">
                        Test..
                    </btn-component>
                </div>
                
                <div v-for="(notifierConfigOption, ncoindex) in getNotifierConfigOptions(notifierConfig.Notifier, notifierConfig.Options)"
                    :key="`notifierConfig-${ncindex}-option-${ncoindex}`"
                    style="margin-left:20px">
                    
                    <backend-input-component
                        v-model:value="notifierConfigOption.value"
                        @update:value="notifierConfig.Options[notifierConfigOption.key] = $event"
                        :config="notifierConfigOption.definition"
                        :readonly="!allowChanges"
                        :action-icon="getPlaceholdersFor(notifierConfig.Notifier, notifierConfigOption).length > 0 ? 'insert_link' : ''"
                        @actionIconClicked="showPlaceholdersFor(notifierConfig, notifierConfigOption.key, notifierConfigOption)"
                        />
                </div>
            </div>
            
            <small v-if="getValidNotifierConfigs(config).length == 0">No notifiers added, config will be disabled.</small>

            <btn-component
                :disabled="!allowChanges" 
                @click.stop="notifierDialogVisible = true" 
                v-if="notifiers != null">
                <icon-component size="20px" class="mr-2">add</icon-component>
                Add notifier
            </btn-component>
        </block-component>

        <!-- ###### LIMITS ###### -->
        <block-component class="mb-4" title="Limits">
            <input-component
                class="mt-2"
                v-model:value="internalConfig.NotificationCountLimit"
                :disabled="!allowChanges"
                name="Notification count remaining"
                description="Remaining number of times to allow notification. When zero is reached the config will be disabled."
                type="number"
                />
            
            <simple-date-time-component
                v-model:value="internalConfig.FromTime"
                :readonly="!allowChanges"
                name="From"
                description="Only allow notifications after this datetime."
                />
            
            <simple-date-time-component
                v-model:value="internalConfig.ToTime"
                :readonly="!allowChanges"
                name="To"
                description="Only allow notifications before this datetime. After this datetime the config will be disabled."
                />

            <input-component
                class="mt-2"
                v-model:value="internalConfig.DistinctNotificationKey"
                :disabled="!allowChanges"
                name="Distinct pattern"
                description="Optionally limit to a single notification for the given duration for each distinct value made from the pattern below that supports placeholders. Requires both a template value and a duration."
                type="text"
                :action-icon="getPayloadPlaceholders().length > 0 ? 'insert_link' : ''"
                actionIconTooltip="Insert placeholder"
                @actionIconClicked="showPayloadPlaceholdersDialog()"
                />

            <timespan-input-component
                class="mt-2"
                v-model:value="internalConfig.DistinctNotificationCacheDuration"
                :disabled="!allowChanges"
                name="Distinct duration"
                description="Duration to silence distinct notifications for, in hours, minutes and seconds."
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

        <dialog-component v-model:value="notifierDialogVisible" v-if="notifiers != null">
            <template #header>Select type of notifier to add</template>
            <template #footer>
                <btn-component color="secondary"  @click="notifierDialogVisible = false">Cancel</btn-component>
            </template>
            <div style="max-height: 500px;">
                <div class="possible-notifiers-list">
                    <div v-for="(notifier, nindex) in notifiers"
                        :key="`possible-notifier-${nindex}`"
                        @click="onAddNotifierClicked(notifier)"
                        class="possible-notifiers-list-item">
                        <div>
                            <icon-component>add</icon-component>
                        </div>

                        <div>
                            <div class="possible-notifier-item-title">{{ notifier.Name }}</div>
                            <div class="possible-notifier-item-description">{{ notifier.Description }}</div>
                        </div>
                    </div>
                </div>
            </div>
        </dialog-component>

        <dialog-component v-model:value="deleteDialogVisible" max-width="290">
            <template #header>Confirm deletion</template>
            <template #footer>
                <btn-component color="secondary" @click="deleteDialogVisible = false">Cancel</btn-component>
                <btn-component color="error" @click="deleteConfig()">Delete it</btn-component>
            </template>
            <div>
                Are you sure you want to delete this configuration?
            </div>
        </dialog-component>

        <dialog-component v-model:value="payloadPlaceholderDialogVisible">
            <template #header>Select placeholder to add</template>
            <template #footer>
                <btn-component color="secondary" @click="hidePayloadPlaceholderDialog()">Cancel</btn-component>
            </template>
            <div style="max-height: 500px;">
                <div class="possible-placeholders-list">
                    <div v-for="(placeholder, placeholderIndex) in getPayloadPlaceholders()"
                        :key="`possible-placeholder-${placeholderIndex}`"
                        @click="onAddPayloadPlaceholderClicked(placeholder)"
                        class="possible-placeholder-list-item">
                        <div>
                            <icon-component>add</icon-component>
                        </div>

                        <div>
                            <div class="possible-placeholder-item-title">{{ `\{${placeholder.toUpperCase()}\}` }}</div>
                        </div>
                    </div>
                </div>
            </div>
        </dialog-component>
        <dialog-component v-model:value="placeholderDialogVisible">
            <template #header>Select placeholder to add</template>
            <template #footer>
                <btn-component color="secondary" @click="hidePlaceholderDialog()">Cancel</btn-component>
            </template>
            <div style="max-height: 500px;">
                <div class="possible-placeholders-list">
                    <div v-for="(placeholder, placeholderIndex) in getPlaceholdersFor((currentPlaceholderDialogTargetConfig == null ? null :currentPlaceholderDialogTargetConfig.Notifier), currentPlaceholderDialogTarget)"
                        :key="`possible-placeholder-${placeholderIndex}`"
                        @click="onAddPlaceholderClicked(placeholder, currentPlaceholderDialogTarget)"
                        class="possible-placeholder-list-item">
                        <div>
                            <icon-component>add</icon-component>
                        </div>

                        <div>
                            <div class="possible-placeholder-item-title">{{ `\{${placeholder.toUpperCase()}\}` }}</div>
                        </div>
                    </div>
                </div>
            </div>
        </dialog-component>
        <dialog-component v-model:value="testDialogVisible">
            <template #header v-if="notifierToTest">Test notifier '{{ notifierToTest.Notifier.Name }}'</template>
            <template #footer>
                <btn-component color="secondary" @click="testDialogVisible = false">Close</btn-component>
            </template>

            <div v-if="notifierToTest">
                <div>
                    <p>No placeholders will be resolved, this is just to test the notifier itself.</p>
                    <btn-component color="primary"
                        @click="testNotifier"
                        :disabled="!allowChanges">
                        Execute notifier
                    </btn-component>
                </div>

                <div v-if="testResponse" class="ml-2 mt-3 mb-3">
                    <h4>Result:</h4>
                    <code class="notif-test-result">{{ testResponse }}</code>
                </div>
            </div>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import SimpleDateTimeComponent from '@components/Common/SimpleDateTimeComponent.vue';
import ConfigFilterComponent from '@components/modules/EventNotifications/ConfigFilterComponent.vue';
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import DateUtils from '@util/DateUtils';
import IdUtils from '@util/IdUtils';
import EventSinkNotificationConfigUtils, { ConfigFilterDescription, ConfigActionDescription } from '@util/EventNotifications/EventSinkNotificationConfigUtils';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import InputComponent from '@components/Common/Basic/InputComponent.vue';
import TimespanInputComponent from '@components/Common/Basic/TimespanInputComponent.vue';
import EventNotificationService from '@services/EventNotificationService';
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import { NewItemActionType } from "@generated/Enums/Core/NewItemActionType";
import { Dictionary, EventSinkNotificationConfig, EventSinkNotificationConfigFilter, FilterMatchType, IEventNotifier, KnownEventDefinition, NotifierConfig, NotifierConfigOptionsItem } from "@models/modules/EventNotifications/EventNotificationModels";
import { StoreUtil } from "@util/StoreUtil";

@Options({
    components: {
        ConfigFilterComponent,
        SimpleDateTimeComponent,
        BlockComponent,
        InputComponent,
        TimespanInputComponent,
        BackendInputComponent
    }
})
export default class EventNotificationConfigComponent extends Vue {
    @Prop({ required: true })
    moduleId!: string;

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
    service: EventNotificationService = new EventNotificationService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.moduleId);
    ASD!: EventSinkNotificationConfig;
    notifierDialogVisible: boolean = false;
    deleteDialogVisible: boolean = false;
    payloadPlaceholderDialogVisible: boolean = false;
    placeholderDialogVisible: boolean = false;
    currentPlaceholderDialogTarget: NotifierConfigOptionsItem | null = null;
    currentPlaceholderDialogTargetConfig: NotifierConfig | null = null;
    currentPlaceholderDialogTargetOptionKey: string | null = null;
    isSaving: boolean = false;
    isDeleting: boolean = false;
    serverInteractionError: string | null = null;
    serverInteractionInProgress: boolean = false;
    testDialogVisible: boolean = false;
    notifierToTest: NotifierConfig | null = null;
    testResponse: string = '';

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
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }
    
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

    showNotifierTest(visibleIndex: number): void {
        const notifier = this.getVisibleNotifierByIndex(visibleIndex);
        if (!notifier) return;

        this.notifierToTest = notifier;
        this.testDialogVisible = true;
        this.testResponse = '';
    }

    testNotifier(): void {
        if (!this.notifierToTest) return;
        
        this.setServerInteractionInProgress(true);
        this.service.TestNotifier(this.notifierToTest, null, {
            onSuccess: (data) => {
                this.setServerInteractionInProgress(false);
                this.testResponse = data;
            },
            onError: (message) => {
                this.setServerInteractionInProgress(false, message);
                this.testResponse = message;
            }
        });
    }

    getVisibleNotifierByIndex(visibleIndex: number): NotifierConfig | null {
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
                return this.internalConfig.NotifierConfigs[i];
            }
        }
        return null;
    }

    getValidNotifierConfigs(): Array<NotifierConfig> {
        return this.internalConfig.NotifierConfigs.filter(x => x.Notifier != null);
    }

    getNotifierOptionInputType(type: string): string {
        if (type == 'Int32' || type == 'Int64')
        {
            return 'number';
        }

        return 'text';
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

    showPayloadPlaceholdersDialog(): void {
        this.payloadPlaceholderDialogVisible = true;
    }

    hidePayloadPlaceholderDialog(): void {
        this.payloadPlaceholderDialogVisible = false;
    }

    onAddPlaceholderClicked(placeholder: string, option: NotifierConfigOptionsItem): void {
        if (this.currentPlaceholderDialogTargetConfig == null
            || this.currentPlaceholderDialogTargetOptionKey == null)
        {
            return;
        }

        let value = this.currentPlaceholderDialogTargetConfig.Options[this.currentPlaceholderDialogTargetOptionKey];
        value = `${(value || '')}\{${placeholder.toUpperCase()}\}`;

        this.currentPlaceholderDialogTargetConfig.Options[this.currentPlaceholderDialogTargetOptionKey] = value;
        // this.currentPlaceholderDialogTargetConfig.Options[this.currentPlaceholderDialogTargetOptionKey] = value;

        this.hidePlaceholderDialog();
    }

    getPlaceholdersFor(notifier: IEventNotifier, option: NotifierConfigOptionsItem): Array<string>
    {
        if (notifier == null || option == null || !option.definition.Flags.includes('SupportsPlaceholders'))
        {
            return [];
        }

        const customPlaceholders = notifier.Placeholders || [];
        const currentEventPlaceholders = this.currentEventDefinitionProperties;
        const extra = (this.currentEventDefinition == null || this.currentEventDefinition.IsStringified)
            ? ["payload"] : [];

        return customPlaceholders
            .concat(currentEventPlaceholders)
            .concat(extra)
            .concat(this.placeholders)
            .filter(x => x != null)
            .map(x => x.toUpperCase());
    }
    

    getPayloadPlaceholders(): Array<string>
    {
        return this.currentEventDefinitionProperties;
    }

    onAddPayloadPlaceholderClicked(placeholder: string): void {
        let value = this.internalConfig.DistinctNotificationKey ?? '';
        value = `${value}\{${placeholder.toUpperCase()}\}`;
        this.internalConfig.DistinctNotificationKey = value;

        this.hidePayloadPlaceholderDialog();
    }

    getNotifierConfigOptions(notifier: IEventNotifier, options: Dictionary<string>): Array<NotifierConfigOptionsItem> {
        return notifier.Options
            .map(def => {
                return {
                    key: def.Id,
                    definition: def,
                    value: options[def.Id]
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

        // Need timeout to first apply any changes from currently selected field.
        setTimeout(() => {
            this.saveConfigInternal();
        }, 50);
    }

    saveConfigInternal(): void {
        this.isSaving = true;
        this.setServerInteractionInProgress(true);

        this.service.SaveConfig(this.internalConfig, null, {
            onSuccess: (data) => this.onConfigSaved(data),
            onError: (message) => this.setServerInteractionInProgress(false, message),
            onDone: () => { this.isSaving = false }
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
        if (this.internalConfig.Id == null)
        {
            return;
        }

        this.deleteDialogVisible = false;
        this.isDeleting = true;
        this.setServerInteractionInProgress(true);

        this.service.DeleteConfig(this.internalConfig.Id, null, {
            onSuccess: (data) => this.onConfigDeleted(data),
            onError: (message) => this.setServerInteractionInProgress(false, message),
            onDone: () => { this.isDeleting = false }
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
    onAddNotifierClicked(notifier: IEventNotifier): void {
        this.notifierDialogVisible = false;
        let conf: NotifierConfig = {
            NotifierId: notifier.Id,
            Notifier: notifier,
            Options: this.createOptionsObjectFor(notifier)
        };
        Object.keys(conf.Options || {}).forEach(x => {
            const inputConfig = notifier.Options.filter(n => n.Id == x)[0];
            if (inputConfig)
            {
                conf.Options[x] = inputConfig.DefaultValue;
            }
        });
        this.internalConfig.NotifierConfigs.push(conf);
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
        this.internalConfig.PayloadFilters[index] = filter;
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

    @media (max-width: 900px) {
        font-size: 22px;
    }

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
.notifier-title {
    font-size: 18px;
    margin-bottom: 10px;
    margin-right: 10px;
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
.notif-test-result {
    width: 100%;
    overflow: auto;
    /* color: #333; */
    padding: 10px;

    &::before {
        content: '';
    }
}
</style>