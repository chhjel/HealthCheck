<!-- src/components/Pages/AuditLogPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            
            <!-- FILTER -->
            <v-container grid-list-md>
                <h1 class="mb-4">Audit log</h1>

                <v-layout row wrap>
                    <v-flex xs12 sm12 md8>
                        <date-time-picker
                            :startDate="filterFromDate"
                            :endDate="filterToDate"
                            :singleDate="false"
                            timeFormat="HH:mm"
                            @onChange="onDateRangeChanged"
                        />
                    </v-flex>

                    <v-flex xs12 sm12 md4 style="text-align: right;">
                        <v-btn @click="loadData" :disabled="auditDataLoadInProgress" class="primary">Search</v-btn>
                        <v-btn @click="resetFilters">Reset</v-btn>
                    </v-flex>

                    <v-flex xs12 sm6 md3>
                        <v-text-field v-model="filterUserName" label="Username"></v-text-field>
                    </v-flex>
                    <v-flex xs12 sm6 md3>
                        <v-text-field v-model="filterAction" label="Action"></v-text-field>
                    </v-flex>
                    <v-flex xs12 sm6 md3>
                        <v-text-field v-model="filterSubject" label="Subject"></v-text-field>
                    </v-flex>
                    <v-flex xs12 sm6 md3>
                        <v-text-field v-model="filterUserId" label="User id"></v-text-field>
                    </v-flex>
                </v-layout>
                        
                <v-data-table
                    :headers="tableHeaders"
                    :items="filteredAuditEvents"
                    :loading="auditDataLoadInProgress"
                    :rows-per-page-items="tableRowsPerPageItems"
                    :pagination.sync="tablePagination"
                    :custom-sort="tableSorter"
                    item-key="Id"
                    expand
                    class="elevation-1 audit-table">
                    <v-progress-linear v-slot:progress color="primary" indeterminate></v-progress-linear>
                    <template v-slot:items="props">
                        <tr
                            @click="props.expanded = !props.expanded"
                            class="audit-table-row">
                            <td width="200">{{ formatDateForTable(props.item.Timestamp) }}</td>
                            <td class="text-xs-left">{{ props.item.UserName }}</td>
                            <td class="text-xs-left">{{ props.item.Action }}</td>
                            <td class="text-xs-left">{{ props.item.Subject }}</td>
                        </tr>
                    </template>
                    <template v-slot:expand="props">
                        <v-card flat>
                            <v-card-text class="row-details">
                                <div class="row-details-user mt-2 mb-2">
                                    <v-icon class="row-details-usericon">person</v-icon>
                                    <div class="row-details-username">{{ props.item.UserName }}</div>
                                    <div class="row-details-userid ml-1">({{ props.item.UserId }})</div>
                                    <div class="row-details-roles ml-1" v-if="props.item.UserAccessRoles.length > 0">
                                        <div v-for="(role, index) in props.item.UserAccessRoles"
                                            :key="`audit-${props.item.Id}-role-${index}`"
                                            class="row-details-role">
                                            {{ role }}
                                        </div>
                                    </div>
                                </div>
                                <div class="row-details-details" v-if="props.item.Details.length > 0">
                                    <ul>
                                        <li v-for="(detail, index) in props.item.Details"
                                            :key="`audit-${props.item.Id}-detail-${index}`">
                                            <div class="row-details-details-title">{{ detail.Key }}:</div> <code class="row-details-details-value">{{ detail.Value }}</code>
                                        </li>
                                    </ul>
                                </div>
                            </v-card-text>
                        </v-card>
                    </template>
                </v-data-table>
            </v-container>

          <!-- CONTENT END -->
        </v-flex>
        </v-layout>
        </v-container>
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Page/FrontEndOptionsViewModel';
import AuditEventViewModel from '../../models/AuditLog/AuditEventViewModel';
import AuditEventFilterInputData from '../../models/AuditLog/AuditEventFilterInputData';
import { AuditEventArea } from '../../models/AuditLog/AuditEventArea';
import DateUtils from '../../util/DateUtils';
import '@lazy-copilot/datetimepicker/dist/datetimepicker.css'
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";

@Component({
    components: {
        DateTimePicker
    }
})
export default class AuditLogPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // Filter fields
    filterArea: AuditEventArea | null = null;
    filterSubject: string = "";
    filterAction: string = "";
    filterUserId: string = "";
    filterUserName: string = "";
    filterFromDate: Date = new Date();
    filterToDate: Date = new Date();
    
    // UI state
    filterFromDateModal: boolean = false;
    filterToDateModal: boolean = false;

    // Table
    tableHeaders: Array<any> = [
        //{ text: 'Area', align: 'left', sortable: true, value: 'Area' },
        { text: 'When', value: 'Timestamp', align: 'left' },
        { text: 'Username', value: 'UserName', align: 'left' },
        { text: 'Action', value: 'Action', align: 'left' },
        { text: 'Subject', value: 'Subject', align: 'left' }
    ];
    tableRowsPerPageItems: Array<any> 
        = [10, 25, 50, {"text":"$vuetify.dataIterator.rowsPerPageAll","value":-1}];
    tablePagination: any = {
        rowsPerPage: 25
    };

    auditDataLoadInProgress: boolean = false;
    auditDataLoadFailed: boolean = false;
    auditDataFailedErrorMessage: string = "";
    
    filteredAuditEvents: Array<AuditEventViewModel> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.resetFilters();
        this.loadData();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    // get AreaChoices(): Array<AuditEventArea | null> {
    //     let choices = new Array<AuditEventArea | null>();
    //     choices.push(null);
    //     for (let value in AuditEventArea) {
    //         choices.push(value as AuditEventArea);
    //     }
    //     return choices;
    // }

    ////////////////
    //  METHODS  //
    //////////////
    resetFilters(): void {
        this.filterFromDate = new Date();
        this.filterFromDate.setDate(this.filterFromDate.getDate() - 2);
        this.filterFromDate.setHours(0);
        this.filterFromDate.setMinutes(0);
        this.filterToDate = new Date();
        this.filterToDate.setHours(23);
        this.filterToDate.setMinutes(59);

        this.filterArea = null;
        this.filterSubject = "";
        this.filterAction = "";
        this.filterUserId = "";
        this.filterUserName = "";
    }

    loadData(): void {
        this.auditDataLoadInProgress = true;
        this.auditDataLoadFailed = false;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.GetFilteredAuditLogEventsEndpoint}${queryStringIfEnabled}`;
        let payload = this.generateFilterPayload();
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
        // .then(response => new Promise<Array<AuditEventViewModel>>(resolve => setTimeout(() => resolve(response), 3000)))
        .then((events: Array<AuditEventViewModel>) => this.onEventDataRetrieved(events))
        .catch((e) => {
            this.auditDataLoadInProgress = false;
            this.auditDataLoadFailed = true;
            this.auditDataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    generateFilterPayload(): AuditEventFilterInputData {
        return {
            FromFilter: new Date(this.filterFromDate),
            ToFilter: new Date(this.filterToDate),
            AreaFilter: this.filterArea,
            SubjectFilter: this.filterSubject,
            ActionFilter: this.filterAction,
            UserIdFilter: this.filterUserId,
            UserNameFilter: this.filterUserName
        };
    }
    
    onEventDataRetrieved(events: Array<AuditEventViewModel>): void {
        this.auditDataLoadInProgress = false;
        let index = -1;
        events.forEach(x => {
            index++;
            x.Id = index.toString();
            x.Timestamp = new Date(x.Timestamp);
        });
        this.filteredAuditEvents = events;
    }

    formatDateForTable(date: Date): string {
        return DateUtils.FormatDate(date, 'MMMM d, yyyy @ HH:mm:ss');
    }

    tableSorter(items: AuditEventViewModel[], index: string, isDescending: boolean): AuditEventViewModel[]
    {
        console.log({ 
            items: items,
            index: index,
            isDescending: isDescending
        });
        if (index === "Timestamp")
        {
            return (isDescending)
                ? items.sort((a:AuditEventViewModel, b:AuditEventViewModel) => a.Timestamp.getTime() - b.Timestamp.getTime())
                : items.sort((a:AuditEventViewModel, b:AuditEventViewModel) => b.Timestamp.getTime() - a.Timestamp.getTime());
        }
        
        return items;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDateRangeChanged(data: any): void {
        this.filterFromDate = data.startDate;
        this.filterToDate = data.endDate;
    }
}
</script>

<style scoped>
.row-details-user {
    display: flex;
    align-items: center;
}
.row-details-username {
    display: inline;
    font-weight: 600;
}
.row-details-userid {
    display: inline;
}
.row-details-roles {
    display: inline;
}
.row-details-role {
    display: inline;
    color: var(--v-secondary-base);
    padding: 5px;
    border: 1px solid var(--v-secondary-base);
    border-radius: 15px;
    margin: 2px;
    font-size: 12px;
}
.row-details-details-title {
    font-weight: 600;
    display: inline;
}
.row-details-details-value {
    color: #333;
}
</style>

<style>
.audit-table-row {
    cursor: pointer;
}
.dateTimePickerWrapper {
}
input.calendarInput {
    color: #000 !important;
}
.calendarTrigger {
    min-width: inherit !important;
}
.calendarTrigger:hover {
    color: #000 !important;
    border: 1px solid #d5dbde !important;
}
.calendarTrigger:hover svg {
    fill: var(--v-primary-base) !important;
    color: var(--v-primary-base) !important;
}
.dateTimeWrapper a.confirm {
    background: var(--v-primary-base) !important;
    font-size: 0 !important;
}
.dateTimeWrapper a.confirm::after {
    content: 'OK';
    font-size: 13px;
}
.timeContainer .bigNumber {
    color: var(--v-primary-base) !important;
}
.dateTimeWrapper .calendar .innerStartDate,.innerEndDate {
    background: var(--v-primary-base) !important;
}
.dateTimeWrapper .calendar .day span:hover {
    background: var(--v-secondary-base) !important;
}
@media (max-width: 600px) {
    .audit-table {
        margin-left: -45px;
        margin-right: -45px;
    }
}
</style>