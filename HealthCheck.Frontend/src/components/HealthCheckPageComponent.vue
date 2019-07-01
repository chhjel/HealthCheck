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
                        prepend-icon="code"
                        value="true">
                        <template v-slot:activator>
                            <v-list-tile>
                                <v-list-tile-title>Test Suites</v-list-tile-title>
                            </v-list-tile>
                        </template>

                        <v-list-tile ripple
                            v-for="(set, index) in testSets"
                            :key="'testset-menu-'+index"
                            @click="setActiveSet(set)">
                            <v-list-tile-action class="ml-4">
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
                <v-toolbar-title>Health Check</v-toolbar-title>
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

                            <test-set-component v-if="activeSet != null" :testSet="activeSet" />
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
import TestSetViewModel from '../models/TestSetViewModel';
import TestSetComponent from './TestSetComponent.vue';

@Component({
    components: {
        TestSetComponent
    }
})
export default class HealthCheckPageComponent extends Vue {
    // UI STATE
    drawerState: boolean = true;
    // activeTab: number = 0;

    testSets: Array<TestSetViewModel> = new Array<TestSetViewModel>();
    activeSet: TestSetViewModel | null = null;

    testSetDataLoadInProgress: boolean = false;
    testSetDataLoadFailed: boolean = false;
    testSetDataFailedErrorMessage: string = "";

    mounted(): void
    {
        this.initTestSets();
    }

    initTestSets(): void {
        this.testSetDataLoadInProgress = true;
        this.testSetDataLoadFailed = false;

        let url = `/HealthCheck/GetTests`;
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

        this.testSets = testSets;
        this.testSetDataLoadInProgress = false;
        
        // Set first as selected
        if (this.testSets.length > 0) {
            this.setActiveSet(this.testSets[0]);
        }
    }

    getTestSetIcon(set: TestSetViewModel): string
    {
        // ToDo get based on results.
        return "extension";
    }

    getTestSetTitleClass(set: TestSetViewModel): string
    {
        return (this.activeSet == set) ? "font-weight-bold" : "font-weight-regular";
    }

    setActiveSet(set: TestSetViewModel): void
    {
        this.activeSet = set;
    }
}
</script>

<style scoped>
.root {
    background-color: #fff;
}
</style>
