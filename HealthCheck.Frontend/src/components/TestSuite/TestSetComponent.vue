<!-- src/components/TestSetComponent.vue -->
<template>
    <div>
        <div class="testset-header">
            <v-btn ripple color="primary" large
                @click.stop.prevent="executeAllTestsInSet()"
                :disabled="anyTestInProgress"
                class="mt-4 run-all-tests-button">

                <v-progress-circular class="mr-2"
                    v-if="anyTestInProgress"
                    :indeterminate="showIndeterminateProgress"
                    :value="allTestsProgress"></v-progress-circular>
                <v-icon color="white" large v-if="!anyTestInProgress">play_arrow</v-icon>
                
                {{executeAllTestsInSetButtonText}}
            </v-btn>

            <h2 class="display-3 testset-title">{{ testSet.Name }}</h2>
            <div class="subheading testset-subtitle">{{ testSet.Description }}</div>
        </div>

        <v-text-field v-model="testFilterText" />
        <div class="mb-4"></div>

        <test-component
            v-for="(test) in filteredTests"
            :key="`set-${testSet.Id}-test-${test.Id}`"
            :test="test"
            :executeTestEndpoint="executeTestEndpoint"
            :inludeQueryStringInApiCalls="inludeQueryStringInApiCalls"
            v-on:testStarted="onTestStarted"
            v-on:testStopped="onTestStopped"
            class="mb-2" />
        
        <div class="mb-4"></div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestSetViewModel from '../../models/TestSetViewModel';
import TestViewModel from '../../models/TestViewModel';
import TestComponent from './TestComponent.vue';

@Component({
    components: {
        TestComponent
    }
})
export default class TestSetComponent extends Vue {
    @Prop({ required: true })
    testSet!: TestSetViewModel;
    
    @Prop({ required: true })
    executeTestEndpoint!: string;
    @Prop({ required: true })
    inludeQueryStringInApiCalls!: string;

    testFilterText: string = "";
    currentlyExecutingTestIds: Array<string> = new Array<string>();
    testsFinishedCount: number = 0;
    testsTotalCount: number = 0;
    showIndeterminateProgress: boolean = true;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
    }
    
    ////////////////
    //  GETTERS  //
    //////////////
    get filteredTests(): Array<TestViewModel>
    {
        return this.testSet.Tests
            .filter(x => x.Name.toLowerCase().indexOf(this.testFilterText.toLowerCase().trim()) != -1);
    }

    get anyTestInProgress(): boolean {
        return this.currentlyExecutingTestIds.length > 0;
    }

    get allTestsProgress(): number {
        return Math.round((this.testsFinishedCount / this.testsTotalCount) * 100);
    }

    get executeAllTestsInSetButtonText(): string
    {
      return this.anyTestInProgress
        ? "Running.."
        : "Run all";
    }

    ////////////////
    //  METHODS  //
    //////////////
    executeAllTestsInSet(): void {
        this.showIndeterminateProgress = false;
        this.testsFinishedCount = 0;
        this.testsTotalCount = this.testSet.Tests.length;
        this.$emit("executeAllTestsInSet");
    }

    onTestStarted(testId: string): void {
        this.currentlyExecutingTestIds.push(testId);
    }

    onTestStopped(testId: string): void {
        this.currentlyExecutingTestIds = this.currentlyExecutingTestIds.filter(x => x != testId);
        this.testsFinishedCount++;
        
        if (!this.anyTestInProgress) {
            this.showIndeterminateProgress = true;
        }
    }
}
</script>

<style scoped>
.run-all-tests-button{
    float: right;
}
/*
.testset-header {
}
.testset-title{
}
.testset-subtitle{
} */
</style>
