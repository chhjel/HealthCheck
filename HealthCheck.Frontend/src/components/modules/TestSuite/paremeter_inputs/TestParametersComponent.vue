<!-- src/components/modules/TestSuite/paremeter_inputs/TestParametersComponent.vue -->
<template>
    <div>
        <v-container grid-list-lg class="parameter-container">
          <v-layout row wrap>
              <v-flex xs12 sm12 :md6="allowMediumSize(parameter)" :lg3="allowSmallSize(parameter)"
                  v-for="(parameter, index) in filteredParameters"
                  :key="`test-${test.Id}-parameter`+index"
                  class="parameter-block"
                  v-show="!parameter.Hidden">

                  <backend-input-component 
                      v-model:value="parameter.Value"
                      :forceType="cleanType(parameter.Type)"
                      :forceName="parameter.Name"
                      :forceDescription="parameter.Description"
                      :isCustomReferenceType="parameter.IsCustomReferenceType"
                      :config="createConfig(parameter, index)"
                      :parameterDetailContext="test.Id"
                      :referenceValueFactoryConfig="parameter.ReferenceValueFactoryConfig"
                      @isAnyJson="onIsAnyJson(parameter)"
                      :feedback="parameter.Feedback" />
              </v-flex>
          </v-layout>
        </v-container>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import TestViewModel from '@models/modules/TestSuite/TestViewModel';
import TestParameterViewModel from '@models/modules/TestSuite/TestParameterViewModel';
import { HCBackendInputConfig } from '@generated/Models/Core/HCBackendInputConfig';
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";

@Options({
    components: {
      BackendInputComponent
    }
})
export default class TestParametersComponent extends Vue {
    @Prop({ required: true })
    test!: TestViewModel;

    mounted(): void {
    }

    allowSmallSize(parameter: TestParameterViewModel): boolean
    {
      const largerParameters = [
        'Guid',
        'Nullable<Guid>',
        'TimeSpan',
        'Nullable<TimeSpan>'
      ];
      return !parameter.FullWidth
        && !largerParameters.some(x => parameter.Type == x)
        && !(parameter.Type == 'String' && parameter.ShowTextArea && parameter.ShowCodeArea);
    }

    allowMediumSize(parameter: TestParameterViewModel): boolean
    {
      return !parameter.FullWidth;
    }

    cleanType(type: string): string {
      if (type.startsWith("Nullable<"))
      {
        type = type.substring("Nullable<".length);
        type = type.substr(0, type.length - 1);
      }
      else if (type.startsWith("Nullable"))
      {
        type = type.substring("Nullable".length);
      }
      return type;
    }

    createConfig(parameter: TestParameterViewModel, index: number): HCBackendInputConfig {
      let flags: Array<string> = [];
      if (parameter.ShowTextArea) { flags.push('TextArea') };
      if (parameter.ShowCodeArea) { flags.push('CodeArea') };
      if (parameter.ReadOnlyList) { flags.push('ReadOnlyList') };

      return {
        Id: parameter.Name,
        Type: parameter.Type,
        Name: parameter.Name,
        Description: parameter.Description,
        NotNull: parameter.NotNull,
        Nullable: parameter.Type.startsWith("Nullable"),
        DefaultValue: parameter.DefaultValue,
        Flags: flags,
        FullWidth: parameter.FullWidth,
        PossibleValues: parameter.PossibleValues,
        ParameterIndex: index,
        ExtraValues: {},
        PropertyInfo: {},
        NullName: parameter.NullName
      };
    }

    onIsAnyJson(parameter: TestParameterViewModel): void {
      parameter.IsUnsupportedJson = true;
    }

    get filteredParameters(): Array<TestParameterViewModel> {
      return this.test.Parameters;//.filter(x => !x.Hidden); //todo: v-show instead
    }
}
</script>

<style scoped>
.parameter-container {
  padding-left: 10px;
  padding-bottom: 0;
}
.parameter-block {
  padding-right: 40px !important;
  padding-left: 0 !important;
}
</style>
