<!-- src/components/Pages/AuditLogPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            
            <h1 class="mb-4">Audit log</h1>

            // ToDo


          <!-- CONTENT END -->
        </v-flex>
        </v-layout>
        </v-container>
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Page/FrontEndOptionsViewModel';
import AuditEventViewModel from '../../models/AuditLog/AuditEventViewModel';
import { AuditEventArea } from '../../models/AuditLog/AuditEventArea';

@Component({
    components: {
    }
})
export default class AuditLogPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    filteredAuditEvents: Array<AuditEventViewModel> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        // this.loadData();
    }

    ////////////////
    //  GETTERS  //
    //////////////

    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        // this.overviewDataLoadInProgress = true;
        // this.overviewDataLoadFailed = false;

        // let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        // let url = `${this.options.GetSiteEventsEndpoint}${queryStringIfEnabled}`;
        // fetch(url, {
        //     credentials: 'include',
        //     method: "GET",
        //     headers: new Headers({
        //         'Content-Type': 'application/json',
        //         Accept: 'application/json',
        //     })
        // })
        // .then(response => response.json())
        // .then((events: Array<SiteEventViewModel>) => this.onEventDataRetrieved(events))
        // .catch((e) => {
        //     this.overviewDataLoadInProgress = false;
        //     this.overviewDataLoadFailed = true;
        //     this.overviewDataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
        //     console.error(e);
        // });
    }
    
    // onEventDataRetrieved(events: Array<SiteEventViewModel>): void {
    //     let index = -1;
    //     events.forEach(x => {
    //         index++;
    //         x.Id = index.toString();
    //         x.Timestamp = new Date(x.Timestamp);
    //     });
    //     this.siteEvents = events;
    // }
}
</script>

<style scoped>
</style>

<style>
</style>