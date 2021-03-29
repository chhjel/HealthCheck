<!-- src/components/modules/TestSuite/paremeter_inputs/TestParametersComponent.vue -->
<template>
    <div>
        <v-container grid-list-lg class="parameter-container">
          <v-layout row wrap>
              <v-flex xs12 sm12 :md6="allowMediumSize(parameter)" :lg3="allowSmallSize(parameter)"
                  v-for="(parameter, index) in test.Parameters"
                  :key="`test-${test.Id}-parameter`+index"
                  class="parameter-block">

                  <backend-input-component 
                      v-model="parameter.Value"
                      :forceType="cleanType(parameter.Type)"
                      :forceName="parameter.Name"
                      :forceDescription="parameter.Description"
                      :isCustomReferenceType="parameter.IsCustomReferenceType"
                      :config="createConfig(parameter, index)"
                      @isAnyJson="parameter.IsUnsupportedJson = true" />
              </v-flex>
          </v-layout>
        </v-container>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestViewModel from  '../../../../models/modules/TestSuite/TestViewModel';
import TestParameterViewModel from  '../../../../models/modules/TestSuite/TestParameterViewModel';
import { HCBackendInputConfig } from 'generated/Models/Core/HCBackendInputConfig';
import BackendInputComponent from "components/Common/Inputs/BackendInputs/BackendInputComponent.vue";

@Component({
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
        'HttpPostedFileBase',
        'List<HttpPostedFileBase>',
        'Guid',
        'Nullable<Guid>'
      ];
      return !parameter.FullWidth
        && !largerParameters.some(x => parameter.Type == x)
        && !(parameter.Type == 'String' && parameter.ShowTextArea);
    }

    allowMediumSize(parameter: TestParameterViewModel): boolean
    {
      return !parameter.FullWidth;
    }

    cleanType(type: string): string {
      if (type.startsWith("Nullable"))
      {
        type = type.substring("Nullable".length);
      }
      return type;
    }

    createConfig(parameter: TestParameterViewModel, index: number): HCBackendInputConfig {
      let flags: Array<string> = [];
      if (parameter.ShowTextArea) { flags.push('TextArea') };
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
        PossibleValues: parameter.PossibleValues,
        ParameterIndex: index
      };
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
