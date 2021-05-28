import Vuex from "vuex";
import FrontEndOptionsViewModel from "../models/Common/FrontEndOptionsViewModel";
import Vue from "vue";

let globalOptions = ((window as any).healthCheckOptions) as FrontEndOptionsViewModel;

Vue.use(Vuex);

export interface ParameterDetails
{
  parameterId: string;
  key: string;
  value: any;
}

export default new Vuex.Store({
    state: {
      globalOptions: globalOptions,
      ui: {
          menuButtonVisible: false,
          menuExpanded: true,
          allowModuleSwitch: true
      },
      tests: {
        options: { },
        templateValues: { },
        parameterDetails: { }
      }
    },
    mutations: {
        showMenuButton (state, visible) {
          state.ui.menuButtonVisible = visible;
        },
        allowModuleSwitch (state, allow) {
          state.ui.allowModuleSwitch = allow;
        },
        setMenuExpanded (state, expanded) {
          state.ui.menuExpanded = expanded;
        },
        toggleMenuExpanded (state) {
            state.ui.menuExpanded = !state.ui.menuExpanded;
        },
        setTestModuleOptions(state, options) {
          state.tests.options = options;
        },
        setTestModuleTemplateValues(state, values) {
          state.tests.templateValues = values;
        },
        setTestParameterDetails(state: any, details: ParameterDetails) {
          if (!state.tests.parameterDetails[details.parameterId])
          {
            state.tests.parameterDetails[details.parameterId] = {};
          }
          state.tests.parameterDetails[details.parameterId][details.key] = details.value;
        }
    }
});
