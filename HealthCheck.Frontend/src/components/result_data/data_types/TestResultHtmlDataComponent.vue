<!-- src/components/result_data/data_types/TestResultHtmlDataComponent.vue -->
<template>
    <div>
      <div v-html="data.Data"></div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestResultDataDumpViewModel from "../../../models/TestResultDataDumpViewModel";

@Component({
    components: {
    }
})
export default class TestResultHtmlDataComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;

    mounted(): void {
    }
}
</script>

<style>
</style>
