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

                <!-- <v-list expand class="menu-items">
                    <v-list-tile ripple
                        v-for="(diagram, diagramIndex) in diagrams"
                        :key="`testset-menu-${diagramIndex}`"
                        @click="setActveDiagram(diagram)">
                        <v-list-tile-title v-text="diagram.title"></v-list-tile-title>
                    </v-list-tile>

                    <v-divider />

                    <v-list-tile ripple 
                        v-if="options.EnableDiagramSandbox"
                        @click="sandboxMode = true">
                        <v-list-tile-title v-text="'Sandbox'"></v-list-tile-title>
                    </v-list-tile>
                </v-list> -->
            </v-navigation-drawer>
            
            <!-- CONTENT -->
            <v-container fluid fill-height class="content-root">
                <v-layout>
                    <v-flex>
                        <v-container>
                            <!-- NO DIAGRAMS INFO -->
                            <!-- <v-alert :value="diagrams.length == 0 && !dataLoadInProgress" type="info">
                                No documentation was found.<br />
                                Decorate backend code with <code>[SequenceDiagramStepAttribute]</code> for sequence diagrams to be generated.
                            </v-alert> -->

                            <!-- DATA LOAD ERROR -->
                            <v-alert :value="dataLoadFailed" type="error">
                            {{ dataFailedErrorMessage }}
                            </v-alert>

                            <!-- LOAD PROGRESS -->
                            <v-progress-linear 
                                v-if="dataLoadInProgress"
                                indeterminate color="green"></v-progress-linear>

                            <!-- SELECTED DATAFLOW -->
                            <!-- <v-layout v-if="currentDiagram != null && !sandboxMode" style="flex-direction: column;">
                            </v-layout> -->
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

@Component({
    components: {
    }
})
export default class DataflowPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // UI STATE
    drawerState: boolean = true;
    dataLoadInProgress: boolean = false;
    dataLoadFailed: boolean = true;
    dataFailedErrorMessage: string = '';

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

    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.dataLoadInProgress = true;
        this.dataLoadFailed = false;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.DiagramsDataEndpoint}${queryStringIfEnabled}`;
        fetch(url, {
            credentials: 'include',
            method: "GET",
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => response.json())
        .then((diagramsData: any) => this.onDataFlowDataRetrieved(diagramsData))
        .catch((e) => {
            this.dataLoadInProgress = false;
            this.dataLoadFailed = true;
            this.dataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    onDataFlowDataRetrieved(data: any): void {
        this.dataLoadInProgress = false;
    }
    
    toggleSideMenu(): void {
        this.drawerState = !this.drawerState;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
</style>
