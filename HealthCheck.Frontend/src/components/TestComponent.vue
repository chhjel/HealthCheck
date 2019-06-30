<!-- src/components/TestComponent.vue -->
<template>
    <div>
      <div class="mt-2"></div>

      <div class="test-item"
        :style="statusBorderStyle">
          <!-- HEADER -->
          <div class="test-header">
            <div>
              <h4 class="test-name">{{ test.Name }}</h4>
              <div v-if="hasDescription">
                <div class="mt-1"></div>
                <h4 class="subheading">{{ test.Description }}</h4>
              </div>
              
              <div class="mt-2"></div>
            </div>
            
            <v-btn ripple color="primary" large
              @click.stop.prevent="onExecuteTestClicked()"
              :disabled="testInProgress"
              class="ma-0 pl-1 pr-3 run-text-button">
              <v-icon color="white" x-large>play_arrow</v-icon>
              {{ executeTestButtonText }}
            </v-btn>
          </div>
          
          <!-- PARAMETERS -->
          <v-container grid-list-lg
            class="parameter-container"
            v-if="test.Parameters.length > 0">
            <v-layout row wrap>
              <v-flex xs12 sm12 md6 lg3
                v-for="(parameter, index) in test.Parameters"
                :key="`test-${test.Id}-parameter`+index"
                class="parameter-block"
              >
                <parameter-input-component :parameter="parameter" />
              </v-flex>
            </v-layout>
          </v-container>
          <div v-if="test.Parameters.length > 0 && showTestResult" class="mb-4"></div>

          <!-- PROGRESS -->
          <v-progress-linear
            v-if="testInProgress"
            :indeterminate="true"
            height="4"
            class="mt-4"></v-progress-linear>

          <!-- ERRORS -->
          <v-alert
              :value="testExecutionFailed"
              type="error">
              {{ testExecutionErrorMessage }}
          </v-alert>
          
          <!-- RESULT -->
          <div v-if="showTestResult" class="mt-1 mr-4">
            <v-icon
              :color="testResultIconColor"
              class="mr-1">{{testResultIcon}}</v-icon>
            
            <div class="result-message">{{this.testResult.Message}}</div>

            <!-- RESULT DATA -->
            <v-expansion-panel v-if="showTestResultData" class="mt-2">
              <v-expansion-panel-content>
                <template v-slot:header>
                  <div>{{ testResultDataTitle }}</div>
                </template>
                <v-card>
                  <v-card-text>
                    <test-result-data-component 
                      v-for="(data, index) in testResult.Data"
                      :key="`test-${test.Id}-result-data`+index"
                      :data="data" />
                  </v-card-text>
                </v-card>
              </v-expansion-panel-content>
            </v-expansion-panel>
          </div>
      </div>

      <div class="mb-6"></div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestViewModel from '../models/TestViewModel';
import TestResultViewModel from '../models/TestResultViewModel';
import ExecuteTestPayload from '../models/ExecuteTestPayload';
import ParameterInputComponent from './paremeter_inputs/ParameterInputComponent.vue';
import TestResultDataComponent from './TestResultDataComponent.vue';

@Component({
    components: {
      ParameterInputComponent,
      TestResultDataComponent
    }
})
export default class TestComponent extends Vue {
    @Prop({ required: true })
    test!: TestViewModel;

    testResult: TestResultViewModel | null = null;
    testInProgress: boolean = false;
    testExecutionFailed: boolean = false;
    testExecutionErrorMessage: string = "";

    mounted(): void {
    }

    get statusBorderStyle(): any
    {
      let borderWidth = 5;
      let borderStyle = "none";

      if (this.testResult == null) borderStyle = "none";
      else if (this.testResult!.StatusCode == 0) borderStyle = `${borderWidth}px solid #4caf50`;
      else if (this.testResult!.StatusCode == 1) borderStyle = `${borderWidth}px solid orange`;
      else if (this.testResult!.StatusCode == 2) borderStyle = `${borderWidth}px solid red`;

      return {
        'border-left': borderStyle,
        'padding-left': (borderStyle == "none" ? "48px" : "43px")
      };
    }

    get executeTestButtonText(): string
    {
      return this.testInProgress
        ? "Running.."
        : "Run";
    }

    get hasDescription(): boolean {
      return this.test.Description != null && this.test.Description.trim().length > 0;
    }

    get showTestResult(): boolean { return this.testResult != null && !this.testInProgress; }
    get showTestResultData(): boolean {
      return this.showTestResult && this.testResult!.Data != null && this.testResult!.Data!.length > 0;
    }

    get testResultIcon(): string
    {
      if (this.testResult == null) return "";
      else if (this.testResult.StatusCode == 0) return "check_circle";
      else if (this.testResult.StatusCode == 1) return "warning";
      else if (this.testResult.StatusCode == 2) return "error";
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

    getInputComponentNameFromType(typeName: string): string
    {
      let componentName = `ParameterInputType${typeName}Component`;
      let componentExists = (this.$options!.components![componentName] != undefined);
      return componentExists 
        ? componentName
        : "UnknownParameterInputComponent";
    }

    onExecuteTestClicked(): void 
    {
      this.testInProgress = true;
      this.executeTest();
    }

    executeTest(): void
    {
        this.testInProgress = true;
        this.testExecutionFailed = false;

        let payload = this.generatePayload(); 
        let url = `/HealthCheck/ExecuteTest`;

        fetch(url, {
            credentials: 'include',
            method: "POST",
            body: JSON.stringify(payload),
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => response.json())
        .then((result: TestResultViewModel) => {
            this.testResult = result;
            this.testInProgress = false;
        })
        .catch((e) => {
            this.testInProgress = false;
            this.testExecutionFailed = true;
            this.testExecutionErrorMessage = `Failed to execute test the following error. ${e}.`;
        });
    }

    generatePayload(): ExecuteTestPayload {
      let parameters = Array<string>();
      for(let param of this.test.Parameters)
      {
        parameters.push(param.Value);
      }

      return {
        TestId: this.test.Id,
        Parameters: parameters
      };
    }
}
</script>

<style scoped>
.test-item {
  /* border: 2px solid #c4c4c4; */
  background-color: #E6F2E7;
  padding: 12px 48px 24px 48px;
}
.test-header {
  display: flex;
  justify-content: space-between;
}
.test-name {
  font-size: 26px;
}
.parameter-container {
  padding-left: 32px;
  background-color: white;
  margin-top: 10px;
}
.parameter-block {
  padding-right: 40px !important;
  padding-left: 0 !important;
}
.run-text-button {
  font-size: 20px;
  min-width: 120px;
  min-height: 50px;
}
.result-message {
  font-size: 18px;
  display: inline;
}
</style>
