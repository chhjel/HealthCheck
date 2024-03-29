<!-- src/components/modules/TestSuite/TestSuitesPageComponent.vue -->
<template>
    <div>
        <!-- NAVIGATION DRAWER -->
        <Teleport to="#module-nav-menu">
            <filterable-list-component
                v-if="!isSingleTestMode"
                :items="menuItems"
                :groupOrders="menuGroupOrders"
                :groupByKey="`GroupName`"
                :sortByKey="`UIOrder`"
                :hrefKey="`Href`"
                :filterKeys="[ 'Name', 'Description', '__TestNames' ]"
                :loading="setSetsLoadStatus.inProgress"
                :disabled="setSetsLoadStatus.inProgress"
                :showFilter="true"
                :groupIfSingleGroup="false"
                ref="filterableList"
                v-on:itemClicked="onMenuItemClicked"
                @itemMiddleClicked="onMenuItemMiddleClicked"
                />
        </Teleport>
        
        
        <div class="content-root">
            <!-- INVALID TESTS -->
            <alert-component :value="hasInvalidTests" type="error">
                <h3>Some invalid tests were found:</h3>
                <ul>
                    <li v-for="(invalidTest, index) in invalidTests"
                        :key="`invalidtest-${index}-${invalidTest.Id}`"
                        class="mt-2 mb-2">
                        <h4 style="display: flex; align-items: center;">
                            "{{ invalidTest.Name }}" 
                            <span class="caption ml-2" style="font-family: monospace;">({{ invalidTest.Id }})</span>
                        </h4>
                        <div class="ma-1">
                            {{ invalidTest.Reason }}
                        </div>
                    </li>
                </ul>
            </alert-component>

            <!-- NO TESTS INFO -->
            <alert-component :value="!hasAnyTests && !setSetsLoadStatus.inProgress" type="info">
            No tests were found.
            </alert-component>

            <!-- DATA LOAD ERROR -->
            <alert-component :value="setSetsLoadStatus.failed" type="error">
            {{ setSetsLoadStatus.errorMessage }}
            </alert-component>

            <!-- LOAD PROGRESS -->
            <progress-linear-component 
                v-if="setSetsLoadStatus.inProgress"
                indeterminate color="success"></progress-linear-component>

            <!-- SINGLE MODE -->
            <a v-if="isSingleTestMode" @click.prevent.stop="exitSingleMode" class="clickable mb-2" style="display: block;">
                &lt;&lt; Back to all tests
            </a>

            <!-- TESTS -->
            <test-set-component
                v-if="activeSet != null"
                :module-id="config.Id"
                :testSet="activeSet"
                v-on:testClicked="onTestClicked" />
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import TestSetViewModel from '@models/modules/TestSuite/TestSetViewModel';
import TestsDataViewModel from '@models/modules/TestSuite/TestsDataViewModel';
import InvalidTestViewModel from '@models/modules/TestSuite/InvalidTestViewModel';
import TestSetComponent from '@components/modules/TestSuite/TestSetComponent.vue';
import FilterInputComponent from '@components/Common/FilterInputComponent.vue';
import TestViewModel from '@models/modules/TestSuite/TestViewModel';
import TestService from '@services/TestService';
import { FetchStatus } from '@services/abstractions/TKServiceBase';
import ModuleOptions from '@models/Common/ModuleOptions';
import ModuleConfig from '@models/Common/ModuleConfig';
import UrlUtils from '@util/UrlUtils';
import StringUtils from "@util/StringUtils";
import { TestModuleOptions } from '@components/modules/TestSuite/TestSuitesPageComponent.vue.models';
import { StoreUtil } from "@util/StoreUtil";
import { FilterableListItem } from "@components/Common/FilterableListComponent.vue.models";
import FilterableListComponent from "@components/Common/FilterableListComponent.vue";
import { RouteLocationNormalized } from "vue-router";
import GroupOptionsViewModel from "@models/modules/TestSuite/GroupOptionsViewModel";
import { nextTick } from "@vue/runtime-core";
import EventBus from "@util/EventBus";

@Options({
    components: {
        TestSetComponent,
        FilterInputComponent,
        FilterableListComponent
    }
})
export default class TestSuitesPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<TestModuleOptions>;
    
    @Ref() readonly filterableList!: FilterableListComponent;

    sets: Array<TestSetViewModel> = [];
    groupOptions: Array<GroupOptionsViewModel> = [];
    menuGroupOrders: { [key:string]:number } | null = null;
    menuItems: Array<FilterableListItem> = [];
    activeSet: TestSetViewModel | null = null;
    invalidTests: Array<InvalidTestViewModel> = new Array<InvalidTestViewModel>();
    isSingleTestMode: boolean = false;

    // Service
    service: TestService = new TestService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    setSetsLoadStatus: FetchStatus = new FetchStatus();

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        StoreUtil.store.commit('showMenuButton', true);
        StoreUtil.store.commit('setTestModuleOptions', this.options.Options);

        this.loadData();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }
    
    get hasInvalidTests(): boolean {
        return this.invalidTests.length > 0;
    }

    get hasAnyTests(): boolean {
        return this.sets.length > 0;
    }
    
    updateMenuItems(): void
    {
        if (!this.sets) this.menuItems = [];
        this.menuItems = this.sets.map(x => {
            (<any>x).__TestNames = x.Tests.map(t => t.Name).join(' ');
            let d = {
                title: x.Name,
                subtitle: '',
                data: x
            };
            // (<any>d)['Href'] = "/woot";
            return d;
        });
    }

    setMenuGroupOrders(): void
    {
        this.menuGroupOrders = null;
        let orders: { [key:string]:number } = {};
        if (this.groupOptions == null || this.groupOptions.length == 0) return;

        this.groupOptions.forEach(g => {
            orders[g.GroupName] = g.UIOrder;
        });

        this.menuGroupOrders = orders;
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.service.GetTests(this.setSetsLoadStatus, { onSuccess: (data) => this.onTestSetDataRetrieved(data) })
    }

    onTestSetDataRetrieved(testsData: TestsDataViewModel): void {
        this.invalidTests = testsData.InvalidTests;
        
        StoreUtil.store.commit('setTestModuleTemplateValues', testsData.ParameterTemplateValues);

        // Init default value
        for(let set of testsData.TestSets) {
            for(let test of set.Tests) {
                for(let param of test.Parameters) {
                    param.Value = param.DefaultValue;

                    if (param.Value == null)
                    {
                        const templateData = testsData.ParameterTemplateValues.filter(x => x.Type == param.Type)[0];
                        if (templateData)
                        {
                            param.Value = templateData.Template;
                        }
                    }
                }
            }
        }

        const singleTestId: string | null = UrlUtils.GetQueryStringParameter('single', null);
        if (singleTestId) {
            this.isSingleTestMode = true;
            EventBus.notify("collapseMenu");
            testsData.TestSets = testsData.TestSets.filter(s => s.Tests.some(t => t.Id == singleTestId));
            if (testsData.TestSets && testsData.TestSets.length > 0) {
                testsData.TestSets[0].AllowRunAll = false;
                testsData.TestSets[0].Tests = testsData.TestSets[0].Tests.filter(t => t.Id == singleTestId);
            }
        }

        this.sets = testsData.TestSets;
        this.groupOptions = testsData.GroupOptions;

        this.setSetsLoadStatus.inProgress = false;
        this.updateMenuItems();
        this.setMenuGroupOrders();
        this.updateSelectionFromUrl();
    }

    exitSingleMode(): void {
        let url = window.location.href;
        url = UrlUtils.RemoveQueryStringParameter(url, 'single');
        window.location.href = url;
    }

    setActiveSet(set: TestSetViewModel, test: string | null = null, updateRoute: boolean = true): void
    {
        this.activeSet = set;
        if (this.filterableList) {
            this.filterableList.setSelectedItem(set);
        }

        if (updateRoute)
        {
            const groupId = (set.GroupName == null) ? 'other' : UrlUtils.EncodeHashPart(set.GroupName);
            const setId = UrlUtils.EncodeHashPart(set.Name);

            let routeParams: any = {
                group: groupId,
                set: setId
            };
            let testId: string | null = null;
            if (test != null)
            {
                testId = UrlUtils.EncodeHashPart(test);
                routeParams['test'] = testId;
            }

            const groupInUrl = StringUtils.stringOrFirstOfArray(this.$route.params.group);
            const setInUrl = StringUtils.stringOrFirstOfArray(this.$route.params.set);
            const testInUrl = StringUtils.stringOrFirstOfArray(this.$route.params.test);
            if (groupInUrl !== groupId || setInUrl != setId || testInUrl != testId)
            {
                this.$router.push({ name: this.config.Id, params: routeParams })
            }
        }
    }

    onMenuItemClicked(item: FilterableListItem): void {
        const set: TestSetViewModel = item.data;
        this.setActiveSet(set);
        
        let testNameToScrollTo: string | null = null;
        if (this.filterableList.filterInputText() && this.filterableList) {
            const filterInput = this.filterableList.filterInputText();
            const matchingName = set.Tests.filter(t => t.Name?.toLowerCase()?.includes(filterInput?.toLowerCase()))[0];
            const matchingDescriptions = set.Tests.filter(t => t.Description?.toLowerCase()?.includes(filterInput?.toLowerCase()))[0];
            testNameToScrollTo = matchingName?.Name || matchingDescriptions?.Name || '';

            if (testNameToScrollTo) {
                setTimeout(() => {
                    const encodedTestName = UrlUtils.EncodeHashPart(testNameToScrollTo);
                    this.setActiveSet(set, encodedTestName)
                    this.scrollToTest(encodedTestName);
                }, 100);
            }
        }
    }

    onMenuItemMiddleClicked(item: FilterableListItem): void {
        const set: TestSetViewModel = item.data;
        if (set)
        {
            const groupPart = (set.GroupName == null) ? 'other' : UrlUtils.EncodeHashPart(set.GroupName);
            const setPart = UrlUtils.EncodeHashPart(set.Name);
            const route = `#/tests/${groupPart}/${setPart}`;
            UrlUtils.openRouteInNewTab(route);
        }
    }

    updateSelectionFromUrl(): void
    {
        const groupFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.group) || '';
        const setFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.set);
        const testFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.test);
        if (setFromHash != null && this.trySetActiveSetFromEncodedValues(groupFromHash, setFromHash, testFromHash)) {
            return;
        }
        
        // Fallback to first set in list
        if(this.sets.length > 0) {
            // todo: sort by ui order to get the topmost one
            this.setActiveSet(this.sets[0]);
        }
    }

    @Watch('$route')
    onRouteChanged(to: RouteLocationNormalized, from: RouteLocationNormalized): void {
        if (to.fullPath.toLowerCase().startsWith('/tests'))
        {
            this.updateSelectionFromUrl();
        }
    }

    trySetActiveSetFromEncodedValues(group: string, set: string, test: string | null = null): boolean
    {
        if (set != null) {
            let matchingSet = this.sets
                .filter(x => (group.length == 0 || UrlUtils.EncodeHashPart(x.GroupName || 'other') == group) 
                              && UrlUtils.EncodeHashPart(x.Name) === set)[0];

            if (matchingSet != null) {
                this.setActiveSet(matchingSet, test, false);
                this.scrollToTest(test);
                return true;
            }
        }

        return false;
    }

    onTestClicked(test: TestViewModel): void {
        const encodedTestName = UrlUtils.EncodeHashPart(test.Name);
        this.setActiveSet(this.activeSet!, encodedTestName)
        this.scrollToTest(encodedTestName);
    }

    scrollToTest(encodedTestName: string | null): void {
        if (encodedTestName == null) {
            return;
        }

        setTimeout(() => {
            const testElement = document.querySelector(`[data-test-title-encoded='${encodedTestName}']`);
            if (testElement != null) {
                window.scrollTo({
                    top: (window.pageYOffset || document.documentElement.scrollTop) 
                        + testElement.getBoundingClientRect().top - 100,
                    behavior: 'smooth'
                });
            }
        }, 10);
    }
}
</script>

<style scoped>
.filter {
    position: relative;
    margin-left: 44px;
    margin-top: 26px;
    margin-bottom: 18px;
    margin-right: 44px;
}
</style>
