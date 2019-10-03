<!-- src/components/Overview/StatusComponent.vue -->
<template>
    <div class="log-entry">
        <b>Timestamp: </b>{{ entry.Timestamp }}<br />
        <b>Filepath: </b>{{ entry.FilePath }}<br />
        <b>Line number: </b>{{ entry.LineNumber }}<br />
        <b>Columns: </b>{{ entry.ColumnValues }}<br />

        <pre>{{ entry.Raw }}</pre>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import LinqUtils from "../../util/LinqUtils";
import LogEntrySearchResultItem from '../../models/LogViewer/LogEntrySearchResultItem';

@Component({
    components: {}
})
export default class LogEntryComponent extends Vue {
    @Prop({ required: true })
    entry!: LogEntrySearchResultItem;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////

    ////////////////
    //  METHODS  //
    //////////////
}
</script>

<style scoped>
</style>