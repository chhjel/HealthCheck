<!-- src/components/modules/TestSuite/result_data/TestResultDataComponent.vue -->
<template>
    <div class="data-dump-wrapper">
        <div v-if="data.Title && data.Title.length > 0"
          class="data-dump-title">{{data.Title}}</div>
        <component
            :class="`data-dump data-dump-${data.Type.toLowerCase()}`"
            :data="data"
            :fullscreen="false"
            :is="getDataComponentNameFromType(data.Type)"
            @hideCopyButton="showCopyButton = false"
            @hideFullscreenButton="showFullscreenButton = false">
        </component>

        <v-card-actions v-if="clean == false" class="data-dump-actions pt-0">
          <v-btn outline small color="secondary-darken2" class="data-dump-action-button mt-2 mr-2"
            v-if="showCopyButton"
            @click="putDataOnClipboard">Copy</v-btn>

          <v-dialog
            v-if="showFullscreenButton"
            v-model="showFullscreen"
            @keydown.esc="showFullscreen = false"
            fullscreen hide-overlay transition="dialog-transition">
            <template v-slot:activator="{ on }">
              <v-btn outline small color="secondary-darken2" class="data-dump-action-button mt-2 mr-2 ml-0" v-on="on">Fullscreen</v-btn>
            </template>
            <v-card>
              <!-- DIALOG TOOLBAR -->
              <v-toolbar dark color="primary">
                <v-btn icon dark @click="showFullscreen = false">
                  <v-icon>close</v-icon>
                </v-btn>
                <v-toolbar-title>{{data.Title}}</v-toolbar-title>
                <v-spacer></v-spacer>
                <v-toolbar-items>
                  <v-btn dark flat @click="putDataOnClipboard">Put data on clipboard</v-btn>
                </v-toolbar-items>
                <v-toolbar-items>
                  <v-btn dark flat @click="showFullscreen = false">Close</v-btn>
                </v-toolbar-items>
              </v-toolbar>
              <!-- DIALOG CONTENTS -->
              <component
                  :class="`data-dump data-dump-${data.Type.toLowerCase()}`"
                  :data="data"
                  :fullscreen="showFullscreen"
                  :is="getDataComponentNameFromType(data.Type)">
              </component>
            </v-card>
          </v-dialog>
        
          <v-btn outline small color="secondary-darken2" class="data-dump-action-button mt-2"
            @click="downloadData" 
            v-if="showDownloadButton">Download '{{ data.DownloadFileName }}'</v-btn>
        </v-card-actions>
        
        <textarea style="display:none;" ref="copyValue" :value="data.Data" />
        <v-snackbar
          v-model="showCopyAlert"
          :timeout="5000"
          :color="copyAlertColor"
          :bottom="true"
        >
          {{ copyAlertText }}
          <v-btn
            flat
            @click="showCopyAlert = false">
            Close
          </v-btn>
        </v-snackbar>
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
    data!: TestResultDataDumpViewModel;
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
      return this.data.Title != null && this.data.Title.length > 0;
    }

    get rowCount(): number {
      let lineCount = this.data.Data.split(/\r\n|\r|\n/).length;
      return Math.min(10, lineCount);
    }

    get showDownloadButton(): boolean {
      return !!this.data.DownloadFileName;
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
      const filename = this.data.DownloadFileName || 'data.txt';
      DownloadUtil.downloadText(filename, this.data.Data || '');
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
<style lang="scss">
.data-dump-action-button {
  .v-btn__content {
    text-overflow: ellipsis;
    white-space: nowrap;
    overflow: hidden;
    max-width: 100%;
    display: inline-block;
  }
}
</style>
