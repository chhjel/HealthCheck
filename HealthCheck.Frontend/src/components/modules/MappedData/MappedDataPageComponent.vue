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
                <div class="leftright-container">
                    <mapped-class-definition-component :def="currentPair.Left" class="left-side" />
                    <div class="middle-section"></div>
                    <mapped-class-definition-component :def="currentPair.Right" class="right-side" />
                </div>
                <div class="members mt-3">
                    <h3 style="text-align:center">Properties</h3>
                    <div style="text-align:center">
                        <btn-component @click="showDetails = !showDetails" class="show-details-btn" flat>Show more details</btn-component>
                    </div>
                    <mapped-member-definition-pair-component
                        v-for="(mDefPair, pIndex) in currentPair.MemberPairs"
                        :key="`member-${id}-${mDefPair.Left.Id}-${pIndex}`"
                        :def="mDefPair"
                        :showDetails="showDetails"
                        :allDefinitions="definitions"
                        @gotoTypeClicked="gotoTypeClicked" />
                </div>
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
import MappedMemberDefinitionPairComponent from "./MappedMemberDefinitionPairComponent.vue";
import { HCMappedClassDefinitionViewModel } from "@generated/Models/Core/HCMappedClassDefinitionViewModel";
import { RouteLocationNormalized } from "vue-router";

@Options({
    components: {
        FilterableListComponent,
        MappedClassDefinitionComponent,
        MappedMemberDefinitionPairComponent
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
    showDetails: boolean = false;

    id: string = IdUtils.generateId();
    routeListener: Function | null = null;
    definitions: Array<HCMappedClassesDefinitionViewModel> = [];
    currentPair: HCMappedClassesDefinitionViewModel | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        StoreUtil.store.commit('showMenuButton', true);

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
    
    onRouteChanged(to: RouteLocationNormalized, from: RouteLocationNormalized): void {
        if (!this.definitions || !to.path.toLowerCase().startsWith('/mappeddata/')) return;

        const oldTypeIdFromHash = StringUtils.stringOrFirstOfArray(from.params.typeId) || null;
        const newTypeIdFromHash = StringUtils.stringOrFirstOfArray(to.params.typeId) || null;
        const typeChanged = oldTypeIdFromHash != newTypeIdFromHash;

        if (typeChanged)
        {
            const matchingStream = this.definitions.filter(x => this.hash(x.Left.Id) == newTypeIdFromHash)[0] || null;
            this.setActiveType(matchingStream, false);
        }
    }

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
            let name = x.Left?.DisplayName == x.Right?.DisplayName
                ? x.Left?.DisplayName
                : `${x.Left?.DisplayName} ↔️ ${x.Right?.DisplayName}`;
            return {
                title: name,
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
    
    gotoTypeClicked(def: HCMappedClassesDefinitionViewModel, middle: boolean): void {
        if (middle) {
            const idHash = this.hash(def.Left.Id);
            const route = `#/mappeddata/${idHash}`;
            UrlUtils.openRouteInNewTab(route);
        } else {
            this.setActiveType(def);
        }
    }
}
</script>

<style scoped lang="scss">
.mapped-data {
    .show-details-btn {
        font-size: 12px;
    }
}
</style>

<style lang="scss">
.mapped-data {
    .leftright-container {
        display: flex;
        flex-wrap: nowrap;
        justify-content: center;
        .left-side {
            padding-right: 10px;
            flex: 1;
            text-align: right;
        }
        .right-side {
            padding-left: 10px;
            flex: 1;
        }
        .middle-section {
            width: 20px;
            display: flex;
            justify-content: center;
            align-items: center;

            &.bordered {
                width: 0;
                border-right: 2px solid var(--color--accent-darken1);
                box-sizing: border-box;
                margin-left: 9px;
                margin-right: 9px;
            }
        }
        .center-vertically {
            display: flex;
            flex-direction: column;
            justify-content: center;
        }
    }
}
</style>