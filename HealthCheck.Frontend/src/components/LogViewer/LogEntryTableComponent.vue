<!-- src/components/Overview/StatusComponent.vue -->
<template>
    <div class="log-entry-table">
        <hr />
        <b> Columns: {{ columnNames }} </b>
        <hr />

        <div>
            <log-entry-component
                v-for="(entry, index) in entries"
                :key="`log-entry-${index}`"
                :entry="entry"
                :customColumnRule="customColumnRule"
                :customColumnMode="customColumnMode"
                />
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import LinqUtils from "../../util/LinqUtils";
import LogEntrySearchResultItem from '../../models/LogViewer/LogEntrySearchResultItem';
import { FilterDelimiterMode } from '../../models/LogViewer/FilterDelimiterMode';
import LogEntryComponent from './LogEntryComponent.vue';

@Component({
    components: { LogEntryComponent }
})
export default class LogEntryTableComponent extends Vue {
    @Prop({ required: true })
    entries!: Array<LogEntrySearchResultItem>;
    @Prop({ required: true })
    customColumnRule!: string;
    @Prop({ required: true })
    customColumnMode!: FilterDelimiterMode;

    columnNames: Array<string> = [];

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
    calcColumns(): void
    {
        this.columnNames = ["todo"];
    }

    // @Watch("customColumnMode")
    // onCustomColumnModeChanged(): void {
    //     this.calcColumnData();
    // }

    // @Watch("customColumnRule")
    // onCustomColumnRuleChanged(): void {
    //     this.calcColumnData();
    // }
}
</script>

<style scoped>
</style>