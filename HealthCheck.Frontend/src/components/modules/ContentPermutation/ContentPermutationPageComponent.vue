<!-- src/components/modules/ContentPermutation/ContentPermutationPageComponent.vue -->
<template>
    <div>
        <!-- NAVIGATION DRAWER -->
        <Teleport to="#module-nav-menu">
            <filterable-list-component
                :items="menuItems"
                :sortByKey="`Name`"
                :hrefKey="`Href`"
                :loading="dataLoadStatus.inProgress"
                :disabled="isLoading"
                :groupIfSingleGroup="false"
                :showFilter="false"
                ref="filterableList"
                v-on:itemClicked="onMenuItemClicked"
                @itemMiddleClicked="onMenuItemMiddleClicked"
                />
        </Teleport>
        
        <div class="content-root">
            <div v-if="currentType">
                <!-- todo: simple string contains filter for props -->
                <!-- todo: frontend grouping -->
                <div>
                </div>

                <h2>{{ currentType.Name }}</h2>
                <p v-if="currentType.Description">{{ currentType.Description }}</p>
                <div v-for="(permutation, pIndex) in currentType.Permutations"
                    :key="`${id}-${permutation.Id}-type-choice-${pIndex}`"
                    @click="onPermutationSelected(permutation)">
                    <h3>Permutation #{{ (permutation.Id+1) }}:</h3>
                    <small v-if="permutation.LastRetrievedCount != null">Latest count: {{ permutation.LastRetrievedCount }}</small>
                    <div v-for="(choice, cIndex) in getChoices(permutation.Choice)"
                        :key="`${id}-${currentType.Id}-type-choice-${cIndex}`">
                        <code>{{ choice.name }}</code>: <code>{{ choice.value }}</code>
                    </div>
                </div>
            </div>

            <code>{{testContent}}</code>
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
import { RouteLocationNormalized } from "vue-router";
import { StoreUtil } from "@util/StoreUtil";
import ContentPermutationService from "@services/ContentPermutationService";
import { HCGetPermutationTypesViewModel } from "@generated/Models/Core/HCGetPermutationTypesViewModel";
import { HCGetPermutatedContentRequest } from "@generated/Models/Core/HCGetPermutatedContentRequest";
import { HCGetPermutatedContentViewModel } from "@generated/Models/Core/HCGetPermutatedContentViewModel";
import { FilterableListItem } from "@components/Common/FilterableListComponent.vue.models";
import HashUtils from "@util/HashUtils";
import UrlUtils from "@util/UrlUtils";
import IdUtils from "@util/IdUtils";
import { HCContentPermutationTypeViewModel } from "@generated/Models/Core/HCContentPermutationTypeViewModel";
import { HCContentPermutationChoiceViewModel } from "@generated/Models/Core/HCContentPermutationChoiceViewModel";
import StringUtils from "@util/StringUtils";

@Options({
    components: {
        FilterableListComponent
    }
})
export default class ContentPermutationPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;
    
    @Ref() readonly filterableList!: FilterableListComponent;

    // Service
    service: ContentPermutationService = new ContentPermutationService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    routeListener: Function | null = null;

    permutationTypes: HCGetPermutationTypesViewModel = {
        Types: []
    };
    currentType: HCContentPermutationTypeViewModel | null = null;
    testContent: HCGetPermutatedContentViewModel | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        StoreUtil.store.commit('showMenuButton', true);
        this.loadPermutationTypes();
        
        await this.$router.isReady();
        this.routeListener = this.$router.afterEach((t, f, err) => this.onRouteChanged(t, f));
    }

    beforeDestroy(): void {
        if (this.routeListener)
        {
            this.routeListener();
        }
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadPermutationTypes(): void {
        this.service.GetPermutationTypes(this.dataLoadStatus, {
            onSuccess: (data) => this.onPermutationTypesRetrieved(data)
        });
    }

    onPermutationTypesRetrieved(data: HCGetPermutationTypesViewModel): void {
        this.permutationTypes = data;
        
        const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.typeId) || null;
        if (this.permutationTypes)
        {
            const matchingType = this.permutationTypes.Types.filter(x => this.hash(x.Id) == idFromHash)[0];
            if (matchingType) {
                this.setActiveType(matchingType, false);
            } else if (this.permutationTypes.Types.length > 0) {
                this.setActiveType(this.permutationTypes.Types[0]);   
            }
        }
    }

    loadContentFor(typeId: string, choiceId: number): void {
        const payload: HCGetPermutatedContentRequest = {
            MaxCount: 5,
            PermutationTypeId: typeId,
            PermutationChoiceId: choiceId
        };
        this.service.GetPermutatedContent(payload, this.dataLoadStatus, {
            onSuccess: (data) => this.onContentLoadedFor(typeId, choiceId, data)
        });
    }

    onContentLoadedFor(typeId: string, choiceId: number, data: HCGetPermutatedContentViewModel): void {
        this.testContent = data;
        const type = this.permutationTypes.Types.find(x => x.Id == typeId);
        const choice = type?.Permutations?.find(x => x.Id == choiceId);
        if (choice) {
            choice.LastRetrievedCount = data.Content.length;
        }
    }

    setActiveType(type: HCContentPermutationTypeViewModel, updateUrl: boolean = true): void {
        this.currentType = type;
        
        (this.filterableList as FilterableListComponent).setSelectedItem(type);

        const idHash = this.hash(type.Id);
        if (updateUrl && StringUtils.stringOrFirstOfArray(this.$route.params.typeId) != StringUtils.stringOrFirstOfArray(idHash))
        {
            this.$router.push(`/contentPermutation/${idHash}`);
        }
    }

    getChoices(choiceObj: any): Array<any> {
        if (choiceObj == null) return [];
        return Object.keys(choiceObj).map(x => ({
            name: this.getPropertyName(x),
            description: this.getPropertyDescription(x),
            value: choiceObj[x]
        }));
    }

    getPropertyName(name: string) : string {
        return this.currentType?.PropertyDetails[name]?.DisplayName || name;
    }

    getPropertyDescription(name: string) : string {
        return this.currentType?.PropertyDetails[name]?.Description || '';
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
            const route = `#/contentPermutation/${idHash}`;
            UrlUtils.openRouteInNewTab(route);
        }
    }

    onPermutationSelected(permutation: HCContentPermutationChoiceViewModel): void {
        this.loadContentFor(this.currentType.Id, permutation.Id);
    }
    
    hash(input: string) { return HashUtils.md5(input); }

    onRouteChanged(to: RouteLocationNormalized, from: RouteLocationNormalized): void {
        if (!this.permutationTypes || !to.path.toLowerCase().startsWith('/contentPermutation/')) return;

        const oldTypeIdFromHash = StringUtils.stringOrFirstOfArray(from.params.typeId) || null;
        const newTypeIdFromHash = StringUtils.stringOrFirstOfArray(to.params.typeId) || null;
        const typeChanged = oldTypeIdFromHash != newTypeIdFromHash;

        if (typeChanged)
        {
            const matchingStream = this.permutationTypes.Types.filter(x => this.hash(x.Id) == newTypeIdFromHash)[0] || null;
            this.setActiveType(matchingStream, false);
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
        const types = this.permutationTypes?.Types;
        if (!types) return [];
        return types.map(x => {
            let d = {
                title: x.Name,
                subtitle: '',
                data: x
            };
            (<any>d)['Href'] = "/";
            return d;
        });
    }
}
</script>

<style scoped lang="scss">
</style>
