<!-- src/components/Common/DataTableComponent.vue -->
<template>
    <div class="data-table">
        <table>
            <thead>
                <tr>
                    <th v-for="(header, headerIndex) in headers"
                        :key="`dtable-header-${headerIndex}`"
                        >{{ header }}</th>
                </tr>
            </thead>
            <tbody>
                <template v-for="(group, groupIndex) in groups">
                    <!-- HEADER -->
                    <tr class="data-table--row-group-header"
                        :key="`dtable-group-${groupIndex}-header`"
                        v-if="group.title.length > 0">
                        <td :colspan="headers.length">{{ group.title }}</td>
                    </tr>

                    <!-- VALUES -->
                    <template v-for="(row, rowIndex) in group.items" :key="`dtable-row-${groupIndex}-${rowIndex}`">
                        <tr class="data-table--row-values">
                            <td v-for="(value, valueIndex) in row.values"
                                :key="`dtable-item-values-${groupIndex}-${rowIndex}-${valueIndex}`"
                                @click="setExpanded(`dtable-row-${groupIndex}-${rowIndex}`)"
                                >
                                <div>{{ value }}</div>
                            </td>
                        </tr>
                        <tr class="data-table--row-expanded"
                            :key="`dtable-row-expanded-${groupIndex}-${rowIndex}`"
                            v-if="isExpanded(`dtable-row-${groupIndex}-${rowIndex}`)">
                            <td :colspan="headers.length">
                                <div name="expandedItem">{{ row }}</div>
                            </td>
                        </tr>
                    </template>
                </template>
            </tbody>
        </table>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";

import { DataTableGroup, DataTableItem } from '@components/Common/DataTableComponent.vue.models';
@Options({
    components: {}
})
export default class DataTableComponent extends Vue
{
    @Prop({ required: true })
    groups!: Array<DataTableGroup>;

    @Prop({ required: true })
    headers!: Array<string>;

    expandedRows: Array<string> = [];

    mounted(): void {
        // this.headers = [ 'Timestamp', 'Display name', 'Details', 'Price', 'More things', 'Expires at', 'Something else' ];
        
        // this.createGroup('Last hour', 3);
        // this.createGroup('Earlier today', 11);
        // this.createGroup('Yesterday', 24);
        // this.createGroup('Earlier', 82);
    }

    isExpanded(id: string): boolean
    {
        return this.expandedRows.some(x => x == id);
    }

    setExpanded(id: string): void
    {
        if (this.isExpanded(id)) {
            this.expandedRows = this.expandedRows.filter(x => x != id);
        } else {
            this.expandedRows.push(id);
        }
    }

    @Watch("groups")
    onGroupsChanged(): void {
        this.expandedRows = [];
    }
}
</script>

<style scoped lang="scss">
.data-table {
    /* background-color: #292929;
    color: #eee; */
    overflow-x: auto;
    padding: 10px;
    background-color: #fff;

    table {
        width: 100%;
        border-collapse: collapse;

        /* th, td {
            min-width: 100px;
        } */

        th {
            text-align: left;
            padding-right: 10px;
            white-space: pre;
        }

        td {
            padding-top: 10px;
            padding-bottom: 10px;
            padding-right: 20px;

            &:first-child {
                padding-left: 10px;
                /* padding-right: 0; */
            }
        }
    }

    .data-table--row-group-header
    {
        font-weight: 600;
    }

    .data-table--row-values {
        cursor: pointer;

        td {
            border-top: 1px solid #ccc;
        }
        &:first-child {
            td {
                border-top: none;
            }
        }
    }
}
</style>