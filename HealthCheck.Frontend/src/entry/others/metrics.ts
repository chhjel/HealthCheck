import "babel-polyfill";
import shadow from 'vue-shadow-dom';

// Polyfills
import "es6-promise/auto";
import 'whatwg-fetch';

import RequestMetricsSummaryComponent from '@components/modules/Metrics/RequestMetricsSummaryComponent.vue';
import { createApp } from "vue";

const appElement = document.getElementById("ctx_02aecea7_e695_4749_bb2a_35e060975968");
if (appElement)
{
	const config = JSON.parse(appElement.dataset.ctxData || '');
	let props = {
		config: config
	};

	createApp(RequestMetricsSummaryComponent, props)
		.use(shadow)
		.mount(appElement);
}
