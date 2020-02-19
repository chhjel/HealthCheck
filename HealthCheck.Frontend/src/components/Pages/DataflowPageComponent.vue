<!-- src/components/Pages/DataflowPageComponent.vue -->
<template>
    <div>
        <v-content>
            <!-- NAVIGATION DRAWER -->
            <v-navigation-drawer
                v-model="drawerState"
                clipped fixed floating app
                mobile-break-point="1000"
                dark
                class="menu testset-menu">

                <v-list expand class="menu-items">
                    <v-progress-linear 
                        v-if="metadataLoadInProgress"
                        indeterminate color="green"></v-progress-linear>
                    
                    <v-list-tile ripple
                        v-for="(stream, streamIndex) in streamMetadatas"
                        :key="`stream-menu-${streamIndex}`"
                        class="testset-menu-item"
                        :class="{ 'active': (selectedStream == stream) }"
                        @click="setActveStream(stream)"
                        :disabled="dataLoadInProgress">
                        <v-list-tile-title v-text="stream.Name"></v-list-tile-title>
                    </v-list-tile>
                </v-list>
            </v-navigation-drawer>
            
            <!-- CONTENT -->
            <v-container fluid fill-height class="content-root">
                <v-layout>
                    <v-flex>
                        <v-container>
                            <!-- DATA LOAD ERROR -->
                            <v-alert :value="dataLoadFailed" type="error">
                            {{ dataFailedErrorMessage }}
                            </v-alert>

                            <!-- SELECTED DATAFLOW INFO -->
                            <v-layout v-if="selectedStream != null" style="flex-direction: column;">
                                <h3>{{ selectedStream.Name }}</h3>
                                <p>{{ selectedStream.Description }}</p>
                            </v-layout>

                            <!-- TABLE START -->
                            <v-data-table
                                :headers="tableHeaders"
                                :items="streamEntries"
                                :loading="dataLoadInProgress"
                                :rows-per-page-items="tableRowsPerPageItems"
                                :pagination.sync="tablePagination"
                                :custom-sort="tableSorter"
                                item-key="Internal__Table__Id"
                                expand
                                class="elevation-1 audit-table">
                                <v-progress-linear v-slot:progress color="primary" indeterminate></v-progress-linear>
                                <template v-slot:no-data>
                                <v-alert :value="true" color="error" icon="warning" v-if="dataLoadFailed">
                                    {{ dataFailedErrorMessage }}
                                </v-alert>
                                </template>
                                <template v-slot:items="props">
                                    <tr
                                        @click="props.expanded = !props.expanded"
                                        class="audit-table-row">
                                        <td
                                            v-for="(col, colIndex) in getTableColumns(props.item, false)"
                                            :key="`dataflow-row-${props.index}-col-${colIndex}`">
                                            {{ col.value }}
                                        </td>
                                        <!-- <td width="200">{{ props }}</td> -->
                                        <!-- <td class="text-xs-left">{{ props }}</td> -->
                                    </tr>
                                </template>
                                <template v-slot:expand="props">
                                    <v-card flat>
                                        <v-card-text class="row-details">
                                            <div
                                                v-for="(col, colIndex) in getTableColumns(props.item, true)"
                                                :key="`dataflow-row-expanded-${props.index}-col-${colIndex}`"
                                                class="expanded-item-details">
                                                <div class="expanded-item-details-title">{{ col.key }}</div>:
                                                <div class="expanded-item-details-value">{{ col.value }}</div>
                                            </div>
                                        </v-card-text>
                                    </v-card>
                                </template>
                            </v-data-table>
                            <!-- TABLE END -->

                        </v-container>
                    </v-flex>
                </v-layout>
            </v-container>
          <!-- CONTENT END -->
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Page/FrontEndOptionsViewModel';
import LoggedEndpointDefinitionViewModel from '../../models/RequestLog/LoggedEndpointDefinitionViewModel';
import LoggedEndpointRequestViewModel from '../../models/RequestLog/LoggedEndpointRequestViewModel';
import { EntryState } from '../../models/RequestLog/EntryState';
import DateUtils from "../../util/DateUtils";
import LinqUtils from "../../util/LinqUtils";
import UrlUtils from "../../util/UrlUtils";
import KeyArray from "../../util/models/KeyArray";
import KeyValuePair from "../../models/Common/KeyValuePair";
import DataflowStreamMetadata from "../../models/Dataflow/DataflowStreamMetadata";
import DataflowEntry from "../../models/Dataflow/DataflowEntry";
import GetDataflowEntriesRequestModel from "../../models/Dataflow/GetDataflowEntriesRequestModel";
import { DataFlowPropertyUIHint, DataFlowPropertyUIVisibilityOption } from "../../models/Dataflow/DataFlowPropertyDisplayInfo";

@Component({
    components: {
    }
})
export default class DataflowPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // UI STATE
    drawerState: boolean = true;
    metadataLoadInProgress: boolean = false;
    dataLoadInProgress: boolean = false;
    dataLoadFailed: boolean = true;
    dataFailedErrorMessage: string = '';

    streamMetadatas: Array<DataflowStreamMetadata> = [];
    selectedStream: DataflowStreamMetadata | null = null;
    streamEntries: Array<DataflowEntry> = [];

    // Table
    tableRowsPerPageItems: Array<any> 
        = [10, 25, 50, {"text":"$vuetify.dataIterator.rowsPerPageAll","value":-1}];
    tablePagination: any = {
        rowsPerPage: 25
    };

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.loadData();
    }

    created(): void {
        this.$parent.$parent.$on("onSideMenuToggleButtonClicked", this.toggleSideMenu);
    }

    beforeDestroy(): void {
      this.$parent.$parent.$off('onSideMenuToggleButtonClicked', this.toggleSideMenu);
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get tableHeaders(): Array<any> {
        return this.getTableHeaders(false);
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.metadataLoadInProgress = true;
        this.dataLoadFailed = false;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.GetDataflowStreamsMetadataEndpoint}${queryStringIfEnabled}`;
        fetch(url, {
            credentials: 'include',
            method: "GET",
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => response.json())
        .then((diagramsData: Array<DataflowStreamMetadata>) => this.onDataFlowMetaDataRetrieved(diagramsData))
        .catch((e) => {
            this.metadataLoadInProgress = false;
            this.dataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    onDataFlowMetaDataRetrieved(data: Array<DataflowStreamMetadata>): void {
        this.metadataLoadInProgress = false;
        this.streamMetadatas = data;
    }

    loadStreamEntries(filter: GetDataflowEntriesRequestModel): void {
        this.streamEntries = [];
        this.dataLoadInProgress = true;
        this.dataLoadFailed = false;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.GetDataflowStreamEntriesEndpoint}${queryStringIfEnabled}`;
        fetch(url, {
            credentials: 'include',
            method: "POST",
            body: JSON.stringify(filter),
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json'
            })
        })
        .then(response => response.json())
        .then((diagramsData: Array<DataflowEntry>) => this.onDataFlowDataRetrieved(diagramsData))
        .catch((e) => {
            this.dataLoadInProgress = false;
            this.dataLoadFailed = true;
            this.dataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    onDataFlowDataRetrieved(data: Array<DataflowEntry>): void {
        this.dataLoadInProgress = false;
        let idCounter = 1;
        this.streamEntries = data.map(x => {
            let extra = {
                Internal__Table__Id: idCounter++
            };
            
            return { ...extra, ... x };
        });
    }

    setActveStream(stream: DataflowStreamMetadata): void {
        this.selectedStream = stream;
        this.loadStreamEntries({
            StreamId: this.selectedStream.Id,
            StreamFilter: {
                Take: 500,
                Skip: 0,
                FromDate: null,
                ToDate: null
            }
        });
    }
    
    toggleSideMenu(): void {
        this.drawerState = !this.drawerState;
    }
    
    getTableHeaders(expanded: boolean): Array<any> {
        if (this.selectedStream == null)
        {
            return [];
        }

        let entry = this.streamEntries[0];
        if (entry == null) return [];

        let headers: Array<any> = [];
        for(let key in entry)
        {
            if (key == 'Internal__Table__Id')
            {
                continue;
            }

            headers.push({
                text: key,
                value: key,
                align: 'left',
                uiOrder: 99999999,
                visibility: DataFlowPropertyUIVisibilityOption.Always,
                type: 'text'
            });
        }
        
        for(let info of this.selectedStream.PropertyDisplayInfo)
        {
            let header = headers.filter(x => x.value == info.PropertyName)[0];
            if (header == null) {
                header = {
                    align: 'left',
                };
                headers.push(header);
            }

            header.text = info.DisplayName || info.PropertyName;
            header.value = info.PropertyName,
            header.uiOrder = info.UIOrder,
            header.uiHint = info.UIHint || 'text',
            header.dateTimeFormat = info.DateTimeFormat,
            header.visibility = info.Visibility
        }

        headers = headers
            .filter(x => x.visibility == DataFlowPropertyUIVisibilityOption.Always 
                      || (expanded && x.visibility == DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded)
                      || (!expanded && x.visibility == DataFlowPropertyUIVisibilityOption.OnlyInList))
            .sort((a, b) => LinqUtils.SortBy(a, b, (x) => x.uiOrder, true));

        return headers;
    }
    
    getTableColumns(entry: DataflowEntry, isExpanded: boolean): Array<any>
    {
        return this.getTableHeaders(isExpanded)
            .map(x => {
                return {
                    key: x.text,
                    value: this.getTableColumnValue((<any>entry)[x.value], x)
                };
            });
    }

    getTableColumnValue(raw: any, header: any): any
    {
        let uiHint = (header != null) ? header.uiHint : null;
        if (uiHint == DataFlowPropertyUIHint.DateTime)
        {
            return DateUtils.FormatDate(new Date(raw), header.dateTimeFormat);
        }
        else
        {
            return raw;
        }
    }

    tableSorter(items: DataflowEntry[], propertyName: string, isDescending: boolean): DataflowEntry[]
    {
        if (propertyName == null) {
            return items;
        }

        let header = this.tableHeaders.filter(x => x.value == propertyName)[0];
        let uiHint = (header != null) ? header.uiHint : null;

        if (uiHint === DataFlowPropertyUIHint.DateTime)
        {
            items = items.sort((a:DataflowEntry, b:DataflowEntry) => new Date((<any>b)[propertyName]).getTime() - new Date((<any>a)[propertyName]).getTime());
        }
        else {
            items = items.sort((a:DataflowEntry, b:DataflowEntry) => {
                var textA = (<any>a)[propertyName].toUpperCase();
                var textB = (<any>b)[propertyName].toUpperCase();
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
}
</script>

<style scoped lang="scss">
.expanded-item-details
{
    padding: 5px;
    border-left: 4px solid #EEE;

    .expanded-item-details-title {
        font-weight: 600;
        display: inline-block;
    }
    .expanded-item-details-value {
        display: inline-block;
    }
}
</style>