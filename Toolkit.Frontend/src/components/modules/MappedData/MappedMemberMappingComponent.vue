<!-- src/components/modules/MappedData/MappedMemberMappingComponent.vue -->
<template>
    <div :class="rootClasses">
        <div v-if="mapping.Error"><code>Error: {{ mapping.Error }}</code><br /></div>

        <div v-if="hasItems" class="mapped-to-items">
            <div class="mapped-to-item-root" :class="rootRefClasses" v-if="rootReferenceNameInMapping">
                <code class="mapped-to-item__name"
                    @click="onMappedNameClicked(mapping.RootReferenceId, false)"
                    @click.middle.stop.prevent="onMappedNameClicked(mapping.RootReferenceId, true)"
                    :title="`${mapping.RootTypeName}`"
                    >{{ rootReferenceNameInMapping }}</code>
            </div>

            <div v-for="(chainItem, chainItemIndex) in mapping.Items"
                 :key="`def-mapping-item-${id}-${chainItemIndex}`"
                 :class="getItemClasses(chainItem)"
                 class="mapped-to-item">
                <code class="mapped-to-item__name"
                    :title="getChainItemTooltip(chainItem)"
                    >{{ getChainItemName(chainItem) }}</code>
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
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import IdUtils from "@util/IdUtils";
import { TKMappedDataDefinitionsViewModel } from "@generated/Models/Core/TKMappedDataDefinitionsViewModel";
import { TKMappedMemberReferenceDefinitionViewModel } from "@generated/Models/Core/TKMappedMemberReferenceDefinitionViewModel";
import { TKMappedMemberReferencePathItemDefinitionViewModel } from "@generated/Models/Core/TKMappedMemberReferencePathItemDefinitionViewModel";
import MappedDataDisplayOptions from "@models/modules/MappedData/MappedDataDisplayOptions";
import MappedDataLinkData from "@models/modules/MappedData/MappedDataLinkData";

@Options({
    components: {
    }
})
export default class MappedMemberMappingComponent extends Vue {
    @Prop({ required: true })
    mapping!: TKMappedMemberReferenceDefinitionViewModel;

    @Prop({ required: true })
    allDefinitions: TKMappedDataDefinitionsViewModel;

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
    getChainItemTooltip(item: TKMappedMemberReferencePathItemDefinitionViewModel): string {
        if (item.IsHardCoded) return 'Hardcoded value';
        return item.FullPropertyTypeName || 'Property not found';
    }

    getChainItemName(item: TKMappedMemberReferencePathItemDefinitionViewModel): string {
        if (item.IsHardCoded) return `'${item.HardCodedValue}'`;
        return (this.displayOptions.showMappedToPropertyNames == 'actual')
            ? item.PropertyName || '[not found]'
            : item.DisplayName;
    }

    getItemClasses(item: TKMappedMemberReferencePathItemDefinitionViewModel): any {
        let classes: any = {};
        classes['valid'] = item.Success;
        classes['invalid'] = !item.Success;
        classes['hardcoded'] = item.IsHardCoded;
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
        classes['hasLinkToDef'] = this.hasRootRefComment;
        return classes;
    }

    get rootReferenceNameInMapping(): string {
        const def = this.allDefinitions.ReferencedDefinitions.find(x => x.ReferenceId == this.mapping.RootReferenceId);
        return def?.NameInMapping || this.mapping.RootReferenceId;
    }

    get hasRootRefComment(): boolean {
        const def = this.allDefinitions.ReferencedDefinitions.find(x => x.ReferenceId == this.mapping.RootReferenceId);
        return def?.Remarks != null && def.Remarks.trim().length > 0;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onMappedNameClicked(name: string, middle: boolean): void {
        const match = this.allDefinitions.ReferencedDefinitions.find(x => x.ReferenceId == name);
        if (match == null) return;

        const payload: MappedDataLinkData = {
            id: match.Id,
            type: "ReferencedDefinition",
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
    &.hardcoded {
        .mapped-to-item__name {
            color: var(--color--primary-base)
        }
    }
}
.hasLinkToDef {
    cursor: pointer;
}
</style>