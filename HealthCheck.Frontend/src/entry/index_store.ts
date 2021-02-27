import Vuex from "vuex";
import FrontEndOptionsViewModel from "../models/Common/FrontEndOptionsViewModel";
import Vue from "vue";

let globalOptions = ((window as any).healthCheckOptions) as FrontEndOptionsViewModel;

Vue.use(Vuex);

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
        templateValues: { }
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
        }
    }
});
