import "babel-polyfill";
import Vue from "vue";
import VueRouter from "vue-router";
import HealthCheckPageComponent from "../components/HealthCheckPageComponent.vue";
import Vuetify from "vuetify";
import ModuleConfig from "../models/Common/ModuleConfig";
import shadow from 'vue-shadow-dom';

// Store
import store from './index_store';

// Router
Vue.use(VueRouter)
import createRouter from './index_routes';

// Special pages
import DownloadPageComponent from '../components/modules/SecureFileDownload/DownloadPageComponent.vue';

// Extensions
import "../util/extensions/StringExtensions";
import "../util/extensions/ArrayExtensions";

// Polyfills
import "es6-promise/auto";
import 'whatwg-fetch';

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
Vue.use(shadow);

let moduleConfig = ((window as any).healthCheckModuleConfigs) as Array<ModuleConfig>;

const loaderElement = document.getElementById('app-loader');
if (loaderElement)
{
	loaderElement.parentNode?.removeChild(loaderElement);
}

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
			<download-page-component />
		</div>
		`,
		data: {
			moduleConfig: moduleConfig
		},
		components: {
			DownloadPageComponent
		}
	});
}
