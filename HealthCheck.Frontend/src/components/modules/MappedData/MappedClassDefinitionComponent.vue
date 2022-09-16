<!-- src/components/modules/MappedData/MappedClassDefinitionComponent.vue -->
<template>
    <div class="class-def">
        <h2>{{ def.DisplayName }}</h2>
        <div>ClassType: <b>{{ def.ClassTypeName }}</b></div>
        <div v-if="def.Remarks" class="mt-2"><p>{{ def.Remarks }}</p></div>

        <div>
            <mapped-member-definition-component
                v-for="(memberDef, memberDefIndex) in def.MemberDefinitions"
                :key="`def-member-${def.Id}-${memberDefIndex}`"
                :def="memberDef"
                :parentDef="def"
                :allDefinitions="allDefinitions"
                :displayOptions="displayOptions"
                />
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import { HCMappedClassDefinitionViewModel } from "@generated/Models/Core/HCMappedClassDefinitionViewModel";
import { HCMappedDataDefinitionsViewModel } from "@generated/Models/Core/HCMappedDataDefinitionsViewModel";
import MappedMemberDefinitionComponent from "./MappedMemberDefinitionComponent.vue";
import MappedDataDisplayOptions from "@models/modules/MappedData/MappedDataDisplayOptions";

@Options({
    components: {
        MappedMemberDefinitionComponent
    }
})
export default class MappedClassDefinitionComponent extends Vue {
    @Prop({ required: true })
    def!: HCMappedClassDefinitionViewModel;

    @Prop({ required: true })
    allDefinitions: HCMappedDataDefinitionsViewModel;

    @Prop({ required: true })
    displayOptions: MappedDataDisplayOptions;

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
}
</script>

<style scoped lang="scss">
</style>