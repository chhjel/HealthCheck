import "babel-polyfill";
import { createApp } from 'vue'
// import Vue from "vue";
// import VueRouter from "vue-router";
import HealthCheckPageComponent from "../components/HealthCheckPageComponent.vue";
import ModuleConfig from "../models/Common/ModuleConfig";
import shadow from 'vue-shadow-dom';

// Store
import store from './index_store';

// Router
import createRouter from './index_routes';

// Special pages
// import DownloadPageComponent from '../components/modules/SecureFileDownload/DownloadPageComponent.vue';

// Extensions
import "../util/extensions/StringExtensions";
import "../util/extensions/ArrayExtensions";

// Polyfills
import "es6-promise/auto";
import 'whatwg-fetch';

let moduleConfig = ((window as any).healthCheckModuleConfigs) as Array<ModuleConfig>;

const loaderElement = document.getElementById('app-loader');
if (loaderElement)
{
	loaderElement.parentNode?.removeChild(loaderElement);
}

const appElement = document.getElementById("app");
if (appElement !== null)
{
	const router = createRouter(moduleConfig);
	
	let props = {
		moduleConfig: moduleConfig
	};
	createApp(HealthCheckPageComponent, props)
		.use(store)
		.use(router)
		.use(shadow)
		.mount(appElement);
}
// else if (document.getElementById("app-download") !== null)
// {
// 	let v = new Vue({
// 		el: "#app-download",
// 		template: `
// 		<div>
// 			<download-page-component />
// 		</div>
// 		`,
// 		data: {
// 			moduleConfig: moduleConfig
// 		},
// 		components: {
// 			DownloadPageComponent
// 		}
// 	});
// }
