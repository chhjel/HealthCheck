<!-- src/components/Common/FilterableListComponent.vue -->
<template>
    <div class="filterable-list">
        <div class="menu-items">
            <filter-input-component class="filter contrast" v-model:value="filterText" v-if="showFilter" />
            <div v-if="!showFilter" class="mb-5"></div>
            <div v-if="!showFilter" style="margin-top: 76px"></div>

            <progress-linear-component 
                v-if="loading"
                indeterminate color="success"></progress-linear-component>
            
            <!-- GROUPS: START -->
            <div v-for="(group, gindex) in groups"
                :key="`filterable-menu-group-${gindex}`"
                class="menu-item-group"
                :class="{ 'open': isGroupOpen(group.title) }"
                v-set-max-height-from-children>
                <div class="menu-item-group--wrapper">
                    <div class="group-item" :class="{ 'open': isGroupOpen(group.title) }"
                        @click="toggleGroup(group.title)">
                        <icon-component class="group-item__arrow">keyboard_arrow_up</icon-component>
                        <div class="group-item__text"><b>{{ group.title }}</b></div>
                        <badge-component class="group-item__badge mr-3" v-if="showFilterCounts">{{ getGroupFilterMatchCount(group) }}</badge-component>
                    </div>

                    <div v-for="(item, itemIndex) in filterItems(group.items)"
                        :key="`filterable-menu-item-${itemIndex}`"
                        class="filterable-menu-item"
                        :class="{ 'active': itemIsSelected(item) }"
                        @click="onItemClicked(item)"
                        @click.middle.stop.prevent="onItemClickedMiddle(item)"
                        @mousedown.middle.stop.prevent
                        :href="getItemHref(item.data)"
                        :disabled="disabled">
                        {{ item.title }}
                        <div class="spacer"></div>
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
            <div
                v-for="(item, itemIndex) in filterItems(ungroupedItems)"
                :key="`stream-menu-${itemIndex}`"
                class="filterable-menu-item"
                :class="{ 'active': itemIsSelected(item) }"
                @click="onItemClicked(item)"
                @click.middle.stop.prevent="onItemClickedMiddle(item)"
                @mousedown.middle.stop.prevent
                :href="getItemHref(item.data)"
                :disabled="disabled">
                <div v-text="item.title"></div>
            </div>

            <div
                v-if="!hasGroups && filterText.length > 0 && filterItems(ungroupedItems).length == 0"
                class="filterable-menu-item no-result-found"
                :disabled="true">
                <div>No results found</div>
            </div>
            <!-- NO GROUP: END -->

        </div>
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

    @Prop({ required: false, default: null })
    groupOrders!: { [key:string]:number } | null;

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

    closedGroups: Array<string> = [];
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
        if (this.groups.length > 0) return [];
        return this.items
            .sort((a, b) => LinqUtils.SortBy(a, b, (x: any) => x[this.sortByKey || '']));
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

        // Sort items within groups
        if (this.sortByKey != null)
        {
            for (let i=0;i<groupList.length;i++)
            {
                groupList[i].items = groupList[i].items.sort((a, b) => LinqUtils.SortBy(a, b, (x: any) => x[this.sortByKey || '']));
            }
        }
        // Sort groups
        if (this.groupOrders != null && this.groupByKey != null)
        {
            groupList = groupList.sort((a, b) => LinqUtils.SortBy(a, b, (x: FilterableListGroup) => {
                const groupName: string = x.items[0].data[this.groupByKey];
                const groupOrder: number = this.groupOrders[groupName] || -999999;
                return groupOrder;
            }));
        }

        return groupList;
    }

    get showFilterCounts(): boolean {
        return this.filterText.length > 0;
    }

    ////////////////
    //  METHODS  //
    //////////////
    isGroupOpen(name: string): boolean {
        return !this.closedGroups.includes(name);
    }

    toggleGroup(name: string): void {
        if (this.isGroupOpen(name)) this.closedGroups.push(name);
        else this.closedGroups = this.closedGroups.filter(x => x != name);
    }

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

    public filterInputText(): string { return this.filterText; }
    public setSelectedItem(data: any): void { this.selectedItemData = data; }
    public getDisplayedItems(): Array<FilterableListItem> { return this.items.filter(x => this.itemFilterMatches(x)); }
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
        return JSON.stringify(item.data) == JSON.stringify(this.selectedItemData);
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
.filter {
    position: relative;
    margin-left: 44px;
    margin-top: 26px;
    margin-bottom: 18px;
    margin-right: 44px;
}
.no-result-found {
    padding-left: 46px;
}
.menu-items {
    padding-bottom: 20px;
}
.filterable-menu-item__icon {
    float: right;
    margin-right: 20px;
}
.filterable-menu-item {
    display: flex;
    cursor: pointer;
    padding-left: 46px;
    &.active {
        padding-left: 42px;
        border-left: 4px solid #d1495b;
    }
}
.menu-item-group {
    overflow: hidden;
    transition: all 0.2s;
    &:not(.open) {
        max-height: 42px !important;
    }
}
.group-item {
    display: flex;
    cursor: pointer;

    &__arrow {
        transition: transform 0.2s;
    }
    &__text {
        flex: 1;
        margin-left: 22px;
    }
    /* &__badge { } */

    &.open {
        .group-item__arrow {
            transform: rotate(180deg);
        }
    }
}
.open 
.group-item, .filterable-menu-item {
    height: 42px;
    align-items: center;
    &.active, &:hover {
        background: hsla(0,0%,100%,.08);
    }
}
</style>

<style lang="scss">
.filterable-list {
    .icon-component {
        color: var(--color--text-light) !important;
    }
}
</style>
