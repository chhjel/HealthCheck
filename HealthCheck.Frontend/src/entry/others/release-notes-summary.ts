import Vue from "vue";
import shadow from 'vue-shadow-dom';

// Polyfills
import "es6-promise/auto";
import 'whatwg-fetch';

import ReleaseNotesSummaryComponent from 'components/modules/ReleaseNotes/ReleaseNotesSummaryComponent.vue';

// Init libs
Vue.use(shadow);

const appElement = document.getElementById("rn_00bfcf84-3633-411e-acd2-b9398d252da7");
if (appElement)
{
	const config = JSON.parse(appElement.dataset.ctxData || '');

	new Vue({
		el: appElement,
		template: `
		<div>
			<release-notes-summary-component :config="config" />
		</div>
		`,
		data: {
			config: config
		},
		components: {
			ReleaseNotesSummaryComponent
		}
	});
}
