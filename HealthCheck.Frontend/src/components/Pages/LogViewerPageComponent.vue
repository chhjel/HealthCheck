<!-- src/components/Pages/LogViewerPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
          
          ToDo: LogViewer here

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
import DateUtils from '../../util/DateUtils';
import '@lazy-copilot/datetimepicker/dist/datetimepicker.css'
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";

@Component({
    components: {
        DateTimePicker
    }
})
export default class AuditLogPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // // Filter fields
    // filterArea: AuditEventArea | null = null;
    // filterSubject: string = "";
    // filterAction: string = "";
    // filterUserId: string = "";
    // filterUserName: string = "";
    // filterFromDate: Date = new Date();
    // filterToDate: Date = new Date();

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        // this.resetFilters();
        // this.loadData();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    // get AreaChoices(): Array<AuditEventArea | null> {
    //     let choices = new Array<AuditEventArea | null>();
    //     choices.push(null);
    //     for (let value in AuditEventArea) {
    //         choices.push(value as AuditEventArea);
    //     }
    //     return choices;
    // }

    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped>
</style>

<style>
</style>