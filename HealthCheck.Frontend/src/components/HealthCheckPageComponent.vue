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

                        <!-- GROUPS IF ANY -->
                        <v-list-group
                            no-action
                            sub-group
                            value="true"
                            v-for="(group, groupIndex) in groupsWithNames"
                                :key="`testset-menu-group-${groupIndex}`">
                            <template v-slot:activator>
                                <v-list-tile>
                                    <v-list-tile-title v-text="group.Name"></v-list-tile-title>
                                    <v-list-tile-action>
                                        <v-icon v-text="getTestSetGroupIcon(group)"></v-icon>
                                    </v-list-tile-action>
                                </v-list-tile>
                            </template>

                            <v-list-tile ripple
                                v-for="(set, setIndex) in group.Sets"
                                :key="`testset-menu-group-${groupIndex}-set-${setIndex}`"
                                @click="setActiveSet(set)">
                                <v-list-tile-action class="ml-4" v-if="testSetHasIcon(set)">
                                    <v-icon v-text="getTestSetIcon(set)"></v-icon>
                                </v-list-tile-action>
                                <v-list-tile-title 
                                    v-text="set.Name"
                                    :class="getTestSetTitleClass(set)"></v-list-tile-title>
                            </v-list-tile>
                        </v-list-group>

                        <!-- WHEN NO GROUPS -->
                        <v-list-tile ripple
                            v-for="(set, index) in testSetsWhenThereIsNoNamedGroups"
                            :key="'testset-menu-'+index"
                            @click="setActiveSet(set)">
                            <v-list-tile-action class="ml-4" v-if="testSetHasIcon(set)">
                                <v-icon v-text="getTestSetIcon(set)"></v-icon>
                            </v-list-tile-action>
                            <v-list-tile-title 
                                v-text="set.Name"
                                :class="getTestSetTitleClass(set)"></v-list-tile-title>
                        </v-list-tile>

                    </v-list-group>
                </v-list>
            </v-navigation-drawer>
            
            <!-- TOOLBAR -->
            <v-toolbar  clipped-left class="hidden-sm-and-up">
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
        .then((testSets: Array<TestSetViewModel>) => this.onTestSetDataRetrieved(testSets))
        .catch((e) => {
            this.testSetDataLoadInProgress = false;
            this.testSetDataLoadFailed = true;
            this.testSetDataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    onTestSetDataRetrieved(testSets: Array<TestSetViewModel>): void {
        // Init default value
        for(let set of testSets) {
            for(let test of set.Tests) {
                for(let param of test.Parameters) {
                    param.Value = param.DefaultValue;
                }
            }
        }

        // ToDo group order and icons. Add group definition?
        LinqUtils.GroupByInto(testSets, "GroupName", (key, items) => {
            return {
                Name: (key === "null") ? null : key,
                Sets: items.sort((a,b) => a.UIOrder - b.UIOrder),
                Icon: null,
                UIOrder: 0,
            }
        }).forEach(x => this.testSetGroups.push(x));

        // Give nameless group a name if any other groups exist
        if (this.testSetGroups.length > 1 && this.groupWithoutName != null) {
            this.groupWithoutName.Name = "Other";
        }

        this.testSetDataLoadInProgress = false;
        this.setInitialActiveTestSet();
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
        return "extension";
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
