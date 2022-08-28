<!-- src/components/modules/Comparison/ComparisonPageComponent.vue -->
<template>
    <div>
        <div class="content-root comparison-module">
            <!-- CONTENT TYPE SELECTION -->
            <div class="type-selection-section">
                <div v-if="hasSelectedContentType"
                    class="selected-contentType">
                    <a class="selected-contentType__cancelLink hoverable-light"
                        @click.stop.prevent="setContentType(null)"
                        href="#">&lt;&lt; back</a>
                    <div class="selected-contentType__header">
                        <h1 class="selected-contentType__title">{{ selectedContentType.name }}-comparison</h1>
                    </div>
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
            <div class="instance-selection-section fadein"
                v-if="hasSelectedContentType">
                <div class="section-divider"></div>
                <h2>Select data to compare</h2>

                <div class="mt-2 instance-selection-blocks">
                    <instance-selection-component
                        class="instance-selection-block"
                        :name="selectedInstances[0]?.name" 
                        :description="selectedInstances[0]?.description"
                        @click="selectInstanceFor(0)" />
                    <instance-selection-component
                        class="instance-selection-block"
                        :name="selectedInstances[1]?.name" 
                        :description="selectedInstances[1]?.description"
                        @click="selectInstanceFor(1)" />
                </div>
            </div>

            <!-- COMPARISON ACTIONS -->
            <div class="difftype-selection-section fadein"
                v-if="hasSelectedInstances">
                <div class="section-divider"></div>
                <h2>Select what comparisons to perform</h2>
                <div class="diff-selection-blocks">
                    <diff-selection-component
                        v-for="(diff, dIndex) in diffTypeSelection"
                        :key="`${id}-${diff.id}-diff-${dIndex}`"
                        class="diff-selection-block"
                        :name="diff.name" 
                        :description="diff.description"
                        :enabled="isDiffEnabled(diff)"
                        @click="toggleDiff(diff)" />
                </div>
            </div>

            <!-- OUTPUT EXECUTION -->
            <div class="output-execution-section fadein"
                v-if="showOutputExecution">
                <div class="section-divider"></div>
                <btn-component class="output-execution-button"
                    color="primary"
                    :disabled="isLoading"
                    @click="compare">Compare</btn-component>
            </div>

            <div ref="outputTopRef"></div>
            
            <!-- LOAD PROGRESS -->
            <progress-linear-component 
                v-if="isLoading"
                indeterminate color="success"></progress-linear-component>

            <!-- DATA LOAD ERROR -->
            <alert-component :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
            {{ dataLoadStatus.errorMessage }}
            </alert-component>

            <!-- OUTPUT -->
            <div class="output-section fadein"
                v-if="showOutput">
                <div class="section-divider"></div>
                <h2>Result</h2>
                <output-component 
                    v-for="(output, oIndex) in outputData"
                    :key="`${id}-output-${oIndex}`"
                    :title="output.title"
                    :dataType="output.dataType"
                    :resultData="output.data" />
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
import InstanceSelectionComponent from "./InstanceSelectionComponent.vue";
import DiffSelectionComponent from "./DiffSelectionComponent.vue";
import OutputComponent from "./Outputs/OutputComponent.vue";

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
interface OutputData {
    title: string;
    dataType: string;
    data: any;
}
@Options({
    components: {
        FilterableListComponent,
        BackendInputComponent,
        InstanceSelectionComponent,
        DiffSelectionComponent,
        OutputComponent
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
    outputData: Array<OutputData> = [];

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
        this.outputData = [];

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

    isDiffEnabled(diff: DiffType): any {
        return this.selectedDiffTypes.some(x => x.id == diff.id);
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

    selectInstanceFor(index: number): void {
        this.selectedInstances[index] = { id: `${index}`, name: `Item #${index}`, description: `Some description here.` };
    }

    compare(): void {
        this.outputData = [
            {
                title: 'Diff A',
                dataType: 'diff',
                data: {
                    originalName: "Test Left",
                    originalContent: "{\n\t\"a\": true\n}",
                    modifiedName: "Test Right",
                    modifiedContent: "{\n\t\"a\": false,\n\t\"b\": \"nope\"\n}"
                }
            },
            {
                title: 'Diff B',
                dataType: 'diff',
                data: {
                    originalName: "AAAA",
                    originalContent: "AAABAA",
                    modifiedName: "BBBB",
                    modifiedContent: "AAACAB"
                }
            }
        ].map(x => {
            x.data = <any>JSON.stringify(x.data);
            return x;
        });
        this.scrollToResults();
    }

    scrollToResults(): void {
        setTimeout(() => {
            const targetElement = this.$refs.outputTopRef as HTMLElement;
            if (targetElement != null) {
                window.scrollTo({
                    top: (window.pageYOffset || document.documentElement.scrollTop) 
                        + targetElement.getBoundingClientRect().top - 50,
                    behavior: 'smooth'
                });
            }
        }, 10);
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
        return this.outputData.length > 0;
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
        position: relative;
        &__header {
            display: flex;
            margin-bottom: 10px;
            justify-content: center;
            margin-left: 75px;
            margin-right: 75px;
        }
        &__title {
        }
        &__description {
            font-size: 14px;
            margin-bottom: 10px;
            color: var(--color--accent-darken8);
        }
        &__cancelLink {
            position: absolute;
            top: 0;
            left: 0;

            background-color: var(--color--accent-base);
            border: 1px solid var(--color--accent-darken2);
            padding: 10px;
            display: inline-block;
            color: var(--color--accent-darken10);
            font-size: 14px;
        }
    }

    .instance-selection-blocks {
        display: flex;
        max-width: 600px;
        margin: auto;
        justify-content: space-evenly;
        align-items: flex-start;

        .instance-selection-block {
            flex: 1;
        }
    }

    .diff-selection-blocks {
        display: flex;
        max-width: 600px;
        margin: auto;
        align-items: center;
        flex-direction: column;

        .diff-selection-block {
            flex: 1;
        }
    }

    .output-execution-button {
        width: 170px;
        height: 40px;
        font-size: 21px !important;
    }

    .section-divider {
        margin: auto;
        margin-top: 12px;
        margin-bottom: 12px;
        border-top: 2px solid var(--color--accent-base);
        max-width: 25%;
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
