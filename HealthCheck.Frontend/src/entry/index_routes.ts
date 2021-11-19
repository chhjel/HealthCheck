// Modules
import TestSuitesPageComponent from '../components/modules/TestSuite/TestSuitesPageComponent.vue';
import OverviewPageComponent from '../components/modules/Overview/OverviewPageComponent.vue';
import AuditLogPageComponent from '../components/modules/AuditLog/AuditLogPageComponent.vue';
import LogViewerPageComponent from '../components/modules/LogViewer/LogViewerPageComponent.vue';
import RequestLogPageComponent from '../components/modules/RequestLog/RequestLogPageComponent.vue';
import DocumentationPageComponent from '../components/modules/Documentation/DocumentationPageComponent.vue';
import DataflowPageComponent from '../components/modules/Dataflow/DataflowPageComponent.vue';
import SettingsPageComponent from '../components/modules/Settings/SettingsPageComponent.vue';
import EventNotificationsPageComponent from '../components/modules/EventNotifications/EventNotificationsPageComponent.vue';
import DynamicCodeExecutionPageComponent from '../components/modules/DynamicCodeExecution/DynamicCodeExecutionPageComponent.vue';
import AccessTokensPageComponent from '../components/modules/AccessTokens/AccessTokensPageComponent.vue';
import SecureFileDownloadPageComponent from '../components/modules/SecureFileDownload/SecureFileDownloadPageComponent.vue';
import EndpointControlPageComponent from '../components/modules/EndpointControl/EndpointControlPageComponent.vue';
import MessagesPageComponent from '../components/modules/Messages/MessagesPageComponent.vue';
import ReleaseNotesPageComponent from '../components/modules/ReleaseNotes/ReleaseNotesPageComponent.vue';
import MetricsPageComponent from '../components/modules/Metrics/MetricsPageComponent.vue';
import DataRepeaterPageComponent from '../components/modules/DataRepeater/DataRepeaterPageComponent.vue';
import CustomPageComponent from '../components/modules/Custom/CustomPageComponent.vue';
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
    'DynamicCodeExecutionPageComponent': DynamicCodeExecutionPageComponent,
    'AccessTokensPageComponent': AccessTokensPageComponent,
    'SecureFileDownloadPageComponent': SecureFileDownloadPageComponent,
    'EndpointControlPageComponent': EndpointControlPageComponent,
    'MessagesPageComponent': MessagesPageComponent,
    'ReleaseNotesPageComponent': ReleaseNotesPageComponent,
    'MetricsPageComponent': MetricsPageComponent,
    'DataRepeaterPageComponent': DataRepeaterPageComponent,
    'CustomPageComponent': CustomPageComponent
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
          component: moduleComponents[config.ComponentName || 'CustomPageComponent'],
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