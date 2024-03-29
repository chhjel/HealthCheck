<!-- src/components/modules/TestSuite/TestResultComponent.vue -->
<template>
    <div>
        <icon-component :color="testResultIconColor"
          v-if="showResultStatusIcon"
          class="mr-1 left">{{testResultIcon}}</icon-component>
        
        <div class="result-message" v-if="showResultMessage">{{ this.testResult.Message }}</div>

        <div v-if="hasStackTrace" class="mt-4">
          <code class="pa-2">{{ this.testResult.StackTrace }}</code>
        </div>

        <!-- DATA DUMPS -->
        <expansion-panel-component 
          class="mt-2 test-result-expansion-panel"
          :class="{ 'clean-mode': testResult.DisplayClean }"
          v-if="showTestResultData"
          :showHeader="!testResult.DisplayClean"
          :cleanMode="testResult.DisplayClean"
          v-model:value="dataExpandedState">
            <template #header v-if="testResult.AllowExpandData && !testResult.DisplayClean">{{ testResultDataTitle }}</template>
            <template #content>
              <div v-if="dataExpandedState">
                <div>
                  <test-result-data-component 
                    v-for="(resultData, index) in testResult.Data"
                    :key="`test-${testResult.TestId}-result-data`+index"
                    :resultData="resultData"
                    :clean="resultData.DisplayClean || testResult.DisplayClean" />
                </div>
              </div>
            </template>
        </expansion-panel-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestResultViewModel } from '@generated/Models/Core/TestResultViewModel';
import TestResultDataComponent from '@components/modules/TestSuite/result_data/TestResultDataComponent.vue';

@Options({
    components: {
      TestResultDataComponent
    }
})
export default class TestResultComponent extends Vue {
    @Prop({ required: true })
    testResult!: TestResultViewModel;
    @Prop({ required: true })
    expandDataOnLoad!: boolean;

    dataExpandedState: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
      if (this.expandDataOnLoad == true 
          || this.testResult.ExpandDataByDefault == true
          || this.testResult.DisplayClean == true) {
        this.dataExpandedState = true;
      }
    }
    
    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch("dataExpandedState")
    onDataExpandedStateChanged(value:boolean, oldValue:boolean): void {
      this.$emit("dataExpandedStateChanged", value);
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get showResultStatusIcon(): boolean {
      // Not clean mode
      if (this.testResult!.DisplayClean !== true)
      {
        // Always show
        return true;
      }
      // Clean mode
      else
      {
        return this.testResult!.StatusCode !== 0
                && this.testResult!.Message != null
                && this.testResult!.Message!.length > 0;
      }
    }

    get showResultMessage(): boolean {
      // Not clean mode
      if (this.testResult!.DisplayClean !== true)
      {
        // Always show
        return true;
      }
      // Clean mode
      else
      {
        return this.testResult!.Message != null
                && this.testResult!.Message!.length > 0;
      }
    }

    get hasStackTrace(): boolean {
      return this.testResult!.StackTrace != null && this.testResult!.StackTrace!.length > 0;
    }

    get showTestResultData(): boolean {
      return this.testResult!.Data != null && this.testResult!.Data!.length > 0;
    }

    get testResultIcon(): string
    {
      if (this.testResult == null) return "";
      else if (this.testResult.StatusCode == 0) return "sentiment_satisfied_alt";
      else if (this.testResult.StatusCode == 1) return "sentiment_dissatisfied";
      else if (this.testResult.StatusCode == 2) return "sentiment_very_dissatisfied";
      else return "";
    }

    get testResultIconColor(): string
    {
      if (this.testResult == null) return "";
      else if (this.testResult.StatusCode == 0) return "green";
      else if (this.testResult.StatusCode == 1) return "orange";
      else if (this.testResult.StatusCode == 2) return "red";
      else return "";
    }

    get testResultDataTitle(): string
    {
      if (!this.showTestResultData) return "";

      let prefix = "View ";

      let datas = this.testResult!.Data;
      let datasWithTitles = datas.filter(x => x.Title != null && x.Title.length > 0);
      let maxNamedToShow = 3;
      let namedToShowCount = (datasWithTitles.length < maxNamedToShow) ? datasWithTitles.length : maxNamedToShow;
      let remainingCount = (datas.length - namedToShowCount);

      if (namedToShowCount > 0) {
        var title = `${prefix} ` + datasWithTitles.slice(0, namedToShowCount).map(x => `'${x.Title}'`).join(", ");
        if (remainingCount > 0) {
          title += ` + ${remainingCount} other` + (remainingCount == 1 ? "" : "s");
        }
        return title;
      }
      else
      {
        return `${prefix} ${remainingCount} data` + (remainingCount == 1 ? "" : "s");
      }
    }

}
</script>

<style scoped>
.result-message {
  font-size: 18px;
  display: inline;
}
</style>
