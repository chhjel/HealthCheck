<!-- src/components/modules/TestSuite/result_data/TestResultDataComponent.vue -->
<template>
    <div class="data-dump-wrapper">
        <div v-if="resultData.Title && resultData.Title.length > 0"
          class="data-dump-title">{{resultData.Title}}</div>
        <component
            :class="`data-dump data-dump-${resultData.Type.toLowerCase()}`"
            :resultData="resultData"
            :fullscreen="false"
            :is="getDataComponentNameFromType(resultData.Type)"
            @hideCopyButton="showCopyButton = false"
            @hideFullscreenButton="showFullscreenButton = false">
        </component>

        <div v-if="clean == false" class="data-dump-actions pt-0">
          <btn-component outline small color="accent" class="data-dump-action-button mt-2 mr-2"
            v-if="showCopyButton"
            @click="putDataOnClipboard">Copy</btn-component>
        
          <btn-component outline small color="accent" class="data-dump-action-button mt-2"
            @click="downloadData" 
            v-if="showDownloadButton">Download '{{ resultData.DownloadFileName }}'</btn-component>

          <btn-component outline small color="accent" class="data-dump-action-button mt-2 mr-2" v-if="showFullscreenButton" @click="showFullscreen=true">Fullscreen</btn-component>
          <dialog-component
            v-model:value="showFullscreen"
            @keydown.esc="showFullscreen = false"
            fullscreen hide-overlay>
            <template #header>
              {{resultData.Title}}
            </template>
            <template #footer>
                  <btn-component dark flat @click="putDataOnClipboard">Put data on clipboard</btn-component>
                  <btn-component dark flat @click="showFullscreen = false">Close</btn-component>
            </template>
            <div>
              <component
                  :class="`data-dump data-dump-${resultData.Type.toLowerCase()}`"
                  :resultData="resultData"
                  :fullscreen="showFullscreen"
                  :is="getDataComponentNameFromType(resultData.Type)">
              </component>
            </div>
          </dialog-component>
        </div>
        
        <textarea style="display:none;" ref="copyValue" :value="resultData.Data" />
        <snackbar-component
          v-model:value="showCopyAlert"
          :timeout="5000"
          :color="copyAlertColor"
        >
          {{ copyAlertText }}
          <btn-component
            flat
            @click="showCopyAlert = false">
            Close
          </btn-component>
        </snackbar-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestResultDataDumpViewModel } from '@generated/Models/Core/TestResultDataDumpViewModel';
// Parameter input components
import UnknownDataTypeComponent from '@components/modules/TestSuite/result_data/UnknownDataTypeComponent.vue';
import TestResultPlainTextDataComponent from '@components/modules/TestSuite/result_data/data_types/TestResultPlainTextDataComponent.vue';
import TestResultCodeDataComponent from '@components/modules/TestSuite/result_data/data_types/TestResultCodeDataComponent.vue';
import TestResultXmlDataComponent from '@components/modules/TestSuite/result_data/data_types/TestResultXmlDataComponent.vue';
import TestResultJsonDataComponent from '@components/modules/TestSuite/result_data/data_types/TestResultJsonDataComponent.vue';
import TestResultHtmlDataComponent from '@components/modules/TestSuite/result_data/data_types/TestResultHtmlDataComponent.vue';
import TestResultImageUrlsDataComponent from '@components/modules/TestSuite/result_data/data_types/TestResultImageUrlsDataComponent.vue';
import TestResultUrlsDataComponent from '@components/modules/TestSuite/result_data/data_types/TestResultUrlsDataComponent.vue';
import TestResultTimelineDataComponent from '@components/modules/TestSuite/result_data/data_types/TestResultTimelineDataComponent.vue';
import TestResultTimingsDataComponent from '@components/modules/TestSuite/result_data/data_types/TestResultTimingsDataComponent.vue';
import TestResultFileDownloadDataComponent from '@components/modules/TestSuite/result_data/data_types/TestResultFileDownloadDataComponent.vue';
import DownloadUtil from '@util/DownloadUtil';

@Options({
    components: {
      // Parameter input components
      UnknownDataTypeComponent,
      TestResultPlainTextDataComponent,
      TestResultXmlDataComponent,
      TestResultJsonDataComponent,
      TestResultHtmlDataComponent,
      TestResultImageUrlsDataComponent,
      TestResultUrlsDataComponent,
      TestResultCodeDataComponent,
      TestResultTimelineDataComponent,
      TestResultTimingsDataComponent,
      TestResultFileDownloadDataComponent
    }
})
export default class TestResultDataComponent extends Vue {
    @Prop({ required: true })
    resultData!: TestResultDataDumpViewModel;
    @Prop()
    clean!: boolean;

    showCopyAlert: boolean = false;
    copyAlertText: string = "";
    copyAlertColor: string = "success";
    showFullscreen: boolean = false;
    showCopyButton: boolean = true;
    showFullscreenButton: boolean = true;

    mounted(): void {
    }

    getDataComponentNameFromType(typeName: string): string
    {
      let componentName = `TestResult${typeName}DataComponent`;
      let componentExists = (this.$options!.components![componentName] != undefined);
      return componentExists 
        ? componentName
        : "UnknownDataTypeComponent";
    }

    get hasTitle(): boolean {
      return this.resultData.Title != null && this.resultData.Title.length > 0;
    }

    get rowCount(): number {
      let lineCount = this.resultData.Data.split(/\r\n|\r|\n/).length;
      return Math.min(10, lineCount);
    }

    get showDownloadButton(): boolean {
      return !!this.resultData.DownloadFileName;
    }
    
    putDataOnClipboard(): void {
      let copySourceElement = this.$refs.copyValue as HTMLTextAreaElement;
      copySourceElement.setAttribute('style', 'display:inherit;');
      copySourceElement.select()

      try {
        let successful = document.execCommand('copy');
        if (successful) {
          this.ShowCopyAlert('Data successfully put on clipboard.', successful);
        } else {
          this.ShowCopyAlert('Oops, unable to copy :(', successful);
        }
      } catch (err) {
        this.ShowCopyAlert('Oops, unable to copy :(', false);
      }

      copySourceElement.setAttribute('style', 'display:none;');
      
      const selection = window.getSelection();
      if (selection != null) {
        selection.removeAllRanges();
      }
    }

    ShowCopyAlert(msg: string, isSuccess: boolean): void {
      this.showCopyAlert = true;
      this.copyAlertText = msg;
      this.copyAlertColor = (isSuccess) ? "success" : "error";
    }

    downloadData(): void {
      const filename = this.resultData.DownloadFileName || 'data.txt';
      DownloadUtil.downloadText(filename, this.resultData.Data || '');
    }
}
</script>

<style scoped lang="scss">
.data-dump-wrapper {
  margin-bottom: 1.2rem;
}
.data-dump-actions {
  padding-left: 0;
  flex-flow: wrap;
}
.data-dump-title {
  font-weight: 600;
  margin-bottom: 6px;
}
.data-dump-action-button {
  max-width: 100%;
}
</style>
