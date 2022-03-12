<!-- src/components/modules/LogViewer/LogEntryTableComponent.vue -->
<template>
    <div class="log-entry-table" v-if="rows.length > 0">

        <div class="v-table__overflow">
            <table class="v-datatable v-table theme--light">
                <thead v-if="showHeaders">
                    <tr>
                        <th class="column text-xs-left"
                            v-for="(columnName, cindex) in columnNames"
                            :key="`log-column-header-${cindex}`">{{ columnName }}</th>
                    </tr>
                    <tr class="v-datatable__progress">
                        <th :colspan="columnNames.length" class="column"></th>
                    </tr>
                </thead>
                <tbody>
                    <template 
                        v-for="(row, entryRowIndex) in rows">
                        <tr class="log-table-row"
                            :class="getEntryRowClasses(row.Entry)"
                            :key="`log-entry-row-${entryRowIndex}`"
                            @click="onRowClicked(entryRowIndex)">

                            <td class="text-xs-left"
                                v-for="(rowValue, vindex) in row.Values"
                                :key="`log-entry-row-value-${vindex}`"
                                valign="top">
                                <pre v-if="preColumns.indexOf(vindex) != -1">{{ rowValue }}</pre>
                                <span v-else>{{ rowValue }}</span>
                                
                                <span class="log-entry-fileref" v-if="vindex == 0">
                                    <code>{{ row.Entry.FilePath.substring(row.Entry.FilePath.lastIndexOf("\\")+1) }}</code> line <code>{{ row.Entry.LineNumber }}</code>
                                </span>
                            </td>
                        </tr>
                        <tr v-if="isRowExpanded(entryRowIndex)"
                            :key="`log-entry-details-row-${entryRowIndex}`"
                            class="log-entry-details-row"
                            :class="getEntrySeverityClass(row.Entry.Severity)">
                            <td :colspan="columnNames.length"
                                valign="top">
                                <span class="log-entry-fileref">
                                    <code>{{ row.Entry.FilePath }}</code> line <code>{{ row.Entry.LineNumber }}</code>
                                </span>
                                <pre v-if="showHeaders">{{ row.Entry.Raw }}</pre>
                            </td>
                        </tr>
                    </template> 
                </tbody>
            </table>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import LinqUtils from '@util/LinqUtils';
import { LogEntrySearchResultItem } from '@generated/Models/Core/LogEntrySearchResultItem';
import { FilterDelimiterMode } from '@models/modules/LogViewer/FilterDelimiterMode';
import DateUtils from '@util/DateUtils';
import * as XRegExp from 'xregexp';
import { LogEntrySeverity } from '@generated/Enums/Core/LogEntrySeverity';

interface TableEntryRow {
    Entry: LogEntrySearchResultItem;
    Values: Array<string>;
}
interface ColumnsAndValues {
    Columns: Array<string>;
    Values: Array<string>;
}

@Options({
    components: { }
})
export default class LogEntryTableComponent extends Vue {
    @Prop({ required: true })
    entries!: Array<LogEntrySearchResultItem>;
    @Prop({ required: true })
    customColumnRule!: string;
    @Prop({ required: true })
    customColumnMode!: FilterDelimiterMode;
    @Prop({ required: true })
    expandAllRows!: boolean;

    showHeaders: boolean = true;
    preColumns: Array<number> = [];
    columnNames: Array<string> = [];
    rows: Array<TableEntryRow> = [];
    expandedRows: Array<number> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.calcColumns();
    }

    ////////////////
    //  GETTERS  //
    //////////////

    ////////////////
    //  METHODS  //
    //////////////
    getEntryRowClasses(entry: LogEntrySearchResultItem): any
    {
        let classes: any = {};
        classes['log-table-row-margin'] = entry.IsMargin;
        classes[this.getEntrySeverityClass(entry.Severity)] = true;
        return classes;
    }

    getEntrySeverityClass(severity: LogEntrySeverity): string
    {
        switch(severity)
        {
            case LogEntrySeverity.Error: return "log-entry-error";
            case LogEntrySeverity.Warning: return "log-entry-warning";
            default: return "log-entry-info";
        }
    }

    isRowExpanded(index: number): boolean {
        return this.expandAllRows || this.expandedRows.indexOf(index) != -1;
    }

    onRowClicked(index: number): void {
        this.toggleExpandedRow(index);
    }

    toggleExpandedRow(index: number): void {
        if (this.isRowExpanded(index)) {
            this.expandedRows = this.expandedRows.filter(x => x != index);
        } else {
            this.expandedRows.push(index);
        }
    }

    calcColumns(): void
    {
        this.columnNames = [];
        this.rows = this.entries.map(entry => {
            const entryData = this.calcColumnValues(entry);
            entryData.Columns.forEach(x => {
                if (this.columnNames.indexOf(x) == -1) {
                    this.columnNames.push(x);
                }
            });

            return {
                Entry: entry,
                Values: entryData.Values
            };
        });
    }
    
    calcColumnValues(entry: LogEntrySearchResultItem): ColumnsAndValues
    {
        let item: ColumnsAndValues = {
            Columns: [ "Timestamp" ],
            Values: [ this.formatDateForTable(new Date(entry.Timestamp)) ]
        };

        if (this.customColumnRule != null && this.customColumnRule.length > 0)
        {
            if (this.customColumnMode == FilterDelimiterMode.Delimiter)
            {
                const parts = entry.Raw.split(this.customColumnRule);
                for (let i=0;i<parts.length;i++) {
                    item.Columns.push(`Column ${(i+1)}`);
                    item.Values.push(parts[i]);
                }
            }
            else if (this.customColumnMode == FilterDelimiterMode.Regex)
            {
                const regexp = XRegExp(this.customColumnRule, "gism");
                const regexMatches = XRegExp.exec(entry.Raw, regexp);

                if (regexMatches != null) {
                    const matchWithoutGroupsKeys = XRegExp.exec('asdasd', XRegExp('asd', 'gism'));
                    const discardedKeys = Object.keys(matchWithoutGroupsKeys);
                    const matchKeys = Object.keys(regexMatches);
                    const groupNames = matchKeys.filter(x => 
                        discardedKeys.indexOf(x) == -1 && !/^[0-9]+$/.test(x)
                    );
                
                    // order columnNames by index of '(?<name>' in pattern
                    let groupPositions: Array<any> = [];
                    groupNames.forEach(groupName => {
                        groupPositions.push({
                            index: this.customColumnRule.indexOf(`(?<${groupName}>`),
                            name: groupName
                        });
                    });

                    groupPositions = groupPositions.sort((a, b) => (a.index > b.index) ? 1 : -1)

                    groupPositions.forEach(group => {
                        item.Columns.push(group.name);
                        item.Values.push(regexMatches[group.name]);
                    });
                }
            }
            
            this.showHeaders = true;
        }
        else {
            this.showHeaders = false;
        }
        
        this.preColumns = [0];
        if (item.Columns.length === 1) {
            item.Columns.push("Raw");
            item.Values.push(entry.Raw);
            this.preColumns = [0, 1];
        }

        return item;
    }
    
    formatDateForTable(date: Date): string {
        return DateUtils.FormatDate(date, 'HH:mm:ss d. MMM. yyyy');
    }

    @Watch("customColumnMode")
    onCustomColumnModeChanged(): void {
        this.calcColumns();
    }

    @Watch("customColumnRule")
    onCustomColumnRuleChanged(): void {
        this.calcColumns();
    }

    @Watch("entries")
    onEntriesChanged(): void {
        this.expandedRows = [];
        this.calcColumns();
    }
}
</script>

<style scoped>
.log-table-row {
    cursor: pointer;
}
.log-table-row-margin {
    background-color: #f3f2f2;
}
.log-table-row-margin:hover {
    background-color: #eaeaea !important;
}
.log-table-row td pre:last-child {
    white-space: normal;
}
.log-entry-details-row td pre {
    padding-left: 20px;
    white-space: normal;
}
.log-entry-fileref {
    padding: 1px;
    display: block;
}
.log-entry-fileref code {
    color: #808080;
    box-shadow: none;
    font-weight: 500;
}
.log-entry-info {
    border-left: 25px solid #e0eefb;
}
.log-entry-info:hover {
    border-left: 25px solid #edf7ff;
}
.log-entry-error {
    border-left: 25px solid var(--v-error-lighten1);
}
.log-entry-error:hover {
    border-left: 25px solid var(--v-error-lighten2);
}
.log-entry-warning {
    border-left: 25px solid var(--v-warning-lighten1);
}
.log-entry-warning:hover {
    border-left: 25px solid var(--v-warning-lighten2);
}
.log-entry-details-row {
    border-left-width: 50px !important;
}
</style>
<style>
.log-entry-table td,
.log-entry-table th {
    max-width: 25%;
    word-break: break-all;
}
</style>