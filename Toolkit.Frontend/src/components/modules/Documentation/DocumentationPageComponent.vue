<!-- src/components/modules/Documentation/DocumentationPageComponent.vue -->
<template>
    <div>
        <!-- NAVIGATION DRAWER -->
        <Teleport to="#module-nav-menu">
            <div class="menu-items">
                <filter-input-component class="filter" v-model:value="diagramFilterText" />

                <div
                    v-for="(menuItem, diagramIndex) in menuItems"
                    :key="`diagram-menu-${diagramIndex}`"
                    class="testset-menu-item"
                    :class="{ 'active': ((currentSequenceDiagram == menuItem.data || currentFlowChart == menuItem.data) && !sandboxMode) }"
                    @click="setActiveDiagram(menuItem)">
                    <div>
                        {{ menuItem.title }}
                        <br>
                        <span style="color: darkgray;">{{ menuItem.subTitle }}</span>
                    </div>
                </div>

                <div 
                    v-if="options.Options.EnableDiagramSandbox"
                    class="testset-menu-item"
                    :class="{ 'active': (sandboxMode) }"
                    @click="showSandboxMode">
                    <div v-text="'Sandbox'"></div>
                </div>
            </div>
        </Teleport>
        
        
        <div class="content-root">
            <div>
                <!-- NO DIAGRAMS INFO -->
                <alert-component :value="sequenceDiagrams.length == 0 && !loadStatus.inProgress && !loadStatus.failed" type="info">
                    No documentation was found.<br />
                    Decorate backend code with <code>[SequenceDiagramStepAttribute]</code> for sequence diagrams to be generated,<br />
                    or with <code>[FlowChartStepAttribute]</code> for flow charts to be generated.
                </alert-component>

                <!-- DATA LOAD ERROR -->
                <alert-component :value="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
                </alert-component>

                <!-- LOAD PROGRESS -->
                <progress-linear-component 
                    v-if="loadStatus.inProgress"
                    indeterminate color="success"></progress-linear-component>

                <!-- SELECTED DIAGRAM -->
                <div v-if="(currentSequenceDiagram != null || currentFlowChart != null) && !sandboxMode"
                    style="flex-direction: column;">
                    <div sm12 md12 lg12>
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
                    </div>

                    <div v-if="selectedStep != null" class="selected-step-details">
                        <b>{{ selectedStep.description }}</b><br />
                        <b>From:</b>
                        <code>{{ selectedStep.data.classNameFrom }}</code>
                        <code>{{ selectedStep.data.methodNameFrom }}</code><br />
                        <b>To:</b>
                        <code>{{ selectedStep.data.classNameTo }}</code>
                        <code>{{ selectedStep.data.methodNameTo }}</code>
                    </div>

                    <checkbox-component
                        v-if="showToggleRemarks"
                        v-model:value="showRemarks"
                        label="Show remarks" style="display:block"></checkbox-component>
                </div>
            </div>

            <!-- SANDBOX -->
            <div v-if="sandboxMode">
                <div>
                    <select-component
                        v-if="false"
                        v-model:value="diagramStyle"
                        :items="diagramStyles"
                        item-text="text" item-value="value">
                    </select-component>

                    <div>
                        <div sm12 lg4>
                            <textarea-component
                                rows="10"
                                v-model:value="sandboxScript"
                                @input="updateSandbox"
                                />
                        </div>
                            
                        <div sm12 lg8>
                            <div grid-list-md>
                                <sequence-diagram-component
                                    class="diagram"
                                    ref="sandboxDiagram"
                                    :steps="sandboxSteps"
                                    :diagramStyle="diagramStyle" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import LoggedEndpointDefinitionViewModel from '@models/modules/RequestLog/LoggedEndpointDefinitionViewModel';
import LoggedEndpointRequestViewModel from '@models/modules/RequestLog/LoggedEndpointRequestViewModel';
import { EntryState } from '@models/modules/RequestLog/EntryState';
import DiagramsDataViewModel from '@models/modules/Documentation/DiagramsDataViewModel';
import DateUtils from '@util/DateUtils';
import LinqUtils from '@util/LinqUtils';
import KeyArray from '@util/models/KeyArray';
import KeyValuePair from '@models/Common/KeyValuePair';
import SequenceDiagramComponent from '@components/Common/SequenceDiagramComponent.vue';
import { SequenceDiagramStep, SequenceDiagramLineStyle, SequenceDiagramStyle } from '@components/Common/SequenceDiagramComponent.vue.models';
import FilterInputComponent from '@components/Common/FilterInputComponent.vue';
import FlowDiagramComponent from '@components/Common/FlowDiagramComponent.vue';
import { FlowDiagramStep, FlowDiagramStepType } from '@components/Common/FlowDiagramComponent.vue.models';
import { FetchStatus } from '@services/abstractions/TKServiceBase';
import DocumentationService from '@services/DocumentationService';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import UrlUtils from '@util/UrlUtils';
import StringUtils from "@util/StringUtils";
import { StoreUtil } from "@util/StoreUtil";

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

@Options({
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

    @Ref() readonly sandboxDiagram!: SequenceDiagramComponent;

    sequenceDiagrams: Array<SequenceDiagramData> = [];
    flowCharts: Array<FlowChartData> = [];
    sandboxSteps: Array<SequenceDiagramStep<SequenceDiagramStepDetails | null>> = [];

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
        StoreUtil.store.commit('showMenuButton', true);

        let restoredSandbox = localStorage.getItem("__TK_Docs_sandbox_sequencediagram_script");
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
        return StoreUtil.store.state.globalOptions;
    }
    
    get showToggleRemarks(): boolean
    {
        if (this.currentSequenceDiagram == null)
        {
            return false;
        }
        
        return this.currentSequenceDiagram.steps.some(x => x.remark != null && x.remark.trim().length > 0);
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

    ////////////////
    //  METHODS  //
    //////////////
    updateSandbox(): void {
        this.sandboxSteps = this.calcSandboxSteps();
    }

    calcSandboxSteps(): Array<SequenceDiagramStep<SequenceDiagramStepDetails | null>>
    {
        if (this.sandboxDiagram == null) return [];
        return this.convertStringToSteps(this.sandboxScript);
    }
    
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
        
        const titleInUrl = StringUtils.stringOrFirstOfArray(this.$route.params.title) || '';
        if (titleInUrl !== title)
        {
            this.$router.push({ name: this.config.Id, params: routeParams })
        }
    }

    updateSelectionFromUrl(): void {
        const selectedItem = StringUtils.stringOrFirstOfArray(this.$route.params.title) || '';
        
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
        this.$nextTick(() => this.updateSandbox());
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
        return this.sandboxDiagram.convertStringToSteps<SequenceDiagramStepDetails | null>(text);
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
        localStorage.setItem("__TK_Docs_sandbox_sequencediagram_script", this.sandboxScript);
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
.filter {
    position: relative;
    margin-left: 44px;
    margin-top: 26px;
    margin-bottom: 18px;
    margin-right: 44px;
}
</style>
