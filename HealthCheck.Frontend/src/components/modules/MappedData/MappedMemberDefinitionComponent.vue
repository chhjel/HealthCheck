<!-- src/components/modules/MappedData/MappedMemberDefinitionComponent.vue -->
<template>
    <div class="member-def" :class="rootClasses">
        <code class="display-name"
            @click="onDisplayNameClicked(false)"
            @click.middle.stop.prevent="onDisplayNameClicked(true)"
            >{{ def.DisplayName }}</code>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import { HCMappedMemberDefinitionViewModel } from "@generated/Models/Core/HCMappedMemberDefinitionViewModel";
import { HCMappedClassesDefinitionViewModel } from "@generated/Models/Core/HCMappedClassesDefinitionViewModel";

@Options({
    components: {
    }
})
export default class MappedMemberDefinitionComponent extends Vue {
    @Prop({ required: true })
    def!: HCMappedMemberDefinitionViewModel;

    @Prop({ required: true })
    allDefinitions: Array<HCMappedClassesDefinitionViewModel>;

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

    get hasLinkToDef(): boolean {
        return this.allDefinitions.some(x => x.Left.Id == this.def.FullTypeName || x.Right.Id == this.def.FullTypeName);
    }

    get rootClasses(): any {
        let classes: any = {};
        classes['hasLinkToDef'] = this.hasLinkToDef;
        return classes;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDisplayNameClicked(middle: boolean): void {
        if (!this.hasLinkToDef) return;
        const match = this.allDefinitions.find(x => x.Left.Id == this.def.FullTypeName || x.Right.Id == this.def.FullTypeName);
        if (match == null) return;
        this.emitGotoType(match, middle);
    }

    emitGotoType(def: HCMappedClassesDefinitionViewModel, middle: boolean): void {
        this.$emit('gotoTypeClicked', def, middle);
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

    &.hasLinkToDef {
        .display-name {
            text-decoration: underline;
            cursor: pointer;
        }
    }
}
</style>