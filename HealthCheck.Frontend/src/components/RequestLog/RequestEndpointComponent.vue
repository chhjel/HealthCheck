<!-- src/components/RequestLog/RequestEndpointComponent.vue -->
<template>
    <div>
        <h3>
            <v-icon large :color="stateColor">{{ stateIcon }}</v-icon>
            {{ entry.Name }}
        </h3>
        <p v-if="entry.Description != null" v-html="entry.Description"></p>
        <a v-if="entry.Url != null" :href="entry.Url">{{ entry.Url }}<br /></a>
        <i>
            Controller: {{ entry.FullControllerName }},
            Type: {{ entry.ControllerType }},
            Action: {{ entry.Action }}, 
            Verb: {{ entry.HttpVerb }}<br />
            <small style="color:gray">({{ entry.EndpointId }})</small>
        </i><br />

        <div v-if="visibleCalls.length > 0">
            <h4>Last {{ visibleCalls.length }} calls without errors:</h4>
            <ul>
                <li
                    v-for="(call, index) in visibleCalls"
                    :key="`entry-call-${entry.Id}-${index}`" >
                    [{{ formatTimestamp(call.Timestamp) }}] {{ call.Url }} (Status code: {{ call.StatusCode }}) (Version: {{ call.Version }})
                </li>
            </ul>
            <br />
        </div>

        <div v-if="visibleErrors.length > 0">
            <h4>Last {{ visibleErrors.length }} errors:</h4>
            <ul>
                <li
                    v-for="(call, index) in visibleErrors"
                    :key="`entry-error-${entry.Id}-${index}`" >
                    [{{ formatTimestamp(call.Timestamp) }}] {{ call.Url }} (Status code: {{ call.StatusCode }})<br />
                    <code>{{ call.ErrorDetails }}</code>
                </li>
            </ul>
            <br />
        </div>

        <!-- <code>{{ JSON.stringify(entry, null, 2) }}</code> -->
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import LoggedActionEntryViewModel from "../../models/RequestLog/LoggedActionEntryViewModel";
import LoggedActionCallEntryViewModel from "../../models/RequestLog/LoggedActionCallEntryViewModel";
import { EntryState } from '../../models/RequestLog/EntryState';
import FrontEndOptionsViewModel from "../../models//Page/FrontEndOptionsViewModel";
import DateUtils from "../../util/DateUtils";
import LinqUtils from "../../util/LinqUtils";

@Component({
    components: {
    }
})
export default class ActionLogEntryComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;
    
    @Prop({ required: true })
    entry!: LoggedActionEntryViewModel;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get visibleCalls(): Array<LoggedActionCallEntryViewModel>
    {
        return this.entry.Calls;//.filter(x => this.showCall(x));
    }

    get visibleErrors(): Array<LoggedActionCallEntryViewModel>
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
    
}
</script>

<style scoped>
</style>