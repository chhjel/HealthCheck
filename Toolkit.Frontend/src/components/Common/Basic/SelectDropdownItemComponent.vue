<!-- src/components/Common/Basic/SelectDropdownItemComponent.vue -->
<template>
    <div class="select-dropdown-item-component select-component__dropdown__item" tabindex="0"
        @click.stop.prevent="onDropdownItemClicked(source)"
        @keyup.enter="onDropdownItemClicked(source)"
        @keydown.esc="hideDropdown"
        :class="dropdownItemClasses(source)">
        <icon-component v-if="isMultiple && valueIsSelected(source.value)" class="mr-1">check_box</icon-component>
        <icon-component v-if="isMultiple && !valueIsSelected(source.value)" class="mr-1">check_box_outline_blank</icon-component>
        {{ source.text }}
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";


// https://github.com/reactjser/vue3-virtual-scroll-list-examples/blob/1ef4b55bb2e749f9d123bebd31baa4a37a7668fc/src/docs/api.md
interface Item {
    value: string;
    text: string;
}
@Options({
    components: {  }
})
export default class SelectDropdownItemComponent extends Vue
{
    @Prop({ required: true })
    source!: Item;

    @Prop({ required: true })
    isMultiple: boolean;

    @Prop({ required: true })
    valueIsSelected: (val: string) => boolean;

    @Prop({ required: true })
    onDropdownItemClicked: (item: Item) => void;

    @Prop({ required: true })
    hideDropdown: () => void;

    dropdownItemClasses(item: Item): any {
        let classes: any = {};
        if (!this.isMultiple && this.valueIsSelected(item.value)) {
            classes['selected'] = true;
        }
        return classes;
    }
}
</script>

<style scoped lang="scss">
.select-component__dropdown__item {
    padding: 5px 10px;
    display: flex;
    align-items: center;
    cursor: pointer;
    transition: all 0.2s;
    @media (max-width: 600px) {
        padding: 10px;
    }

    &.selected {
        font-weight: 600;
    }
    &:hover {
        background-color: #f6f6f6;
    }
}
</style>