<!-- src/components/modules/RequestLog/RequestEndpointComponent.vue -->
<template>
    <div>
        <h3 :data-endpoint-id="encodedEndpointId">
            <v-icon large :color="stateColor">{{ stateIcon }}</v-icon>
            <span class="info-trigger" @click="showDetails = !showDetails">
                <span v-if="!isGrouped">{{ entry.Controller }} - </span>{{ entry.Name }}
            </span>
        </h3>
        <p v-if="entry.Description != null" v-html="entry.Description"></p>
        <a target="_blank" v-if="entry.Url != null" :href="entry.Url">{{ entry.Url }}<br /></a>
        <i v-if="showDetails">
            <b>Controller:</b> {{ entry.FullControllerName }} | 
            <b>Action:</b> {{ entry.Action }} | 
            <b>Type:</b> {{ entry.ControllerType }} | 
            <b>Verb:</b> {{ entry.HttpVerb }}<br />
            <span style="white-space: nowrap; cursor: pointer;" @click="onIdClicked(entry.EndpointId)"><b>Id:</b> {{ entry.EndpointId }}</span>
        </i>

        <div v-if="visibleCalls.length > 0">
            <br />
            <h4>Last {{ visibleCalls.length }} calls without errors:</h4>
            <ul>
                <li
                    v-for="(call, index) in visibleCalls"
                    :key="`entry-call-${entry.Id}-${index}`" >
                    [{{ formatTimestamp(call.Timestamp) }}]
                    <a target="_blank" v-if="call.Url != null" :href="call.Url">{{ call.Url }}</a>
                    <span class="label-blob" v-if="call.StatusCode != null">Status code: {{ call.StatusCode }}</span>
                    <span class="label-blob" v-if="call.Version != null">v{{ call.Version }}</span>
                    <span class="label-blob clickable" v-if="call.SourceIP != null" @click="onIPClicked(call.SourceIP)">IP: {{ call.SourceIP }}</span>
                </li>
            </ul>
        </div>

        <div v-if="visibleErrors.length > 0">
            <br />
            <h4>Last {{ visibleErrors.length }} errors:</h4>
            <ul>
                <li
                    v-for="(call, index) in visibleErrors"
                    :key="`entry-error-${entry.Id}-${index}`" >
                    [{{ formatTimestamp(call.Timestamp) }}]
                    <a target="_blank" v-if="call.Url != null" :href="call.Url">{{ call.Url }}</a>
                    <span class="label-blob" v-if="call.StatusCode != null">Status code: {{ call.StatusCode }}</span>
                    <span class="label-blob" v-if="call.Version != null">v{{ call.Version }}</span>
                    <span class="label-blob clickable" v-if="call.SourceIP != null" @click="onIPClicked(call.SourceIP)">IP: {{ call.SourceIP }}</span>
                    <code>{{ call.ErrorDetails }}</code>
                </li>
            </ul>
        </div>

        <div v-if="visibleCalls.length == 0 && visibleErrors.length == 0">
            <small>No requests logged for this endpoint yet.</small>
        </div>

        <!-- <code>{{ JSON.stringify(entry, null, 2) }}</code> -->
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import LoggedEndpointDefinitionViewModel from  '../../../models/modules/RequestLog/LoggedEndpointDefinitionViewModel';
import LoggedEndpointRequestViewModel from  '../../../models/modules/RequestLog/LoggedEndpointRequestViewModel';
import { EntryState } from  '../../../models/modules/RequestLog/EntryState';
import DateUtils from  '../../../util/DateUtils';
import LinqUtils from  '../../../util/LinqUtils';

@Component({
    components: {
    }
})
export default class ActionLogEntryComponent extends Vue {
    @Prop({ required: true })
    entry!: LoggedEndpointDefinitionViewModel;
    
    @Prop({ required: true })
    isGrouped!: boolean;

    showDetails: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get encodedEndpointId(): string {
      return encodeURI(this.entry.EndpointId);
    }

    get visibleCalls(): Array<LoggedEndpointRequestViewModel>
    {
        return this.entry.Calls;//.filter(x => this.showCall(x));
    }

    get visibleErrors(): Array<LoggedEndpointRequestViewModel>
    {
        return this.entry.Errors;//.filter(x => this.showCall(x));
    }

    get state(): EntryState {
        if (this.entry.Errors.length > 0) {
            return EntryState.Error;
        }
        else if (this.entry.Calls.length > 0) {
            return EntryState.Success;
        }
        else {
            return EntryState.Undetermined;
        }
    }

    get isSuccess(): boolean { return this.state === EntryState.Success; }
    get isError(): boolean { return this.state === EntryState.Error; }
    get isUndetermined(): boolean { return this.state === EntryState.Undetermined; }

    get stateIcon(): string {
        if (this.state === EntryState.Success) {
            return 'mdi-emoticon-happy-outline';
        }
        else if (this.state === EntryState.Error) {
            return 'mdi-emoticon-sad-outline';
        }
        else {
            return 'mdi-cow';
        }
    }

    get stateColor(): string {
        if (this.state === EntryState.Success) {
            return 'green darken-2';
        }
        else if (this.state === EntryState.Error) {
            return 'red darken-2';
        }
        else {
            return 'gray';
        }
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    formatTimestamp(date: Date): string {
        return DateUtils.FormatDate(date, "dd/MM/yy HH:mm:ss");
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onIPClicked(ip: string): void {
        this.$emit('IPClicked', ip);
    }

    onIdClicked(id: string): void {
        this.$emit('IdClicked', id);
    }
}
</script>

<style scoped lang="scss">
.info-trigger {
    cursor: help;
}
.label-blob
{
    border-radius: 3px;
    border: 1px solid gray;
    padding: 2px;
    margin: 2px;
    font-size: 10px;

    &.clickable {
        cursor: pointer;
        text-decoration: underline;
        transition: background-color 0.1s ease;

        &:hover {
            background-color: lightgray;
        }
    }
}
</style>