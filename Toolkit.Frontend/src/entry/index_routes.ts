// Modules
import TestSuitesPageComponent from '../components/modules/TestSuite/TestSuitesPageComponent.vue';
import EventCalendarComponent from '../components/modules/Overview/OverviewPageComponent.vue';
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
import DataExportPageComponent from '../components/modules/DataExport/DataExportPageComponent.vue';
import ContentPermutationPageComponent from '../components/modules/ContentPermutation/ContentPermutationPageComponent.vue';
import ComparisonPageComponent from '../components/modules/Comparison/ComparisonPageComponent.vue';
import GoToPageComponent from '../components/modules/GoTo/GoToPageComponent.vue';
import MappedDataPageComponent from '../components/modules/MappedData/MappedDataPageComponent.vue';
import JobsPageComponent from '../components/modules/Jobs/JobsPageComponent.vue';
// [tool:pageImport]
import CustomPageComponent from '../components/modules/Custom/CustomPageComponent.vue';
import NoPageAvailablePageComponent from '../components/NoPageAvailablePageComponent.vue';
import DevPageComponent from '../components/modules/Dev/DevPageComponent.vue';
import ModuleOptions from '../models/Common/ModuleOptions';
import ModuleConfig from '../models/Common/ModuleConfig';
import { nextTick } from 'vue';
import { createRouter, createWebHashHistory, Router, RouteRecordRaw } from 'vue-router';

export default function createTKRouter(moduleConfig: Array<ModuleConfig>): Router {
  let moduleComponents: Record<string, any> = {
    'TestSuitesPageComponent': TestSuitesPageComponent,
    'OverviewPageComponent': EventCalendarComponent,
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
    'DataExportPageComponent': DataExportPageComponent,
    'ContentPermutationPageComponent': ContentPermutationPageComponent,
    'ComparisonPageComponent': ComparisonPageComponent,
    'GoToPageComponent': GoToPageComponent,
    'MappedDataPageComponent': MappedDataPageComponent,
    'JobsPageComponent': JobsPageComponent,
    // [tool:pageRecord]
    'CustomPageComponent': CustomPageComponent
};

let moduleOptions = ((window as any).toolkitModuleOptions) as Record<string, ModuleOptions<any>>;
  const initialWindowTitle = document.title;
  let routes: Array<RouteRecordRaw> = [];
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
          meta: { title: (r: RouteRecordRaw) => `${config.Name} | ${initialWindowTitle}` }
      });
  });

  if (moduleConfig.length == 0)
  {
    routes.push({ path: '/:catchAll(.*)*', component: NoPageAvailablePageComponent });
  }
  routes.push({ path: '/styling', component: DevPageComponent });
  const router = createRouter({
    routes: routes,
    history: createWebHashHistory()
  });

  router.afterEach((to, from) => {
    nextTick(() => {
        if (to != null && to.meta != null && to.meta.title != null)
        {
            document.title = (<any>to).meta.title(to);
        }        
    })
  })

  return router;
}
