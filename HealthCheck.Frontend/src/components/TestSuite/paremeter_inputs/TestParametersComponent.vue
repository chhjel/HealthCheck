<!-- src/components/paremeter_inputs/TestParametersComponent.vue -->
<template>
    <div>
        <v-container grid-list-lg class="parameter-container">
          <v-layout row wrap>
              <v-flex xs12 sm12 md6 :lg3="allowSmallSize(parameter)"
                  v-for="(parameter, index) in test.Parameters"
                  :key="`test-${test.Id}-parameter`+index"
                  class="parameter-block">
                  <parameter-input-component :parameter="parameter" />
              </v-flex>
          </v-layout>
        </v-container>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestViewModel from '../../../models/TestSuite/TestViewModel';
import ParameterInputComponent from './ParameterInputComponent.vue';
import TestParameterViewModel from '../../../models/TestSuite/TestParameterViewModel';

@Component({
    components: {
      ParameterInputComponent
    }
})
export default class TestParametersComponent extends Vue {
    @Prop({ required: true })
    test!: TestViewModel;

    mounted(): void {
    }

    allowSmallSize(parameter: TestParameterViewModel): boolean
    {
      return parameter.Type !== 'HttpPostedFileBase';
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
