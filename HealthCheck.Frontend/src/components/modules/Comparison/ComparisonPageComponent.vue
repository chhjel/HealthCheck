<!-- src/components/modules/Comparison/ComparisonPageComponent.vue -->
<template>
    <div>
        <div class="content-root comparison-module">
            <!-- CONTENT TYPE SELECTION -->
            <div class="type-selection-section">
                <div v-if="hasSelectedContentType"
                    class="selected-contentType">
                    <a class="selected-contentType__cancelLink hoverable-light mb-4"
                       @click.stop.prevent="setContentType(null)"
                       href="#">&lt;&lt; select another type</a>
                    <h1 class="selected-contentType__title">{{ selectedContentType.name }}-comparison</h1>
                    <div class="selected-contentType__description"
                        v-if="selectedContentType.description">{{ selectedContentType.description }}</div>
                </div>
                <div v-if="!hasSelectedContentType">
                    <h1>What do you want to compare?</h1>
                    <div class="type-selection-blocks">
                        <div v-for="(contentType, cIndex) in contentTypeSelection"
                            :key="`${id}-${contentType.id}-contentType-${cIndex}`"
                            class="type-selection-block clickable hoverable-lift-light"
                            :class="contentTypeClasses(contentType)"
                            @click="setContentType(contentType)"
                            @click.middle.stop.prevent="onContentTypeMiddleClicked(contentType)">
                            <div class="type-selection-block__title">
                                {{ contentType.name }}
                            </div>
                            <div class="type-selection-block__description" v-if="contentType.description">
                                {{ contentType.description }}
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- INSTANCE SELECTION -->
            <div class="instance-selection-section fadein mt-4"
                v-if="hasSelectedContentType">
                <h2>Select data to compare</h2>

                <div class="mt-2">
                    <div @click="selectDummyData">Todo: huge buttons, click for dialog w/ search</div>
                </div>
            </div>

            <!-- COMPARISON ACTIONS -->
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

            <!-- OUTPUT EXECUTION -->
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

            <!-- OUTPUT -->
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
        this.loadContentTypes();
        
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
    selectDummyData(): void {
        this.selectedInstances = [
            { id: "22", name: 'Item 22', description: 'Some description here.' },
            { id: "88", name: 'Item 88', description: 'Some description here.' },
        ];
    }

    loadContentTypes(): void {
        // this.service.GetPermutationTypes(this.dataLoadStatus, {
        //     onSuccess: (data) => this.onContentTypesRetrieved(data)
        // });
        this.onContentTypesRetrieved(null);
    }

    onContentTypesRetrieved(data: any): void {
        // this.permutationTypes = data;
        
        const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.typeId) || null;
        if (this.contentTypeSelection)
        {
            const matchingType = this.contentTypeSelection.filter(x => this.hash(x.id) == idFromHash)[0];
            if (matchingType) {
                this.setContentType(matchingType, false);
            }
        }
    }

    setContentType(type: ContentType | null, updateUrl: boolean = true): void {
        this.selectedContentType = type;
        this.selectedDiffTypes = Array.from(this.diffTypeSelection);
        this.selectedInstances = [];

        if (type == null) {
            this.$router.push(`/comparison`);
        } else {
            const idHash = this.hash(type.id);
            if (updateUrl && StringUtils.stringOrFirstOfArray(this.$route.params.typeId) != StringUtils.stringOrFirstOfArray(idHash))
            {
                this.$router.push(`/comparison/${idHash}`);
            }
        }
    }

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
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onContentTypeMiddleClicked(type: ContentType): void {
        if (type && type.id)
        {
            const idHash = this.hash(type.id);
            const route = `#/comparison/${idHash}`;
            UrlUtils.openRouteInNewTab(route);
        }
    }
    
    onRouteChanged(to: RouteLocationNormalized, from: RouteLocationNormalized): void {
        if (!this.contentTypeSelection || !to.path.toLowerCase().startsWith('/comparison')) return;

        const oldTypeIdFromHash = StringUtils.stringOrFirstOfArray(from.params.typeId) || null;
        const newTypeIdFromHash = StringUtils.stringOrFirstOfArray(to.params.typeId) || null;
        const typeChanged = oldTypeIdFromHash != newTypeIdFromHash;

        if (typeChanged)
        {
            const matchingType = this.contentTypeSelection.filter(x => this.hash(x.id) == newTypeIdFromHash)[0] || null;
            this.setContentType(matchingType, false);
        }
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

    .type-selection-blocks {
        display: flex;
        flex-wrap: wrap;
        justify-content: center;
    }

    .type-selection-block {
        padding: 20px;
        margin: 20px;
        text-align: left;
        border: 4px solid var(--color--accent-base);

        &__title {
            font-size: 20px;
            font-weight: 600;
        }
        &__description {
            margin-top: 10px;
            font-size: 15px;
            color: #6a6a6a;
        }
    }

    .selected-contentType {
        &__title {
            margin-bottom: 10px;
        }
        &__description {
            font-size: 14px;
            margin-bottom: 10px;
            color: var(--color--accent-darken8);
        }
        &__cancelLink {
            background-color: var(--color--accent-base);
            border: 1px solid var(--color--accent-darken2);
            padding: 10px;
            display: inline-block;
            color: var(--color--accent-darken10);
            font-size: 14px;
        }
    }

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
