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

      <div class="mb-6"></div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestViewModel from '../models/TestViewModel';
import TestResultViewModel from '../models/TestResultViewModel';
import ExecuteTestPayload from '../models/ExecuteTestPayload';
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

    ////////////////
    //  GETTERS  //
    //////////////
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

    get showTestResult(): boolean {
      return !this.testExecutionFailed && this.testResult != null && !this.testInProgress; 
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
      this.testInProgress = true;
      this.executeTest();
    }

    executeTest(): void
    {
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
.run-text-button {
  font-size: 20px;
  min-width: 120px;
  min-height: 50px;
}
</style>
