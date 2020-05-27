// Modules
import TestSuitesPageComponent from '../components/TestSuite/TestSuitesPageComponent.vue';
import OverviewPageComponent from '../components/Overview/OverviewPageComponent.vue';
import AuditLogPageComponent from '../components/AuditLog/AuditLogPageComponent.vue';
import LogViewerPageComponent from '../components/LogViewer/LogViewerPageComponent.vue';
import RequestLogPageComponent from '../components/RequestLog/RequestLogPageComponent.vue';
import DocumentationPageComponent from '../components/Documentation/DocumentationPageComponent.vue';
import DataflowPageComponent from '../components/Dataflow/DataflowPageComponent.vue';
import SettingsPageComponent from '../components/Settings/SettingsPageComponent.vue';
import EventNotificationsPageComponent from '../components/EventNotifications/EventNotificationsPageComponent.vue';
import DynamicCodeExecutionPageComponent from '../components/DynamicCodeExecution/DynamicCodeExecutionPageComponent.vue';
import NoPageAvailablePageComponent from '../components/NoPageAvailablePageComponent.vue';
import Vue, { VueConstructor } from "vue";
import VueRouter, { RouteConfig } from 'vue-router';
import ModuleOptions from '../models/Common/ModuleOptions';
import ModuleConfig from '../models/Common/ModuleConfig';

export default function createRouter(moduleConfig: Array<ModuleConfig>): VueRouter {
  let moduleComponents: Record<string, VueConstructor<Vue>> = {
    'TestSuitesPageComponent': TestSuitesPageComponent,
    'OverviewPageComponent': OverviewPageComponent,
    'AuditLogPageComponent': AuditLogPageComponent,
    'LogViewerPageComponent': LogViewerPageComponent,
    'RequestLogPageComponent': RequestLogPageComponent,
    'DocumentationPageComponent': DocumentationPageComponent,
    'DataflowPageComponent': DataflowPageComponent,
    'SettingsPageComponent': SettingsPageComponent,
    'EventNotificationsPageComponent': EventNotificationsPageComponent,
    'DynamicCodeExecutionPageComponent': DynamicCodeExecutionPageComponent
};

let moduleOptions = ((window as any).healthCheckModuleOptions) as Record<string, ModuleOptions<any>>;
  const initialWindowTitle = document.title;
  let routes: Array<RouteConfig> = [];
  moduleConfig
      .filter(config => config.LoadedSuccessfully)
      .forEach(config => {
      routes.push({
          name: config.Id,
          path: config.RoutePath,
          component: moduleComponents[config.ComponentName],
          props: {
              config: config,
              options: moduleOptions[config.Id]
          },
          meta: { title: (r: RouteConfig) => `${config.Name} | ${initialWindowTitle}` }
      });
  });

  if (moduleConfig.length == 0)
  {
    routes.push({ path: '/*', component: NoPageAvailablePageComponent });
  }
  
  const router = new VueRouter({
    routes: routes,
  });

  router.afterEach((to, from) => {
    Vue.nextTick(() => {
        if (to != null && to.meta != null && to.meta.title != null)
        {
            document.title = to.meta.title(to);
        }        
    })
  })

  return router;
}