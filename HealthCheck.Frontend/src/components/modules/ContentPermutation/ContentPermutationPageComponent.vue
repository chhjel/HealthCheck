<!-- src/components/modules/ContentPermutation/ContentPermutationPageComponent.vue -->
<template>
    <div>
        <!-- NAVIGATION DRAWER -->
        <Teleport to="#module-nav-menu">
            <filterable-list-component
                :items="menuItems"
                :sortByKey="`Name`"
                :hrefKey="`Href`"
                :loading="typesLoadStatus.inProgress"
                :disabled="isLoading"
                :groupIfSingleGroup="false"
                :showFilter="false"
                ref="filterableList"
                v-on:itemClicked="onMenuItemClicked"
                @itemMiddleClicked="onMenuItemMiddleClicked"
                />
        </Teleport>
        
        <div class="content-root content-permutations">
            <!-- PERMUTATIONS -->
            <div v-if="currentType">
                <!-- todo: frontend grouping of combinations -->
                <h2>{{ currentType.Name }}</h2>
                <p v-if="currentType.Description">{{ currentType.Description }}</p>

                <div class="content-permutations__filter mb-2">
                    <div class="mb-2"><b>Filter</b></div>
                    <div class="content-permutations__filter-inputs">
                        <backend-input-component
                            v-for="(filterInput, fIndex) in filterInputs"
                            :key="`${id}-${currentType.Id}-filterinput-${fIndex}`"
                            v-model:value="filter[filterInput.Id]"
                            class="content-permutations__filter-input"
                            :config="filterInput"
                            :readonly="isLoading"
                            />
                        <text-field-component
                            v-model:value="contentCountToRequest"
                            label="Number of examples to search for"
                            type="number"
                            :min="1"
                            :max="maxAllowedContentCount"
                            class="content-permutations__filter-input" />
                    </div>
                </div>

                <h3 class="mb-2">{{ filteredPermutations.length }} possible combinations</h3>
                <div class="content-permutations__choices">
                    <div v-for="(permutation, pIndex) in filteredPermutations"
                        :key="`${id}-${permutation.Id}-type-choice-${pIndex}`"
                        @click="onPermutationSelected(permutation)"
                        class="content-permutations__choice clickable hoverable hoverable-lift-light"
                        :class="permutationClasses(permutation)">
                        <div v-for="(choice, cIndex) in getChoices(permutation.Choice)"
                            :key="`${id}-${currentType.Id}-type-choice-${cIndex}`">
                            <code>{{ choice.name }} = {{ choice.value }}</code>
                        </div>
                        <div v-if="permutation.LastRetrievedCount != null"
                            class="content-permutations__choice__meta">Latest count: {{ permutation.LastRetrievedCount }}</div>
                    </div>
                </div>
            </div>

            <!-- LOAD PROGRESS -->
            <progress-linear-component 
                v-if="isLoading"
                indeterminate color="success"></progress-linear-component>

            <!-- DATA LOAD ERROR -->
            <alert-component :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
            {{ dataLoadStatus.errorMessage }}
            </alert-component>

            <!-- CONTENT -->
            <div v-if="exampleContent" class="mt-4">
                <h3 class="mb-2">Found {{ exampleContent.Content.length }} examples</h3>
                <div class="content-permutations__contents">
                    <div v-for="(content, cIndex) in exampleContent.Content"
                        :key="`${id}-${content.Id}-content-${cIndex}`"
                        class="content-permutations__content"
                        :class="contentClasses(content)">
                        <!-- Main part -->
                        <a class="content-permutations__content__mainPart"
                            :href="content.MainUrl">
                            <div class="content-permutations__content__title">
                                {{ content.Title }}
                            </div>
                            <div class="content-permutations__content__image"
                                v-if="content.ImageUrl"
                                :style="`background-image: url('${content.ImageUrl}')`"></div>
                        </a>
                        <!-- Description -->
                        <div v-if="content.Description"
                             class="content-permutations__content__description">
                            {{ content.Description }}
                        </div>
                        <!-- AdditionalUrls -->
                        <div v-if="content.AdditionalUrls && content.AdditionalUrls.length > 0"
                             class="content-permutations__content__urls">
                            <a v-for="(url, uIndex) in content.AdditionalUrls"
                                :key="`${id}-${content.Id}-content-${uIndex}`"
                                :href="url.Url"
                                class="content-permutations__content__url">
                                {{ url.Title }}
                            </a>
                        </div>
                    </div>
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
import { HCPermutatedContentItemViewModel } from "@generated/Models/Core/HCPermutatedContentItemViewModel";
import { HCBackendInputConfig } from "@generated/Models/Core/HCBackendInputConfig";
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import { nextTick } from "@vue/runtime-core";

@Options({
    components: {
        FilterableListComponent,
        BackendInputComponent
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
    typesLoadStatus: FetchStatus = new FetchStatus();
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    routeListener: Function | null = null;

    permutationTypes: HCGetPermutationTypesViewModel = {
        Types: []
    };
    currentType: HCContentPermutationTypeViewModel | null = null;
    currentPermutation: HCContentPermutationChoiceViewModel | null = null;
    exampleContent: HCGetPermutatedContentViewModel | null = null;
    filter: object = {};

    filterInputs: Array<HCBackendInputConfig> = [];
    contentCountToRequest: number = 1;

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
            MaxCount: this.contentCountToRequest,
            PermutationTypeId: typeId,
            PermutationChoiceId: choiceId
        };
        this.service.GetPermutatedContent(payload, this.dataLoadStatus, {
            onSuccess: (data) => this.onContentLoadedFor(typeId, choiceId, data)
        });
        this.exampleContent = null;
    }

    onContentLoadedFor(typeId: string, choiceId: number, data: HCGetPermutatedContentViewModel): void {
        this.exampleContent = data;
        const type = this.permutationTypes.Types.find(x => x.Id == typeId);
        const choice = type?.Permutations?.find(x => x.Id == choiceId);
        if (choice) {
            choice.LastRetrievedCount = data.Content.length;
        }
    }

    setActiveType(type: HCContentPermutationTypeViewModel, updateUrl: boolean = true): void {
        this.currentType = type;
        this.currentPermutation = null;
        this.resetFilter();
        this.exampleContent = null;
        (this.filterableList as FilterableListComponent).setSelectedItem(type);

        const firstPermutation = type.Permutations[0];
        this.filterInputs = [];
        if (firstPermutation) {
            const allowedFilterInputKeys = Object.keys(firstPermutation.Choice);
            this.filterInputs = this.currentType.PropertyConfigs
                .filter(x => allowedFilterInputKeys.includes(x.Id));
        }
        this.contentCountToRequest = type.DefaultContentCount;

        const idHash = this.hash(type.Id);
        if (updateUrl && StringUtils.stringOrFirstOfArray(this.$route.params.typeId) != StringUtils.stringOrFirstOfArray(idHash))
        {
            this.$router.push(`/contentPermutation/${idHash}`);
        }
    }

    resetFilter(): void {
        this.filter = {};
        this.currentType.PropertyConfigs.forEach(x => {
            this.filter[x.Id] = null;
        });
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
        return this.currentType?.PropertyConfigs?.find(x => x.Id == name)?.Name || name;
    }

    getPropertyDescription(name: string) : string {
        return this.currentType?.PropertyConfigs?.find(x => x.Id == name)?.Description || '';
    }

    permutationClasses(permutation: HCContentPermutationChoiceViewModel): any {
        let classes = {};
        if (this.currentPermutation?.Id == permutation?.Id) {
            classes['active'] = true;
        }
        return classes;
    }

    contentClasses(content: HCPermutatedContentItemViewModel): any {
        let classes = {};
        if (content.ImageUrl) classes['has-image'] = true;
        return classes;
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
        this.currentPermutation = permutation;
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

    matchesFilter(item: HCContentPermutationChoiceViewModel): boolean {
        const filterProps = Object.keys(this.filter);
        for (let i=0;i<filterProps.length;i++)
        {
            const key = filterProps[i];
            let filterValue = this.filter[key];
            if (filterValue === null || filterValue === '') continue;

            if (typeof filterValue !== 'string')
            {
                filterValue = JSON.stringify(filterValue);
            }
            filterValue = filterValue?.toLowerCase();
            
            let choiceValue = item.Choice[key];
            if (typeof choiceValue !== 'string')
            {
                choiceValue = JSON.stringify(choiceValue);
            }
            choiceValue = choiceValue?.toLowerCase();

            // Handle enum until this is refactored
            const def = this.filterInputs.find(x => x.Id == key);
            const type = def?.Type;
            if (type == "Enum" && !def.PossibleValues.some(v => v.toLowerCase() == filterValue)) {
                continue;
            }

            if (choiceValue?.includes(filterValue) !== true) return false;
        }
        return true;
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

    get filteredPermutations(): Array<HCContentPermutationChoiceViewModel> {
        if (!this.currentType || !this.currentType.Permutations || this.currentType.Permutations.length == 0) return [];
        return this.currentType.Permutations
            .filter(x => this.matchesFilter(x));
    }

    get maxAllowedContentCount(): number {
        return this.currentType?.MaxAllowedContentCount || 0;
    }

    @Watch("contentCountToRequest")
    onContentCountToRequestChanged(): void {
        nextTick(() => {
            if (!this.contentCountToRequest) this.contentCountToRequest = 1;
            else if (this.contentCountToRequest < 1) this.contentCountToRequest = 1;
            else if (this.contentCountToRequest > this.maxAllowedContentCount) this.contentCountToRequest = this.maxAllowedContentCount;
        });
    }
}
</script>

<style scoped lang="scss">
.content-permutations {
    &__filter {
        padding: 10px;
        &-inputs {
            border-left: 2px solid var(--color--accent-base);
            padding-left: 10px;
            margin-bottom: 10px;
            display: flex;
            flex-wrap: wrap;
            align-items: flex-end;
        }
        &-input {
            margin-right: 40px;
            margin-bottom: 20px;
        }
    }
    
    &__choices {
        display: flex;
        flex-wrap: wrap;
        border-left: 2px solid var(--color--accent-base);
        padding-left: 10px;
        margin-left: 10px;
    }

    &__choice {
        border: 4px solid var(--color--accent-base);
        margin: 5px;
        padding: 10px;
        transition: all 0.4s;

        &__meta {
            font-size: 12px;
            color: var(--color--info-darken7);
            margin-top: 8px;
        }

        &.active {
            border: 4px solid var(--color--accent-darken4);
            box-shadow: none;
            border-radius: 8px;
        }

        code {
            color: var(--color--primary-darken1);
        }
    }

    &__contents {
        display: flex;
        flex-wrap: wrap;
        justify-content: space-evenly;
    }
    &__content {
        width: 24%;
        min-width: 240px;
        overflow: hidden;
        margin-bottom: 20px;
        border: 1px solid var(--color--accent-base);
        box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.02), 0 3px 2px 0 rgba(0, 0, 0, 0.02), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
        transition: all 0.4s;

        &:hover {
            box-shadow: 0 0 12px 2px rgb(0 0 0 / 21%) !important;
        }

        &__mainPart {
            position: relative;
            display: inline-block;
            width: 100%;
            text-decoration: none;
        }
        &.has-image {
            .content-permutations__content__mainPart {
                min-height: 150px;
            }
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
        &__title {
            padding: 15px;
            font-size: 20px;
            color: #fff;
            background-color: var(--color--primary-darken1);
            text-align: center;
        }
        &__description {
            padding: 8px 5px;
        }
        &__urls {
            padding: 5px;
            background-color: #e7e7e7;
            border-top: 1px solid var(--color--accent-base);
        }
        &__url {
            padding: 2px;
            display: flex;
            align-items: center;
            font-size: 14px;
            &::before {
                content: '🔗';
                margin-right: 2px;
            }
        }
        &.has-image {
            .content-permutations__content__title {
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
}
</style>
