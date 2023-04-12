import shadow from 'vue-shadow-dom';

// Polyfills
import "es6-promise/auto";
import 'whatwg-fetch';

import ReleaseNotesSummaryComponent from '@components/modules/ReleaseNotes/ReleaseNotesSummaryComponent.vue';
import { createApp } from 'vue';

const appElement = document.getElementById("rn_00bfcf84-3633-411e-acd2-b9398d252da7");
if (appElement)
{
	const config = JSON.parse(appElement.dataset.ctxData || '');
	let props = {
		config: config
	};

	createApp(ReleaseNotesSummaryComponent, props)
		.use(shadow)
		.mount(appElement);
}
