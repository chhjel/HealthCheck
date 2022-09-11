<!-- src/components/modules/GoTo/GoToPageComponent.vue -->
<template>
    <div>
        <div class="content-root goto-module">


            <div class="query-input__wrapper">
                <text-field-component
                    v-model:value="query"
                    :placeholder="'Enter some id or value..'"
                    :disabled="isLoading"
                    class="query-input"
                    solo
                    @keyup.enter="executeGoto" />
                <btn-component flat color="primary" class="query-input__button"
                    :disabled="dataLoadStatus.inProgress || selectedResolverIds.length == 0"
                    @click="executeGoto">
                    <icon-component>search</icon-component>
                    Search
                </btn-component>
            </div>
            <div class="mt-3 mb-5">
                <select-component
                    label="What to look for"
                    class="included-types"
                    v-model:value="selectedResolverIds"
                    :items="resolverOptions"
                    :disabled="isLoading"
                    multiple chips>
                </select-component>
            </div>

            <!-- LOAD PROGRESS -->
            <div v-if="dataLoadStatus.inProgress" class="mb-2 search-label">
                {{ searchLabel }}
            </div>
            <progress-linear-component 
                class="loader"
                v-if="isLoading"
                :height="16"
                indeterminate color="success"></progress-linear-component>
            <!-- TYPES LOAD ERROR -->
            <alert-component :value="typesLoadStatus.failed" v-if="typesLoadStatus.failed" type="error">
            {{ typesLoadStatus.errorMessage }}
            </alert-component>

            <!-- DATA LOAD ERROR -->
            <alert-component :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
            {{ dataLoadStatus.errorMessage }}
            </alert-component>

            <div class="results-label" v-if="resultsLabel">{{ resultsLabel }}</div>
            
            <div v-if="showResults" class="fadein results">
                <div v-for="(result, rIndex) in results"
                    :key="`${id}-${result.Name}-${rIndex}`"
                    class="result">
                    <component :is="(result.Data && result.Data.Urls && result.Data.Urls.length > 0) ? 'a' : 'div'"
                        :href="(result.Data && result.Data.Urls && result.Data.Urls.length > 0) ? result.Data.Urls[0].Url : null"
                        class="result__header" 
                        :class="resultHeaderClasses(result)" v-if="result.Data" >
                        <div class="result__name">{{ result.Data.Name }}</div>
                        <div class="result__image"
                            v-if="result.Data.ImageUrl"
                            :style="`background-image: url('${result.Data.ImageUrl}')`"></div>
                    </component>
                    <div v-if="result.Data" class="result__data">
                        <div class="result__description">{{ result.Data.Description }}</div>
                        <ul class="result__urls">
                            <li v-for="(url, uIndex) in result.Data.Urls"
                                :key="`${id}-${result.Name}-${rIndex}-${url.Url}-${uIndex}`"
                                class="result__url">
                                <a :href="url.Url">{{ url.Name }}</a>
                            </li>
                        </ul>
                    </div>
                    <div class="result__source">
                        <span v-if="result.Data && result.Data.ResolvedFrom">
                            Matched on 
                        </span>
                        {{ getResolverName(result.ResolverId) }}
                        <span v-if="result.Data && result.Data.ResolvedFrom">
                            {{ result.Data.ResolvedFrom }}
                        </span>
                    </div>
                    <code class="result__error" v-if="result.Error">{{ result.Error }}</code>
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
import IdUtils from "@util/IdUtils";
import GoToService from "@services/GoToService";
import { HCGoToResolverDefinition } from "@generated/Models/Core/HCGoToResolverDefinition";
import { HCGoToResolvedDataWithResolverId } from "@generated/Models/Core/HCGoToResolvedDataWithResolverId";
import UrlUtils from "@util/UrlUtils";
import { RouteLocationNormalized } from "vue-router";

@Options({
    components: {
        FilterableListComponent
    }
})
export default class ComparisonPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;
    
    @Ref() readonly filterableList!: FilterableListComponent;

    // Service
    service: GoToService = new GoToService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    typesLoadStatus: FetchStatus = new FetchStatus();
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    routeListener: Function | null = null;
    resolverDefinitions: Array<HCGoToResolverDefinition> = [];

    selectedResolverIds: Array<string> = [];
    query: string = '';
    results: Array<HCGoToResolvedDataWithResolverId> = [];
    hasSearchedAtLeastOnce: boolean = false;
    searchLabel: string = '';
    searchLabelIndex: number = 0;
    searchLabelTimeoutRef: any;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadResolvers();
        await this.$router.isReady();
        this.routeListener = this.$router.afterEach((t, f, err) => this.onRouteChanged(t, f));
    }

    beforeDestroy(): void {
        if (this.routeListener)
        {
            this.routeListener();
        }
    }

    beforeUnmount(): void {
        clearInterval(this.searchLabelTimeoutRef);
    }

    ////////////////
    //  METHODS  //
    //////////////
    resetSearchLabel(): void {
        if (this.searchLabelTimeoutRef) clearInterval(this.searchLabelTimeoutRef);
        this.searchLabelTimeoutRef = setInterval(this.onSearchLabelTimeToChange, 800);
        this.searchLabelIndex = 0;
        this.searchLabel = 'Searching';
    }

    stopSearchLabel(): void {
        if (this.searchLabelTimeoutRef) clearInterval(this.searchLabelTimeoutRef);
    }

    onSearchLabelTimeToChange(): void {
        const searchLabels = [
            '.Searching.',
            '..Searching..',
            'Searching harder',
            '.Searching harder.',
            '..Searching harder..',
            '...Searching harder...',
            'This is taking longer than expected',
            '.This is taking longer than expected.',
            '..This is taking longer than expected..',
            '...This is taking longer than expected...',
            '....This is taking longer than expected....',
            '🤔',
            '.🤔.',
            '..🤔..',
            '...🤔...',
            '....🤔....',
            '😴',
            '😴',
            '😴',
            '🙄',
            'Trying some more',
            '.Trying some more.',
            '..Trying some more..'
        ];

        if (this.dataLoadStatus?.inProgress === true) {
            this.searchLabel = searchLabels[this.searchLabelIndex];
            this.searchLabelIndex++;
            if (this.searchLabelIndex >= searchLabels.length) this.searchLabelIndex = 0;
        }
    }

    loadResolvers(): void {
        this.service.GetResolversDefinitions(this.typesLoadStatus, {
            onSuccess: (data) => this.onResolversRetrieved(data)
        });
    }

    autoNavNext: boolean = false;
    onResolversRetrieved(data: Array<HCGoToResolverDefinition>): void {
        this.resolverDefinitions = data;
        this.selectedResolverIds = data.map(x => x.Id);

        const query = UrlUtils.GetQueryStringParameter('query', null);
        const auto = UrlUtils.GetQueryStringParameter('auto', null) === 'true';
        if (query) {
            this.query = query;
            if (auto) {
                this.autoNavNext = UrlUtils.GetQueryStringParameter('autoNav', null) === 'true';
                this.executeGoto();
            }
        }
    }

    executeGoto(): void {
        this.resetSearchLabel();
        this.service.Goto(this.selectedResolverIds, this.query,
            this.dataLoadStatus, {
            onSuccess: (data) => this.onGotoResultRetrieved(data)
        });
    }

    onGotoResultRetrieved(data: Array<HCGoToResolvedDataWithResolverId>): void {
        this.results = data;
        this.hasSearchedAtLeastOnce = true;

        if (this.autoNavNext) {
            this.autoNavNext = false;
            if (this.results.length == 1 && this.results[0].Data?.Urls[0] != null) {
                window.location.href = this.results[0].Data.Urls[0].Url;
            }
        }
    }

    getResolverName(id: string): string {
        return this.resolverDefinitions.find(x => x.Id == id)?.Name || '<unknown>';
    }

    resultHeaderClasses(result: HCGoToResolvedDataWithResolverId): any {
        let classes = {};
        if (result.Data && result.Data.ImageUrl) classes['has-image'] = true;
        return classes;
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch('$route')
    onRouteChanged(to: RouteLocationNormalized, from: RouteLocationNormalized): void {
        if (from.fullPath.toLowerCase().startsWith('/goto') && !to.fullPath.toLowerCase().startsWith('/goto'))
        {
            UrlUtils.ClearQueryStringParameter('query');
            UrlUtils.ClearQueryStringParameter('auto');
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

    get resolverOptions(): Array<any> {
        return this.resolverDefinitions.map(x => ({
            value: x.Id,
            text: x.Name
        }));
    }

    get showResults(): boolean {
        return this.hasSearchedAtLeastOnce && this.results.length > 0;
    }

    get resultsLabel(): string {
        if (!this.hasSearchedAtLeastOnce) return '';
        else if (this.results.length == 0) return 'No results found';
        else if (this.results.length == 1) return `Found a single potential match`;
        else return `Found ${this.results.length} potential matches`;
    }
}
</script>
<style lang="scss">
.goto-module {
    .included-types {
        .input-component--header-name {
            color: var(--color--accent-darken5) !important;
            font-weight: normal !important;
        }
    }
}
</style>
<style scoped lang="scss">
.goto-module {
    text-align: center;

    .query-input__wrapper 
    {
        padding-bottom: 5px !important;
        display: flex;
        flex-wrap: nowrap;
        align-items: center;
        justify-content: center;

        .query-input {
            height: 72px;
            min-width: 50%;
            @media (min-width: 960px) {
                padding-left: 104px;
            }
        }
        .query-input__button {
            height: 72px;
            margin: 0;
            margin-top: 2px;
        }
    }

    .included-types {
        display: inline-block;
    }

    .loader {
        max-width: 500px;
        margin: auto;
    }

    .search-label {
        font-family: monospace;
        color: var(--color--accent-darken6);
        font-variant: all-small-caps;
        font-size: 19px;
    }

    .results-label {
        font-size: 24px;
    }
    .results {
        display: flex;
        flex-wrap: wrap;
        align-items: flex-start;
        justify-content: center;
    }
    .result {
        margin: 20px;
        text-align: left;
        border: 4px solid var(--color--accent-base);
        position: relative;

        &__data {
            padding: 10px;
            padding-bottom: 10px;
            padding-top: 5px;
        }
        &__name {
            padding: 10px;
            font-size: 18px;
            font-weight: 600;
        }
        &__image {
            pointer-events: none;
            position: absolute;
            top: 0;
            left: 0;
            bottom: 0;
            right: 0;
            background-repeat: no-repeat;
            background-size: cover;
            background-position: center;
        }
        &__header {
            position: relative;
            overflow: hidden;
            display: block;

            &.has-image {
                min-height: 150px;

                .result__name {
                    color: #fff;
                    background-color: #33333378;
                    position: absolute;
                    bottom: 0;
                    left: 0;
                    right: 0;
                    z-index: 1;
                }
            }
        }
        &__description {
            font-size: 13px;
            color: var(--color--accent-darken7);
            margin-bottom: 5px;
            white-space: pre;
        }
        &__urls {
            padding-left: 20px;
            margin-top: 10px;
            margin-bottom: 0;
        }
        &__source {
            padding: 3px;
            font-size: 10px;
            font-family: monospace;
            color: var(--color--accent-darken5);
            text-align: center;
        }
        &__error {
            border: none;
            box-shadow: none;
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
