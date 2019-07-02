<!-- src/components/TestSetComponent.vue -->
<template>
    <div>
        <h2 class="display-3">{{ testSet.Name }}</h2>
        <div class="subheading">{{ testSet.Description }}</div>
        <div class="mb-4"></div>

        <test-component
            v-for="(test, index) in testSet.Tests"
            :key="'test'+index"
            :test="test"
            class="mb-2" />
        
        <div class="mb-4"></div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestSetViewModel from '../models/TestSetViewModel';
import TestComponent from './TestComponent.vue';

@Component({
    components: {
        TestComponent
    }
})
export default class TestSetComponent extends Vue {
    @Prop({ required: true })
    testSet!: TestSetViewModel;

    mounted(): void {
    }
}
</script>

<style scoped>
</style>
