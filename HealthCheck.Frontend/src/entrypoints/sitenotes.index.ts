import 'babel-polyfill'
import Vue from "vue";
import Vuetify from 'vuetify'

// Extensions
import '../util/extensions/StringExtensions';
import '../util/extensions/ArrayExtensions';

import InjectedSiteNotesComponent from '../components/SiteNotes/InjectedSiteNotesComponent.vue';

Vue.use(Vuetify, {
    iconfont: 'fa',
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

let elId = 'sitenotes-app-c26c1905-3bd4-4f7f-8671-c5ce508d7b23';
var elem = document.createElement('div');
elem.id = elId;
document.body.appendChild(elem);
        
let v = new Vue({
    el: `#${elId}`,
    template: `
    <div>
        <injected-site-notes-component />
    </div>
    `,
    data: {
        options: (window as any).healthCheckOptions,
    },
    components: {
        InjectedSiteNotesComponent
    }
});
