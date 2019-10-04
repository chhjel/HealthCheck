<!-- src/components/Overview/StatusComponent.vue -->
<template>
    <div class="log-entry">
        <b>Timestamp: </b>{{ entry.Timestamp }}<br />
        <b>Filepath: </b>{{ entry.FilePath }}:{{ entry.LineNumber }}<br />

        <b> {{ columnNames }} </b>
        <b> {{ columnValues }} </b>

        <pre>{{ entry.Raw }}</pre>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import LinqUtils from "../../util/LinqUtils";
import LogEntrySearchResultItem from '../../models/LogViewer/LogEntrySearchResultItem';
import { FilterDelimiterMode } from '../../models/LogViewer/FilterDelimiterMode';

@Component({
    components: {}
})
export default class LogEntryComponent extends Vue {
    @Prop({ required: true })
    entry!: LogEntrySearchResultItem;
    @Prop({ required: true })
    customColumnRule!: string;
    @Prop({ required: true })
    customColumnMode!: FilterDelimiterMode;

    columnNames: Array<string> = [];
    columnValues: Array<string> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.calcColumnData();
    }

    ////////////////
    //  GETTERS  //
    //////////////

    ////////////////
    //  METHODS  //
    //////////////
    calcColumnData(): void
    {
        this.columnNames = [];
        this.columnValues = [];

        if (this.customColumnRule != null && this.customColumnRule.length > 0)
        {
            if (this.customColumnMode == FilterDelimiterMode.Delimiter)
            {
                const parts = this.entry.Raw.split(this.customColumnRule);
                for (let i=0;i<parts.length;i++) {
                    this.columnNames.push(`Column ${(i+1)}`);
                    this.columnValues.push(parts[i]);
                }
            }
            else if (this.customColumnMode == FilterDelimiterMode.Regex)
            {
                const regexp = new RegExp(this.customColumnRule, "gism");
                const regexMatches = <any>regexp.exec(this.entry.Raw);
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
        }
        
        if (this.columnNames.length === 0) {
            this.columnNames = [""];
            this.columnValues = [this.entry.Raw];
        }
    }

    @Watch("customColumnMode")
    onCustomColumnModeChanged(): void {
        this.calcColumnData();
    }

    @Watch("customColumnRule")
    onCustomColumnRuleChanged(): void {
        this.calcColumnData();
    }
}
</script>

<style scoped>
.log-entry {

}
</style>