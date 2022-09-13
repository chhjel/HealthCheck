<!-- src/components/modules/MappedData/MappedMemberDefinitionPairComponent.vue -->
<template>
    <div class="member-pair" :class="rootClasses">

        <div class="leftright-container" >
            <div class="left-side center-vertically">
                <mapped-member-definition-component
                    v-for="(mDef, dIndex) in def.Left"
                    :key="`memberdef-${id}-${def.Left.Id}-${dIndex}-left`"
                    :def="mDef"
                    :parentDef="parentDefs.Left"
                    :allDefinitions="allDefinitions"
                    @gotoTypeClicked="gotoTypeClicked" />
            </div>
            <div class="middle-section">↔️</div>
            <div class="right-side center-vertically">
                <mapped-member-definition-component
                    v-for="(mDef, dIndex) in def.Right"
                    :key="`memberdef-${id}-${def.Right.Id}-${dIndex}-right`"
                    :def="mDef"
                    :parentDef="parentDefs.Right"
                    :allDefinitions="allDefinitions"
                    @gotoTypeClicked="gotoTypeClicked" />
            </div>
        </div>

        <div class="leftright-container details" v-if="showDetails">
            <div class="left-side">
                <mapped-member-definition-details-component
                    v-for="(mDef, dIndex) in def.Left"
                    :key="`memberdef-${id}-${def.Left.Id}-${dIndex}-left-dets`"
                    :def="mDef" />
            </div>
            <div class="middle-section bordered"></div>
            <div class="right-side">
                <mapped-member-definition-details-component
                    v-for="(mDef, dIndex) in def.Right"
                    :key="`memberdef-${id}-${def.Right.Id}-${dIndex}-right-dets`"
                    :def="mDef" />
            </div>
        </div>
        
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import { HCMappedMemberDefinitionPairViewModel } from "@generated/Models/Core/HCMappedMemberDefinitionPairViewModel";
import MappedMemberDefinitionComponent from "./MappedMemberDefinitionComponent.vue";
import IdUtils from "@util/IdUtils";
import MappedMemberDefinitionDetailsComponent from "./MappedMemberDefinitionDetailsComponent.vue";
import { HCMappedClassesDefinitionViewModel } from "@generated/Models/Core/HCMappedClassesDefinitionViewModel";

@Options({
    components: {
        MappedMemberDefinitionComponent,
        MappedMemberDefinitionDetailsComponent
    }
})
export default class MappedMemberDefinitionPairComponent extends Vue {
    @Prop({ required: true })
    def!: HCMappedMemberDefinitionPairViewModel;

    @Prop({ required: true })
    allDefinitions: Array<HCMappedClassesDefinitionViewModel>;

    @Prop({ required: false, default: false })
    showDetails!: boolean;

    @Prop({ required: true })
    parentDefs: HCMappedClassesDefinitionViewModel;
    
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

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get rootClasses(): any {
        let classes: any = {};
        classes['details-open'] = this.showDetails;
        return classes;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    gotoTypeClicked(def: HCMappedClassesDefinitionViewModel, middle: boolean): void {
        this.$emit('gotoTypeClicked', def, middle);
    }
}
</script>

<style scoped lang="scss">
.member-pair {
    border-top: 1px solid var(--color--accent-darken2);
    padding-top: 5px;
    margin-top: 5px;

    .details {
        margin-top: 10px;
    }
}
</style>