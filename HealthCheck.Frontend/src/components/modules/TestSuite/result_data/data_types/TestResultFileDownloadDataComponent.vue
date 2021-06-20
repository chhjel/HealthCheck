<!-- src/components/modules/TestSuite/result_data/data_types/TestResultFileDownloadDataComponent.vue -->
<template>
    <div>
      WIP result
      <code>{{ data }}</code>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import TestResultDataDumpViewModel from  '../../../../../models/modules/TestSuite/TestResultDataDumpViewModel';

@Component({
    components: {
    }
})
export default class TestResultFileDownloadDataComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;

    mounted(): void {
      this.$emit('hideCopyButton');
      this.$emit('hideFullscreenButton');
    }
}
</script>

<style scoped>
</style>
