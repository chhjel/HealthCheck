<!-- src/components/modules/TestSuite/result_data/data_types/TestResultFileDownloadDataComponent.vue -->
<template>
    <div>
      <div v-if="fileData">
        <p v-if="fileData.Description" class="mb-1">{{ fileData.Description }}</p>
        <btn-component :href="downloadUrl" target="_blank">
          <icon-component left dark>file_download</icon-component>
          {{ fileData.Name }}
        </btn-component>
      </div>
    </div>
</template>

<script lang="ts">
import FrontEndOptionsViewModel from "@models/Common/FrontEndOptionsViewModel";
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestResultDataDumpViewModel } from '@generated/Models/Core/TestResultDataDumpViewModel';
import { StoreUtil } from "@util/StoreUtil";

interface FileDownloadData
{
  Id: string;
  Type: string;
  Name: string;
  Description: string | null;
}

@Options({
    components: {
    }
})
export default class TestResultFileDownloadDataComponent extends Vue {
    @Prop({ required: true })
    resultData!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;

    fileData: FileDownloadData | null = null;

    mounted(): void {
      this.$emit('hideCopyButton');
      this.$emit('hideFullscreenButton');

      this.fileData = JSON.parse(this.resultData.Data) as FileDownloadData;
    }
    
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get downloadUrl(): string {
      if (!this.fileData)
      {
        return '';
      }

      const base = this.globalOptions.EndpointBase;
      const type = this.fileData.Type ? this.fileData.Type : '';
      return `${base}/TMDownloadFile/${type}__${this.fileData.Id}`;
    }
}
</script>

<style scoped>
</style>
