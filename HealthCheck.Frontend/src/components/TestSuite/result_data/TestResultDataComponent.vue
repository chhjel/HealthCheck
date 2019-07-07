<!-- src/components/result_data/TestResultDataComponent.vue -->
<template>
    <div>
        <component
            :class="`data-dump data-dump-${data.Type.toLowerCase()}`"
            :data="data"
            :is="getDataComponentNameFromType(data.Type)">
        </component>

        <v-card-actions>
          <v-btn flat color="secondary-darken2" @click="putDataOnCLipboard">Copy</v-btn>
          <v-btn flat color="secondary-darken2">Fullscreen</v-btn>
          // ToDo move snackbar here
        </v-card-actions>
        
        <textarea type="hidden" ref="copyValue" :value="data.Data" />
        <v-snackbar
          v-model="showCopyAlert"
          :timeout="5000"
          :color="copyAlertColor"
          :top="true"
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
import { Vue, Component, Prop } from "vue-property-decorator";
import TestViewModel from '../../../models/TestViewModel';
import TestResultDataDumpViewModel from "../../../models/TestResultDataDumpViewModel";
// Parameter input components
import UnknownDataTypeComponent from './UnknownDataTypeComponent.vue';
import TestResultPlainTextDataComponent from './data_types/TestResultPlainTextDataComponent.vue';
import TestResultXmlDataComponent from './data_types/TestResultXmlDataComponent.vue';
import TestResultJsonDataComponent from './data_types/TestResultJsonDataComponent.vue';
import TestResultHtmlDataComponent from './data_types/TestResultHtmlDataComponent.vue';
import TestResultImageUrlsDataComponent from './data_types/TestResultImageUrlsDataComponent.vue';

@Component({
    components: {
      // Parameter input components
      UnknownDataTypeComponent,
      TestResultPlainTextDataComponent,
      TestResultXmlDataComponent,
      TestResultJsonDataComponent,
      TestResultHtmlDataComponent,
      TestResultImageUrlsDataComponent
    }
})
export default class TestResultDataComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;

    showCopyAlert: boolean = false;
    copyAlertText: string = "";
    copyAlertColor: string = "success";

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
    
    putDataOnCLipboard(): void {
      let copySourceElement = this.$refs.copyValue as HTMLTextAreaElement;
      copySourceElement.setAttribute('type', 'text');
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

      copySourceElement.setAttribute('type', 'hidden')
      window.getSelection().removeAllRanges()
    }

    ShowCopyAlert(msg: string, isSuccess: boolean): void {
      this.showCopyAlert = true;
      this.copyAlertText = msg;
      this.copyAlertColor = (isSuccess) ? "success" : "error";
    }
}
</script>

<style scoped>
</style>
