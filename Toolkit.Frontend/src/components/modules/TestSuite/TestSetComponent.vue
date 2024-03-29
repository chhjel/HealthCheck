<!-- src/components/modules/TestSuite/TestSetComponent.vue -->
<template>
    <div>
        <div class="testset-header">
            <div class="title-button-wrapper">
                <h2 class="testset-title font-weight-bold">{{ testSet.Name }}</h2>
                <btn-component color="primary" outline round
                    v-if="testSet.AllowRunAll"
                    @click.stop.prevent="executeAllTestsInSet()"
                    :disabled="anyTestInProgress"
                    class="run-all-tests-button">

                    <progress-circular-component class="mr-2"
                        v-if="anyTestInProgress" size="22"
                        :indeterminate="showIndeterminateProgress"
                        :value="allTestsProgress"></progress-circular-component>
                    <icon-component color="primary"  v-if="!anyTestInProgress">play_arrow</icon-component>
                    
                    {{executeAllTestsInSetButtonText}}
                </btn-component>
            </div>
            <div class="subheading testset-subtitle" v-html="testSet.Description"></div>
        </div>

        <div class="mb-4" style="clear:both;"></div>

        <test-component
            v-for="(test) in filteredTests"
            :key="`set-${testSet.Id}-test-${test.Id}`"
            :test="test"
            :module-id="moduleId"
            v-on:testStarted="onTestStarted"
            v-on:testStopped="onTestStopped"
            v-on:testClicked="onTestClicked"
            class="mb-4" />
        
        <div class="mb-4"></div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestSetViewModel } from '@generated/Models/Core/TestSetViewModel';
import { TestViewModel } from '@generated/Models/Core/TestViewModel';
import TestComponent from '@components/modules/TestSuite/TestComponent.vue';
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import EventBus from "@util/EventBus";

@Options({
    components: {
        TestComponent
    }
})
export default class TestSetComponent extends Vue {
    @Prop({ required: true })
    moduleId!: string;

    @Prop({ required: true })
    testSet!: TestSetViewModel;

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
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }
    
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

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch("testSet")
    onTestSetChanged(): void {
        this.currentlyExecutingTestIds = new Array<string>();
        this.testsFinishedCount = 0;
        this.testsTotalCount = 0;
        this.showIndeterminateProgress = true;
    }

    ////////////////
    //  METHODS  //
    //////////////
    executeAllTestsInSet(): void {
        this.showIndeterminateProgress = false;
        this.testsFinishedCount = 0;
        this.testsTotalCount = this.testSet.Tests.length;
        // this.$emit("executeAllTestsInSet");
        EventBus.notify("executeAllTestsInSet");
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

    onTestClicked(test: TestViewModel): void {
        this.$emit('testClicked', test);
    }
}
</script>

<style scoped>
.run-all-tests-button{
    border-radius: 25px;
    text-transform: inherit;
    padding-right: 16px;
    border-width: 2px;
    font-weight: 600;
}
.testset-title{
    font-size: 26px;
}
.title-button-wrapper {
    display: flex;
    align-items: center;
}
</style>
