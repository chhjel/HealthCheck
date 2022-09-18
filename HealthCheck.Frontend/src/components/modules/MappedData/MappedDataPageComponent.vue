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

            <!-- Todo: show example values if any -->

            <!-- CONTENT -->
            <div v-if="currentDef">
                <div class="filters mb-4" v-if="currentDefHasAnyFilterableThings">
                    <!-- <h3>Included data</h3> -->
                    <div class="filters__inputs">
                        <select-component
                            label="Properties"
                            v-if="currentDefHasAnyNamedProperties"
                            v-model:value="displayOptions.showPropertyNames"
                            @change="onFilterChanged"
                            :items="propertyNameOptions"
                            class="mr-3"></select-component>
                        <select-component
                            label="Mapped properties"
                            v-if="currentDefHasAnyNamedMappings"
                            v-model:value="displayOptions.showMappedToPropertyNames"
                            @change="onFilterChanged"
                            :items="propertyNameOptions"
                            class="mr-3"></select-component>
                        <checkbox-component
                            label="Show comments"
                            v-if="currentDefHasAnyComments"
                            v-model:value="displayOptions.showPropertyRemarks"
                            @change="onFilterChanged"
                            class="mr-3" />
                        <checkbox-component
                            label="Example values"
                            v-if="currentDefHasExampleValues"
                            v-model:value="displayOptions.showExampleValues"
                            @change="onFilterChanged"
                            class="mr-3" />
                    </div>
                </div>

                <mapped-class-definition-component
                    :def="currentDef"
                    :allDefinitions="definitions"
                    :exampleData="exampleDataForCurrentDef"
                    :displayOptions="displayOptions"
                    @gotoData="gotoData" />
            </div>
        </div>

        <!-- DIALOGS -->
        <dialog-component v-model:value="refTypeDialogVisible"
            max-width="620"
            :persistent="isLoading">
            <template #header>{{ refDefInDialog?.DisplayName }}</template>
            <template #footer>
                <btn-component color="secondary"
                    :disabled="isLoading"
                    :loading="isLoading"
                    @click="refTypeDialogVisible = false">Cancel</btn-component>
            </template>
            <div>
                <!-- NO PRESETS YET -->
                <div v-if="refDefInDialog">
                    <p>{{ refDefInDialog.Remarks }}</p>
                </div>
            </div>
        </dialog-component>
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
import MappedDataLinkData from "@models/modules/MappedData/MappedDataLinkData";
import { HCMappedReferencedTypeDefinitionViewModel } from "@generated/Models/Core/HCMappedReferencedTypeDefinitionViewModel";
import { HCMappedMemberDefinitionViewModel } from "@generated/Models/Core/HCMappedMemberDefinitionViewModel";
import { HCMappedMemberReferenceDefinitionViewModel } from "@generated/Models/Core/HCMappedMemberReferenceDefinitionViewModel";
import { HCMappedExampleValueViewModel } from "@generated/Models/Core/HCMappedExampleValueViewModel";

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
    exampleData: Array<HCMappedExampleValueViewModel> = [];
    currentDef: HCMappedClassDefinitionViewModel | null = null;
    displayOptions: MappedDataDisplayOptions = {
        showPropertyNames: "serialized",
        showPropertyRemarks: true,
        showMappedToTypes: false,
        showMappedToDeclaringTypes: false,
        showMappedToPropertyNames: "serialized",
        showExampleValues: true
    };
    refTypeDialogVisible: boolean = false;
    refDefInDialog: HCMappedReferencedTypeDefinitionViewModel | null = null;
    propertyNameOptions: Array<any> = [
        { value: 'actual', text: 'Property names' },
        { value: 'serialized', text: 'Serialized names' }
    ];

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

        this.service.GetExampleValues(this.dataLoadStatus, {
            onSuccess: (data) => this.onExamplesRetrieved(data)
        });
    }

    onExamplesRetrieved(data: Array<HCMappedExampleValueViewModel>): void {
        this.exampleData = data;
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

    showRefDefDialog(def: HCMappedReferencedTypeDefinitionViewModel): void {
        this.refDefInDialog = def;
        this.refTypeDialogVisible = true;
    }

    memberPropNameAndDisplayNamesMatches(m: HCMappedMemberDefinitionViewModel): boolean {
        return m.PropertyName == m.DisplayName
            && m.Children.every(c => this.memberPropNameAndDisplayNamesMatches(c));
    }

    memberMappingPropNameAndDisplayNamesMatches(m: HCMappedMemberDefinitionViewModel): boolean {
        return m.MappedTo.every(t => this.memberMappingMatches(t));
    }
    memberMappingMatches(m: HCMappedMemberReferenceDefinitionViewModel): boolean {
        return m.RootTypeName == m.RootReferenceId
            && m.Items.every(i => i.PropertyName == i.DisplayName)
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

    get currentDefHasAnyFilterableThings(): boolean {
        return this.currentDefHasAnyComments
        || this.currentDefHasAnyNamedProperties
        || this.currentDefHasAnyNamedMappings;
    }

    get currentDefHasAnyComments(): boolean {
        if (!this.currentDef) return false;
        return this.currentDef.MemberDefinitions.some(x => x.Remarks != null && x.Remarks.trim().length > 0);
    }

    get currentDefHasAnyNamedProperties(): boolean {
        if (!this.currentDef) return false;
        return this.currentDef.MemberDefinitions.some(x => !this.memberPropNameAndDisplayNamesMatches(x));
    }

    get currentDefHasAnyNamedMappings(): boolean {
        if (!this.currentDef) return false;
        return this.currentDef.MemberDefinitions.some(x => !this.memberMappingPropNameAndDisplayNamesMatches(x));
    }

    get exampleDataForCurrentDef(): HCMappedExampleValueViewModel {
        return this.exampleData.find(x => x.DataTypeName == this.currentDef?.ClassTypeName);
    }

    get currentDefHasExampleValues(): boolean {
        return this.exampleDataForCurrentDef != null;
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

    gotoData(data: MappedDataLinkData): void {
        if (data.type == "ClassDefinition") {
            const def = this.definitions.ClassDefinitions.find(x => x.Id == data.id);
            if (def) this.gotoTypeClicked(def, data.newWindow);
        }
        else if (data.type == "ReferencedDefinition") {
            const def = this.definitions.ReferencedDefinitions.find(x => x.Id == data.id);
            if (def.Remarks) this.showRefDefDialog(def);
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
