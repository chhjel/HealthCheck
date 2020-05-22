<!-- src/components/Pages/TestSuitesPageComponent.vue -->
<template>
    <div>
        <v-content>
            <!-- NAVIGATION DRAWER -->
            <v-navigation-drawer
                v-model="drawerState"
                clipped fixed floating app
                mobile-break-point="1000"
                dark
                class="menu testset-menu">
                                
                <v-list expand class="menu-items">
                    <filter-input-component class="filter" v-model="testSetFilterText" />
                    
                    <!-- GROUPS IF ANY -->
                    <v-list-group
                        no-action
                        sub-group
                        prepend-icon="keyboard_arrow_up"
                        value="true"
                        v-for="(group) in groupsWithNames"
                        :key="`testset-menu-group-${group.Id}`">
                        <template v-slot:activator>
                            <v-list-tile>
                                <v-icon
                                    class="mr-2"
                                    v-if="getTestSetGroupIcon(group) != null" 
                                    v-text="getTestSetGroupIcon(group)"></v-icon>
                                <v-list-tile-title v-text="group.Name"></v-list-tile-title>
                                <v-badge class="mr-3" v-if="showFilterCounts">
                                    <template v-slot:badge>
                                        <span>{{ getGroupFilterMatchCount(group) }}</span>
                                    </template>
                                </v-badge>
                            </v-list-tile>
                        </template>

                        <v-list-tile ripple class="testset-menu-item"
                            :class="{ 'active': (activeSet == set) }"
                            v-for="(set) in filterTestSets(group.Sets)"
                            :key="`testset-menu-group-${group.Id}-set-${set.Id}`"
                            @click="setActiveSet(set)">
                            <v-icon
                                class="mr-2"
                                v-text="getTestSetIcon(set)" 
                                v-if="testSetHasIcon(set)"></v-icon>
                            <v-list-tile-title
                                v-text="set.Name"
                                :class="getTestSetTitleClass(set)"></v-list-tile-title>
                            <v-badge class="mr-3" v-if="showFilterCounts">
                                <template v-slot:badge>
                                    <span>{{ getSetFilterMatchCount(set) }}</span>
                                </template>
                            </v-badge>
                        </v-list-tile>
                    </v-list-group>

                    <!-- WHEN NO GROUPS -->
                    <v-list-tile ripple
                        :class="{ 'active': (activeSet == set) }"
                        v-for="(set) in filterTestSets(testSetsWhenThereIsNoNamedGroups)"
                        :key="`testset-menu-${set.Id}`"
                        @click="setActiveSet(set)">
                        <v-icon
                            class="mr-2"
                            v-text="getTestSetIcon(set)"
                            v-if="testSetHasIcon(set)"></v-icon>
                        <v-list-tile-title 
                            v-text="set.Name"
                            :class="getTestSetTitleClass(set)"></v-list-tile-title>
                        <v-badge class="mr-2" v-if="showFilterCounts">
                            <template v-slot:badge>
                                <span>{{ getSetFilterMatchCount(set) }}</span>
                            </template>
                        </v-badge>
                    </v-list-tile>
                </v-list>
            </v-navigation-drawer>
            
            <!-- CONTENT -->
            <v-container fluid fill-height class="content-root">
                <v-layout>
                    <v-flex>
                        <v-container>
                            <!-- INVALID TESTS -->
                            <v-alert :value="hasInvalidTests" type="error">
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
                            </v-alert>

                            <!-- DATA LOAD ERROR -->
                            <v-alert :value="setSetsLoadStatus.failed" type="error">
                            {{ setSetsLoadStatus.errorMessage }}
                            </v-alert>

                            <!-- LOAD PROGRESS -->
                            <v-progress-linear 
                                v-if="setSetsLoadStatus.inProgress"
                                indeterminate color="green"></v-progress-linear>

                            <!-- TESTS -->
                            <test-set-component
                                v-if="activeSet != null"
                                :module-id="config.Id"
                                :testSet="activeSet"
                                v-on:testClicked="onTestClicked" />
                        </v-container>
                    </v-flex>
                </v-layout>
            </v-container>
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Common/FrontEndOptionsViewModel';
import TestSetViewModel from '../../models/TestSuite/TestSetViewModel';
import TestSetGroupViewModel from '../../models/TestSuite/TestSetGroupViewModel';
import TestsDataViewModel from  '../../models/TestSuite/TestsDataViewModel';
import InvalidTestViewModel from "../../models/TestSuite/InvalidTestViewModel";
import TestSetComponent from '.././TestSuite/TestSetComponent.vue';
import FilterInputComponent from '.././Common/FilterInputComponent.vue';
import LinqUtils from '../../util/LinqUtils';
import TestViewModel from "../../models/TestSuite/TestViewModel";
import TestService from  "../../services/TestService";
import { FetchStatus } from "../../services/abstractions/HCServiceBase";
import ModuleOptions from "../../models/Common/ModuleOptions";
import ModuleConfig from "../../models/Common/ModuleConfig";
import UrlUtils from "../../util/UrlUtils";

@Component({
    components: {
        TestSetComponent,
        FilterInputComponent
    }
})
export default class TestSuitesPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;

    // UI STATE
    drawerState: boolean = true;
    testSetFilterText: string = "";

    testSetGroups: Array<TestSetGroupViewModel> = new Array<TestSetGroupViewModel>();
    activeSet: TestSetViewModel | null = null;
    invalidTests: Array<InvalidTestViewModel> = new Array<InvalidTestViewModel>();

    // Service
    service: TestService = new TestService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    setSetsLoadStatus: FetchStatus = new FetchStatus();

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.$store.commit('showMenuButton', true);

        this.loadData();
    }

    created(): void {
        this.$parent.$parent.$on("onSideMenuToggleButtonClicked", this.toggleSideMenu);
    }

    beforeDestroy(): void {
      this.$parent.$parent.$off('onSideMenuToggleButtonClicked', this.toggleSideMenu);
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }
    
    get hasInvalidTests(): boolean {
        return this.invalidTests.length > 0;
    }

    get hasAnyTests(): boolean {
        return this.testSetGroups.length > 0;
    }

    get groupsWithNames(): Array<TestSetGroupViewModel> {
      return this.testSetGroups.filter(x => x.Name != null);
    }

    get groupWithoutName(): TestSetGroupViewModel {
      return this.testSetGroups.filter(x => x.Name == null)[0];
    }

    get testSetsWhenThereIsNoNamedGroups(): Array<TestSetViewModel> {
      return this.hasAnyGroups || this.groupWithoutName == null ? Array<TestSetViewModel>() : this.groupWithoutName!.Sets || Array<TestSetViewModel>();
    }

    get hasAnyGroups(): boolean {
        return this.groupsWithNames.length > 0;
    }

    get allTestSets(): Array<TestSetViewModel> {
        return this.testSetGroups.map(x => x.Sets).reduce((a, b) => a.concat(b));
    }

    get showFilterCounts(): boolean {
        return this.testSetFilterText.length > 0;
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.service.GetTests(this.setSetsLoadStatus, { onSuccess: (data) => this.onTestSetDataRetrieved(data) })
    }

    onTestSetDataRetrieved(testsData: TestsDataViewModel): void {
        this.invalidTests = testsData.InvalidTests;

        // Init default value
        for(let set of testsData.TestSets) {
            for(let test of set.Tests) {
                for(let param of test.Parameters) {
                    param.Value = param.DefaultValue;
                }
            }
        }

        let groupIndex = -1;
        LinqUtils.GroupByInto(testsData.TestSets, (item) => item.GroupName || "", (key, items) => {
            groupIndex++;
            return {
                Id: `g${groupIndex}`,
                Name: (key === "") ? null : key,
                Sets: items.sort((a,b) => b.UIOrder - a.UIOrder),
                Icon: null,
                UIOrder: (key === "") ? -1 : 0,
            }
        })
        .forEach(x => this.testSetGroups.push(x));

        // Give nameless group a name if any other groups exist
        if (this.testSetGroups.length > 1 && this.groupWithoutName != null) {
            this.groupWithoutName.Name = "Other";
            
            this.testSetGroups.forEach(group => {
                let groupOptions = testsData.GroupOptions.filter(x => x.GroupName == group.Name)[0];
                if (groupOptions != null) {
                    group.UIOrder = groupOptions.UIOrder;
                }
            });
        }

        this.testSetGroups = this.testSetGroups.sort((a,b) => b.UIOrder - a.UIOrder);

        this.setSetsLoadStatus.inProgress = false;
        this.updateSelectionFromUrl();
    }

    filterTestSets(sets: Array<TestSetViewModel>) : Array<TestSetViewModel> {
        return sets.filter(x => this.testSetFilterMatches(x));
    }

    testSetFilterMatches(set: TestSetViewModel): boolean {
        return set.Name.toLowerCase().indexOf(this.testSetFilterText.toLowerCase().trim()) != -1
            || set.Tests.some(x => x.Name.toLowerCase().indexOf(this.testSetFilterText.toLowerCase().trim()) != -1);
    }

    getGroupFilterMatchCount(group: TestSetGroupViewModel): number {
        const initialValue = 0;
        return group.Sets.reduce((sum, obj) => sum + this.getSetFilterMatchCount(obj), initialValue);
    }

    getSetFilterMatchCount(set: TestSetViewModel): number {
        return set.Tests.filter(x => x.Name.toLowerCase().indexOf(this.testSetFilterText.toLowerCase().trim()) != -1).length;
    }

    testSetHasIcon(set: TestSetViewModel): boolean
    {
        return this.getTestSetIcon(set) != null;
    }

    getTestSetIcon(set: TestSetViewModel): string | null
    {
        return null; //set.Icon || "sentiment_satisfied_alt";
    }

    getTestSetTitleClass(set: TestSetViewModel): string
    {
        return (this.activeSet == set) ? "font-weight-black active" : "font-weight-regular";
    }

    getTestSetGroupIcon(group: TestSetGroupViewModel): string | null
    {
        return null; //"folder_open"; //group.Icon || "extension";
    }

    setActiveSet(set: TestSetViewModel, test: string | null = null, updateRoute: boolean = true): void
    {
        this.activeSet = set;

        if (updateRoute)
        {
            let routeParams: any = {
                group: (set.GroupName == null) ? 'other' : UrlUtils.EncodeHashPart(set.GroupName),
                set: UrlUtils.EncodeHashPart(set.Name)
            };
            if (test != null)
            {
                routeParams['test'] = UrlUtils.EncodeHashPart(test);
            }

            this.$router.push({ name: this.config.Id, params: routeParams })
        }
    }

    updateSelectionFromUrl(): void
    {
        const groupFromHash = this.$route.params.group || '';
        const setFromHash = this.$route.params.set;
        const testFromHash = this.$route.params.test;
        if (setFromHash != null && this.trySetActiveSetFromEncodedValues(groupFromHash, setFromHash, testFromHash)) {
            return;
        }
        
        // Fallback to first set in list
        if(this.allTestSets.length > 0) {
            this.setActiveSet(this.allTestSets[0]);
        }
    }

    @Watch('$route')
    onRouteChanged(to: any, from: any): void {
        this.updateSelectionFromUrl();
    }

    trySetActiveSetFromEncodedValues(group: string, set: string, test: string | null = null): boolean
    {
        if (set != null) {
            let matchingSet = this.allTestSets
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

    toggleSideMenu(): void {
        this.drawerState = !this.drawerState;
    }
}
</script>

<style scoped>
.menu {
    /* background-color: var(--v-primary-base); */
    box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.02), 0 3px 2px 0 rgba(0, 0, 0, 0.02), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
}
.filter {
    position: relative;
    margin-left: 44px;
    margin-top: 26px;
    margin-bottom: 18px;
    margin-right: 44px;
}
@media (max-width: 960px) {
    .menu-items { 
        margin-top: 67px;
    }
}
</style>

<style>
.testset-menu {
    background-color: hsla(0, 0%, 16%, 1) !important;
}
/* .testset-menu-item>a {
    color: #fff;
} */
.v-list__group__header--active .v-list__group__header__prepend-icon .v-icon {
    color: #fff;
}
.testset-menu-item.active a {
    background: hsla(0,0%,100%,.08);
}
.testset-menu .v-list__tile--link:hover {
    background: hsla(0,0%,100%,.08);
}
.testset-menu-item.active .v-list__tile {
    /* border-left: 4px solid var(--v-primary-base); */
    border-left: 4px solid #d1495b;
    padding-left: 42px !important;
}
.v-list__group::before, .v-list__group::after {
    display: none;
}
.v-list__tile {
    height: 42px;
}
.v-list__group.v-list__group--active {
    margin-bottom: 15px;
}
.testset-menu .v-list__group__header .v-list__tile__title {
    font-weight: 600;
}
.v-list__group {
    margin-bottom: 10px;
}
.menu .v-list__group__header__prepend-icon {
    padding-left: 14px !important;
    min-width: inherit !important;
}
.menu-items .v-list__tile--link {
    padding-left: 46px !important;
}
</style>