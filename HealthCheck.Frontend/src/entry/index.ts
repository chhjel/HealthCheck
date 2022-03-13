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
import createHCRouter from './index_routes';

// Special pages
import DownloadPageComponent from '@components/modules/SecureFileDownload/DownloadPageComponent.vue';

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
	const router = createHCRouter(moduleConfig);
	
	let props = {
		moduleConfig: moduleConfig
	};
	createApp(HealthCheckPageComponent, props)
		.use(store)
		.use(router)
		.use(shadow)
		.mount(appElement);
}
else if (document.getElementById("app-download") !== null)
{
	const appElement = document.getElementById("app-download");

	let props = { moduleConfig: moduleConfig };
	createApp(DownloadPageComponent, props)
		.use(shadow)
		.mount(appElement);
}
