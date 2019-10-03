<!-- src/components/Overview/StatusComponent.vue -->
<template>
    <div class="log-entry">
        <b>Timestamp: </b>{{ entry.Timestamp }}<br />
        <b>Filepath: </b>{{ entry.FilePath }}<br />
        <b>Line number: </b>{{ entry.LineNumber }}<br />
        <b>Columns: </b>{{ entry.ColumnValues }}<br />

        <b> {{ columnNames }} </b>
        <b> {{ columnValues }} </b>
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

    columnRegex: RegExp | null = /(?<Date>.*,[0-9]{3}) \[(?<Thread>[0-9]+)\] (?<Severity>\w+) (?<Message>[^\n]*)\n?(?<Details>.*)/gism;
    columnNames: Array<string> = [];
    columnValues: Array<string> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.initColumnData();
    }

    ////////////////
    //  GETTERS  //
    //////////////

    ////////////////
    //  METHODS  //
    //////////////
    initColumnData(): void
    {
        if (this.columnRegex != null) {
            const regexMatches = <any>this.columnRegex.exec(this.entry.Raw);
            if (regexMatches != null) {
                const groupNames = Object.keys(regexMatches.groups);
                
                this.columnNames = groupNames;
                // ToDo: order columnNames by index of '(?<name>' in pattern
                // ToDo: column config per group: hide, inline/block, html-template?

                groupNames.forEach(groupName => {
                    this.columnValues.push(regexMatches.groups[groupName]);
                });
            }
        }
        
        if (this.columnNames.length === 0) {
            this.columnNames = [""];
            this.columnValues = [this.entry.Raw];
        }
    }
}
</script>

<style scoped>
.log-entry {

}
</style>