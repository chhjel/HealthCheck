<!-- src/components/HealthCheckPageComponent.vue -->
<template>
    <div>
        <v-app light>
            <!-- NAVIGATION DRAWER -->
            <v-navigation-drawer
                v-model="drawerState"
                clipped fixed floating app>
                <v-list>
                    <v-list-tile>
                        <v-list-tile-action>
                        <v-icon>dashboard</v-icon>
                        </v-list-tile-action>
                        <v-list-tile-title>Overview</v-list-tile-title>
                    </v-list-tile>

                    <v-list-group
                        v-if="hasAnyTests"
                        :no-action="hasAnyGroups"
                        prepend-icon="code"
                        value="true">
                        <template v-slot:activator>
                            <v-list-tile>
                                <v-list-tile-title>Test Suites</v-list-tile-title>
                            </v-list-tile>
                        </template>

                        <v-text-field v-model="testSetFilterText" />

                        <!-- GROUPS IF ANY -->
                        <v-list-group
                            no-action
                            sub-group
                            value="true"
                            v-for="(group) in groupsWithNames"
                                :key="`testset-menu-group-${group.Id}`">
                            <template v-slot:activator>
                                <v-list-tile>
                                    <v-icon v-text="getTestSetGroupIcon(group)"></v-icon>
                                    <v-list-tile-title v-text="group.Name"></v-list-tile-title>
                                    <v-badge class="mr-2" v-if="showFilterCounts">
                                        <template v-slot:badge>
                                            <span>{{ getGroupFilterMatchCount(group) }}</span>
                                        </template>
                                    </v-badge>
                                </v-list-tile>
                            </template>

                            <v-list-tile ripple
                                v-for="(set) in filterTestSets(group.Sets)"
                                :key="`testset-menu-group-${group.Id}-set-${set.Id}`"
                                @click="setActiveSet(set)">
                                <v-icon 
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
                        </v-list-group>

                        <!-- WHEN NO GROUPS -->
                        <v-list-tile ripple
                            v-for="(set) in filterTestSets(testSetsWhenThereIsNoNamedGroups)"
                            :key="`testset-menu-${set.Id}`"
                            @click="setActiveSet(set)">
                            <v-icon
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

                    </v-list-group>
                </v-list>
            </v-navigation-drawer>
            
            <!-- TOOLBAR -->
            <v-toolbar  clipped-left class="hidden-md-and-up">
                <v-toolbar-side-icon @click.stop="drawerState = !drawerState"></v-toolbar-side-icon>
                <v-toolbar-title>{{ options.ApplicationTitle }}</v-toolbar-title>
            </v-toolbar>

            <!-- CONTENT -->
            <v-content>
                <v-container fluid fill-height>
                    <v-layout>
                        <v-flex>
                            <v-alert
                                :value="testSetDataLoadFailed"
                                type="error">
                            {{ testSetDataFailedErrorMessage }}
                            </v-alert>

                            <v-progress-circular 
                                v-if="testSetDataLoadInProgress"
                                indeterminate color="green"></v-progress-circular>

                            <test-set-component
                                v-if="activeSet != null"
                                :testSet="activeSet"
                                :executeTestEndpoint="options.ExecuteTestEndpoint"
                                :inludeQueryStringInApiCalls="options.InludeQueryStringInApiCalls" />
                        </v-flex>
                    </v-layout>
                </v-container>
            </v-content>

            <!-- FOOTER -->
            <v-footer app fixed>
                <span>&copy; {{ new Date().getFullYear() }}</span>
            </v-footer>
        </v-app>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../models/FrontEndOptionsViewModel';
import TestSetViewModel from '../models/TestSetViewModel';
import TestSetGroupViewModel from '../models/TestSetGroupViewModel';
import TestsDataViewModel from  '../models/TestsDataViewModel';
import TestSetComponent from './TestSuite/TestSetComponent.vue';
import LinqUtils from '../util/LinqUtils';

@Component({
    components: {
        TestSetComponent
    }
})
export default class HealthCheckPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;
    
    // UI STATE
    drawerState: boolean = true;
    testSetFilterText: string = "";

    testSetGroups: Array<TestSetGroupViewModel> = new Array<TestSetGroupViewModel>();
    activeSet: TestSetViewModel | null = null;

    testSetDataLoadInProgress: boolean = false;
    testSetDataLoadFailed: boolean = false;
    testSetDataFailedErrorMessage: string = "";

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.initTestSets();
    }

    ////////////////
    //  GETTERS  //
    //////////////
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
      return this.hasAnyGroups ? Array<TestSetViewModel>() : this.groupWithoutName!.Sets || Array<TestSetViewModel>();
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
    initTestSets(): void {
        this.testSetDataLoadInProgress = true;
        this.testSetDataLoadFailed = false;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.GetTestsEndpoint}${queryStringIfEnabled}`;
        fetch(url, {
            credentials: 'include',
            method: "GET",
            // body: JSON.stringify(payload),
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => response.json())
        .then((testsData: TestsDataViewModel) => this.onTestSetDataRetrieved(testsData))
        .catch((e) => {
            this.testSetDataLoadInProgress = false;
            this.testSetDataLoadFailed = true;
            this.testSetDataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    onTestSetDataRetrieved(testsData: TestsDataViewModel): void {
        // Init default value
        for(let set of testsData.TestSets) {
            for(let test of set.Tests) {
                for(let param of test.Parameters) {
                    param.Value = param.DefaultValue;
                }
            }
        }

        let groupIndex = -1;
        LinqUtils.GroupByInto(testsData.TestSets, "GroupName", (key, items) => {
            groupIndex++;
            return {
                Id: `g${groupIndex}`,
                Name: (key === "null") ? null : key,
                Sets: items.sort((a,b) => b.UIOrder - a.UIOrder),
                Icon: null,
                UIOrder: (key === "null") ? -1 : 0,
            }
        })
        .forEach(x => this.testSetGroups.push(x));

        // Give nameless group a name if any other groups exist
        if (this.testSetGroups.length > 1 && this.groupWithoutName != null) {
            this.groupWithoutName.Name = "Other";
            
            this.testSetGroups.forEach(group => {
                let groupOptions = testsData.GroupOptions.filter(x => x.GroupName == group.Name)[0];
                if (groupOptions != null) {
                    group.Icon = groupOptions.Icon;
                    group.UIOrder = groupOptions.UIOrder;
                }
            });
        }

        this.testSetGroups = this.testSetGroups.sort((a,b) => b.UIOrder - a.UIOrder)

        this.testSetDataLoadInProgress = false;
        this.setInitialActiveTestSet();
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
        return set.Icon;
    }

    getTestSetTitleClass(set: TestSetViewModel): string
    {
        return (this.activeSet == set) ? "font-weight-bold" : "font-weight-regular";
    }

    getTestSetGroupIcon(group: TestSetGroupViewModel): string
    {
        return group.Icon || "extension";
    }

    urlParameterCurrentSet: string = "set";
    setActiveSet(set: TestSetViewModel): void
    {
        this.activeSet = set;

        let params = new URLSearchParams(location.search);
        params.set(this.urlParameterCurrentSet, set.Name);
        let newUrl = `${window.location.pathname}?${params.toString()}`;
        window.history.replaceState(null, window.name, newUrl);
    }

    setInitialActiveTestSet(): void
    {
        // Attempt to get from query string first
        let params = new URLSearchParams(location.search);
        if (params.has(this.urlParameterCurrentSet)) {
            let queryStringValue = params.get(this.urlParameterCurrentSet);
            let matchingSet = this.allTestSets.filter(x => x.Name === queryStringValue)[0];

            if (matchingSet != null) {
                this.setActiveSet(matchingSet);
                return;
            }
        }
        
        // Fallback to first set in list
        if(this.allTestSets.length > 0) {
            this.setActiveSet(this.allTestSets[0]);
        }
    }
}
</script>

<style scoped>
.root {
    background-color: #fff;
}
</style>
