<!-- src/components/Common/EventNotificationConfigComponent.vue -->
<template>
    <div class="root">
        <v-switch 
            v-model="config.Enabled" 
            :disabled="allowChanges"
            label="Enabled"
            color="secondary"
        ></v-switch>

        <v-btn color="success"
            @click="onSaveConfigClicked()"
            :loading="isSaving"
            :disabled="allowChanges">
            <v-icon size="20px" class="mr-2">save</v-icon>
            {{ (config.Id == null ? 'Save new notification config' : 'Save changes') }}
        </v-btn>

        <v-btn color="error"
            @click="onDeleteConfigClicked()"
            :loading="isDeleting"
            :disabled="allowChanges">
            <v-icon size="20px" class="mr-2">delete</v-icon>
            Delete
        </v-btn>
        
        <v-alert
            :value="serverInteractionError != null && serverInteractionError.length > 0"
            type="error" >
            {{ serverInteractionError }}
        </v-alert>

        <pre v-if="config.LastChangedBy != null && config.LastChangedBy.length > 0">
            Last changed at {{ formatDate(config.LastChangedAt) }} by '{{ config.LastChangedBy }}' 
        </pre>
        <pre v-if="config.LastNotifiedAt != null">
            Last notified at {{ formatDate(config.LastNotifiedAt) }}
        </pre>

        <h3>Limits</h3>

        <v-text-field
            type="number"
            label="Notification count remaining"
            v-model="config.NotificationCountLimit"
            :disabled="allowChanges"
            required clearable />
            <!-- v-on:change="onValueChanged" -->
        
        <simple-date-time-component
            v-model="config.FromTime"
            :readonly="allowChanges"
            label="From"
            />
        
        <simple-date-time-component
            v-model="config.ToTime"
            :readonly="allowChanges"
            label="To"
            />

        <h3>Event id filter</h3>
        <config-filter-component
            v-model="config.EventIdFilter"
            :allow-property-name="false"
            :readonly="allowChanges"
            />
        
        <h3>Payload filters</h3>
        <config-filter-component
            v-for="(payloadFilter, pfindex) in config.PayloadFilters"
            :key="`payloadFilter-${pfindex}`"
            v-model="config.PayloadFilters[pfindex]"
            :readonly="allowChanges"
            :allow-property-name="true"
            />

        <h3>Notify using</h3>
        <div v-for="(notifierConfig, ncindex) in getValidNotifierConfigs(config)"
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
        <v-btn :disabled="allowChanges">
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
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { EventSinkNotificationConfigFilter, FilterMatchType, EventSinkNotificationConfig, NotifierConfig, Dictionary, IEventNotifier, NotifierConfigOptionsItem } from "../../models/EventNotifications/EventNotificationModels";
import SimpleDateTimeComponent from '.././Common/SimpleDateTimeComponent.vue';
import ConfigFilterComponent from '.././EventNotifications/ConfigFilterComponent.vue';
import FrontEndOptionsViewModel from "../../models/Page/FrontEndOptionsViewModel";
import DateUtils from "../../util/DateUtils";

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

    @Prop({ required: false, default: false })
    readonly!: boolean;

    isSaving: boolean = false;
    isDeleting: boolean = false;
    serverInteractionError: string | null = null;
    serverInteractionInProgress: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    beforeMount(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get allowChanges(): boolean {
        return this.readonly || this.serverInteractionInProgress;
    }

    ////////////////
    //  METHODS  //
    //////////////
    removeValidNotifierConfig(visibleIndex: number): void {
        let index = -1;
        for(let i=0;i<this.config.NotifierConfigs.length;i++)
        {
            if (this.config.NotifierConfigs[i].Notifier == null)
            {
                continue;
            }
            index++;

            if (index == visibleIndex)
            {
                this.config.NotifierConfigs.splice(i, 1);
                return;
            }
        }
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
    
    setServerInteractionInProgress(inProgress: boolean, err: string | null = null): void
    {
        this.serverInteractionError = err;
        this.serverInteractionInProgress = inProgress;
    }

    saveConfig(): void {
        this.isSaving = true;
        this.setServerInteractionInProgress(true);

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.SaveEventNotificationConfigEndpoint}${queryStringIfEnabled}`;
        let payload = {
            config: this.config
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

    onConfigSaved(config: EventSinkNotificationConfig): void {
        this.isSaving = false;
        this.setServerInteractionInProgress(false);
        this.$emit('configSaved', config);
    }

    deleteConfig(): void {
        this.isDeleting = true;
        this.setServerInteractionInProgress(true);

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.DeleteEventNotificationConfigEndpoint}${queryStringIfEnabled}`;
        let payload = {
            configId: this.config.Id
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

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onSaveConfigClicked(): void {
        this.saveConfig();
    }

    onDeleteConfigClicked(): void {
        this.deleteConfig();
    }
}
</script>

<style scoped lang="scss">
.root {
}
</style>