<!-- src/components/modules/MappedData/MappedClassDefinitionComponent.vue -->
<template>
    <div class="class-def">
        <h2 :title="`Class type: '${def.ClassTypeName}'`">{{ def.DisplayName }}</h2>
        <div v-if="def.Remarks" class="mt-2 class-def-remarks"><p><span v-html="def.Remarks"></span></p></div>

        <div>
            <mapped-member-definition-component
                v-for="(memberDef, memberDefIndex) in def.MemberDefinitions"
                :key="`def-member-${def.Id}-${memberDefIndex}`"
                :def="memberDef"
                :parentDef="def"
                :allDefinitions="allDefinitions"
                :displayOptions="displayOptions"
                :exampleData="exampleData"
                @gotoData="gotoData"
                />
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import { TKMappedClassDefinitionViewModel } from "@generated/Models/Core/TKMappedClassDefinitionViewModel";
import { TKMappedDataDefinitionsViewModel } from "@generated/Models/Core/TKMappedDataDefinitionsViewModel";
import MappedMemberDefinitionComponent from "./MappedMemberDefinitionComponent.vue";
import MappedDataDisplayOptions from "@models/modules/MappedData/MappedDataDisplayOptions";
import MappedDataLinkData from "@models/modules/MappedData/MappedDataLinkData";
import { TKMappedExampleValueViewModel } from "@generated/Models/Core/TKMappedExampleValueViewModel";

@Options({
    components: {
        MappedMemberDefinitionComponent
    }
})
export default class MappedClassDefinitionComponent extends Vue {
    @Prop({ required: true })
    def!: TKMappedClassDefinitionViewModel;

    @Prop({ required: true })
    allDefinitions: TKMappedDataDefinitionsViewModel;

    @Prop({ required: true })
    displayOptions: MappedDataDisplayOptions;

    @Prop({ required: true })
    exampleData: TKMappedExampleValueViewModel;

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

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    gotoData(data: MappedDataLinkData): void {
        this.$emit('gotoData', data);
    }
}
</script>

<style scoped lang="scss">
.class-def-remarks {
    white-space: pre;
}
</style>