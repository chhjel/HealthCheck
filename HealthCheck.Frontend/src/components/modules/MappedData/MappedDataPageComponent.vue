<!-- src/components/modules/MappedData/MappedDataPageComponent.vue -->
<template>
    <div class="mapped-data">
        <!-- NAVIGATION DRAWER -->
        <Teleport to="#module-nav-menu">
            <filterable-list-component 
                :items="menuItems"
                :groupByKey="`GroupName`"
                :sortByKey="`GroupName`"
                :showFilter="false"
                :loading="isLoading"
                :disabled="isLoading"
                ref="filterableList"
                v-on:itemClicked="onMenuItemClicked"
                @itemMiddleClicked="onMenuItemMiddleClicked"
                />
        </Teleport>

        <div class="content-root">
            <!-- LOAD PROGRESS -->
            <progress-linear-component 
                v-if="isLoading"
                indeterminate color="success"></progress-linear-component>

            <!-- DATA LOAD ERROR -->
            <alert-component :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
            {{ dataLoadStatus.errorMessage }}
            </alert-component>

            <!-- CONTENT -->
            <div v-if="currentPair">
                <h2>Left</h2>
                <mapped-class-definition-component :def="currentPair.Left" />
                <h2>Right</h2>
                <mapped-class-definition-component :def="currentPair.Right" />
                <h2>Members</h2>
                <mapped-member-definition-component 
                    v-for="(mDef, mIndex) in currentPair.MemberPairs"
                    :key="`member-${id}-${mDef.Left.Id}-${mIndex}`"
                    :def="mDef" />
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import FilterableListComponent from '@components/Common/FilterableListComponent.vue';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import { StoreUtil } from "@util/StoreUtil";
import MappedDataService from "@services/MappedDataService";
import IdUtils from "@util/IdUtils";
import { HCMappedClassesDefinitionViewModel } from "@generated/Models/Core/HCMappedClassesDefinitionViewModel";
import StringUtils from "@util/StringUtils";
import HashUtils from "@util/HashUtils";
import { FilterableListItem } from "@components/Common/FilterableListComponent.vue.models";
import UrlUtils from "@util/UrlUtils";
import MappedClassDefinitionComponent from "./MappedClassDefinitionComponent.vue";
import MappedMemberDefinitionComponent from "./MappedMemberDefinitionComponent.vue";

@Options({
    components: {
        FilterableListComponent,
        MappedClassDefinitionComponent,
        MappedMemberDefinitionComponent
    }
})
export default class MappedDataPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;
    
    @Ref() readonly filterableList!: FilterableListComponent;

    // Service
    service: MappedDataService = new MappedDataService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    definitions: Array<HCMappedClassesDefinitionViewModel> = [];
    currentPair: HCMappedClassesDefinitionViewModel | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        StoreUtil.store.commit('showMenuButton', true);

        this.loadData();
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.service.GetDefinitions(this.dataLoadStatus, {
            onSuccess: (data) => this.onDefinitionsRetrieved(data)
        });
    }

    onDefinitionsRetrieved(data: Array<HCMappedClassesDefinitionViewModel>): void {
        this.definitions = data;
        
        const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.typeId) || null;
        if (this.definitions)
        {
            const matchingType = this.definitions.filter(x => this.hash(x.Left.Id) == idFromHash)[0];
            if (matchingType) {
                this.setActiveType(matchingType, false);
            } else if (this.definitions.length > 0) {
                this.setActiveType(this.definitions[0]);   
            }
        }
    }
    
    hash(input: string) { return HashUtils.md5(input); }

    setActiveType(type: HCMappedClassesDefinitionViewModel, updateUrl: boolean = true): void {
        this.currentPair = type;
        (this.filterableList as FilterableListComponent).setSelectedItem(type);

        const idHash = this.hash(type.Left.Id);
        if (updateUrl && StringUtils.stringOrFirstOfArray(this.$route.params.typeId) != StringUtils.stringOrFirstOfArray(idHash))
        {
            this.$router.push(`/mappeddata/${idHash}`);
        }
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get isLoading(): boolean {
        return this.dataLoadStatus.inProgress;
    }
    
    get menuItems(): Array<FilterableListItem>
    {
        return this.definitions.map(x => {
            return {
                title: `${x.Left?.DisplayName} ↔️ ${x.Right?.DisplayName}`,
                subtitle: '',
                data: x
            };
        });
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onMenuItemClicked(item: FilterableListItem): void {
        this.setActiveType(item.data);
    }

    onMenuItemMiddleClicked(item: FilterableListItem): void {
        if (item && item.data && item.data.Id)
        {
            const idHash = this.hash(item.data.Id);
            const route = `#/mappeddata/${idHash}`;
            UrlUtils.openRouteInNewTab(route);
        }
    }
}
</script>

<style scoped lang="scss">
.mapped-data {

}
</style>