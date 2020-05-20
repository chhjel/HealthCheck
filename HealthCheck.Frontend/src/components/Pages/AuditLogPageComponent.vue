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
                            ref="filterDate"
                            :startDate="filterFromDate"
                            :endDate="filterToDate"
                            :singleDate="false"
                            timeFormat="HH:mm"
                            @onChange="onDateRangeChanged"
                        />
                    </v-flex>

                    <v-flex xs12 sm12 md4 style="text-align: right;">
                        <v-btn @click="loadData" :disabled="loadStatus.inProgress" class="primary">Search</v-btn>
                        <v-btn @click="resetFilters">Reset</v-btn>
                    </v-flex>

                    <v-flex xs12 sm6 md3>
                        <v-text-field v-model="filterUserName" label="Username"
                            @blur="loadData"
                            @keyup.enter="loadData" />
                    </v-flex>
                    <v-flex xs12 sm6 md3>
                        <v-text-field v-model="filterAction" label="Action"
                            @blur="loadData"
                            @keyup.enter="loadData" />
                    </v-flex>
                    <v-flex xs12 sm6 md3>
                        <v-text-field v-model="filterSubject" label="Subject"
                            @blur="loadData"
                            @keyup.enter="loadData" />
                    </v-flex>
                    <v-flex xs12 sm6 md3>
                        <v-text-field v-model="filterUserId" label="User id"
                            @blur="loadData"
                            @keyup.enter="loadData" />
                    </v-flex>
                </v-layout>
                        
                <v-data-table
                    :headers="tableHeaders"
                    :items="filteredAuditEvents"
                    :loading="loadStatus.inProgress"
                    :rows-per-page-items="tableRowsPerPageItems"
                    :pagination.sync="tablePagination"
                    :custom-sort="tableSorter"
                    item-key="Id"
                    expand
                    class="elevation-1 audit-table">
                    <v-progress-linear v-slot:progress color="primary" indeterminate></v-progress-linear>
                    <template v-slot:no-data>
                    <v-alert :value="true" color="error" icon="warning" v-if="loadStatus.failed">
                        {{ loadStatus.errorMessage }}
                    </v-alert>
                    </template>
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
                                            class="role-tag">
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
import FrontEndOptionsViewModel from '../../models/Common/FrontEndOptionsViewModel';
import AuditEventViewModel from '../../models/AuditLog/AuditEventViewModel';
import AuditEventFilterInputData from '../../models/AuditLog/AuditEventFilterInputData';
import DateUtils from '../../util/DateUtils';
import '@lazy-copilot/datetimepicker/dist/datetimepicker.css'
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";
import AuditLogService from "../../services/AuditLogService";
import { FetchStatus } from "../../services/abstractions/HCServiceBase";

@Component({
    components: {
        DateTimePicker
    }
})
export default class AuditLogPageComponent extends Vue {
    // Filter fields
    filterArea: string | null = null;
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

    // Service
    service: AuditLogService = new AuditLogService(this.globalOptions);
    loadStatus: FetchStatus = new FetchStatus();
    
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
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }

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

        let dateFilterFormat = 'yyyy MMM d  HH:mm';
        (<any>this.$refs.filterDate).selectDateString 
            = `${DateUtils.FormatDate(this.filterFromDate, dateFilterFormat)} - ${DateUtils.FormatDate(this.filterToDate, dateFilterFormat)}`;
    }

    loadData(): void {
        let payload = this.generateFilterPayload();
        this.service.Search(payload, this.loadStatus, {
            onSuccess: (data) => this.onEventDataRetrieved(data),
            onError: (err) => this.filteredAuditEvents = []
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
        // console.log({ 
        //     items: items,
        //     index: index,
        //     isDescending: isDescending
        // });
        if (index == null) {
            // Do nothing
        }
        else if (index === "Timestamp")
        {
            items = items.sort((a:AuditEventViewModel, b:AuditEventViewModel) => b.Timestamp.getTime() - a.Timestamp.getTime());
        }
        else {
            items = items.sort((a:AuditEventViewModel, b:AuditEventViewModel) => {
            
                var textA = (<any>a)[index].toUpperCase();
                var textB = (<any>b)[index].toUpperCase();
                return (textA < textB) ? -1 : (textA > textB) ? 1 : 0;
            });
        }
        
        if (isDescending === true) {
            items = items.reverse();
        }

        return items;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDateRangeChanged(data: any): void {
        this.filterFromDate = data.startDate;
        this.filterToDate = data.endDate;
        this.loadData();
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
/* .row-details-role {
    display: inline;
    color: var(--v-secondary-base);
    padding: 5px;
    border: 1px solid var(--v-secondary-base);
    border-radius: 15px;
    margin: 2px;
    font-size: 12px;
} */
.role-tag {
    display: inline;
    color: #4c41a5;
    background-color: #d5d5f7;
    padding: 6px;
    margin-left: 8px;
    align-self: center;
    font-weight: 500;
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
.audit-table th i {
    margin-left: 5px;
}
.audit-table-row {
    cursor: pointer;
}
/* .dateTimePickerWrapper {
} */
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