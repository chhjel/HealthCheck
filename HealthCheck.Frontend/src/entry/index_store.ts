import { StoreUtil } from "@util/StoreUtil";
import { createStore, Store } from "vuex";
import FrontEndOptionsViewModel from "../models/Common/FrontEndOptionsViewModel";

let globalOptions = ((window as any).healthCheckOptions) as FrontEndOptionsViewModel;

export interface ParameterDetails
{
  parameterId: string;
  key: string;
  value: any;
}

const store = createStore({
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
    },
    input: {
      ctrlIsHeldDown: false,
      altIsHeldDown: false,
      shiftIsHeldDown: false
    }
  },
  mutations: {
      setCtrlHeldDown (state: any, isDown) {
        state.input.ctrlIsHeldDown = isDown;
      },
      setAltHeldDown (state: any, isDown) {
        state.input.altIsHeldDown = isDown;
      },
      setShiftHeldDown (state: any, isDown) {
        state.input.shiftIsHeldDown = isDown;
      },
      showMenuButton (state: any, visible) {
        state.ui.menuButtonVisible = visible;
      },
      allowModuleSwitch (state: any, allow) {
        state.ui.allowModuleSwitch = allow;
      },
      setMenuExpanded (state: any, expanded) {
        state.ui.menuExpanded = expanded;
      },
      toggleMenuExpanded (state: any) {
          state.ui.menuExpanded = !state.ui.menuExpanded;
      },
      setTestModuleOptions(state: any, options) {
        state.tests.options = options;
      },
      setTestModuleTemplateValues(state: any, values) {
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

StoreUtil.store = store;

export default store;
