<!-- src/components/modules/Documentation/DocumentationPageComponent.vue -->
<template>
    <div class="docpage">
        <v-content>
            <!-- NAVIGATION DRAWER -->
            <v-navigation-drawer
                v-model="drawerState"
                clipped fixed floating app
                mobile-break-point="1000"
                dark
                class="menu testset-menu">

                <v-list expand class="menu-items">
                    <filter-input-component class="filter" v-model="diagramFilterText" />

                    <v-list-tile ripple
                        v-for="(menuItem, diagramIndex) in menuItems"
                        :key="`diagram-menu-${diagramIndex}`"
                        class="testset-menu-item"
                        :class="{ 'active': ((currentSequenceDiagram == menuItem.data || currentFlowChart == menuItem.data) && !sandboxMode) }"
                        @click="setActiveDiagram(menuItem)">
                        <v-list-tile-title>
                            {{ menuItem.title }}
                            <br>
                            <span style="color: darkgray;">{{ menuItem.subTitle }}</span>
                        </v-list-tile-title>
                    </v-list-tile>

                    <v-divider />

                    <v-list-tile ripple 
                        v-if="options.Options.EnableDiagramSandbox"
                        class="testset-menu-item"
                        :class="{ 'active': (sandboxMode) }"
                        @click="showSandboxMode">
                        <v-list-tile-title v-text="'Sandbox'"></v-list-tile-title>
                    </v-list-tile>
                </v-list>
            </v-navigation-drawer>
            
            <!-- CONTENT -->
            <v-container fluid fill-height class="content-root">
                <v-layout>
                    <v-flex>
                        <v-container>
                            <!-- NO DIAGRAMS INFO -->
                            <v-alert :value="sequenceDiagrams.length == 0 && !loadStatus.inProgress && !loadStatus.failed" type="info">
                                No documentation was found.<br />
                                Decorate backend code with <code>[SequenceDiagramStepAttribute]</code> for sequence diagrams to be generated,<br />
                                or with <code>[FlowChartStepAttribute]</code> for flow charts to be generated.
                            </v-alert>

                            <!-- DATA LOAD ERROR -->
                            <v-alert :value="loadStatus.failed" type="error">
                            {{ loadStatus.errorMessage }}
                            </v-alert>

                            <!-- LOAD PROGRESS -->
                            <v-progress-linear 
                                v-if="loadStatus.inProgress"
                                indeterminate color="green"></v-progress-linear>

                            <!-- SELECTED DIAGRAM -->
                            <v-layout v-if="(currentSequenceDiagram != null || currentFlowChart != null) && !sandboxMode"
                                style="flex-direction: column;">
                                <v-flex sm12 md12 lg12>
                                    <flow-diagram-component
                                        class="diagram"
                                        v-if="currentFlowChart != null"
                                        :steps="currentFlowChart.steps"
                                        :title="currentFlowChart.title" />

                                    <sequence-diagram-component
                                        class="diagram"
                                        v-if="currentSequenceDiagram != null"
                                        :title="currentSequenceDiagram.title"
                                        :steps="currentSequenceDiagram.steps"
                                        :showRemarks="showRemarks"
                                        :diagramStyle="diagramStyle"
                                        :clickable="options.Options.EnableDiagramDetails"
                                        v-on:stepClicked="onStepClicked" />
                                </v-flex>

                                <div v-if="selectedStep != null" class="selected-step-details">
                                    <b>{{ selectedStep.description }}</b><br />
                                    <b>From:</b>
                                    <code>{{ selectedStep.data.classNameFrom }}</code>
                                    <code>{{ selectedStep.data.methodNameFrom }}</code><br />
                                    <b>To:</b>
                                    <code>{{ selectedStep.data.classNameTo }}</code>
                                    <code>{{ selectedStep.data.methodNameTo }}</code>
                                </div>

                                <v-checkbox
                                    v-if="showToggleRemarks"
                                    v-model="showRemarks"
                                    label="Show remarks" style="display:block"></v-checkbox>
                            </v-layout>
                        </v-container>

                        <!-- SANDBOX -->
                        <v-layout v-if="sandboxMode">
                            <v-flex>
                                <v-select
                                    v-if="false"
                                    v-model="diagramStyle"
                                    :items="diagramStyles"
                                    item-text="text" item-value="value" color="secondary">
                                </v-select>

                                <v-layout>
                                    <v-flex sm12 lg4>
                                        <textarea
                                            style="width: 100%; border: 1px solid #ccc; height: 100%; padding: 5px;"
                                            v-model="sandboxScript"
                                            />
                                    </v-flex>
                                        
                                    <v-flex sm12 lg8>
                                        <v-container grid-list-md>
                                            <sequence-diagram-component
                                                class="diagram"
                                                :steps="sandboxSteps"
                                                :diagramStyle="diagramStyle" />
                                        </v-container>
                                    </v-flex>
                                </v-layout>
                            </v-flex>
                        </v-layout>
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
import LoggedEndpointDefinitionViewModel from  '../../../models/modules/RequestLog/LoggedEndpointDefinitionViewModel';
import LoggedEndpointRequestViewModel from  '../../../models/modules/RequestLog/LoggedEndpointRequestViewModel';
import { EntryState } from  '../../../models/modules/RequestLog/EntryState';
import DiagramsDataViewModel from  '../../../models/modules/Documentation/DiagramsDataViewModel';
import DateUtils from  '../../../util/DateUtils';
import LinqUtils from  '../../../util/LinqUtils';
import KeyArray from  '../../../util/models/KeyArray';
import KeyValuePair from  '../../../models/Common/KeyValuePair';
import SequenceDiagramComponent, { SequenceDiagramStep, SequenceDiagramLineStyle, SequenceDiagramStyle } from  '../../Common/SequenceDiagramComponent.vue';
import FilterInputComponent from  '../../Common/FilterInputComponent.vue';
import FlowDiagramComponent, { FlowDiagramStep, FlowDiagramStepType } from  '../../Common/FlowDiagramComponent.vue';
import { FetchStatus } from  '../../../services/abstractions/HCServiceBase';
import DocumentationService from  '../../../services/DocumentationService';
import ModuleConfig from  '../../../models/Common/ModuleConfig';
import ModuleOptions from  '../../../models/Common/ModuleOptions';
import UrlUtils from  '../../../util/UrlUtils';

interface FlowChartData
{
    title: string;
    steps: Array<FlowDiagramStep<FlowChartStepDetails | null>>;
}
interface FlowChartStepDetails
{
    className: string,
    methodName: string,
}
interface SequenceDiagramData
{
    title: string;
    steps: Array<SequenceDiagramStep<SequenceDiagramStepDetails | null>>;
}
interface SequenceDiagramStepDetails
{
    classNameFrom: string,
    classNameTo: string,
    methodNameFrom: string,
    methodNameTo: string
}
interface DocMenuItem
{
    title: string;
    subTitle: string | null;
    type: 'sequence-diagram' | 'flow-chart';
    data: any;
}
interface DocumentationPageOptions
{
    EnableDiagramSandbox: boolean;
    EnableDiagramDetails: boolean;
}

@Component({
    components: {
        SequenceDiagramComponent,
        FilterInputComponent,
        FlowDiagramComponent
    }
})
export default class DocumentationPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<DocumentationPageOptions>;

    sequenceDiagrams: Array<SequenceDiagramData> = [];
    flowCharts: Array<FlowChartData> = [];

    service: DocumentationService = new DocumentationService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    loadStatus: FetchStatus = new FetchStatus();

    // UI STATE
    diagramFilterText: string = "";
    sandboxMode: boolean = false;
    showRemarks: boolean = true;

    currentSequenceDiagram: SequenceDiagramData | null = null;
    currentFlowChart: FlowChartData | null = null;

    selectedStep: SequenceDiagramStep<SequenceDiagramStepDetails> | null = null;
    diagramStyle: SequenceDiagramStyle = SequenceDiagramStyle.Default;
    sandboxScript: string = `
Frontend --> Web: User sends form
Web -> Web: Validate input
opt Invoice only
Web -> External Service: Data is sent to 3rd party | remark: Some remark here
Web -> Database: Backup of data is stored in database
end
Web -> Frontend: Confirmation is delivered
`;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.$store.commit('showMenuButton', true);

        let restoredSandbox = localStorage.getItem("__HC_Docs_sandbox_sequencediagram_script");
        if (restoredSandbox != null)
        {
            this.sandboxScript = restoredSandbox;
        }

        this.loadData();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }
    
    get showToggleRemarks(): boolean
    {
        if (this.currentSequenceDiagram == null)
        {
            return false;
        }
        
        return this.currentSequenceDiagram.steps.some(x => x.remark != null && x.remark.trim().length > 0);
    }

    get sandboxSteps(): Array<SequenceDiagramStep<SequenceDiagramStepDetails | null>>
    {
        return this.convertStringToSteps(this.sandboxScript);
    }

    get diagramStyles(): Array<any>
    {
        return [
            { text: "Default", value: SequenceDiagramStyle.Default },
            { text: "Test", value: SequenceDiagramStyle.Test }
        ];
    }

    get showFilterCounts(): boolean {
        return this.diagramFilterText.length > 0;
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
    updateUrl(): void {
        let title: string | null = null;
        
        if (this.sandboxMode == true)
        {
            title = 'sandbox';
        }
        else if (this.currentSequenceDiagram != null)
        {
            title = UrlUtils.EncodeHashPart(this.currentSequenceDiagram.title);
        }
        else if (this.currentFlowChart != null)
        {
            title = UrlUtils.EncodeHashPart(this.currentFlowChart.title);
        }

        let routeParams: any = {};
        if (title != null)
        {
            routeParams['title'] = title;
        }
        
        const titleInUrl = this.$route.params.title;
        if (titleInUrl !== title)
        {
            this.$router.push({ name: this.config.Id, params: routeParams })
        }
    }

    updateSelectionFromUrl(): void {
        const selectedItem = this.$route.params.title;
        
        if (selectedItem !== undefined && selectedItem.length > 0) {
            let seqDiagram = this.sequenceDiagrams.filter(x => UrlUtils.EncodeHashPart(x.title) == selectedItem)[0];
            if (seqDiagram != null)
            {
                this.setActiveSequenceDiagram(seqDiagram);
            }

            let flowchart = this.flowCharts.filter(x => UrlUtils.EncodeHashPart(x.title) == selectedItem)[0];
            if (flowchart != null)
            {
                this.setActiveFlowChart(flowchart);
            }
        }

        if (this.currentSequenceDiagram == null && this.currentFlowChart == null)
        {
            if (this.sequenceDiagrams.length > 0)
            {
                this.setActiveSequenceDiagram(this.sequenceDiagrams[0]);
            }
            else if (this.flowCharts.length > 0)
            {
                this.setActiveFlowChart(this.flowCharts[0]);
            }
        }

        if (selectedItem == 'sandbox')
        {
            this.sandboxMode = true;
        }
    }

    loadData(): void {
        this.service.GetDiagramsData(this.loadStatus, { onSuccess: (data) => this.onDiagramsDataRetrieved(data)});
    }

    onDiagramsDataRetrieved(diagramsData: DiagramsDataViewModel): void {
        this.sequenceDiagrams = diagramsData
            .SequenceDiagrams
            .map((x) => {
                return {
                    title: x.Name,
                    // description: x.Description,
                    steps: x.Steps.map(s => {
                        return {
                            from: s.From,
                            to: s.To,
                            description: s.Description || '',
                            note: s.Note || undefined,
                            remark: s.Remarks || undefined,
                            optional: s.OptionalGroupName || undefined,
                            // style: s.Direction
                            data: {
                                classNameFrom: s.ClassNameFrom,
                                classNameTo: s.ClassNameTo,
                                methodNameFrom: s.MethodNameFrom,
                                methodNameTo: s.MethodNameTo
                            }
                        }
                    })
                }
            });
        
        this.flowCharts = diagramsData
            .FlowCharts
            .map((x:any) => {
                return {
                    title: this.sequenceDiagrams.some(s => s.title == x.Name) ? `${x.Name} flow` : x.Name,
                    steps: x.Steps.map((s:any) => {
                        return {
                            title: s.Title,
                            type: s.Type,
                            data: {
                                className: s.ClassName,
                                methodName: s.MethodName
                            },
                            connections: s.Connections.map((c:any) => {
                                return {
                                    target: c.Target,
                                    label: c.Label
                                }
                            })
                        }
                    })
                }
            });
        
        this.updateSelectionFromUrl();
    }

    convertStringToSteps(text: string): Array<SequenceDiagramStep<SequenceDiagramStepDetails | null>>
    {
        let lines = text.split('\n');
        
        let currentOptional: string | undefined = undefined;
        let steps: Array<SequenceDiagramStep<SequenceDiagramStepDetails | null>> = [];
        for(let i=0; i<lines.length; i++)
        {
            let line = lines[i];
            
            let isNormalLine = line.indexOf('->') > -1 && line.indexOf(':') > line.indexOf('->');
            if (!isNormalLine)
            {
                if (line.startsWith('opt '))
                {
                    currentOptional = line.substring(4).trim();
                }
                else if(line.trim() == 'end')
                {
                    currentOptional = undefined;
                }
                continue;
            }

            // ["A -> B", ": note"]
            let mainParts = line.split(':');

            // A -> B
            let fromTo = mainParts[0].split('->');
            let style: SequenceDiagramLineStyle | undefined = undefined;
            let from = fromTo[0].trim();
            let to = fromTo[1].trim();
            // --> arrow means dashed style
            if (from.endsWith('-'))
            {
                from = from.substring(0, from.length - 1).trim();
                style = SequenceDiagramLineStyle.Dashed;
            }

            // : note
            let otherParts = line.split('|');
            let description = otherParts[0].split(':')[1].trim();
            let note: undefined | string = undefined;
            let remark: undefined | string = undefined;
            for (let p=1; p < otherParts.length; p++)
            {
                let part = otherParts[p].split(':', 2).map(a => a.trim());
                let partKey = part[0];
                let partValue = part[1];
                if (partKey == "note")
                {
                    note = partValue;
                }
                else if (partKey == "remark")
                {
                    remark = partValue;
                }
            }

            let step = {
                from: from,
                to: to,
                description: description,
                note: note,
                remark: remark,
                optional: currentOptional,
                style: style,
                data: null
            };
            steps.push(step);
        }
        return steps;
    }

    get menuItems(): Array<DocMenuItem>
    {
        let items: Array<DocMenuItem> = [];

        this.sequenceDiagrams.filter(x => this.sequenceDiagramMatches(x)).forEach(element => {
            items.push({
                title: element.title,
                subTitle: 'Sequence diagram',
                data: element,
                type: "sequence-diagram"
            });
        });
        this.flowCharts.filter(x => this.flowChartMatches(x)).forEach(element => {
            items.push({
                title: element.title,
                subTitle: 'Flow chart',
                data: element,
                type: "flow-chart"
            });
        });

        items = items.sort((a, b) => LinqUtils.SortBy(a, b, x => x.title));
        return items;
    }

    sequenceDiagramMatches(data: SequenceDiagramData): boolean {
        return data.title.toLowerCase().indexOf(this.diagramFilterText.toLowerCase().trim()) != -1;
    }

    flowChartMatches(data: FlowChartData): boolean {
        return data.title.toLowerCase().indexOf(this.diagramFilterText.toLowerCase().trim()) != -1;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch("sandboxScript")
    onSandboxScriptChanged(): void
    {
        localStorage.setItem("__HC_Docs_sandbox_sequencediagram_script", this.sandboxScript);
    }

    showSandboxMode(): void {
        this.sandboxMode = true;
        this.updateUrl();
    }

    setActiveDiagram(item: DocMenuItem): void {
        let flowChart = this.flowCharts.filter(x => x == item.data)[0];
        let sequenceDiagram = this.sequenceDiagrams.filter(x => x == item.data)[0];
        if (flowChart != null)
        {
            this.setActiveFlowChart(flowChart);
        }
        else if (sequenceDiagram != null)
        {
            this.setActiveSequenceDiagram(sequenceDiagram);
        }
    }

    setActiveSequenceDiagram(diagram: SequenceDiagramData, updateUrl: boolean = true): void {
        this.sandboxMode = false;
        this.currentSequenceDiagram = diagram;
        this.currentFlowChart = null;
        this.selectedStep = null;

        if (updateUrl)
        {
            this.updateUrl();
        }
    }
    
    setActiveFlowChart(chart: FlowChartData, updateUrl: boolean = true): void {
        this.sandboxMode = false;
        this.currentFlowChart = chart;
        this.currentSequenceDiagram = null;
        this.selectedStep = null;

        if (updateUrl)
        {
            this.updateUrl();
        }
    }

    onStepClicked(step: SequenceDiagramStep<SequenceDiagramStepDetails>): void
    {
        if (this.options.Options.EnableDiagramDetails == true)
        {
            this.selectedStep = step;
        }
    }
}
</script>

<style scoped lang="scss">
.diagram {
    background-color: #fff;
    padding: 20px;
    border: 1px solid #353535;
}
.selected-step-details {
    padding: 20px;
    margin-top: 10px;
    background-color: #fff;
    border: 1px solid gray;
}
.menu {
    box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.02), 0 3px 2px 0 rgba(0, 0, 0, 0.02), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
}
.filter {
    position: relative;
    margin-left: 44px;
    margin-top: 26px;
    margin-bottom: 18px;
    margin-right: 44px;
}
@media (max-width: 960px) {
    .menu-items { 
        margin-top: 67px;
    }
}
.docpage >>> .v-list__tile {
    height: 62px;

    .v-list__tile__title {
        height: 48px;
    }
}
</style>
