<!-- src/components/modules/MappedData/MappedClassDefinitionComponent.vue -->
<template>
    <div>
        <code>DisplayName: {{ def.DisplayName }}</code><br />
        <code>ClassTypeName: {{ def.ClassTypeName }}</code><br />
        <code>MapsToDefinitionId: {{ def.MapsToDefinitionId }}</code><br />
        <code>DataSourceName: {{ def.DataSourceName }}</code><br />
        <code>Remarks: {{ def.Remarks }}</code>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import { HCMappedClassDefinitionViewModel } from "@generated/Models/Core/HCMappedClassDefinitionViewModel";

@Options({
    components: {
    }
})
export default class MappedClassDefinitionComponent extends Vue {
    @Prop({ required: true })
    def!: HCMappedClassDefinitionViewModel;

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