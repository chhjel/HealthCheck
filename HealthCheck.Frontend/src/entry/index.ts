import "babel-polyfill";
import Vue from "vue";
import VueRouter from "vue-router";
import HealthCheckPageComponent from "../components/HealthCheckPageComponent.vue";
import Vuetify from "vuetify";
import ModuleConfig from "../models/Common/ModuleConfig";

// Store
import store from './index_store';

// Router
Vue.use(VueRouter)
import createRouter from './index_routes';

// Extensions
import "../util/extensions/StringExtensions";
import "../util/extensions/ArrayExtensions";

// Polyfills
import "es6-promise/auto";

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

let moduleConfig = ((window as any).healthCheckModuleConfigs) as Array<ModuleConfig>;

if (document.getElementById("app") !== null)
{
	const router = createRouter(moduleConfig);
	
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
}
else if (document.getElementById("app-download") !== null)
{
	let v = new Vue({
		el: "#app-download",
		template: `
		<div>
			woop :]
		</div>
		`,
		data: {
			moduleConfig: moduleConfig
		},
		components: {
			
		}
	});
}
