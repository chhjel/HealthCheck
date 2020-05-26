import "babel-polyfill";
import Vue, { VueConstructor } from "vue";
import Vuex from "vuex";
import VueRouter, { RouteConfig } from "vue-router";
import HealthCheckPageComponent from "./components/HealthCheckPageComponent.vue";
import Vuetify from "vuetify";
import ModuleConfig from "./models/Common/ModuleConfig";
import ModuleOptions from "./models/Common/ModuleOptions";
import FrontEndOptionsViewModel from './models/Common/FrontEndOptionsViewModel';

// Extensions
import "./util/extensions/StringExtensions";
import "./util/extensions/ArrayExtensions";

// Polyfills
import "es6-promise/auto";

// Modules
import TestSuitesPageComponent from './components/TestSuite/TestSuitesPageComponent.vue';
import OverviewPageComponent from './components/Overview/OverviewPageComponent.vue';
import AuditLogPageComponent from './components/AuditLog/AuditLogPageComponent.vue';
import LogViewerPageComponent from './components/LogViewer/LogViewerPageComponent.vue';
import RequestLogPageComponent from './components/RequestLog/RequestLogPageComponent.vue';
import DocumentationPageComponent from './components/Documentation/DocumentationPageComponent.vue';
import DataflowPageComponent from './components/Dataflow/DataflowPageComponent.vue';
import SettingsPageComponent from './components/Settings/SettingsPageComponent.vue';
import EventNotificationsPageComponent from './components/EventNotifications/EventNotificationsPageComponent.vue';
import DynamicCodeExecutionPageComponent from './components/DynamicCodeExecution/DynamicCodeExecutionPageComponent.vue';
let moduleComponents: Record<string, VueConstructor<Vue>> = {
    'TestSuitesPageComponent': TestSuitesPageComponent,
    'OverviewPageComponent': OverviewPageComponent,
    'AuditLogPageComponent': AuditLogPageComponent,
    'LogViewerPageComponent': LogViewerPageComponent,
    'RequestLogPageComponent': RequestLogPageComponent,
    'DocumentationPageComponent': DocumentationPageComponent,
    'DataflowPageComponent': DataflowPageComponent,
    'SettingsPageComponent': SettingsPageComponent,
    'EventNotificationsPageComponent': EventNotificationsPageComponent,
    'DynamicCodeExecutionPageComponent': DynamicCodeExecutionPageComponent
};

// Init libs
Vue.use(Vuetify, {
	iconfont: "fa",
	options: {
		customProperties: true
	},
	theme: {
		// 0a1925
		primary: "#18618c",
		secondary: "#263238",
		accent: "#18618c",
		//   error: "#de4a4a"
		error: "#d62839"
	}
});

let globalOptions = ((window as any).healthCheckOptions) as FrontEndOptionsViewModel;

Vue.use(Vuex);
const store = new Vuex.Store({
    state: {
      globalOptions: globalOptions,
      ui: {
          menuButtonVisible: false,
          menuExpanded: true,
          allowModuleSwitch: true
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
        }
    }
});

const initialWindowTitle = document.title;
let moduleConfig = ((window as any).healthCheckModuleConfigs) as Array<ModuleConfig>;
let moduleOptions = ((window as any).healthCheckModuleOptions) as Record<string, ModuleOptions<any>>;
let routes: Array<RouteConfig> = [];
moduleConfig
    .filter(config => config.LoadedSuccessfully)
    .forEach(config => {
    routes.push({
        name: config.Id,
        path: config.RoutePath,
        component: moduleComponents[config.ComponentName],
        props: {
            config: config,
            options: moduleOptions[config.Id]
        },
        meta: { title: (r: RouteConfig) => `${config.Name} | ${initialWindowTitle}` }
    });
});
// routes.push({ path: '/*', component: ModuleNotFoundPageComponent });

Vue.use(VueRouter)
const router = new VueRouter({
    routes: routes,
});
router.afterEach((to, from) => {
    Vue.nextTick(() => {
        if (to != null && to.meta != null && to.meta.title != null)
        {
            document.title = to.meta.title(to);
        }        
    })
  })

let v = new Vue({
	el: "#app",
	template: `
    <div>
        <health-check-page-component 
            :module-config="moduleConfig"
            />
    </div>
    `,
	store: store,
	router: router,
	data: {
        moduleConfig: moduleConfig
    },
	components: {
        HealthCheckPageComponent
	}
});
