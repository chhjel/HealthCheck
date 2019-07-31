<!-- src/components/TestComponent.vue -->
<template>
    <div>
      <div class="test-item">
          <!-- HEADER -->
          <div class="test-header" :class="{'no-details': !showDetails}">
            <div class="test-status-label subheading font-weight-bold"
              :class="statusClass"
              v-if="hasStatus">{{statusText}}</div>
            <h4 class="test-name">
              {{ test.Name }}
              <!-- <v-icon>link</v-icon> -->
            </h4>
            <div class="test-duration" v-if="showTestDuration">{{ prettifyDuration(testResult.DurationInMilliseconds) }}</div>

            <v-btn ripple color="primary" 
              @click.stop.prevent="onExecuteTestClicked()"
              :disabled="testInProgress"
              class="ma-0 pl-1 pr-3 run-test-button">
              <v-icon color="white" large>play_arrow</v-icon>
              {{ executeTestButtonText }}
            </v-btn>
          </div>
          
          <div class="test-details" v-if="showDetails">
            <!-- DESCRIPTION -->
            <div v-if="hasDescription">
              <div class="mt-3"></div>
              <h4 class="subheading"
                :class="{ 'mb-4':  (showTestResult && test.Parameters.length == 0) }">{{ test.Description }}</h4>
            </div>
            
            <!-- PARAMETERS -->
            <test-parameters-component 
              v-if="test.Parameters.length > 0"
              :test="test" />
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
            <test-result-component 
              v-if="showTestResult" 
              :testResult="testResult"
              :expandDataOnLoad="resultDataExpandedState"
              v-on:dataExpandedStateChanged="onDataExpandedStateChanged"
              class="mt-1 mr-4"  />
          </div>
      </div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestViewModel from '../../models/TestSuite/TestViewModel';
import TestResultViewModel from '../../models/TestSuite/TestResultViewModel';
import ExecuteTestPayload from '../../models/TestSuite/ExecuteTestPayload';
import TestParametersComponent from './paremeter_inputs/TestParametersComponent.vue';
import TestResultComponent from './TestResultComponent.vue';

@Component({
    components: {
      TestParametersComponent,
      TestResultComponent
    }
})
export default class TestComponent extends Vue {
    @Prop({ required: true })
    test!: TestViewModel;

    @Prop({ required: true })
    executeTestEndpoint!: string;
    @Prop({ required: true })
    inludeQueryStringInApiCalls!: string;

    testResult: TestResultViewModel | null = null;
    testInProgress: boolean = false;
    testExecutionFailed: boolean = false;
    testExecutionErrorMessage: string = "";
    resultDataExpandedState: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
      this.$parent.$on('executeAllTestsInSet', this.executeTest);
      this.testResult = this.test.TestResult;
    }

    beforeDestroy(): void {
      this.$parent.$off('executeAllTestsInSet', this.executeTest);
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get showDetails(): boolean {
      return this.hasDescription 
      || this.test.Parameters.length > 0 
      || this.testInProgress 
      || this.testExecutionFailed 
      || this.showTestResult;
    }

    get hasStatus(): boolean {
      return this.statusText.length > 0;
    }

    get statusText(): string {
      if (this.testResult == null || this.testInProgress) return "";
      else if (this.testResult!.StatusCode == 0) return "success";
      else if (this.testResult!.StatusCode == 1) return "warning";
      else if (this.testResult!.StatusCode == 2) return "failed";
      else return "";
    }

    get statusClass(): string {
      if (this.testResult == null || this.testInProgress) return "";
      else if (this.testResult!.StatusCode == 0) return "label-success";
      else if (this.testResult!.StatusCode == 1) return "label-warning";
      else if (this.testResult!.StatusCode == 2) return "label-error";
      else return "";
    }

    get executeTestButtonText(): string
    {
      return this.testInProgress
        ? (this.test.RunningButtonText != null && this.test.RunningButtonText.length > 0) ? this.test.RunningButtonText : "Running.."
        : (this.test.RunButtonText != null && this.test.RunButtonText.length > 0) ? this.test.RunButtonText : "Run";
    }

    get hasDescription(): boolean {
      return this.test.Description != null && this.test.Description.trim().length > 0;
    }

    get showTestResult(): boolean {
      return !this.testExecutionFailed && this.testResult != null && !this.testInProgress; 
    }

    get showTestDuration(): boolean {
      return this.testResult != null; 
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDataExpandedStateChanged(expanded: boolean): void
    {
      this.resultDataExpandedState = expanded;
    }

    ////////////////
    //  METHODS  //
    //////////////
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
      this.executeTest();
    }

    executeTest(): void
    {
      this.$emit("testStarted", this.test.Id);
      this.testInProgress = true;
      this.testExecutionFailed = false;

      let payload = this.generatePayload();
      let queryStringIfEnabled = this.inludeQueryStringInApiCalls ? window.location.search : '';
      let url = `${this.executeTestEndpoint}${queryStringIfEnabled}`;

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
          this.test.TestResult = result;
          this.testResult = this.test.TestResult;
          this.testInProgress = false;
          this.$emit("testStopped", this.test.Id);
      })
      .catch((e) => {
          this.test.TestResult = null;
          this.testResult = this.test.TestResult;
          this.testInProgress = false;
          this.testExecutionFailed = true;
          this.testExecutionErrorMessage = `Failed to execute test with the following error. ${e}.`;
          this.$emit("testStopped", this.test.Id);
      });
    }

    generatePayload(): ExecuteTestPayload {
      let parameters = Array<string | null>();
      for(let param of this.test.Parameters)
      {
        parameters.push(param.Value);
      }

      return {
        TestId: this.test.Id,
        Parameters: parameters
      };
    }

    prettifyDuration(milliseconds: number): string {
      if (milliseconds <= 0) {
        return "< 0ms";
      } else if(milliseconds > 1000) {
        let seconds = milliseconds / 1000;
        let multiplier = Math.pow(10, 2);
        seconds = Math.round(seconds * multiplier) / multiplier;
        return `${seconds}s`;
      } else {
        return `${milliseconds}ms`;
      }
    }
}
</script>

<style scoped>
.test-item {
  border-radius: 0 25px 0 25px;
  background-color: #fff;
}
.test-header {
  display: flex;
  padding-left: 24px;
  border-radius: 0 25px 0 0;
}
.test-header.no-details {
  border-radius: 0 25px 0 25px;
}
.run-test-button {
  font-size: 20px;
  min-width: 120px;
  min-height: 53px;
  border-radius: 0 25px 0 25px;
  text-transform: inherit;
}
.run-test-button .v-icon {
    margin-right: 5px;
}
.test-details {
  padding: 0px 48px 24px 24px;
}
.test-name {
  flex-grow: 1;
  font-size: 22px;
  margin-top: 10px;
}
.test-duration {
  padding-right: 10px;
  color: hsla(273, 40%, 80%, 1);
  display: flex;
  align-items: center;
}
.test-status-label {
  color: #fff;
  background-color: var(--v-success-base);
  height: 33px;
  padding: 8px;
  margin-right: 8px;
  padding-top: 5px;
  align-self: center;
}
.test-status-label.label-success {
  color: #317711;
  background-color: #c7e6c8;
}
.test-status-label.label-warning {
  color: #df6d03;
  background-color: #f3d5b2;
}
.test-status-label.label-error {
  color: #c20404;
  background-color: #eeb2b2;
}
</style>
