import 'babel-polyfill'
import Vue from "vue";
import HealthCheckPageComponent from "./components/HealthCheckPageComponent.vue";
import Vuetify from 'vuetify'

Vue.use(Vuetify, {
    iconfont: 'fa',
    options: {
        customProperties: true
    },
    theme: {
      primary: "#12436D",
      secondary: "#4C2E5F",
      accent: "#12436D",
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
