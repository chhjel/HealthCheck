<!-- src/components/HealthCheckPageComponent.vue -->
<template>
    <div>
        <v-app :dark="darkTheme" class="root">
            <v-content>
                <v-container fluid fill-height>
                    <v-layout justify-center>
                    <v-flex xs12 sm10 md10>
                        <v-tabs v-model="activeTab" 
                            class="pr-4 pl-4">
                            <v-tab ripple>
                                <!-- <b></b> -->
                                Run checks
                            </v-tab>
                            <v-tab ripple>
                                Status
                            </v-tab>

                            <v-tab-item>
                                <div class="pa-2">
                                    <v-alert
                                        :value="testSetDataLoadFailed"
                                        type="error">
                                    {{ testSetDataFailedErrorMessage }}
                                    </v-alert>

                                    <v-progress-circular 
                                        v-if="testSetDataLoadInProgress"
                                        indeterminate color="green"></v-progress-circular>

                                    <test-set-component
                                        v-for="(set, index) in testSets"
                                        :key="'testset'+index"
                                        :testSet="set" />
                                </div>
                            </v-tab-item>
                            
                            <v-tab-item>
                                <v-card flat>
                                    ToDo
                                </v-card>
                            </v-tab-item>
                        </v-tabs>
                    </v-flex>
                    </v-layout>
                </v-container>
                </v-content>
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
    // @Prop({ required: true })
    // bundle!: ReleaseNotesBundle;

    darkTheme: boolean = false;
    activeTab: number = 0;
    testSets: Array<TestSetViewModel> = new Array<TestSetViewModel>();

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
        for(let set of testSets) {
            for(let test of set.Tests) {
                for(let param of test.Parameters) {
                    param.Value = param.DefaultValue;
                }
            }
        }
        this.testSets = testSets;
        this.testSetDataLoadInProgress = false;
    }
}
</script>

<style scoped>
.root {
    background-color: #fff;
}
</style>
