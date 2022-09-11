<!-- src/components/modules/MappedData/MappedMemberDefinitionComponent.vue -->
<template>
    <div>
        <code>{{ def.Left.DisplayName }}</code> ↔️ <code>{{ def.Right.DisplayName }}</code><br />
        <code>{{ def }}</code>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import { HCMappedMemberDefinitionViewModel } from "@generated/Models/Core/HCMappedMemberDefinitionViewModel";

@Options({
    components: {
    }
})
export default class MappedMemberDefinitionComponent extends Vue {
    @Prop({ required: true })
    def!: HCMappedMemberDefinitionViewModel;

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