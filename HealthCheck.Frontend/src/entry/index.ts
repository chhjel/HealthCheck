import "babel-polyfill";
import { App, createApp } from 'vue'
// import Vue from "vue";
// import VueRouter from "vue-router";
import HealthCheckPageComponent from "@components/HealthCheckPageComponent.vue";
import ModuleConfig from "@models/Common/ModuleConfig";
import shadow from 'vue-shadow-dom';

// Debugging
if (window.location.search?.includes('hc_debug_warning=true'))
{
	var oldWarningFunction = console.warn;
	console.warn = function (message?: any, ...optionalParams: any[]) {
		oldWarningFunction.apply(console, [`CONSOLE.WARN() INTERCEPTED: "${message}"`]);
		oldWarningFunction.apply(console, [message, ...optionalParams]);
		if (!message.includes('Feature flags __VUE_OPTIONS_API__')) debugger;
	};
}

// Store
import store from './index_store';

// Router
import createHCRouter from './index_routes';

// Special pages
import DownloadPageComponent from '@components/modules/SecureFileDownload/DownloadPageComponent.vue';

// Extensions
import "@util/extensions/StringExtensions";
import "@util/extensions/ArrayExtensions";

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
	const app = createApp(HealthCheckPageComponent, props)
		.use(store)
		.use(router)
		.use(shadow);
	registerGlobalComponents(app);
	app.mount(appElement);
}
else if (document.getElementById("app-download") !== null)
{
	const appElement = document.getElementById("app-download");

	let props = { moduleConfig: moduleConfig };
	const app = createApp(DownloadPageComponent, props)
		.use(shadow);
	registerGlobalComponents(app);
	app.mount(appElement);
}

// Register global components
import ToolbarComponent from '@components/Common/Basic/ToolbarComponent.vue';
import BtnComponent from '@components/Common/Basic/BtnComponent.vue';
import IconComponent from '@components/Common/Basic/IconComponent.vue';
import AlertComponent from '@components/Common/Basic/AlertComponent.vue';
import ProgressLinearComponent from '@components/Common/Basic/ProgressLinearComponent.vue';
import DialogComponent from '@components/Common/Basic/DialogComponent.vue';
import BadgeComponent from '@components/Common/Basic/BadgeComponent.vue';
import TooltipComponent from '@components/Common/Basic/TooltipComponent.vue';
import TextareaComponent from '@components/Common/Basic/TextareaComponent.vue';
import TextFieldComponent from '@components/Common/Basic/TextFieldComponent.vue';
import ChipComponent from '@components/Common/Basic/ChipComponent.vue';
import MenuComponent from '@components/Common/Basic/MenuComponent.vue';
import ExpansionPanelComponent from '@components/Common/Basic/ExpansionPanelComponent.vue';
import DatePickerComponent from '@components/Common/Basic/DatePickerComponent.vue';
import CalendarComponent from '@components/Common/Basic/CalendarComponent.vue';
import TimelineComponent from '@components/Common/Basic/TimelineComponent.vue';
import ProgressCircularComponent from '@components/Common/Basic/ProgressCircularComponent.vue';
import SnackbarComponent from '@components/Common/Basic/SnackbarComponent.vue';
import CarouselComponent from '@components/Common/Basic/CarouselComponent.vue';
import StepperComponent from '@components/Common/Basic/StepperComponent.vue';
import SelectComponent from '@components/Common/Basic/SelectComponent.vue';
import CheckboxComponent from '@components/Common/Basic/CheckboxComponent.vue';
import SwitchComponent from '@components/Common/Basic/SwitchComponent.vue';
function registerGlobalComponents(app: App<Element>): void {
	app.component("ToolbarComponent", ToolbarComponent);
	app.component("BtnComponent", BtnComponent);
	app.component("IconComponent", IconComponent);
	app.component("AlertComponent", AlertComponent);
	app.component("ProgressLinearComponent", ProgressLinearComponent);
	app.component("DialogComponent", DialogComponent);
	app.component("BadgeComponent", BadgeComponent);
	app.component("TooltipComponent", TooltipComponent);
	app.component("TextareaComponent", TextareaComponent);
	app.component("TextFieldComponent", TextFieldComponent);
	app.component("ChipComponent", ChipComponent);
	app.component("MenuComponent", MenuComponent);
	app.component("ExpansionPanelComponent", ExpansionPanelComponent);
	app.component("DatePickerComponent", DatePickerComponent);
	app.component("CalendarComponent", CalendarComponent);
	app.component("TimelineComponent", TimelineComponent);
	app.component("ProgressCircularComponent", ProgressCircularComponent);
	app.component("SnackbarComponent", SnackbarComponent);
	app.component("CarouselComponent", CarouselComponent);
	app.component("StepperComponent", StepperComponent);
	app.component("SelectComponent", SelectComponent);
	app.component("CheckboxComponent", CheckboxComponent);
	app.component("SwitchComponent", SwitchComponent);
}
