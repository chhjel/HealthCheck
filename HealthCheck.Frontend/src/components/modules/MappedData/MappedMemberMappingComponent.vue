<!-- src/components/modules/MappedData/MappedMemberMappingComponent.vue -->
<template>
    <div :class="rootClasses">
        <div v-if="mapping.Error"><code>Error: {{ mapping.Error }}</code><br /></div>

        <div v-if="hasItems" class="mapped-to-items">
            <div class="mapped-to-item-root" :class="rootRefClasses">
                <code class="mapped-to-item__name">{{ mapping.RootReferenceId }}</code>
            </div>

            <div v-for="(chainItem, chainItemIndex) in mapping.Items"
                 :key="`def-mapping-item-${id}-${chainItemIndex}`"
                 :class="getItemClasses(chainItem)"
                 class="mapped-to-item">
                <code class="mapped-to-item__name">{{ getChainItemName(chainItem) }}</code>
            </div>
        </div>

        <div v-if="hasItems" class="mapped-to-items-details">
            <div v-for="(chainItem, chainItemIndex) in mapping.Items"
                 :key="`def-mapping-item-${id}-${chainItemIndex}-err`">
                <div v-if="chainItem.PropertyTypeName && displayOptions.showMappedToTypes">Property: {{ chainItem.PropertyTypeName }} {{ chainItem.PropertyName }}</div>
                <div v-if="chainItem.DeclaringTypeName && displayOptions.showMappedToDeclaringTypes">Declaring type: {{ chainItem.DeclaringTypeName }}</div>
                <div v-if="chainItem.Error"><code>Error: {{ chainItem.Error }}</code></div>
            </div>
        </div>
        <!--
        <code>Path: {{ mapping.Path }}</code><br />
        <code>RootTypeName: {{ mapping.RootTypeName }}</code><br />
        <code>RootReferenceId: {{ mapping.RootReferenceId }}</code><br />
        -->
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import IdUtils from "@util/IdUtils";
import { HCMappedDataDefinitionsViewModel } from "@generated/Models/Core/HCMappedDataDefinitionsViewModel";
import { HCMappedMemberReferenceDefinitionViewModel } from "@generated/Models/Core/HCMappedMemberReferenceDefinitionViewModel";
import { HCMappedMemberReferencePathItemDefinitionViewModel } from "@generated/Models/Core/HCMappedMemberReferencePathItemDefinitionViewModel";
import MappedDataDisplayOptions from "@models/modules/MappedData/MappedDataDisplayOptions";

@Options({
    components: {
    }
})
export default class MappedMemberMappingComponent extends Vue {
    @Prop({ required: true })
    mapping!: HCMappedMemberReferenceDefinitionViewModel;

    @Prop({ required: true })
    allDefinitions: HCMappedDataDefinitionsViewModel;

    @Prop({ required: true })
    displayOptions: MappedDataDisplayOptions;
    
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
    getChainItemName(item: HCMappedMemberReferencePathItemDefinitionViewModel): string {
        return this.displayOptions.showMappedToPropertyNames
            ? item.PropertyName || '[not found]'
            : item.DisplayName;
    }
    getItemClasses(item: HCMappedMemberReferencePathItemDefinitionViewModel): any {
        let classes: any = {};
        classes['valid'] = item.Success;
        classes['invalid'] = !item.Success;
        return classes;
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get hasItems(): boolean {
        return this.mapping.Items != null && this.mapping.Items.length > 0;
    }

    get isValid(): boolean {
        return this.mapping.Success && this.mapping.Items.every(x => x.Success);
    }

    get rootClasses(): any {
        let classes: any = {};
        classes['valid'] = this.isValid;
        classes['invalid'] = !this.isValid;
        return classes;
    }

    get rootRefClasses(): any {
        const valid = !this.mapping.Error;
        let classes: any = {};
        classes['valid'] = valid;
        classes['invalid'] = !valid;
        return classes;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    // onDisplayNameClicked(middle: boolean): void {
    //     if (!this.hasLinkToDef) return;
    //     const match = this.allDefinitions.ClassDefinitions.find(x => x.Id == this.def.FullPropertyTypeName);
    //             // || this.allDefinitions.ReferencedDefinitions.some(x => x.TypeName == this.def.FullPropertyTypeName);
    //     if (match == null) return;
    //     this.emitGotoType(match, middle);
    // }

    // emitGotoType(def: HCMappedClassDefinitionViewModel, middle: boolean): void {
    //     this.$emit('gotoTypeClicked', def, middle);
    // }
}
</script>

<style scoped lang="scss">
.mapped-to-items {
    display: flex;
    flex-wrap: wrap;
}
.mapped-to-item, .mapped-to-item-root {
    &:not(:last-child) {
        &::after {
            content: '.';
        }
    }
    &__name {
        font-size: 16px;
    }

    &.valid {
        .mapped-to-item__name {
            color: var(--color--success-darken4)
        }
    }
}
</style>