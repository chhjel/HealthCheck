<!-- src/components/Pages/DocumentationPageComponent.vue -->
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
                    <filter-input-component class="filter" v-model="diagramFilterText" />

                    <v-list-tile ripple
                        v-for="(diagram, diagramIndex) in filterDocumentation(diagrams)"
                        :key="`diagram-menu-${diagramIndex}`"
                        class="testset-menu-item"
                        :class="{ 'active': (currentDiagram == diagram && !sandboxMode) }"
                        @click="setActveDiagram(diagram)">
                        <v-list-tile-title v-text="diagram.title"></v-list-tile-title>
                    </v-list-tile>

                    <v-divider />

                    <v-list-tile ripple 
                        v-if="options.EnableDiagramSandbox"
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
                            <!-- TEST_BEGIN -->
                            <flow-diagram-component :steps="testSteps" />
                            <!-- TEST_END -->

                            <!-- NO DIAGRAMS INFO -->
                            <v-alert :value="diagrams.length == 0 && !diagramsDataLoadInProgress" type="info">
                                No documentation was found.<br />
                                Decorate backend code with <code>[SequenceDiagramStepAttribute]</code> for sequence diagrams to be generated.
                            </v-alert>

                            <!-- DATA LOAD ERROR -->
                            <v-alert :value="diagramsDataLoadFailed" type="error">
                            {{ diagramsDataFailedErrorMessage }}
                            </v-alert>

                            <!-- LOAD PROGRESS -->
                            <v-progress-linear 
                                v-if="diagramsDataLoadInProgress"
                                indeterminate color="green"></v-progress-linear>

                            <!-- SELECTED DIAGRAM -->
                            <v-layout v-if="currentDiagram != null && !sandboxMode" style="flex-direction: column;">
                                <v-flex sm12 md12 lg12>
                                    <sequence-diagram-component
                                        class="diagram"
                                        :title="currentDiagram.title"
                                        :steps="currentDiagram.steps"
                                        :showRemarks="showRemarks"
                                        :diagramStyle="diagramStyle"
                                        :clickable="options.EnableDiagramDetails"
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
import FrontEndOptionsViewModel from '../../models/Page/FrontEndOptionsViewModel';
import LoggedEndpointDefinitionViewModel from '../../models/RequestLog/LoggedEndpointDefinitionViewModel';
import LoggedEndpointRequestViewModel from '../../models/RequestLog/LoggedEndpointRequestViewModel';
import { EntryState } from '../../models/RequestLog/EntryState';
import DiagramsDataViewModel from '../../models/Documentation/DiagramsDataViewModel';
import DateUtils from "../../util/DateUtils";
import LinqUtils from "../../util/LinqUtils";
import UrlUtils from "../../util/UrlUtils";
import KeyArray from "../../util/models/KeyArray";
import KeyValuePair from "../../models/Common/KeyValuePair";
import SequenceDiagramComponent, { DiagramStep, DiagramLineStyle, DiagramStyle } from "../Common/SequenceDiagramComponent.vue";
import FilterInputComponent from '.././Common/FilterInputComponent.vue';
import FlowDiagramComponent, { FlowDiagramStep, FlowDiagramStepType } from '.././Common/FlowDiagramComponent.vue';

interface DiagramData
{
    title: string;
    // description: string | null;
    steps: Array<DiagramStep<DiagramStepDetails | null>>;
}
interface DiagramStepDetails
{
    classNameFrom: string,
    classNameTo: string,
    methodNameFrom: string,
    methodNameTo: string
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
    options!: FrontEndOptionsViewModel;

    ////
    testSteps: Array<FlowDiagramStep<DiagramStepDetails>> = [
		{
			title: 'Start',
            data: <any>{},
            type: FlowDiagramStepType.Start,
			connections: [
				{ target: 'Is it still broken?', label: null }
			]
		},
		{
			title: 'Is it still broken?',
			data: <any>{},
            type: FlowDiagramStepType.If,
			connections: [
				{ target: 'Seems everything\nis ok then!', label: 'Yes' },
				{ target: 'Pls fix', label: 'No' }
			]
		},
		{
			title: 'Pls fix',
			data: <any>{},
			connections: [
				{ target: 'Is it still broken?', label: null }
			]
		},
		{
			title: 'Seems everything\nis ok then!',
			data: <any>{},
			connections: [
				{ target: 'End', label: 'Good! Some long label text here!\nAnd a new line :]\nAnd one more!' }
			]
		},
		{
			title: 'End',
            type: FlowDiagramStepType.End,
			data: <any>{},
			connections: []
		}
	];
    ////

    // UI STATE
    drawerState: boolean = true;
    diagramFilterText: string = "";
    diagramsDataLoadInProgress: boolean = false;
    diagramsDataLoadFailed: boolean = true;
    diagramsDataFailedErrorMessage: string = '';
    sandboxMode: boolean = false;

    showRemarks: boolean = true;
    diagrams: Array<DiagramData> = [];
    currentDiagram: DiagramData | null = null;
    selectedStep: DiagramStep<DiagramStepDetails> | null = null;
    diagramStyle: DiagramStyle = DiagramStyle.Default;
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
        let restoredSandbox = localStorage.getItem("sandbox_sequencediagram_script");
        if (restoredSandbox != null)
        {
            this.sandboxScript = restoredSandbox;
        }

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
    get showToggleRemarks(): boolean
    {
        if (this.currentDiagram == null)
        {
            return false;
        }
        
        return this.currentDiagram.steps.some(x => x.remark != null && x.remark.trim().length > 0);
    }

    get sandboxSteps(): Array<DiagramStep<DiagramStepDetails | null>>
    {
        return this.convertStringToSteps(this.sandboxScript);
    }

    get diagramStyles(): Array<any>
    {
        return [
            { text: "Default", value: DiagramStyle.Default },
            { text: "Test", value: DiagramStyle.Test }
        ];
    }

    get showFilterCounts(): boolean {
        return this.diagramFilterText.length > 0;
    }

    ////////////////
    //  METHODS  //
    //////////////
    // Invoked from parent
    public onPageShow(): void {
        const parts = (<any>window).documentationState;
        if (parts != null && parts != undefined) {
            this.updateUrl(parts);
        }
    }

    setFromUrl(forcedParts: Array<string> | null = null): void {
        const parts = forcedParts || UrlUtils.GetHashParts();
        
        const selectedItem = parts[1];
        if (selectedItem !== undefined && selectedItem.length > 0) {
            let doc = this.diagrams.filter(x => UrlUtils.EncodeHashPart(x.title) == selectedItem)[0];
            if (doc != null)
            {
                this.setActveDiagram(doc);
            }
        }

        if (this.currentDiagram == null && this.diagrams.length > 0)
        {
            this.setActveDiagram(this.diagrams[0]);
        }

        if (selectedItem == 'sandbox')
        {
            this.sandboxMode = true;
            this.updateUrl();
        }
    }

    updateUrl(parts?: Array<string> | null): void {
        if (parts == null)
        {
            parts = ['documentation'];

            if (this.sandboxMode == true)
            {
                parts.push('sandbox');
            }
            else if (this.currentDiagram != null)
            {
                parts.push(UrlUtils.EncodeHashPart(this.currentDiagram.title));
            }
        }

        UrlUtils.SetHashParts(parts);
        
        // Some dirty technical debt before transitioning to propper routing :-)
        (<any>window).documentationState = parts;
    }

    loadData(): void {
        this.diagramsDataLoadInProgress = true;
        this.diagramsDataLoadFailed = false;

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
        .then((diagramsData: DiagramsDataViewModel) => this.onDiagramsDataRetrieved(diagramsData))
        .catch((e) => {
            this.diagramsDataLoadInProgress = false;
            this.diagramsDataLoadFailed = true;
            this.diagramsDataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    onDiagramsDataRetrieved(diagramsData: DiagramsDataViewModel): void {
        this.diagrams = diagramsData
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
        
        this.diagramsDataLoadInProgress = false;

        const originalUrlHashParts = UrlUtils.GetHashParts();
        this.setFromUrl(originalUrlHashParts);
    }

    convertStringToSteps(text: string): Array<DiagramStep<DiagramStepDetails | null>>
    {
        let lines = text.split('\n');
        
        let currentOptional: string | undefined = undefined;
        let steps: Array<DiagramStep<DiagramStepDetails | null>> = [];
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
            let style: DiagramLineStyle | undefined = undefined;
            let from = fromTo[0].trim();
            let to = fromTo[1].trim();
            // --> arrow means dashed style
            if (from.endsWith('-'))
            {
                from = from.substring(0, from.length - 1).trim();
                style = DiagramLineStyle.Dashed;
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
    
    toggleSideMenu(): void {
        this.drawerState = !this.drawerState;
    }

    filterDocumentation(data: Array<DiagramData>) : Array<DiagramData> {
        return data.filter(x => this.documentationFilterMatches(x));
    }

    documentationFilterMatches(data: DiagramData): boolean {
        return data.title.toLowerCase().indexOf(this.diagramFilterText.toLowerCase().trim()) != -1;
            // || (data.description != null 
            //     && data.description.toLowerCase().indexOf(this.diagramFilterText.toLowerCase().trim()) != -1);
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch("sandboxScript")
    onSandboxScriptChanged(): void
    {
        localStorage.setItem("sandbox_sequencediagram_script", this.sandboxScript);
    }

    showSandboxMode(): void {
        this.sandboxMode = true;
        this.updateUrl();
    }

    setActveDiagram(diagram: DiagramData): void {
        this.sandboxMode = false;
        this.currentDiagram = diagram;
        this.selectedStep = null;
        this.updateUrl();
    }
    
    onStepClicked(step: DiagramStep<DiagramStepDetails>): void
    {
        if (this.options.EnableDiagramDetails == true)
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
</style>
