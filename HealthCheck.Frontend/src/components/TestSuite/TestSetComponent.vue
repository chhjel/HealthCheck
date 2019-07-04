<!-- src/components/TestSetComponent.vue -->
<template>
    <div>
        <h2 class="display-3">{{ testSet.Name }}</h2>
        <div class="subheading">{{ testSet.Description }}</div>

        <v-text-field v-model="testFilterText" />
        <div class="mb-4"></div>

        <test-component
            v-for="(test) in filteredTests"
            :key="`set-${testSet.Id}-test-${test.Id}`"
            :test="test"
            :executeTestEndpoint="executeTestEndpoint"
            :inludeQueryStringInApiCalls="inludeQueryStringInApiCalls"
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

    get filteredTests(): Array<TestViewModel>
    {
        return this.testSet.Tests
            .filter(x => x.Name.toLowerCase().indexOf(this.testFilterText.toLowerCase().trim()) != -1);
    }
}
</script>

<style scoped>
</style>
