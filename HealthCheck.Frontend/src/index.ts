import 'babel-polyfill'
import Vue from "vue";
import HealthCheckPageComponent from "./components/HealthCheckPageComponent.vue";
import Vuetify from 'vuetify'

// Highlight.js
import VueHighlightJS from 'vue-highlight.js';
import xml from 'highlight.js/lib/languages/xml';
import json from 'highlight.js/lib/languages/json';
import 'highlight.js/styles/default.css';
Vue.use(VueHighlightJS, {
	languages: {
		xml,
		json
	}
});

// Extensions
import './util/extensions/StringExtensions';

Vue.use(Vuetify, {
    iconfont: 'fa',
    options: {
        customProperties: true
    },
    theme: {
      primary: "#6908b9",
      secondary: "#3c0777",
      accent: "#6908b9",
      error: "#de4a4a"
    }
});

let v = new Vue({
    el: "#app",
    template: `
    <div>
        <health-check-page-component :options="options" />
    </div>
    `,
    data: {
        options: (window as any).healthCheckOptions,
    },
    components: {
        HealthCheckPageComponent
    }
});
