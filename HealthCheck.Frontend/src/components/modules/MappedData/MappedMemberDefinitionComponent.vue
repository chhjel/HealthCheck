<!-- src/components/modules/MappedData/MappedMemberDefinitionComponent.vue -->
<template>
    <div class="member-def" :class="rootClasses">
        <div v-if="def.Remarks && displayOptions.showPropertyRemarks" class="member-comment">// <span v-html="def.Remarks"></span></div>

        <div class="member-def__split">
            <!-- LEFT -->
            <div class="member-def__left">
                <div class="member-def__header">
                    <code class="display-name"
                        @click="onDisplayNameClicked(false)"
                        @click.middle.stop.prevent="onDisplayNameClicked(true)"
                        :title="`${def.FullPropertyTypeName}`"
                        >{{ displayOptions.showPropertyNames == 'actual' ? def.PropertyName : def.DisplayName }}</code>
                    <span class="example-value" v-if="showExampleValues" :title="exampleValue"> = {{ exampleValue }}</span>
                    <div class="line-to-right" v-if="hasMappings">
                        <span class="line-to-right__start"></span>
                        <span class="line-to-right__end"></span>
                    </div>
                </div>
                <div v-if="def.Error"><code>Error: {{ def.Error }}</code></div>
            </div>
            
            <!-- RIGHT -->
            <div class="member-def__right" v-if="hasMappings">
                <mapped-member-mapping-component
                    v-for="(mapping, mappingIndex) in def.MappedTo"
                    :key="`def-mapping-${def.Id}-${id}-${mappingIndex}`"
                    :mapping="mapping"
                    :allDefinitions="allDefinitions"
                    :displayOptions="displayOptions"
                    @gotoData="gotoData"
                    class="member-mapping mb-1"
                    />
            </div>
        </div>
        
        <div v-if="hasChildren" style="margin-bottom: 10px; margin-top: 10px;">
            <mapped-member-definition-component
                v-for="(memberDef, memberDefIndex) in def.Children"
                :key="`def-child-${def.Id}-${id}-${memberDefIndex}`"
                :def="memberDef"
                :parentDef="def"
                :allDefinitions="allDefinitions"
                :displayOptions="displayOptions"
                :exampleData="exampleData"
                @gotoData="gotoData"
                style="margin-left: 10px; padding-left: 10px; border-left: 2px solid lightgray;"
                />
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import { HCMappedMemberDefinitionViewModel } from "@generated/Models/Core/HCMappedMemberDefinitionViewModel";
import { HCMappedClassDefinitionViewModel } from "@generated/Models/Core/HCMappedClassDefinitionViewModel";
import IdUtils from "@util/IdUtils";
import { HCMappedDataDefinitionsViewModel } from "@generated/Models/Core/HCMappedDataDefinitionsViewModel";
import MappedMemberMappingComponent from "./MappedMemberMappingComponent.vue";
import MappedDataDisplayOptions from "@models/modules/MappedData/MappedDataDisplayOptions";
import MappedDataLinkData from "@models/modules/MappedData/MappedDataLinkData";
import { HCMappedExampleValueViewModel } from "@generated/Models/Core/HCMappedExampleValueViewModel";

@Options({
    components: {
        MappedMemberDefinitionComponent,
        MappedMemberMappingComponent
    }
})
export default class MappedMemberDefinitionComponent extends Vue {
    @Prop({ required: true })
    def!: HCMappedMemberDefinitionViewModel;

    @Prop({ required: true })
    parentDef: HCMappedClassDefinitionViewModel;

    @Prop({ required: true })
    allDefinitions: HCMappedDataDefinitionsViewModel;

    @Prop({ required: true })
    displayOptions: MappedDataDisplayOptions;

    @Prop({ required: true })
    exampleData: HCMappedExampleValueViewModel;
    
    id: string = IdUtils.generateId();

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
    }

    ////////////////
    //  METHODS  //
    //////////////
    getExampleValueFor(path: string, obj: any | null): string {
        if (!obj) return null;
        return obj[path] || null;
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get hasLinkToDef(): boolean {
        return false;
        // if (this.def.FullPropertyTypeName == this.parentDef.Id) return false;
        // return this.allDefinitions.ClassDefinitions.some(x => x.Id == this.def.FullPropertyTypeName)
        //         || this.allDefinitions.ReferencedDefinitions.some(x => x.TypeName == this.def.FullPropertyTypeName);
    }

    get rootClasses(): any {
        let classes: any = {};
        classes['hasLinkToDef'] = this.hasLinkToDef;
        return classes;
    }

    get hasChildren(): boolean {
        return this.def.Children != null && this.def.Children.length > 0;
    }

    get hasMappings(): boolean {
        return this.def.MappedTo != null && this.def.MappedTo.length > 0;
    }

    get showExampleValues(): boolean {
        return this.displayOptions.showExampleValues
            && this.exampleData != null && this.exampleData.Values != null
            && Object.keys(this.exampleData.Values).includes(this.def.FullPropertyPath);
    }

    // todo example values for mappings as well
    get exampleValue(): string {
        const obj = this.exampleData?.Values;
        const value = this.getExampleValueFor(this.def.FullPropertyPath, obj);
        if (value === null) return 'null';
        else return value;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDisplayNameClicked(middle: boolean): void {
        if (!this.hasLinkToDef) return;
        const match = this.allDefinitions.ClassDefinitions.find(x => x.Id == this.def.FullPropertyTypeName);
                // || this.allDefinitions.ReferencedDefinitions.some(x => x.TypeName == this.def.FullPropertyTypeName);
        if (match == null) return;

        const payload: MappedDataLinkData = {
            id: match.Id,
            type: "ClassDefinition",
            newWindow: middle
        };
        this.gotoData(payload);
    }

    gotoData(data: MappedDataLinkData): void {
        this.$emit('gotoData', data);
    }
}
</script>

<style scoped lang="scss">
.member-def {
    margin-top: 2px;
    margin-bottom: 2px;
    
    .display-name {
        font-size: 16px;
        color: var(--color--primary-base);
    }
    .example-value {
        color: var(--color--accent-darken5);
        font-size: 13px;
        margin-left: 5px;
        max-width: 221px;
        white-space: nowrap;
        text-overflow: ellipsis;
        overflow: hidden;
        display: block;
    }

    &.hasLinkToDef {
        .display-name {
            text-decoration: underline;
            cursor: pointer;
        }
    }

    &__split {
        display: flex;
        /* border-bottom: 1px solid var(--color--accent-darken1); */
        padding-bottom: 5px;
        margin-bottom: 5px;
        padding-top: 5px;
    }
    &__left, &__right {
        flex: 1;
        position: relative;
    }
    &__header {
        display: flex;
        align-items: center;
        .line-to-right {
            flex: 1;
            border-top: 2px solid var(--color--accent-darken1);
            margin-left: 10px;
            margin-right: 10px;
            margin-top: 8px;
            display: flex;
            justify-content: space-between;
            &__start, &__end {
                width: 9px;
                height: 9px;
                border-left: 2px solid var(--color--accent-darken1);
                border-bottom: 2px solid var(--color--accent-darken1);
                transform: rotate(45deg);
                margin-top: -6px;
                box-sizing: border-box;
            }
            &__end {
                transform: rotate(-135deg);
            }
        }
    }

    .member-comment {
        color: var(--color--accent-darken9);
        font-size: 14px;
    }
    
    .member-mapping {
        &:not(:first-child) {
            &::before {
                content: ' ';
                width: 9px;
                height: 9px;
                border-left: 2px solid var(--color--accent-darken1);
                border-bottom: 2px solid var(--color--accent-darken1);
                box-sizing: border-box;
                position: absolute;
                left: -16px;
            }
        }
    }
}
</style>