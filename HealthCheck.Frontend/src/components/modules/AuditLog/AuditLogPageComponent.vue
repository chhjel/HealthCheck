<!-- src/components/modules/AuditLog/AuditLogPageComponent.vue -->
<template>
    <div>
        <div class="content-root">
            <!-- FILTER -->
            <div class="mb-4">
                <h1 class="mb-4">Audit log</h1>

                <div class="flex layout mb-3">
                    <div class="xs12 sm12 md8">
                        <date-picker-component range v-model:value="filterDate" :disabled="loadStatus.inProgress" :clearable="false"
                            @update:value="onDateRangeChanged" rangePresets="past" />
                    </div>

                    <div class="xs12 sm12 md4" style="text-align: right;">
                        <btn-component @click="loadData" :disabled="loadStatus.inProgress" class="primary">Search</btn-component>
                        <btn-component @click="resetFilters">Reset</btn-component>
                    </div>

                    <div class="xs12 sm6 md3 mb-2 pr-2">
                        <text-field-component v-model:value="filterUserName" label="Username"
                            @blur="loadData"
                            @keyup.enter="loadData" />
                    </div>
                    <div class="xs12 sm6 md3 mb-2 pr-2">
                        <text-field-component v-model:value="filterAction" label="Action"
                            @blur="loadData"
                            @keyup.enter="loadData" />
                    </div>
                    <div class="xs12 sm6 md3 mb-2 pr-2">
                        <text-field-component v-model:value="filterSubject" label="Subject"
                            @blur="loadData"
                            @keyup.enter="loadData" />
                    </div>
                    <div class="xs12 sm6 md3 mb-2 pr-2">
                        <text-field-component v-model:value="filterUserId" label="User id"
                            @blur="loadData"
                            @keyup.enter="loadData" />
                    </div>
                </div>

                <alert-component :value="true" color="error" icon="warning" v-if="loadStatus.failed">
                    {{ loadStatus.errorMessage }}
                </alert-component>
                <progress-linear-component color="primary" indeterminate v-if="loadStatus.inProgress"></progress-linear-component>

                <data-table-component
                    :headers="tableHeaders.map(x => x.text)"
                    :groups="auditEntryGroups"
                    class="audit-table">
                    <template v-slot:cell="{ value }">
                        <span>{{ value }}</span>
                    </template>
                    <template v-slot:expandedItem="{ item }">
                        <div class="row-details">
                            <div class="row-details-user mt-2 mb-2">
                                <icon-component class="row-details-usericon">person</icon-component>
                                <div class="row-details-username">{{ item.expandedValues[0].UserName }}</div>
                                <div class="row-details-userid ml-1">({{ item.expandedValues[0].UserId }})</div>
                                <div class="row-details-roles ml-1" v-if="item.expandedValues[0].UserAccessRoles.length > 0">
                                    <div v-for="(role, index) in item.expandedValues[0].UserAccessRoles"
                                        :key="`audit-${item.expandedValues[0].Id}-role-${index}`"
                                        class="role-tag">
                                        {{ role }}
                                    </div>
                                </div>
                            </div>
                            <div class="row-details-details" v-if="item.expandedValues[0].Details.length > 0">
                                <ul>
                                    <li v-for="(detail, index) in item.expandedValues[0].Details"
                                        :key="`audit-${item.expandedValues[0].Id}-detail-${index}`">
                                        <div class="row-details-details-title">{{ detail.Key }}:</div> <code class="row-details-details-value">{{ detail.Value }}</code>
                                    </li>
                                </ul>
                            </div>
                            <div class="row-details-blobs" v-if="item.expandedValues[0].Blobs.length > 0">
                                <ul>
                                    <li v-for="(blob, index) in item.expandedValues[0].Blobs"
                                        :key="`audit-${item.expandedValues[0].Id}-blob-${index}`">
                                        <div class="row-details-blobs-title">{{ blob.Key }}: </div> 
                                        <a class="row-details-blobs-value clickable"
                                            @click.prevent="onViewBlobClicked(item.expandedValues[0], blob)"
                                            >[View]</a>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </template>
                </data-table-component>
            </div>
        </div>

        <!-- ##################### -->
        <dialog-component v-model:value="showBlobContentsDialog" max-width="90%">
            <template #header>{{ currentBlobTitle }}</template>
            <template #footer>
                <btn-component color="secondary" @click="hideBlobContentsDialog()">Close</btn-component>
            </template>

            <div>
                <p class="blob-details">{{ currentBlobDetails }}</p>
                <progress-linear-component color="primary" indeterminate v-if="loadStatus.inProgress"></progress-linear-component>
                
                <alert-component :value="true" color="error" icon="warning" v-if="currentBlobError != null">
                    {{ currentBlobError }}
                </alert-component>

                <code class="blob-contents" v-if="currentBlobContents != null">{{ currentBlobContents }}</code>
            </div>
        </dialog-component>
        <!-- ##################### -->
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import DateUtils from '@util/DateUtils';
import AuditLogService from '@services/AuditLogService';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import KeyValuePair from '@models/Common/KeyValuePair';
import AuditEventViewModel from "@models/modules/AuditLog/AuditEventViewModel";
import AuditEventFilterInputData from "@models/modules/AuditLog/AuditEventFilterInputData";
import { StoreUtil } from "@util/StoreUtil";
import DataTableComponent from "@components/Common/DataTableComponent.vue";
import { DataTableGroup } from "@components/Common/DataTableComponent.vue.models";

@Options({
    components: {
        DataTableComponent
    }
})
export default class AuditLogPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;

    // Filter fields
    filterArea: string | null = null;
    filterSubject: string = "";
    filterAction: string = "";
    filterUserId: string = "";
    filterUserName: string = "";
    filterDate: Array<Date> = [new Date(), new Date()];
    get filterFromDate(): Date { return this.filterDate[0] };
    get filterToDate(): Date { return this.filterDate[1] };
    set filterFromDate(v: Date) { this.filterDate[0] = v; };
    set filterToDate(v: Date) { this.filterDate[1] = v; };

    // Table
    tableHeaders: Array<any> = [
        //{ text: 'Area', align: 'left', sortable: true, value: 'Area' },
        { text: 'Timestamp', value: 'Timestamp', align: 'left' },
        { text: 'UserName', value: 'UserName', align: 'left' },
        { text: 'Action', value: 'Action', align: 'left' },
        { text: 'Subject', value: 'Subject', align: 'left' }
    ];

    // Blobs
    showBlobContentsDialog: boolean = false;
    currentBlobTitle: string = '';
    currentBlobContents: string | null = '';
    currentBlobDetails: string = '';
    currentBlobError: string | null = '';

    // Service
    service: AuditLogService = new AuditLogService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    loadStatus: FetchStatus = new FetchStatus();
    
    filteredAuditEvents: Array<AuditEventViewModel> = [];
    auditEntryGroups: Array<DataTableGroup> = [];

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
        return StoreUtil.store.state.globalOptions;
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
    }

    loadData(): void {
        let payload = this.generateFilterPayload();
        this.service.Search(payload, this.loadStatus, {
            onSuccess: (data) => this.onEventDataRetrieved(data),
            onError: (err) => {
                this.filteredAuditEvents = [];
                this.auditEntryGroups = [];
            }
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
        this.filteredAuditEvents = events.sort((a,b) => b.Timestamp.getTime() - a.Timestamp.getTime());

        this.auditEntryGroups = [];
        let group: DataTableGroup = { title: '', items: [] };
        this.auditEntryGroups.push(group);
        this.filteredAuditEvents.forEach(e => {
            group.items.push({
                values: [ this.formatDateForTable(e.Timestamp), e.UserName, e.Action, e.Subject ],
                expandedValues: [ e ]
            });
        });
    }

    formatDateForTable(date: Date): string {
        return DateUtils.FormatDate(date, 'MMMM d, yyyy @ HH:mm:ss');
    }

    hideBlobContentsDialog(): void {
        this.showBlobContentsDialog = false;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDateRangeChanged(): void {
        this.loadData();
    }

    onViewBlobClicked(event: AuditEventViewModel, blob: KeyValuePair<string, string>): void {
        this.showBlobContentsDialog = true;

        const name = blob.Key;
        const id = blob.Value;

        this.currentBlobTitle = name;

        console.log(event);
        console.log(blob);
        let detailParts = [
            { name: 'Timestamp', value: this.formatDateForTable(event.Timestamp) },
            { name: 'Action', value: event.Action },
            { name: 'Subject', value: event.Subject },
            { name: 'User', value: event.UserName },
            { name: 'User Id', value: event.UserId }
        ]
        .filter(x => x.value != null && x.value.length > 0)
        .map(x => `${x.name}: ${x.value}`);

        this.currentBlobDetails = detailParts.join(' | ');
        this.currentBlobContents = null;
        this.currentBlobError = null;
        this.service.GetBlobContent(id, this.loadStatus, {
            onSuccess: (contents) => {
                if (contents != null)
                {
                    this.currentBlobContents = contents;
                }
                else
                {
                    this.currentBlobError = `Blob contents for id ${id} not found.`;
                }
            }
        });
    }
}
</script>

<style scoped lang="scss">
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
    /* color: #333; */
}
.row-details-blobs-title {
    font-weight: 600;
    display: inline;
}
.blob-details {
    font-size: small;
    font-weight: 600;
}
.blob-contents {
    width: 100%;
    overflow: auto;
    white-space: pre;

    &::before {
        content: '';
    }
}
</style>

<style lang="scss">
.audit-table th i {
    margin-left: 5px;
}
.audit-table .data-table--row-values {
    cursor: pointer;
    transition: 0.1s;
    &:hover {
        background-color: #eee;
    }
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
    fill: var(--color--primary-base) !important;
    color: var(--color--primary-base) !important;
}
.dateTimeWrapper a.confirm {
    background: var(--color--primary-base) !important;
    font-size: 0 !important;
}
.dateTimeWrapper a.confirm::after {
    content: 'OK';
    font-size: 13px;
}
.timeContainer .bigNumber {
    color: var(--color--primary-base) !important;
}
.dateTimeWrapper .calendar .innerStartDate,.innerEndDate {
    background: var(--color--primary-base) !important;
}
.dateTimeWrapper .calendar .day span:hover {
    background: var(--color--secondary-base) !important;
}
@media (max-width: 600px) {
    .audit-table {
        margin-left: -45px;
        margin-right: -45px;
    }
}
</style>