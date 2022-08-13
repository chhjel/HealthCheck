<!-- src/components/modules/TestSuite/paremeter_inputs/TestParametersComponent.vue -->
<template>
    <div class="parameter-container flex layout">
          <div v-for="(parameter, index) in filteredParameters"
              :key="`test-${test.Id}-parameter`+index"
              class="parameter-block"
              :class="parameterClasses(parameter)"
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
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { HCBackendInputConfig } from '@generated/Models/Core/HCBackendInputConfig';
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import { TestViewModel } from "@generated/Models/Core/TestViewModel";
import { TestParameterViewModel } from "@generated/Models/Core/TestParameterViewModel";
import { HCUIHint } from "@generated/Enums/Core/HCUIHint";

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
      return !parameter.UIHints.includes(HCUIHint.FullWidth)
        && !largerParameters.some(x => parameter.Type == x)
        && !((parameter.Type == 'DateTimeOffset[]' || parameter.Type == 'DateTime[]' || parameter.Type == 'Nullable<DateTimeOffset>[]' || parameter.Type == 'Nullable<DateTime>[]') && parameter.UIHints.includes(HCUIHint.DateRange))
        && !(parameter.Type == 'String' && parameter.UIHints.includes(HCUIHint.TextArea) && parameter.UIHints.includes(HCUIHint.CodeArea));
    }

    allowMediumSize(parameter: TestParameterViewModel): boolean
    {
      return !parameter.UIHints.includes(HCUIHint.FullWidth);
    }

    cleanType(type: string): string {
      if (type.startsWith("Nullable<"))
      {
        type = type.substring("Nullable<".length);
        const lastIndexOfEndTag = type.lastIndexOf(">");
        if (lastIndexOfEndTag > -1) {
          type = type.slice(0, lastIndexOfEndTag) + type.slice(lastIndexOfEndTag + 1);
        }
      }
      else if (type.startsWith("Nullable"))
      {
        type = type.substring("Nullable".length);
      }
      return type;
    }

    createConfig(parameter: TestParameterViewModel, index: number): HCBackendInputConfig {
      let flags: Array<string> = [];

      return {
        Id: parameter.Name,
        Type: parameter.Type,
        Name: parameter.Name,
        Description: parameter.Description,
        Nullable: parameter.Type.startsWith("Nullable"),
        DefaultValue: parameter.DefaultValue,
        Flags: flags,
        UIHints: parameter.UIHints,
        PossibleValues: parameter.PossibleValues,
        ParameterIndex: index,
        ExtraValues: {},
        PropertyInfo: {},
        NullName: parameter.NullName,
        TextPattern: parameter.TextPattern
      };
    }

    onIsAnyJson(parameter: TestParameterViewModel): void {
      parameter.IsUnsupportedJson = true;
    }

    get filteredParameters(): Array<TestParameterViewModel> {
      return this.test.Parameters;//.filter(x => !x.Hidden); //todo: v-show instead
    }

    parameterClasses(parameter: TestParameterViewModel): Array<string> {
      let classes: Array<string> = [
        'xs12'
      ];
      if (this.allowMediumSize(parameter)) classes.push('md6');
      if (this.allowSmallSize(parameter)) classes.push('lg3');
      // xs-12 sm-12 :md-6="allowMediumSize(parameter)" :lg-3="allowSmallSize(parameter)"
      return classes;
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
  margin-bottom: 10px;
}
</style>
