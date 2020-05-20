import "babel-polyfill";
import Vue from "vue";
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
import NoPageAvailablePageComponent from './components/Pages/NoPageAvailablePageComponent.vue';
import TestSuitesPageComponent from './components/TestSuite/TestSuitesPageComponent.vue';
import OverviewPageComponent from './components/Pages/OverviewPageComponent.vue';
import AuditLogPageComponent from './components/Pages/AuditLogPageComponent.vue';
import LogViewerPageComponent from './components/Pages/LogViewerPageComponent.vue';
import RequestLogPageComponent from './components/RequestLog/RequestLogPageComponent.vue';
import DocumentationPageComponent from './components/Pages/DocumentationPageComponent.vue';
import DataflowPageComponent from './components/Pages/DataflowPageComponent.vue';
import SettingsPageComponent from './components/Settings/SettingsPageComponent.vue';
import EventNotificationsPageComponent from './components/Pages/EventNotificationsPageComponent.vue';
let moduleComponents = [
    TestSuitesPageComponent,
    OverviewPageComponent,
    AuditLogPageComponent,
    LogViewerPageComponent,
    RequestLogPageComponent,
    DocumentationPageComponent,
    DataflowPageComponent,
    SettingsPageComponent,
    EventNotificationsPageComponent
];

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
          menuButtonVisible: false
      }
    },
    mutations: {
      showMenuButton (state, visible) {
        state.ui.menuButtonVisible = visible;
      }
    }
});

let moduleConfig = ((window as any).healthCheckModuleConfigs) as Array<ModuleConfig>;
let moduleOptions = ((window as any).healthCheckModuleOptions) as Record<string, ModuleOptions<any>>;
let routes: Array<RouteConfig> = [];
moduleConfig.forEach(config => {
    routes.push({
        name: config.Id,
        path: config.RoutePath,
        component: moduleComponents.filter(x => x.name == config.ComponentName)[0],
        props: {
            config: config,
            options: moduleOptions[config.Id]
        }
    });
});
// routes.push({ path: '/*', component: ModuleNotFoundPageComponent });

Vue.use(VueRouter)
const router = new VueRouter({
    routes: routes
});

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
