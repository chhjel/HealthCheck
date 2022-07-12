<template>
    <div class="toolbar-component" :class="rootClasses">
        <div class="toolbar-component__prefix">
            <slot name="prefix"></slot>
        </div>
        <div class="toolbar-component__content" ref="mainToolbarContent" :style="contentStyle">
            <btn-component flat
                v-for="(item, index) in this.items"
                :key="`toolbar-component-item-${index}`"
                :href="item.href"
                :class="{ 'active-tab': item.active }"
                @click.left.prevent="onItemClick(item)"
                :data-tindex="index">
                    <icon-component class="mr-1" v-if="item.icon">{{ item.icon }}</icon-component>
                    {{ item.label }}
            </btn-component>
        </div>
        <div class="toolbar-component__overflow" ref="overflowContainer">
            <btn-component flat
                v-for="(item, index) in this.overflowMenuItems"
                :key="`toolbar-component-item-o-${index}`"
                :href="item.href"
                :class="{ 'active-tab': item.active }"
                @click.left.prevent="onItemClick(item)"
                :data-tindex="index">
                    <icon-component class="mr-1" v-if="item.icon">{{ item.icon }}</icon-component>
                    {{ item.label }}
            </btn-component>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Ref, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from "@util/ValueUtils";
import { ToolbarComponentMenuItem } from './ToolbarComponent.vue.models';

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
    
    observer: IntersectionObserver;
    itemStates: Array<ItemData> = [];
    overflowWidth: number = 0;

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

    get contentStyle(): any {
        let style: any = {};
        style['padding-right'] = `${this.overflowWidth}px`;
        return style;
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
    background-color: #f5f5f5;
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
        overflow-y: hidden;
        overflow-x: auto;
        overflow: overlay hidden;
        -ms-overflow-style: none;
        height: 56px;

        &::-webkit-scrollbar {
            display: none;
        }
         
        @media (min-width: 960px) {
            height: 64px;
        }
    }

    &__overflow {
        position: absolute;
        top: 0;
        right: 0;
        background: var(--color--background-bright);
        border: 1px solid #000;
        display: flex;
        flex-direction: column;
    }
}
</style>
