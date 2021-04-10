import "babel-polyfill";
import Vue from "vue";
import shadow from 'vue-shadow-dom';

// Polyfills
import "es6-promise/auto";
import 'whatwg-fetch';

import RequestMetricsSummaryComponent from 'components/modules/Metrics/RequestMetricsSummaryComponent.vue';

// Init libs
Vue.use(shadow);

const appElement = document.getElementById("ctx_02aecea7_e695_4749_bb2a_35e060975968");
if (appElement)
{
	const config = JSON.parse(appElement.dataset.ctxData || '');

	new Vue({
		el: appElement,
		template: `
		<div>
			<request-metrics-summary-component :config="config" />
		</div>
		`,
		data: {
			config: config
		},
		components: {
			RequestMetricsSummaryComponent
		}
	});
}
