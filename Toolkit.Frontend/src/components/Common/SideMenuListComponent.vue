<!-- src/components/Common/SideMenuListComponent.vue -->
<template>
    <div class="side-menu-list">
        <div class="menu-items">
            <div
                v-for="(item, itemIndex) in items"
                :key="`side-menu-item-${itemIndex}`"
                class="side-menu-item"
                :class="{ 'active': itemIsSelected(item) }"
                @click="onItemClicked(item)"
                @click.middle.stop.prevent="onItemClickedMiddle(item)"
                @mousedown.middle.stop.prevent
                :disabled="disabled">
                <div v-text="item.label"></div>
                <div class="spacer"></div>
                <icon-component v-if="item.icon"
                    class="side-menu-item__icon" color="#555">{{ item.icon }}</icon-component>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";

import { SideMenuListItem } from '@components/Common/SideMenuListComponent.vue.models';
import EventBus from "@util/EventBus";
@Options({
    components: {
    }
})
export default class SideMenuListComponent extends Vue {
    @Prop({ required: true })
    items!: Array<SideMenuListItem>;

    @Prop({ required: false, default: false })
    disabled!: boolean;

    selectedItemId: string | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////

    ////////////////
    //  METHODS  //
    //////////////
    public setSelectedItemById(id: string): void { this.selectedItemId = id; }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    itemIsSelected(item: SideMenuListItem): boolean
    {
        return item.id == this.selectedItemId;
    }

    onItemClicked(item: SideMenuListItem): void
    {
        this.setSelectedItemById(item.id);
        this.$emit('itemClicked', item);
        EventBus.notify('FilterableList.itemClicked', item);
    }

    onItemClickedMiddle(item: SideMenuListItem): void
    {
        this.$emit('itemMiddleClicked', item);
    }
}
</script>

<style scoped lang="scss">
.menu-items {
    padding-bottom: 20px;
}
.side-menu-item {
    display: flex;
    cursor: pointer;
    padding-left: 46px;
    height: 42px;
    align-items: center;
    &.active, &:hover {
        background: hsla(0,0%,100%,.08);
    }
    &.active {
        padding-left: 42px;
        border-left: 4px solid #d1495b;
    }

    &__icon {
        float: right;
        margin-right: 20px;
    }
}
.icon-component {
    color: var(--color--text-light) !important;
}
</style>
