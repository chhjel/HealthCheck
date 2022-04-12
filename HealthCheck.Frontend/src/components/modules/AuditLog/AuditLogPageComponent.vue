<!-- src/components/modules/AuditLog/AuditLogPageComponent.vue -->
<template>
    <div>
        <div class="content-root">
            <!-- FILTER -->
            <div grid-list-md>
                <h1 class="mb-4">Audit log</h1>

                <div row wrap>
                    <div xs12 sm12 md8>
                        <datepicker range v-model="filterDate" :disabled="loadStatus.inProgress"
                            :clearable="false"
                            @update:modelValue="onDateRangeChanged" />
                    </div>

                    <div xs12 sm12 md4 style="text-align: right;">
                        <btn-component @click="loadData" :disabled="loadStatus.inProgress" class="primary">Search</btn-component>
                        <btn-component @click="resetFilters">Reset</btn-component>
                    </div>

                    <div xs12 sm6 md3>
                        <text-field-component v-model:value="filterUserName" label="Username"
                            @blur="loadData"
                            @keyup.enter="loadData" />
                    </div>
                    <div xs12 sm6 md3>
                        <text-field-component v-model:value="filterAction" label="Action"
                            @blur="loadData"
                            @keyup.enter="loadData" />
                    </div>
                    <div xs12 sm6 md3>
                        <text-field-component v-model:value="filterSubject" label="Subject"
                            @blur="loadData"
                            @keyup.enter="loadData" />
                    </div>
                    <div xs12 sm6 md3>
                        <text-field-component v-model:value="filterUserId" label="User id"
                            @blur="loadData"
                            @keyup.enter="loadData" />
                    </div>
                </div>

                <data-table-component
                    :headers="tableHeaders"
                    :items="filteredAuditEvents"
                    :loading="loadStatus.inProgress"
                    :rows-per-page-items="tableRowsPerPageItems"
                    v-model:pagination="tablePagination"
                    :custom-sort="tableSorter"
                    item-key="Id"
                    expand
                    class="audit-table">
                    <progress-linear-component color="primary" indeterminate></progress-linear-component>
                    <template v-slot:no-data>
                    <alert-component :value="true" color="error" icon="warning" v-if="loadStatus.failed">
                        {{ loadStatus.errorMessage }}
                    </alert-component>
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
                        <div flat>
                            <div class="row-details">
                                <div class="row-details-user mt-2 mb-2">
                                    <icon-component class="row-details-usericon">person</icon-component>
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
                                <div class="row-details-blobs" v-if="props.item.Blobs.length > 0">
                                    <ul>
                                        <li v-for="(blob, index) in props.item.Blobs"
                                            :key="`audit-${props.item.Id}-blob-${index}`">
                                            <div class="row-details-blobs-title">{{ blob.Key }}:</div> 
                                            <a class="row-details-blobs-value"
                                                @click.prevent="onViewBlobClicked(props.item, blob)"
                                                >[View]</a>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </template>
                </data-table-component>
            </div>
        </div>

        <!-- ##################### -->
        <dialog-component v-model:value="showBlobContentsDialog"
            @keydown.esc="showBlobContentsDialog = false"
            max-width="90%" scrollable
            content-class="audit-blob-dialog">
            <div>
                <div class="headline">{{ currentBlobTitle }}</div>
                <div class="pt-0">
                    <p class="blob-details">{{ currentBlobDetails }}</p>
                    <progress-linear-component color="primary" indeterminate v-if="loadStatus.inProgress"></progress-linear-component>
                    
                    <alert-component :value="true" color="error" icon="warning" v-if="currentBlobError != null">
                        {{ currentBlobError }}
                    </alert-component>

                    <code class="blob-contents" v-if="currentBlobContents != null">{{ currentBlobContents }}</code>
                </div>
                                <div>
                                        <btn-component color="primary" @click="hideBlobContentsDialog()">Close</btn-component>
                </div>
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
/* .row-details-role {
    display: inline;
    color: var(--color--secondary-base);
    padding: 5px;
    border: 1px solid var(--color--secondary-base);
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