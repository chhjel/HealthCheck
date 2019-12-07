import 'babel-polyfill'
import Vue from "vue";
import HealthCheckPageComponent from "../components/HealthCheckPageComponent.vue";
import Vuetify from 'vuetify'

// Extensions
import '../util/extensions/StringExtensions';
import '../util/extensions/ArrayExtensions';

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
