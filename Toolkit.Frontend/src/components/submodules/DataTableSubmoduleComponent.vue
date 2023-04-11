<!-- src/components/submodules/DataTableSubmoduleComponent.vue -->
<template>
    <div class="submodule">
        <div>
            <vue-good-table
                :columns="columns"
                :rows="rows"
                :line-numbers="true"
                :pagination-options="{
                    enabled: true,
                    mode: 'pages',
                    perPage: 10,
                    position: 'bottom',
                    perPageDropdown: [10,20,30,40,50],
                    dropdownAllowAll: false,
                    setCurrentPage: 1,
                    nextLabel: 'next',
                    prevLabel: 'prev',
                    rowsPerPageLabel: 'Rows per page',
                    ofLabel: 'of',
                    pageLabel: 'page', // for 'pages' mode
                    allLabel: 'All'
                }"
                :sort-options="{
                    enabled: true,
                    multipleColumns: true
                }"
                :search-options="{
                    enabled: true,
                    trigger: 'enter'
                }"
                styleClass="vgt-table striped bordered"
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
import { VueGoodTable } from 'vue-good-table-next';

@Options({
    components: {
        PagingComponent,
        VueGoodTable
    }
})
export default class DiffComponent extends Vue {
    @Prop()
    subModuleOptions: DataTableSubmoduleOptions;
    
    id: string = IdUtils.generateId();
    columns: Array<any> = [];
    rows: Array<any> = [];

    created(): void {
        this.columns = this.subModuleOptions.Headers.map(h => ({
            label: h,
            field: h
        }));
        this.rows = this.subModuleOptions.Rows.map((r, i) => {
            let rowData = {};
            this.subModuleOptions.Headers.forEach((h, hIndex) => {
                rowData[h] = r[hIndex];
            });
            return { ...{ id: i }, ...rowData };
        });
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

    ////////////////////////////////////////////////////////////
    //// EVENTHANDLERS /////////////////////////////////////////
    ////////////////////////////////////////////////////////////
}
</script>

<style scoped lang="scss">
</style>
