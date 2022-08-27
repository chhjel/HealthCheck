<!-- src/components/modules/Comparison/ComparisonPageComponent.vue -->
<template>
    <div>
        <div class="content-root comparison-module">
            <div class="type-selection-section">
                <div v-if="hasSelectedContentType">
                    <h2>1. {{ selectedContentType.name }}-comparison</h2>
                    <code>{{ selectedContentType }}</code>
                    <div @click="setContentType(null)">[Clear]</div>
                </div>
                <div v-if="!hasSelectedContentType">
                    <h2>What do you want to compare?</h2>
                    <div v-for="(contentType, cIndex) in contentTypeSelection"
                        :key="`${id}-${contentType.id}-contentType-${cIndex}`"
                        :class="contentTypeClasses(contentType)"
                        @click="setContentType(contentType)">
                        {{ contentType.name }} - {{ contentType.description }}
                    </div>
                </div>
            </div>

            <div class="instance-selection-section fadein mt-4"
                v-if="hasSelectedContentType">
                <h2>2. Select data to compare</h2>

                <div class="mt-2">
                    <div @click="selectDummyData">Todo: huge buttons, click for dialog w/ search</div>
                </div>
            </div>

            <div class="difftype-selection-section fadein mt-4"
                v-if="hasSelectedInstances">
                <h2>3. Select what comparisons to include</h2>
                <div>
                    <div v-for="(diff, dIndex) in diffTypeSelection"
                        :key="`${id}-${diff.id}-diff-${dIndex}`"
                        :class="diffTypeClasses(diff)"
                        @click="toggleDiff(diff)">
                        {{ diff.name }} - {{ diff.description }}
                    </div>
                    <code>{{ selectedDiffTypes }}</code>
                </div>
            </div>

            <div class="output-execution-section fadein mt-4"
                v-if="showOutputExecution">
                <btn-component large>Compare</btn-component>
            </div>

            <!-- LOAD PROGRESS -->
            <progress-linear-component 
                v-if="isLoading"
                indeterminate color="success"></progress-linear-component>

            <!-- DATA LOAD ERROR -->
            <alert-component :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
            {{ dataLoadStatus.errorMessage }}
            </alert-component>

            <div class="output-section fadein mt-4"
                v-if="showOutput">
                <h2>Result</h2>
                <diff-component
                    class="codeeditor codeeditor__input"
                    :allowFullscreen="true"
                    originalName="Test Left"
                    :originalContent="originalContent"
                    modifiedName="Test Right"
                    :modifiedContent="modifiedContent"
                    :readOnly="true"
                    />
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
import { RouteLocationNormalized } from "vue-router";
import { StoreUtil } from "@util/StoreUtil";
import ComparisonService from "@services/ComparisonService";
import { FilterableListItem } from "@components/Common/FilterableListComponent.vue.models";
import HashUtils from "@util/HashUtils";
import UrlUtils from "@util/UrlUtils";
import IdUtils from "@util/IdUtils";
import StringUtils from "@util/StringUtils";
import { HCBackendInputConfig } from "@generated/Models/Core/HCBackendInputConfig";
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import DiffComponent from "@components/Common/DiffComponent.vue";

interface ContentType {
    id: string;
    name: string;
    description: string;
}
interface DiffType {
    id: string;
    name: string;
    description: string;
}
interface InstanceSelection {
    id: string;
    name: string;
    description: string;
}
@Options({
    components: {
        FilterableListComponent,
        DiffComponent,
        BackendInputComponent
    }
})
export default class ComparisonPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;
    
    @Ref() readonly filterableList!: FilterableListComponent;

    // Service
    service: ComparisonService = new ComparisonService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    typesLoadStatus: FetchStatus = new FetchStatus();
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    routeListener: Function | null = null;

    selectedContentType: ContentType | null = null;
    selectedDiffTypes: Array<DiffType> = [];
    selectedInstances: Array<InstanceSelection> = [];

    originalContent: string = "{\n\t\"a\": true\n}";
    modifiedContent: string = "{\n\t\"a\": false,\n\t\"b\": \"nope\"\n}";

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        // StoreUtil.store.commit('showMenuButton', true);
        this.loadComparisonTypes();
        
        await this.$router.isReady();
        this.routeListener = this.$router.afterEach((t, f, err) => this.onRouteChanged(t, f));

        this.setContentType(this.contentTypeSelection[0]);
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
    selectDummyData(): void {
        this.selectedInstances = [
            { id: "22", name: 'Item 22', description: 'Some description here.' },
            { id: "88", name: 'Item 88', description: 'Some description here.' },
        ];
    }

    loadComparisonTypes(): void {
        // this.service.GetPermutationTypes(this.dataLoadStatus, {
        //     onSuccess: (data) => this.onComparisonTypesRetrieved(data)
        // });
    }

    // onComparisonTypesRetrieved(data: HCGetPermutationTypesViewModel): void {
    //     this.permutationTypes = data;
        
    //     const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.typeId) || null;
    //     if (this.permutationTypes)
    //     {
    //         const matchingType = this.permutationTypes.Types.filter(x => this.hash(x.Id) == idFromHash)[0];
    //         if (matchingType) {
    //             this.setActiveType(matchingType, false);
    //         } else if (this.permutationTypes.Types.length > 0) {
    //             this.setActiveType(this.permutationTypes.Types[0]);   
    //         }
    //     }
    // }

    // setActiveType(type: HCContentPermutationTypeViewModel, updateUrl: boolean = true): void {
    //     this.currentType = type;
    //     this.currentPermutation = null;
    //     this.resetFilter();
    //     this.exampleContent = null;
    //     (this.filterableList as FilterableListComponent).setSelectedItem(type);

    //     const firstPermutation = type.Permutations[0];
    //     this.filterInputs = [];
    //     if (firstPermutation) {
    //         const allowedFilterInputKeys = Object.keys(firstPermutation.Choice);
    //         this.filterInputs = this.currentType.PropertyConfigs
    //             .filter(x => allowedFilterInputKeys.includes(x.Id));
    //     }
    //     this.contentCountToRequest = type.DefaultContentCount;

    //     const idHash = this.hash(type.Id);
    //     if (updateUrl && StringUtils.stringOrFirstOfArray(this.$route.params.typeId) != StringUtils.stringOrFirstOfArray(idHash))
    //     {
    //         this.$router.push(`/contentPermutation/${idHash}`);
    //     }
    // }

    hash(input: string) { return HashUtils.md5(input); }

    diffTypeClasses(diff: DiffType): any {
        let classes = {};
        if (this.selectedDiffTypes.some(x => x.id == diff.id)) {
            classes['active'] = true;
        }
        return classes;
    }

    contentTypeClasses(type: ContentType): any {
        let classes = {};
        if (this.selectedContentType?.id == type.id) {
            classes['active'] = true;
        }
        return classes;
    }

    toggleDiff(diff: DiffType): void {
        const selected = this.selectedDiffTypes.some(x => x.id == diff.id);
        if (selected) {
            this.selectedDiffTypes = this.selectedDiffTypes.filter(x => x.id != diff.id);
        }
        else {
            this.selectedDiffTypes.push(diff);
        }
    }

    setContentType(type: ContentType | null): void {
        this.selectedContentType = type;
        this.selectedDiffTypes = Array.from(this.diffTypeSelection);
        this.selectedInstances = [];
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onMenuItemClicked(item: FilterableListItem): void {
        // this.setActiveType(item.data);
    }

    onMenuItemMiddleClicked(item: FilterableListItem): void {
        if (item && item.data && item.data.Id)
        {
            const idHash = this.hash(item.data.Id);
            const route = `#/comparison/${idHash}`;
            UrlUtils.openRouteInNewTab(route);
        }
    }
    
    onRouteChanged(to: RouteLocationNormalized, from: RouteLocationNormalized): void {
    //     if (!this.permutationTypes || !to.path.toLowerCase().startsWith('/comparison/')) return;

    //     const oldTypeIdFromHash = StringUtils.stringOrFirstOfArray(from.params.typeId) || null;
    //     const newTypeIdFromHash = StringUtils.stringOrFirstOfArray(to.params.typeId) || null;
    //     const typeChanged = oldTypeIdFromHash != newTypeIdFromHash;

    //     if (typeChanged)
    //     {
    //         const matchingStream = this.permutationTypes.Types.filter(x => this.hash(x.Id) == newTypeIdFromHash)[0] || null;
    //         this.setActiveType(matchingStream, false);
    //     }
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get isLoading(): boolean {
        return this.dataLoadStatus.inProgress || this.typesLoadStatus.inProgress;
    }

    get hasSelectedContentType(): boolean {
        return !!this.selectedContentType;
    }

    get hasSelectedInstances(): boolean {
        return this.selectedInstances.length == 2;
    }

    get hasSelectedAnyDiffType(): boolean {
        return this.selectedDiffTypes.length > 0;
    }

    get showOutputExecution(): boolean {
        return this.hasSelectedContentType
            && this.hasSelectedAnyDiffType
            && this.hasSelectedInstances;
    }

    get showOutput(): boolean {
        return this.hasSelectedInstances;
    }

    get contentTypeSelection(): Array<ContentType> {
        return [
            { id: 'FindProduct', name: 'Product', description: 'Some desc.' },
            { id: 'PurchaseOrder', name: 'Order', description: ''  },
        ];
    }

    get diffTypeSelection(): Array<DiffType> {
        return [
            { id: 'JsonDiff', name: 'Raw Json Diff', description: 'Some desc.' },
            { id: 'ExclusionCheck', name: 'Mutual Exclusions', description: '' },
        ];
    }
}
</script>

<style scoped lang="scss">
.comparison-module {
    text-align: center;

    .codeeditor {
        box-shadow: 0 2px 4px 1px rgba(0, 0, 0, 0.15), 0 3px 2px 0 rgba(0,0,0,.02), 0 1px 2px 0 rgba(0,0,0,.06);

        &__input {
            position: relative;
            //           max   toolbar  output  other
            height: calc(100vh - 47px - 30vh - 109px);
        }

        &__output {
            height: 30vh;
        }
    }

    .fadein {
        animation: fade-in .3s ease-in-out;
    }
}

@keyframes fade-in {
  0% {
    display: none;
    opacity: 0;
  }
  1% {
    display: block;
    opacity: 0;
  }
  100% {
    opacity: 1;
  }
}
</style>
