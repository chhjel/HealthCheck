<!-- src/components/TestComponent.vue -->
<template>
    <div>
      <div class="test-item pa-4">
          <!-- HEADER -->
          <div class="test-header" 
            :class="{'no-details': !showDetails}"
            :data-test-title-encoded="encodedTestTitle"
            @click="$emit('testClicked', test)">

            <div class="test-status-label subheading font-weight-bold"
              :class="statusClass"
              v-if="hasStatus">{{statusText}}</div>
            <h4 class="test-name">
              {{ test.Name }}
              <!-- <v-icon>link</v-icon> -->
            </h4>
            <div class="test-duration" v-if="showTestDuration">{{ prettifyDuration(testResult.DurationInMilliseconds) }}</div>

            <v-btn ripple color="error"
              @click.stop.prevent="cancelTest()"
              v-if="(testInProgress || showCancellationButtonUntilNextRun) && test.IsCancellable"
              :disabled="cancellationInProgress"
              class="ma-0 mr-2 mt-2 pl-1 pr-3 cancel-test-button">
              <v-icon color="white">cancel</v-icon>
              {{ cancelTestButtonText }}
            </v-btn>
            
            <v-btn ripple color="primary" 
              @click.stop.prevent="onExecuteTestClicked()"
              :disabled="testInProgress || showCancellationButtonUntilNextRun"
              class="ma-0 pl-1 pr-3 run-test-button"
              :class="{ 'cancellable': test.IsCancellable }">
              <v-icon color="white" large>play_arrow</v-icon>
              {{ executeTestButtonText }}
            </v-btn>
          </div>
          
          <div class="test-details" v-if="showDetails">
            <!-- DESCRIPTION -->
            <div v-if="hasDescription">
              <div class="mt-3"></div>
              <h4 class="subheading"
                :class="{ 'mb-4':  (showTestResult && test.Parameters.length == 0) }"
                v-html="test.Description"></h4>
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
import UrlUtils from '../../util/UrlUtils';

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
    cancelTestEndpoint!: string;
    @Prop({ required: true })
    inludeQueryStringInApiCalls!: string;

    testResult: TestResultViewModel | null = null;
    testInProgress: boolean = false;
    cancellationInProgress: boolean = false;
    showCancellationButtonUntilNextRun: boolean = false;
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
    get encodedTestTitle(): string {
      return UrlUtils.EncodeHashPart(this.test.Name);
    }

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

    get cancelTestButtonText(): string
    {
      return this.cancellationInProgress ? "Cancelling.." : "Cancel";
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

    cancelTest(): void
    {
      this.cancellationInProgress = true;
      this.showCancellationButtonUntilNextRun = false;

      let queryStringIfEnabled = this.inludeQueryStringInApiCalls ? window.location.search : '';
      let queryString = queryStringIfEnabled;
      let payload = `testId=${this.test.Id}`;
      if (queryString.indexOf('?') === -1) {
        payload = `?${payload}`;
      } else {
        payload = `&${payload}`;
      }
      queryString = `${queryString}${payload}`;

      let url = `${this.cancelTestEndpoint}${queryString}`;

      fetch(url, {
          credentials: 'include',
          method: "POST",
          headers: new Headers({
            'Content-Type': 'application/x-www-form-urlencoded'
          }),
      })
      .then(response => {
      })
      .catch((e) => {
      });
    }

    executeTest(): void
    {
      this.$emit("testStarted", this.test.Id);
      this.testInProgress = true;
      this.cancellationInProgress = false;
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
          this.cancellationInProgress = false;
          this.$emit("testStopped", this.test.Id);
          this.showCancellationButtonUntilNextRun = false;

          // ToDo dont do this :-)
          if (this.test.TestResult.Message == 'Test is already running and must be cancelled before it can run again.') {
            this.showCancellationButtonUntilNextRun = true;
          }
      })
      .catch((e) => {
          this.test.TestResult = null;
          this.testResult = this.test.TestResult;
          this.testInProgress = false;
          this.cancellationInProgress = false;
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
  /* border-radius: 0 25px 0 25px; */
  border-radius: 25px;
  background-color: #fff;
  box-shadow: #d5d7d5 4px 4px 6px 0px;
}
.test-header {
  display: flex;
  padding-left: 24px;
  border-radius: 0 25px 0 0;
  cursor: pointer;
}
.test-header.no-details {
  border-radius: 0 25px 0 25px;
}
.run-test-button {
  font-size: 20px;
  min-width: 120px;
  min-height: 53px;
  border-radius: 25px;
  /* border-radius: 0 25px 0 25px; */
  text-transform: inherit;
}
.run-test-button .v-icon {
    margin-right: 5px;
}
.run-test-button.v-btn--disabled.cancellable {
    /* padding-left: 30px !important; */
}
.cancel-test-button {
  /* font-size: 20px;
  min-width: 120px;
  min-height: 53px;
  border-radius: 0 0 0 25px;
  text-transform: inherit;
  margin-right: -25px !important;
  padding-right: 40px !important;
  z-index: 1; */
}
.cancel-test-button .v-icon {
    /* margin-right: 5px; */
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
