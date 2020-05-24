<!-- src/components/Common/FilterableListComponent.vue -->
<template>
    <div>
        <v-list expand class="menu-items">
            <filter-input-component class="filter" v-model="filterText" />

            <v-progress-linear 
                v-if="loading"
                indeterminate color="green"></v-progress-linear>
            
            <!-- GROUPS: START -->
            <v-list-group
                no-action
                sub-group
                prepend-icon="keyboard_arrow_up"
                value="true"
                v-for="(group, gindex) in groups"
                :key="`filterable-menu-group-${gindex}`">
                <template v-slot:activator>
                    <v-list-tile>
                        <v-list-tile-title v-text="group.title"></v-list-tile-title>
                        <v-badge class="mr-3" v-if="showFilterCounts">
                            <template v-slot:badge>
                                <span>{{ getGroupFilterMatchCount(group) }}</span>
                            </template>
                        </v-badge>
                    </v-list-tile>
                </template>

                <v-list-tile ripple
                    v-for="(item, itemIndex) in filterItems(group.items)"
                    :key="`filterable-menu-item-${itemIndex}`"
                    class="testset-menu-item"
                    :class="{ 'active': itemIsSelected(item) }"
                    @click="onItemClicked(item)"
                    :disabled="disabled">
                    <v-list-tile-title>
                        {{ item.title }}
                        <br v-if="item.subTitle != null">
                        <span style="color: darkgray;" v-if="item.subTitle != null">{{ item.subTitle }}</span>
                    </v-list-tile-title>
                </v-list-tile>
            </v-list-group>
            <!-- GROUPS: END -->

            <!-- NO GROUP: START -->
            <v-list-tile ripple
                v-for="(item, itemIndex) in filterItems(ungroupedItems)"
                :key="`stream-menu-${itemIndex}`"
                class="testset-menu-item"
                :class="{ 'active': itemIsSelected(item) }"
                @click="onItemClicked(item)"
                :disabled="disabled">
                <v-list-tile-title v-text="item.title"></v-list-tile-title>
            </v-list-tile>
            <!-- NO GROUP: END -->

        </v-list>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Common/FrontEndOptionsViewModel';
import { EntryState } from '../../models/RequestLog/EntryState';
import DateUtils from "../../util/DateUtils";
import LinqUtils from "../../util/LinqUtils";
import KeyArray from "../../util/models/KeyArray";
import KeyValuePair from "../../models/Common/KeyValuePair";
// @ts-ignore
import FilterInputComponent from '.././Common/FilterInputComponent.vue';

export interface FilterableListGroup
{
    title: string;
    items: Array<FilterableListItem>;
}
export interface FilterableListItem
{
    title: string;
    subtitle: string | null;
    data: any;
}

@Component({
    components: {
        FilterInputComponent
    }
})
export default class FilterableListComponent extends Vue {
    @Prop({ required: true })
    items!: Array<FilterableListItem>;

    @Prop({ required: true })
    groupByKey!: string;

    @Prop({ required: false, default: null })
    sortByKey!: string | null;

    @Prop({ required: false, default: [] })
    filterKeys!: Array<string>;

    @Prop({ required: false, default: false })
    loading!: boolean;

    @Prop({ required: false, default: false })
    disabled!: boolean;

    @Prop({ required: false, default: true })
    groupIfSingleGroup!: boolean;

    filterText: string = "";
    selectedItemData: any = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get ungroupedItems(): Array<FilterableListItem>
    {
        return (this.groups.length == 0) ? this.items : [];
    }

    get groups(): Array<FilterableListGroup>
    {
        let groupList: Array<FilterableListGroup> = [];

        LinqUtils.GroupByInto(this.items, (x: any) => x.data[this.groupByKey], 
            (key, items) => groupList.push({
                title: key,
                items: items
            }));

        const groupsRequiredForGrouping = (this.groupIfSingleGroup ? 1 : 2);
        if (groupList.length < groupsRequiredForGrouping)
        {
            return [];
        }

        if (this.sortByKey != null)
        {
            groupList = groupList.sort((a, b) => LinqUtils.SortBy(a, b, (x: any) => x[this.sortByKey || '']));
        }

        return groupList;
    }

    get showFilterCounts(): boolean {
        return this.filterText.length > 0;
    }

    ////////////////
    //  METHODS  //
    //////////////
    filterItems(data: Array<FilterableListItem>) : Array<FilterableListItem> {
        return data.filter(x => this.itemFilterMatches(x));
    }

    itemFilterMatches(item: FilterableListItem): boolean {
        for (let index in this.filterKeys)
        {
            let key = this.filterKeys[index];
            let value = item.data[key];
            if (value != null && value.toLowerCase().indexOf(this.filterText.toLowerCase().trim()) != -1)
            {
                return true;
            }
        }
        return false;
    }

    getGroupFilterMatchCount(group: FilterableListGroup): number {
        const initialValue = 0;
        return group.items.reduce((sum, obj) => sum + this.getItemFilterMatchCount(obj), initialValue);
    }

    getItemFilterMatchCount(item: FilterableListItem): number {
        return this.itemFilterMatches(item) ? 1 : 0;
    }

    public setSelectedItem(data: any): void
    {
        this.selectedItemData = data;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    itemIsSelected(item: FilterableListItem): boolean
    {
        return item.data == this.selectedItemData;
    }

    onItemClicked(item: FilterableListItem): void
    {
        this.setSelectedItem(item.data);
        this.$emit('itemClicked', item);
    }
}
</script>

<style scoped lang="scss">
.menu {
    box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.02), 0 3px 2px 0 rgba(0, 0, 0, 0.02), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
}
.filter {
    position: relative;
    margin-left: 44px;
    margin-top: 26px;
    margin-bottom: 18px;
    margin-right: 44px;
}
@media (max-width: 960px) {
    .menu-items { 
        margin-top: 67px;
    }
}
</style>