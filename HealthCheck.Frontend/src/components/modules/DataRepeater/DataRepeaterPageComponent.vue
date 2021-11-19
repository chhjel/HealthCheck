<!-- src/components/modules/DataRepeater/DataRepeaterPageComponent.vue -->
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

                <filterable-list-component
                    :items="menuItems"
                    :groupByKey="`GroupName`"
                    :sortByKey="`GroupName`"
                    :filterKeys="[ 'Name', 'Description' ]"
                    :loading="metadataLoadStatus.inProgress"
                    :disabled="dataLoadStatus.inProgress"
                    ref="filterableList"
                    v-on:itemClicked="onMenuItemClicked"
                    />
            </v-navigation-drawer>
            
            <!-- CONTENT -->
            <v-container fluid fill-height class="content-root">
                <v-layout>
                    <v-flex>
                        <v-container>
                            <!-- LOAD PROGRESS -->
                            <v-progress-linear 
                                v-if="dataLoadStatus.inProgress"
                                indeterminate color="green"></v-progress-linear>

                            <!-- DATA LOAD ERROR -->
                            <v-alert :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
                            {{ dataLoadStatus.errorMessage }}
                            </v-alert>

                            <div v-if="selectedStream">
                                <div v-for="(action, aIndex) in selectedStream.Actions"
                                    :key="`action-${aIndex}-${action.Id}`">
                                    <h3>{{ action.Name }}</h3>
                                    <p v-if="action.Description">{{ action.Description }}</p>

                                    <backend-input-component
                                        v-for="(parameterDef, pIndex) in action.ParameterDefinitions"
                                        :key="`action-parameter-item-${aIndex}-${pIndex}-${action.Id}`"
                                        class="action-parameter-item"
                                        v-model="actionParameters[action.Id][pIndex]"
                                        :config="parameterDef"
                                        />
                                </div>
                            </div>

                            <!-- <code>{{ streamDefinitions }}</code> -->

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
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import FilterableListComponent, { FilterableListItem } from  '../../Common/FilterableListComponent.vue';
import { FetchStatus } from  '../../../services/abstractions/HCServiceBase';
import ModuleConfig from  '../../../models/Common/ModuleConfig';
import ModuleOptions from  '../../../models/Common/ModuleOptions';
import DataRepeaterService from  '../../../services/DataRepeaterService';
import { HCGetDataRepeaterStreamDefinitionsViewModel } from "generated/Models/Core/HCGetDataRepeaterStreamDefinitionsViewModel";
import { HCDataRepeaterStreamViewModel } from "generated/Models/Core/HCDataRepeaterStreamViewModel";
import BackendInputComponent from "components/Common/Inputs/BackendInputs/BackendInputComponent.vue";

@Component({
    components: {
        FilterableListComponent,
        BackendInputComponent
    }
})
export default class DataRepeaterPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;

    // UI STATE
    streamsFilterText: string = "";
    
    // Service
    service: DataRepeaterService = new DataRepeaterService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();
    metadataLoadStatus: FetchStatus = new FetchStatus();

    streamDefinitions: HCGetDataRepeaterStreamDefinitionsViewModel | null = null;
    selectedStream: HCDataRepeaterStreamViewModel | null = null;
    actionParameters: any = {}; //Array<string> = [];

    // Filter/pagination
    pageIndex: number = 0;
    pageSize: number = 50;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.$store.commit('showMenuButton', true);

        this.resetFilter();
        this.loadStreamDefinitions();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }
    
    get menuItems(): Array<FilterableListItem>
    {
        if (!this.streamDefinitions) return [];
        return this.streamDefinitions.Streams.map(x => {
            return {
                title: x.Name,
                subtitle: '',
                data: x
            };
        });
    }

    ////////////////////
    //  Parent Menu  //
    //////////////////
    drawerState: boolean = this.storeMenuState;
    get storeMenuState(): boolean {
        return this.$store.state.ui.menuExpanded;
    }
    @Watch("storeMenuState")
    onStoreMenuStateChanged(): void {
        this.drawerState = this.storeMenuState;
    }

    ////////////////
    //  METHODS  //
    //////////////
    resetFilter(): void {
        this.pageIndex = 0;
        this.pageSize = 50;
    }

    loadStreamDefinitions(): void {
        this.service.GetStreamDefinitions(this.metadataLoadStatus, {
            onSuccess: (data) => this.onStreamDefinitionsRetrieved(data)
        });
    }

    onStreamDefinitionsRetrieved(data: HCGetDataRepeaterStreamDefinitionsViewModel | null): void {
        this.streamDefinitions = data;
        this.streamDefinitions?.Streams.forEach(s => {
            s.Actions.forEach(a => {
                this.actionParameters[a.Id] = [];
            });
        });

        if (this.streamDefinitions && this.streamDefinitions.Streams.length > 0)
        {
            this.setActiveStream(this.streamDefinitions.Streams[0]);
        }
    }

    setActiveStream(stream: HCDataRepeaterStreamViewModel, updateUrl: boolean = true): void {
        if (this.dataLoadStatus.inProgress) {
            return;
        }

        this.selectedStream = stream;
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onMenuItemClicked(item: FilterableListItem): void {
        this.setActiveStream(item.data);
    }
}
</script>

<style scoped lang="scss">
.filter-choice {
    &.selected {
        color: #fff;
        font-weight: 600;
    }
}
.menu {
    box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.02), 0 3px 2px 0 rgba(0, 0, 0, 0.02), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
}
@media (max-width: 960px) {
    .menu-items { 
        margin-top: 67px;
    }
}
</style>