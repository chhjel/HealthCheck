<!-- src/components/common/SequenceDiagramComponent.vue -->
<template>
    <div>
        <div class="sequence-diagram sequence-diagram__style--default">
            <div class="sequence-diagram__header">Some diagram</div>

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
                    :class="getStepClasses(sindex, step)">
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

            <div v-if="hasRemarks" class="sequence-diagram__remarks">
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
import { Vue, Component, Prop } from "vue-property-decorator";

@Component({
    components: {
    }
})
export default class SequenceDiagramComponent extends Vue
{
    @Prop({ required: false, default: true })
    showRemarks!: boolean

    // Arrow to self: from == to
    steps: Array<DiagramStep> = [
        {
            from: 'Frontend',
            to: 'Web',
            description: 'Initiate some stuff',
        },
        {
            from: 'Web',
            to: 'Web',
            description: 'Verify self',
            remark: 'Link to something important here perhaps',
            note: 'Extra note here'
        },
        {
            from: 'Web',
            to: 'OrderFileWatcher Service',
            description: 'Something else',
            style: DiagramLineStyle.Dashed
        },
        {
            from: 'OrderFileWatcher Service',
            to: 'File',
            description: 'Store some things and do some other things.\nAnd a bit more stuff etc etc.\nAnd one more.',
        },
        {
            from: 'File',
            to: 'CRM',
            description: 'Read some things',
        },
        {
            from: 'CRM',
            to: 'OrderFileWatcher Service',
            description: 'Verify some things',
            optional: 'invoice only',
        },
        {
            from: 'OrderFileWatcher Service',
            to: 'CRM',
            description: 'Approve some things',
            optional: 'invoice only',
        },
        {
            from: 'CRM',
            to: 'Web',
            description: 'Notify',
            remark: 'Using SignalR, view https://asd.com for more details'
        },
        {
            from: 'Web',
            to: 'Frontend',
            description: 'Report back',
        }
    ];

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
            let remarkNumber = null;
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

            let stepColumnStart = this.columns.indexOf(step.from) + 2;
            let stepColumnEnd = this.columns.indexOf(step.to) + 2;
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
        
        let styleName = 'style--default';
        if (isGoingLeft)
        {
            styleName = 'style--dashed';
        }
        // Custom overrides
        if (step.data.style === DiagramLineStyle.Dashed)
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
    // ToDo click to switch between count/percentage
}
export interface DiagramStep {
    from: string;
    to: string;
    description: string;
    note?: string;
    remark?: string;
    optional?: string;
    style?: DiagramLineStyle;
}
export enum DiagramLineStyle {
    Default = 0,
    Dashed = 1
}
interface InternalDiagramStep {
    data: DiagramStep;
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

    .sequence-diagram__header {
        font-weight: 600;
        text-align: center;
        margin-bottom: 16px;
        font-size: 20px;
    }

    .sequence-diagram__grid {
        display: grid;
        row-gap: 12px;

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
        }

        /* .sequence-diagram__grid-footer {
        } */

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
            z-index: 1;

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

    &.sequence-diagram__style--default {
        /* .sequence-diagram__grid-header {
        } */

        /* .sequence-diagram__grid-footer {
        } */

        /* .sequence-diagram__grid-column-line {
        } */

        /* .sequence-diagram__grid-item {
        } */

        /* .sequence-diagram__grid-optional-area {
            .sequence-diagram__grid-optional-area-text {
            }
        } */
    }
}
</style>