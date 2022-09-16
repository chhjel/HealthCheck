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
            <div v-if="currentDef">
                <div class="filters mb-4">
                    <h3>Included data</h3>
                    <div class="filters__inputs">
                        <checkbox-component
                            label="Property names"
                            v-model:value="displayOptions.showPropertyNames"
                            @change="onFilterChanged"
                            :disabled="isLoading"
                            class="mr-3" />
                        <checkbox-component
                            label="Property comments"
                            v-model:value="displayOptions.showPropertyRemarks"
                            @change="onFilterChanged"
                            :disabled="isLoading"
                            class="mr-3" />
                        <checkbox-component
                            label="Mapped types"
                            v-model:value="displayOptions.showMappedToTypes"
                            @change="onFilterChanged"
                            :disabled="isLoading"
                            class="mr-3" />
                        <checkbox-component
                            label="Mapped declaring types"
                            v-model:value="displayOptions.showMappedToDeclaringTypes"
                            @change="onFilterChanged"
                            :disabled="isLoading"
                            class="mr-3" />
                        <checkbox-component
                            label="Mapped property names"
                            v-model:value="displayOptions.showMappedToPropertyNames"
                            @change="onFilterChanged"
                            :disabled="isLoading"
                            class="mr-3" />
                    </div>
                </div>

                <mapped-class-definition-component
                    :def="currentDef"
                    :allDefinitions="definitions"
                    :displayOptions="displayOptions" />
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
import StringUtils from "@util/StringUtils";
import HashUtils from "@util/HashUtils";
import { FilterableListItem } from "@components/Common/FilterableListComponent.vue.models";
import UrlUtils from "@util/UrlUtils";
import MappedClassDefinitionComponent from "./MappedClassDefinitionComponent.vue";
import { RouteLocationNormalized } from "vue-router";
import { HCMappedDataDefinitionsViewModel } from "@generated/Models/Core/HCMappedDataDefinitionsViewModel";
import { HCMappedClassDefinitionViewModel } from "@generated/Models/Core/HCMappedClassDefinitionViewModel";
import MappedDataDisplayOptions from "@models/modules/MappedData/MappedDataDisplayOptions";

@Options({
    components: {
        FilterableListComponent,
        MappedClassDefinitionComponent
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
    routeListener: Function | null = null;
    definitions: HCMappedDataDefinitionsViewModel = {
        ClassDefinitions: [],
        ReferencedDefinitions: []
    };
    currentDef: HCMappedClassDefinitionViewModel | null = null;
    displayOptions: MappedDataDisplayOptions = {
        showPropertyNames: false,
        showPropertyRemarks: true,
        showMappedToTypes: false,
        showMappedToDeclaringTypes: false,
        showMappedToPropertyNames: false
    };

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        StoreUtil.store.commit('showMenuButton', true);
        
        const localStorageOptsJson = localStorage.getItem('__hc_mapping_displayOpts');
        if (localStorageOptsJson) try { this.displayOptions = JSON.parse(localStorageOptsJson) } catch {};

        this.loadData();
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
    loadData(): void {
        this.service.GetDefinitions(this.dataLoadStatus, {
            onSuccess: (data) => this.onDefinitionsRetrieved(data)
        });
    }

    onDefinitionsRetrieved(data: HCMappedDataDefinitionsViewModel): void {
        this.definitions = data;
        
        const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.typeId) || null;
        if (this.definitions)
        {
            const matchingType = this.definitions.ClassDefinitions.filter(x => this.hash(x.Id) == idFromHash)[0];
            if (matchingType) {
                this.setActiveType(matchingType, false);
            } else if (this.definitions.ClassDefinitions.length > 0) {
                this.setActiveType(this.definitions.ClassDefinitions[0]);   
            }
        }
    }
    
    hash(input: string) { return HashUtils.md5(input); }
    
    onRouteChanged(to: RouteLocationNormalized, from: RouteLocationNormalized): void {
        if (!this.definitions || !to.path.toLowerCase().startsWith('/mappeddata/')) return;

        const oldTypeIdFromHash = StringUtils.stringOrFirstOfArray(from.params.typeId) || null;
        const newTypeIdFromHash = StringUtils.stringOrFirstOfArray(to.params.typeId) || null;
        const typeChanged = oldTypeIdFromHash != newTypeIdFromHash;

        if (typeChanged)
        {
            const matchingStream = this.definitions.ClassDefinitions.filter(x => this.hash(x.Id) == newTypeIdFromHash)[0] || null;
            this.setActiveType(matchingStream, false);
        }
    }

    setActiveType(type: HCMappedClassDefinitionViewModel, updateUrl: boolean = true): void {
        this.currentDef = type;
        (this.filterableList as FilterableListComponent).setSelectedItem(type);

        const idHash = this.hash(type.Id);
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
        return this.definitions.ClassDefinitions.map(x => {
            return {
                title: x.DisplayName,
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
    
    gotoTypeClicked(def: HCMappedClassDefinitionViewModel, middle: boolean): void {
        if (middle) {
            const idHash = this.hash(def.Id);
            const route = `#/mappeddata/${idHash}`;
            UrlUtils.openRouteInNewTab(route);
        } else {
            this.setActiveType(def);
        }
    }

    onFilterChanged(): void {
        localStorage.setItem('__hc_mapping_displayOpts', JSON.stringify(this.displayOptions));
    }
}
</script>

<style scoped lang="scss">
.filters {
    background-color: var(--color--accent-base);
    border: 1px solid var(--color--accent-darken1);
    padding: 10px;
    &__inputs {
        display: flex;
        flex-wrap: wrap;
    }
}
</style>
