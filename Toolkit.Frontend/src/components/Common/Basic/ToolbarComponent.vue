<template>
    <div class="toolbar-component" :class="rootClasses">
        <div class="toolbar-component__prefix">
            <slot name="prefix"></slot>
        </div>
        <div class="toolbar-component__content"
            ref="mainToolbarContent"
            :style="contentStyle">
            <template v-for="(item, index) in items"
                    :key="`toolbar-component-item-${index}`">
                <btn-component flat
                    v-if="!item.isSpacer"
                    :href="item.href"
                    :class="{ 'active-tab': item.active }"
                    @click.left.prevent="onItemClick(item)"
                    :data-tindex="index">
                        <icon-component class="mr-1" v-if="item.icon">{{ item.icon }}</icon-component>
                        {{ item.label }}
                </btn-component>
                <div class="spacer"
                    :data-tindex="index"
                    v-if="item.isSpacer"></div>
            </template>
        </div>
        <div class="toolbar-component__overflow-button hoverable-lift-light"
            ref="overflowButton"
            :style="overflowButtonStyle"
            @click="showOverflow = !showOverflow">
            <icon-component class="mr-1">more_horiz</icon-component>
            +{{ overflowMenuItemCount }}
        </div>
        <div class="toolbar-component__overflow" ref="overflowContainer" v-show="showOverflow">
            <template
                v-for="(item, index) in overflowMenuItems"
                :key="`toolbar-component-item-o-${index}`">
                <btn-component flat
                    :href="item.href"
                    v-if="!item.isSpacer"
                    :class="{ 'active-tab': item.active }"
                    @click.left.prevent="onItemClick(item)"
                    :data-tindex="index">
                        <icon-component class="mr-1" v-if="item.icon">{{ item.icon }}</icon-component>
                        {{ item.label }}
                </btn-component>
            </template>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Ref, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from "@util/ValueUtils";
import { ToolbarComponentMenuItem } from './ToolbarComponent.vue.models';
import EventBus, { CallbackUnregisterShortcut } from "@util/EventBus";

interface ItemData {
    item: ToolbarComponentMenuItem;
    overflow: boolean;
    index: number;
}

@Options({
    components: {}
})
export default class ToolbarComponent extends Vue {
    @Prop({ required: true })
    items!: Array<ToolbarComponentMenuItem>;

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: false })
    fixed!: string | boolean;

    @Prop({ required: false, default: false })
    dark!: string | boolean;

    @Ref() readonly mainToolbarContent!: HTMLElement;
    @Ref() readonly overflowContainer!: HTMLElement;
    @Ref() readonly overflowButton!: HTMLElement;
    
    callbacks: Array<CallbackUnregisterShortcut> = [];
    observer: IntersectionObserver;
    itemStates: Array<ItemData> = [];
    overflowWidth: number = 0;
    showOverflow: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
        this.items.forEach((x, i) => {
            this.itemStates.push({
                item: x,
                index: i,
                overflow: false
            });
        });

        this.callbacks = [
            EventBus.on("onWindowClick", this.onWindowClick.bind(this))
        ];
        this.initObserver();
    }

    beforeUnmount(): void {
        this.disconnectObserver();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get rootClasses(): any {
        let classes = {
            'fixed': this.isFixed,
            'dark': this.isDark
        };
        classes[this.color || 'accent'] = true;
        return classes;
    }

    get isFixed(): boolean { return ValueUtils.IsToggleTrue(this.fixed); }
    get isDark(): boolean { return ValueUtils.IsToggleTrue(this.dark); }

    get overflowMenuItems(): Array<ToolbarComponentMenuItem> {
        return this.itemStates.filter(x => x.overflow).map(x => x.item);
    };

    get overflowMenuItemCount(): number {
        return this.overflowMenuItems.filter(x => !x.isSpacer).length;
    }

    get isOverflowing(): boolean {
        return this.itemStates.some(x => x.overflow);
    }

    get contentStyle(): any {
        let style: any = {};
        style['padding-right'] = `${this.overflowWidth}px`;
        return style;
    }

    get overflowButtonStyle(): any {
        return {
            'display': this.isOverflowing ? '' : 'none'
        };
    }

    ////////////////
    //  METHODS  //
    //////////////
    initObserver(): void {
        let options: IntersectionObserverInit = {
            root: this.mainToolbarContent,
            rootMargin: '0px',
            threshold: 1.0
        };

        this.observer = new IntersectionObserver(this.handleObservedIntersection, options);
        Array.from(this.mainToolbarContent.children).forEach((item) => {
            this.observer.observe(item);
        });
    }

    disconnectObserver(): void {
        this.observer.disconnect();
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    handleObservedIntersection(entries: IntersectionObserverEntry[]): void {
        entries.forEach(x => {
            const e = x.target as HTMLElement;
            const isVisible = x.isIntersecting
            e.style.visibility = isVisible ? '' : 'hidden';

            const index = Number(e.dataset.tindex);
            const state = this.itemStates.find(s => s.index == index);
            state.overflow = !isVisible;
        });

        this.$nextTick(() => {
            this.overflowWidth = this.overflowContainer.offsetWidth;
        });
    };

    onItemClick(item: ToolbarComponentMenuItem): void {
        if (item.onClick)
        {
            item.onClick(item);
        }
        this.showOverflow = false;
    }

    onWindowClick(e: MouseEvent): void {
        this.$nextTick(() => {
            if (this.overflowContainer == null || this.overflowContainer == null || !(e.target instanceof Element)) return;
            if (this.overflowContainer.contains(e.target) || this.overflowButton.contains(e.target)) return;
            this.showOverflow = false;
        });
    }
	
    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch("items", { deep: true })
    onItemsChanged(): void {
        const activeItem = this.items.find(x => x.active);
        this.itemStates.forEach(x => x.item.active = activeItem?.label == x.item.label);
    }

}
</script>

<style scoped lang="scss">
.toolbar-component {
    background-color: #fff;
    width: 100%;
    position: relative;
    z-index: 99;
    display: flex;

    &.fixed {
        position: fixed;
        top: 0;
        left: 0;
    }
    
    .btn-component {
        border: none;
        box-shadow: none !important;
        padding: 5px 16px;
        flex-shrink: 0;
        margin: 0;
    }
    .active-tab {
        font-weight: 900 !important;
        background-color: var(--color--accent-lighten1) !important;
    }

    &__content {
        display: flex;
        flex-direction: row;
        flex-wrap: nowrap;
        overflow: hidden;
        width: 100%;

        height: 56px;
        @media (min-width: 960px) { height: 64px; }
    }

    &__overflow {
        position: absolute;
        right: 0;
        background: var(--color--background-bright);
        box-shadow: 0 0 12px 2px rgb(0 0 0 / 21%);
        display: flex;
        flex-direction: column;
        overflow-y: auto;

        top: 56px;
        max-height: calc(100vh - 56px - 10px);
        @media (min-width: 960px) {
            top: 64px;
            max-height: calc(100vh - 64px - 10px);
        }
    }

    &__overflow-button {
        min-width: 100px;
        display: flex;
        justify-content: center;
        align-items: center;
        cursor: pointer;
        font-size: 14px;
    }
}
</style>
