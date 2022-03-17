<!-- src/components/Common/FilterableListComponent.vue -->
<template>
    <div>
        <list-component expand class="menu-items">
            <filter-input-component class="filter" v-model:value="filterText" v-if="showFilter" />
            <div v-if="!showFilter" class="mb-5"></div>
            <div v-if="!showFilter" style="margin-top: 76px"></div>

            <progress-linear-component 
                v-if="loading"
                indeterminate color="green"></progress-linear-component>
            
            <!-- GROUPS: START -->
            <div
                no-action
                sub-group
                prepend-icon="keyboard_arrow_up"
                value="true"
                v-for="(group, gindex) in groups"
                :key="`filterable-menu-group-${gindex}`">
                <div>
                    <div><b>{{ group.title }}</b></div>
                    <badge-component class="mr-3" v-if="showFilterCounts">{{ getGroupFilterMatchCount(group) }}</badge-component>
                </div>

                <div ripple
                    v-for="(item, itemIndex) in filterItems(group.items)"
                    :key="`filterable-menu-item-${itemIndex}`"
                    class="testset-menu-item"
                    :class="{ 'active': itemIsSelected(item) }"
                    @click="onItemClicked(item)"
                    @click.middle.stop.prevent="onItemClickedMiddle(item)"
                    @mousedown.middle.stop.prevent
                    :href="getItemHref(item.data)"
                    :disabled="disabled">
                    <div>
                        {{ item.title }}
                        <icon-component
                            v-for="(icon, iindex) in getItemIcons(item.data)"
                            :key="`filterable-menu-item-${itemIndex}-icon-${iindex}`"
                            class="filterable-menu-item__icon"
                            color="#555"
                            >{{ icon }}</icon-component>
                        <br v-if="item.subTitle != null">
                        <span style="color: darkgray;" v-if="item.subTitle != null">{{ item.subTitle }}</span>
                    </div>
                </div>
            </div>
            <!-- GROUPS: END -->

            <!-- NO GROUP: START -->
            <div ripple
                v-for="(item, itemIndex) in filterItems(ungroupedItems)"
                :key="`stream-menu-${itemIndex}`"
                class="testset-menu-item"
                :class="{ 'active': itemIsSelected(item) }"
                @click="onItemClicked(item)"
                @click.middle.stop.prevent="onItemClickedMiddle(item)"
                @mousedown.middle.stop.prevent
                :href="getItemHref(item.data)"
                :disabled="disabled">
                <div v-text="item.title"></div>
            </div>

            <div ripple
                v-if="!hasGroups && filterText.length > 0 && filterItems(ungroupedItems).length == 0"
                class="testset-menu-item no-result-found"
                :disabled="true">
                <div>No results found</div>
            </div>
            <!-- NO GROUP: END -->

        </list-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { EntryState } from '@models/modules/RequestLog/EntryState';
import DateUtils from '@util/DateUtils';
import LinqUtils from '@util/LinqUtils';
import KeyArray from '@util/models/KeyArray';
import KeyValuePair from '@models/Common/KeyValuePair';
// @ts-ignore
import FilterInputComponent from '@components/Common/FilterInputComponent.vue';

import { FilterableListGroup, FilterableListItem } from '@components/Common/FilterableListComponent.vue.models';
@Options({
    components: {
        FilterInputComponent
    }
})
export default class FilterableListComponent extends Vue {
    @Prop({ required: true })
    items!: Array<FilterableListItem>;

    @Prop({ required: false, default: '' })
    groupByKey!: string;

    @Prop({ required: false, default: null })
    iconsKey!: string | null;

    @Prop({ required: false, default: null })
    hrefKey!: string | null;

    @Prop({ required: false, default: null })
    sortByKey!: string | null;

    @Prop({ required: false, default: () => [] })
    filterKeys!: Array<string>;

    @Prop({ required: false, default: false })
    loading!: boolean;

    @Prop({ required: false, default: false })
    disabled!: boolean;

    @Prop({ required: false, default: true })
    groupIfSingleGroup!: boolean;

    @Prop({ required: false, default: true })
    showFilter!: boolean;

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
    get hasGroups(): boolean
    {
        return this.groups.length > 0;
    }
    
    get ungroupedItems(): Array<FilterableListItem>
    {
        return (this.groups.length == 0) ? this.items : [];
    }

    get groups(): Array<FilterableListGroup>
    {
        let groupList: Array<FilterableListGroup> = [];
        if (this.groupByKey.length == 0) return groupList;

        LinqUtils.GroupByInto(this.items, (x: any) => x.data[this.groupByKey] || 'Other', 
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
        return this.showFilter ? data.filter(x => this.itemFilterMatches(x)) : data;
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

    getItemIcons(data: any): Array<string>
    {
        if (this.iconsKey == null) return [];
        const icons = data[this.iconsKey] || [];
        return icons;
    }

    getItemHref(data: any): string | null {
        if (this.hrefKey == null) return null;
        const href = data[this.hrefKey] || null;
        return href;
    }

    public setSelectedItem(data: any): void
    {
        this.selectedItemData = data;
    }

    public setSelectedItemByFilter(filter: ((data: any) => boolean)): void
    {
        const item = this.items.filter(x => filter(x))[0];
        if (item != null)
        {
            this.selectedItemData = item.data;
        }
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

    onItemClickedMiddle(item: FilterableListItem): void
    {
        this.$emit('itemMiddleClicked', item);
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
.no-result-found {
    padding-left: 46px;
}
.filterable-menu-item__icon {
    float: right;
}
</style>

<style lang="scss">
.no-result-found {
    .v-list__tile {
        padding-left: 0;
    }
}
</style>