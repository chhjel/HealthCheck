<!-- src/components/result_data/TestResultDataComponent.vue -->
<template>
    <div>
        <component
            :class="`data-dump data-dump-${data.Type.toLowerCase()}`"
            :data="data"
            :is="getDataComponentNameFromType(data.Type)">
        </component>

        <v-card-actions>
          <v-btn flat color="secondary-darken2">Copy</v-btn>
          <v-btn flat color="secondary-darken2">Fullscreen</v-btn>
        </v-card-actions>
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

@Component({
    components: {
      // Parameter input components
      UnknownDataTypeComponent,
      TestResultPlainTextDataComponent,
      TestResultXmlDataComponent,
      TestResultJsonDataComponent,
      TestResultHtmlDataComponent
    }
})
export default class TestResultDataComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;

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
}
</script>

<style>
/* .data-textarea .v-input__slot {
  border: none;
  box-shadow: none !important;
} */
/* .data-textarea .v-input__slot::before {
  border: none !important;
} */
</style>
