<!-- src/components/modules/MappedData/MappedMemberDefinitionDetailsComponent.vue -->
<template>
    <div class="member-def-details">
        <div>Property: <b>{{ typeName }} {{ propertyName }}</b></div>
        <p v-if="def.Remarks">{{ def.Remarks }}</p>
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
export default class MappedMemberDefinitionDetailsComponent extends Vue {
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

    get typeName(): string {
        let name = this.def.FullTypeName;
        if (name?.includes('.')) name = name.substring(name.lastIndexOf('.')+1);
        return name;
    }

    get propertyName(): string {
        return this.def.PropertyName;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
.member-def-details {
    font-size: 14px;
}
</style>