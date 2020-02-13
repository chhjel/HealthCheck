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
                    <v-list-tile ripple
                        v-for="(diagram, diagramIndex) in diagrams"
                        :key="`testset-menu-${diagramIndex}`"
                        @click="setActveDiagram(diagram)">
                        <v-list-tile-title v-text="diagram.title"></v-list-tile-title>
                    </v-list-tile>
                </v-list>
            </v-navigation-drawer>
            
            <!-- CONTENT -->
            <v-container fluid fill-height class="content-root">
                <v-layout>
                    <v-flex>
                        <v-select
                            v-model="diagramStyle"
                            :items="diagramStyles"
                            item-text="text" item-value="value" color="secondary">
                        </v-select>

                        <v-layout>
                            <v-flex sm12 lg4>
                                <textarea
                                    style="width: 100%; border: 1px solid gray;"
                                    rows="10"
                                    v-model="test"
                                    />
                            </v-flex>
                                
                            <v-flex sm12 lg8>
                                <v-container grid-list-md>
                                    <sequence-diagram-component
                                        class="diagram"
                                        :steps="steps"
                                        :diagramStyle="diagramStyle" />
                                </v-container>
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
import DateUtils from "../../util/DateUtils";
import LinqUtils from "../../util/LinqUtils";
import UrlUtils from "../../util/UrlUtils";
import KeyArray from "../../util/models/KeyArray";
import KeyValuePair from "../../models/Common/KeyValuePair";
import SequenceDiagramComponent, { DiagramStep, DiagramLineStyle, DiagramStyle } from "../Common/SequenceDiagramComponent.vue";

interface DiagramData
{
    title: string;
    steps: Array<DiagramStep>;
}

@Component({
    components: {
        SequenceDiagramComponent
    }
})
export default class DocumentationPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // UI STATE
    drawerState: boolean = true;

    diagramStyle: DiagramStyle = DiagramStyle.Default;
    test: string = `
Frontend --> Web: User sends form
Web -> Web: Validate input
opt Invoice only
Web -> External Service: Data is sent to 3rd party | remark: Some remark here
Web -> Database: Backup of data is stored in database
end
Web -> Frontend: Confirmation is delivered
`;

    diagrams: Array<DiagramData> = [
        {
            title: "Test Diagram A",
            steps: this.convertStringToSteps(this.test)
        },
        {
            title: "Test Diagram B",
            steps: this.convertStringToSteps(this.test)
        }
    ];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
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
    get steps(): Array<DiagramStep>
    {
        return this.convertStringToSteps(this.test);
    }

    get diagramStyles(): Array<any>
    {
        return [
            { text: "Default", value: DiagramStyle.Default },
            { text: "Test", value: DiagramStyle.Test }
        ];
    }

    ////////////////
    //  METHODS  //
    //////////////
    convertStringToSteps(text: string): Array<DiagramStep>
    {
        let lines = text.split('\n');
        
        let currentOptional: string | undefined = undefined;
        let steps: Array<DiagramStep> = [];
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
                style: style
            };
            steps.push(step);
        }
        return steps;
    }
    
    toggleSideMenu(): void {
        this.drawerState = !this.drawerState;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    setActveDiagram(diagram: DiagramData): void {
        console.log(diagram);
    }
}
</script>

<style scoped lang="scss">
.diagram {
    background-color: #fff;
    padding: 20px;
    border: 1px solid #353535;
}
</style>
