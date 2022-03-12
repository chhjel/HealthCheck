<!-- src/components/Common/SequenceDiagramComponent.vue -->
<template>
    <div>
        <div class="sequence-diagram" :class="diagramClasses">
            <div class="sequence-diagram__header" v-if="title != null">{{ title }}</div>

            <div class="sequence-diagram__grid">
                <!-- VERTICAL LINES -->
                <div class="sequence-diagram__grid-column-line"
                    v-for="(column, cindex) in columns"
                    :key="`column-vertical-${cindex}`"
                    :style="getColumnLineStyle(cindex)"></div>

                <!-- HEADER -->
                <div class="sequence-diagram__grid-header"
                    v-for="(column, cindex) in columns"
                    :key="`column-header-${cindex}`"
                    :style="getHeaderStyle(cindex)">{{ column }}</div>
                
                <!-- STEPS -->
                <div class="sequence-diagram__grid-item"
                    v-for="(step, sindex) in stepData"
                    :key="`column-step-${sindex}`"
                    :style="getStepStyle(sindex, step)"
                    :class="getStepClasses(sindex, step)"
                    @click="onStepClicked(step)">
                        <span class="description">{{ step.data.description }}<sup v-if="showRemarks && step.remarkNumber != null"> {{ step.remarkNumber }}</sup></span>
                        <div class="arrow-self" v-if="step.columnEnd == step.columnStart"></div>
                        <div :class="getStepArrowClasses(sindex, step)"></div>
                </div>

                <!-- OPTIONALS -->
                <div class="sequence-diagram__grid-optional-area"
                    v-for="(optional, oindex) in optionalAreas"
                    :key="`optional-area-${oindex}`"
                    :style="getOptionalAreaStyle(optional)">
                    <div class="sequence-diagram__grid-optional-area-text">{{ optional.text }}</div>
                </div>
                
                <!-- FOOTER -->
                <div class="sequence-diagram__grid-footer"
                    v-for="(column, cindex) in columns"
                    :key="`column-footer-${cindex}`"
                    :style="getFooterStyle(cindex)">{{ column }}</div>
            </div>

            <!-- REMARKS -->
            <div v-if="hasRemarks && showRemarks" class="sequence-diagram__remarks">
                <div class="sequence-diagram__remarks-item"
                    v-for="(remark, rindex) in stepRemarks"
                    :key="`remark-${rindex}`">
                    <sup>{{ remark.remarkNumber }}</sup> {{ remark.text }}
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { SequenceDiagramStep , SequenceDiagramLineStyle , SequenceDiagramStyle  } from './SequenceDiagramComponent.vue.models';
@Options({
    components: {
    }
})
export default class SequenceDiagramComponent extends Vue
{
    @Prop({ required: false, default: null })
    title!: string | null;

    @Prop({ required: true, default: true })
    steps!: Array<SequenceDiagramStep<any>>;

    @Prop({ required: false, default: true })
    showRemarks!: boolean;

    @Prop({ required: false, default: false })
    clickable!: boolean;

    @Prop({ required: false, default: SequenceDiagramStyle.Default })
    diagramStyle!: SequenceDiagramStyle;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get columns(): Array<string>
    {
        let values = new Set<string>();
        this.steps.forEach((x) => { values.add(x.from); values.add(x.to); });
        return Array.from(values);
    }

    get stepData(): Array<InternalDiagramStep>
    {
        let remarkCounter = 0;
        return this.steps.map(x => {
            let remarkNumber: number | null = null;
            if (x.remark != null)
            {
                remarkCounter++;
                remarkNumber = remarkCounter;
            }

            return {
                data: x,
                columnStart: this.columns.indexOf(x.from) + 1,
                columnEnd: this.columns.indexOf(x.to) + 1,
                remarkNumber: remarkNumber
            };
        });
    }

    get stepRemarks(): Array<InternalStepRemark>
    {
        return this.stepData
            .filter(x => x.remarkNumber != null)
            .map(x => {
                return {
                    step: x,
                    remarkNumber: x.remarkNumber || 0,
                    text: x.data.remark || ''
                };
            });
    }

    get hasRemarks(): boolean
    {
        return this.stepRemarks.length > 0;
    }

    get optionalAreas(): Array<InternalOptionalArea>
    {
        let areas = new Array<InternalOptionalArea>();
        let currentArea: InternalOptionalArea | null = null;
        for(let i=0;i<this.steps.length;i++)
        {
            let step = this.steps[i];
         
            // No optional, end current area
            if (step.optional == null)
            {
                currentArea = null;
                continue;
            }

            let fromColumn = this.columns.indexOf(step.from) + 2;
            let toColumn = this.columns.indexOf(step.to) + 2;
            let stepColumnStart = Math.min(fromColumn, toColumn);
            let stepColumnEnd = Math.max(fromColumn, toColumn);
            let stepRow = i + 2;

            // No existing area or is optional and not same as last
            if (currentArea == null || currentArea.text != step.optional)
            {
                currentArea = {
                    text: step.optional,
                    columnStart: stepColumnStart,
                    columnEnd: stepColumnEnd,
                    rowStart: stepRow,
                    rowEnd: stepRow
                };
                areas.push(currentArea);
            }
            // Else expand existing
            else {
                currentArea.columnStart = Math.min(currentArea.columnStart, stepColumnStart);
                currentArea.columnEnd = Math.max(currentArea.columnEnd, stepColumnEnd);
                currentArea.rowStart = Math.min(currentArea.rowStart, stepRow);
                currentArea.rowEnd = Math.max(currentArea.rowEnd, stepRow) + 1;
            }
        }
        return areas;
    }

    get diagramClasses(): Array<string>
    {
        let classes = [
            `sequence-diagram__style--${this.diagramStyle.toLowerCase()}`
        ];

        if (this.clickable == true)
        {
            classes.push('clickable');
        }
        
        return classes;
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    getHeaderStyle(index: number): any {
        return {
            'grid-column-start': (index+2),
            'grid-column-end': (index+2)
        };
    }

    getFooterStyle(index: number): any {
        return {
            'grid-column-start': (index+2),
            'grid-column-end': (index+2),
            'grid-row-start': this.steps.length + 3,
            'grid-row-end': this.steps.length + 3
        };
    }

    getColumnLineStyle(index: number): any {
        return {
            'grid-column-start': (index+2),
            'grid-column-end': (index+2),
            'grid-row-start': 2,
            'grid-row-end': this.steps.length + 3
        };
    }

    getStepStyle(stepIndex: number, step: InternalDiagramStep): any {
        let start = step.columnStart + 1;
        let end = step.columnEnd + 1;
        let isGoingToSelf = step.columnEnd == step.columnStart;

        let style = {
            'grid-column-start': start,
            'grid-column-end': end,
            'grid-row-start': stepIndex + 2,
            'grid-row-end': stepIndex + 2,
            'text-align': (isGoingToSelf ? 'left' : 'center')
        };

        return style;
    }

    getStepClasses(stepIndex: number, step: InternalDiagramStep): any {
        let isLast = (stepIndex == this.steps.length - 1);
        let isGoingLeft = step.columnEnd < step.columnStart;
        let isGoingRight = step.columnEnd > step.columnStart;
        let isGoingToSelf = step.columnEnd == step.columnStart;

        let prevStep = this.stepData[stepIndex-1];
        let nextStep = this.stepData[stepIndex+1];
        let isFirstInOptional = (step.data.optional != undefined && (prevStep == null || step.data.optional != prevStep.data.optional));
        let isLastInOptional = (step.data.optional != undefined && (nextStep == null || step.data.optional != nextStep.data.optional));
        
        let styleName = 'style--default';
        if (isGoingLeft)
        {
            styleName = 'style--dashed';
        }
        // Custom overrides
        if (step.data.style === SequenceDiagramLineStyle.Dashed)
        {
            styleName = 'style--dashed';
        }

        let classes = [ styleName ];
        if (isLast)
        {
            classes.push('last-step');
        }

        if (isGoingLeft)
        {
            classes.push('direction--left');
        }
        else if (isGoingRight)
        {
            classes.push('direction--right');
        }
        else if (isGoingToSelf)
        {
            classes.push('direction--self');
        }

        if (isFirstInOptional)
        {
            classes.push('first-in-optional-group');
        }
        if (isLastInOptional)
        {
            classes.push('last-in-optional-group');
        }

        return classes;
    }

    getStepArrowClasses(stepIndex: number, step: InternalDiagramStep): any {
        let isGoingLeft = step.columnEnd < step.columnStart;
        let isGoingRight = step.columnEnd > step.columnStart;
        let isGoingToSelf = step.columnEnd == step.columnStart;
        let classes = {
            'arrow-left': isGoingLeft || isGoingToSelf,
            'arrow-right': isGoingRight
        };

        return classes;
    }

    getOptionalAreaStyle(optional: InternalOptionalArea): any
    {
        return {
            'grid-column-start': optional.columnStart,
            'grid-column-end': optional.columnEnd,
            'grid-row-start': optional.rowStart,
            'grid-row-end': optional.rowEnd
        };
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onStepClicked(step: InternalDiagramStep): void
    {
        this.$emit('stepClicked', step.data);
    }
}
interface InternalDiagramStep {
    data: SequenceDiagramStep<any>;
    remarkNumber: number | null;
    columnStart: number;
    columnEnd: number;
}
interface InternalStepRemark {
    step: InternalDiagramStep;
    remarkNumber: number;
    text: string;
}
interface InternalOptionalArea {
    text: string;
    columnStart: number;
    columnEnd: number;
    rowStart: number;
    rowEnd: number;
}
</script>

<style scoped lang="scss">
.sequence-diagram {
    overflow: auto;

    &.clickable {
        .sequence-diagram__grid-item {
            cursor: pointer;

            &:hover {
                &::after {
                    content: " ";
                    border: 2px solid #e0e0e0;
                    position: absolute;
                    left: -5px;
                    top: 0px;
                    right: -5px;
                    bottom: -15px;
                }
            }
        }
    }

    .sequence-diagram__header {
        font-weight: 600;
        text-align: center;
        margin-bottom: 16px;
        font-size: 20px;
    }

    .sequence-diagram__grid {
        display: grid;
        row-gap: 12px;
        margin-left: 15px;

        .sequence-diagram__grid-header,
        .sequence-diagram__grid-footer {
            justify-self: left;
            margin-left: -15px;
            margin-right: 25px;
            display: flex;
            justify-content: center;
            align-items: center;
            text-align: center;
            /* transform: translateX(-50%); */
            
            border: 1px solid gray;
            border-radius: 5px;
            padding: 5px;
            font-weight: 600;
        }
        .sequence-diagram__grid-header {
            grid-row-start: 1;
            grid-row-end: 1;
            margin-bottom: -10px;
        }

        .sequence-diagram__grid-footer {
            margin-top: -10px;
        }

        .sequence-diagram__grid-column-line {
            border-left: 1px dashed #c4c4c4;
        }

        .sequence-diagram__grid-item {
            grid-column-start: 2;
            grid-column-end: 4;
            grid-row-start: 2;
            grid-row-end: 3;
            position: relative;

            margin-top: 4px;
            margin-bottom: 4px;
            padding: 10px;

            .description
            {
                white-space: pre-line;
            }

            &.last-step {
                margin-bottom: 20px;
            }

            &.first-in-optional-group
            {
                margin-top: 30px;
            }

            &.last-in-optional-group
            {
                margin-bottom: 22px;
            }

            &.direction--right,
            &.direction--left {
                &.style--default { border-bottom-style: solid; }
                &.style--dashed { border-bottom-style: dashed; }

                border-bottom: 2px gray;
            }
            
            &.direction--self {
                &.style--default .arrow-self { border-style: solid; border-left-style: none; }
                &.style--dashed .arrow-self { border-style: dashed; border-left-style: none; }
            }

            .arrow-right {
                width: 0;
                height: 0;
                border-top: 10px solid transparent;
                border-bottom: 10px solid transparent;
                border-left: 10px solid black;
                position: absolute;
                right: -2px;
                bottom: -11px;
            }

            .arrow-left {
                width: 0;
                height: 0;
                border-top: 10px solid transparent;
                border-bottom: 10px solid transparent; 
                border-right:10px solid black; 
                position: absolute;
                left: -2px;
                bottom: -11px;
            }

            .arrow-self {
                max-width: 40px;
                height: 30px;
                border-top: 2px black;
                border-right: 2px black;
                border-bottom: 2px black;
                border-left: none;
                border-top-right-radius: 50%;
                border-bottom-right-radius: 50%;
                margin-left: -10px;
                margin-top: 10px;
                margin-bottom: -11px;
            }
        }

        .sequence-diagram__grid-optional-area {
            border: 3px solid #95b592;
            border-left-width: 2px;
            border-right-width: 2px;
            border-radius: 5px;
            margin: -6px;
            margin-bottom: 5px;
            margin-top: 5px;
            z-index: 1;
            pointer-events: none;

            .sequence-diagram__grid-optional-area-text {
                display: inline-block;
                border-right: 2px solid #95b592;
                border-bottom: 2px solid #95b592;
                border-bottom-right-radius: 5px;
                background-color: #95b592;
                padding: 1px;
            }
        }
    }

    .sequence-diagram__remarks {
        margin-top: 20px;
        padding: 20px;

        .sequence-diagram__remarks-item {
            margin: 10px 20px;
        }
    }

    &.sequence-diagram__style--test {
        color: #226622;
        font-family: 'Courier New', Courier, monospace;
        
        .sequence-diagram__grid-header {
            font-weight: 600;
            font-size: 20px;
            border: none;
            border-bottom: 3px solid green;
            border-radius: 0;
        }

        .sequence-diagram__grid-footer {
            display: none;
        }

        .sequence-diagram__grid-optional-area {
            border-radius: 0;
            border-color: gray; 
            
            .sequence-diagram__grid-optional-area-text {
                color: white;
                border-color: gray;
                background-color: gray;
                font-weight: 600;
            }
        }

        .direction--right,
        .direction--left {
            border-bottom-color: lightgray !important;
        }

        .arrow-right {
            border-left-color: lightgray !important;
        }

        .arrow-left {
            border-right-color: lightgray !important;
        }

        .arrow-self {
            border-top-color: lightgray !important;
            border-right-color: lightgray !important;
            border-bottom-color: lightgray !important;
        }
    }

    &.sequence-diagram__style--default {
        /* .sequence-diagram__grid-header,
        .sequence-diagram__grid-footer {
        } */

        /* .sequence-diagram__grid-column-line {
        } */

        .sequence-diagram__grid-item {
            .description {
                text-shadow: 0 0 10px #ffffff;
            }
        }

        /* .sequence-diagram__grid-optional-area {
            .sequence-diagram__grid-optional-area-text {
            }
        } */
    }
}
</style>