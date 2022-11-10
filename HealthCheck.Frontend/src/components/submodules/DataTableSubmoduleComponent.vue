<!-- src/components/submodules/DataTableSubmoduleComponent.vue -->
<template>
    <div class="submodule">
        <div>
            <paging-component
                :count="rowCount"
                :pageSize="pageSize"
                v-model:value="pageIndex"
                :asIndex="true"
                class="mb-2 mt-2"
                />
            
            <table class="table">
                <thead>
                    <draggable
                        v-model="headerIndices"
                        :itemKey="x => `datatable-${id}-header-${x}`"
                        group="grp"
                        style="min-height: 10px"
                        tag="tr"
                        @end="onDragEnded">
                        <template #item="{element}">
                            <th class="datatable-header">{{ subModuleOptions.Headers[element] }}</th>
                        </template>
                    </draggable>
                </thead>
                <tbody>
                    <tr v-for="(row, rowIndex) in currentRows"
                        :key="`datatable-${id}-${pageIndex}-${rowIndex}`"
                        class="datatable-row">
                        <td v-for="(headerIndex, colIndex) in headerIndices"
                            :key="`datatable-${id}-${pageIndex}-${rowIndex}-col-${colIndex}`"
                            class="datatable-cell">
                            {{row[headerIndex]}}
                        </td>
                    </tr>
                </tbody>
            </table>
        
            <paging-component
                :count="rowCount"
                :pageSize="pageSize"
                v-model:value="pageIndex"
                :asIndex="true"
                class="mb-2 mt-2"
                />
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import { DataTableSubmoduleOptions } from "./DataTableSubmodule";
import IdUtils from "@util/IdUtils";
import PagingComponent from "@components/Common/Basic/PagingComponent.vue";
import draggable from 'vuedraggable'

@Options({
    components: {
        PagingComponent,
        draggable
    }
})
export default class DiffComponent extends Vue {
    @Prop()
    subModuleOptions: DataTableSubmoduleOptions;
    
    id: string = IdUtils.generateId();
    headerIndices: Array<number> = [];
    sortByHeaderIndex: number | null = null;
    sortAscending: boolean = false;

    // Pagination
    pageSizeDefault: number = 50;
    pageIndex: number = 0;
    pageSize: number = this.pageSizeDefault;

    created(): void {
        this.headerIndices = this.subModuleOptions.Headers.map((x, i) => i);
    }

    mounted(): void {
    }

    beforeDestroy(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get rowCount(): number { return this.subModuleOptions.Rows.length; }

    get currentRows(): Array<Array<string>> {
        return this.subModuleOptions.Rows
            .slice(this.pageIndex * this.pageSize)
            .slice(0, this.pageSize);
    }

    ////////////////////////////////////////////////////////////
    //// EVENTHANDLERS /////////////////////////////////////////
    ////////////////////////////////////////////////////////////
    onDragEnded(e: any): void {
        const oldIndex = e.oldIndex;
        const newIndex = e.newIndex;
    }
}
</script>

<style scoped lang="scss">
</style>
