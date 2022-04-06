<!-- src/components/Common/Basic/SelectComponent.vue -->
<template>
    <div class="select-component" :class="rootClasses">
        <input-header-component :name="label" :description="description" :showDescriptionOnStart="showDescriptionOnStart" />
        
        <div class="input-wrapper" ref="wrapperElement">
            <div class="select-component__input input input-padding-2" @click="onInputClicked" ref="inputElement" tabindex="0">
                <div v-for="(item, iIndex) in selectedItems"
                    :key="`${id}-item-${iIndex}`"
                    class="select-component__input-chip">
                    <span class="select-component__input-chip-value">{{ item.text }}</span>
                    <div class="select-component__input-chip-remove accent clickable hoverable" tabindex="0"
                        @click.stop.prevent="removeValue(item.value)"
                        v-if="showItemRemoveButton">
                        <icon-component>clear</icon-component>
                    </div>
                </div>
                <div v-if="showInput" class="select-component__textInput-wrapper">
                    <input type="text" class="select-component__textInput input" :disabled="!allowModify"
                        v-model="filter"
                        :placeholder="placeholderText"
                        @keyup.enter="onFilterEnter"
                        @focus="onFilterFocus" />
                </div>
                <span class="select-component__placeholder input-placeholder"
                    v-if="placeholderText && !showInput">{{ placeholderText }}</span>
            </div>
        </div>
        <div class="select-component__dropdown box-shadow" v-show="showDropdown" ref="dropdownElement">
            <!-- <div class="select-component__dropdown__search">
                Search here
            </div> -->
            <div class="select-component__dropdown__items">
                <div v-for="(item, iIndex) in filteredOptionItems"
                    :key="`${id}-item-${iIndex}`"
                    class="select-component__dropdown__item" tabindex="0"
                    @click.stop.prevent="onDropdownItemClicked(item)">
                    <icon-component v-if="isMultiple && valueIsSelected(item.value)" class="mr-1">check_box</icon-component>
                    <icon-component v-if="isMultiple && !valueIsSelected(item.value)" class="mr-1">check_box_outline_blank</icon-component>
                    {{ item.text }}
                </div>
            </div>
            <div v-if="noDataText && filteredOptionItems.length == 0">{{ noDataText }}</div>
        </div>

        <progress-linear-component v-if="isLoading" indeterminate height="3" />

        <div class="select-component__error input-error" v-if="error != null && error.length > 0">{{ error }}</div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import IdUtils from "@util/IdUtils";
import InputHeaderComponent from "./InputHeaderComponent.vue";
import ValueUtils from "@util/ValueUtils";
import EventBus, { CallbackUnregisterShortcut } from "@util/EventBus";

interface Item {
    value: string;
    text: string;
}

@Options({
    components: { InputHeaderComponent }
})
export default class SelectComponent extends Vue
{
    @Prop({ required: true })
    value!: string | Array<string>;
    
    @Prop({ required: false, default: '' })
    label!: string;
    
    @Prop({ required: false, default: '' })
    description!: string;
    
    @Prop({ required: false, default: false })
    showDescriptionOnStart!: boolean;

    @Prop({ required: true })
    items!: any;
    
    @Prop({ required: false, default: '- Nothing selected -' })
    placeholder!: string;
    
    @Prop({ required: false, default: '- No data -' })
    noDataText!: string;
    
    @Prop({ required: false, default: 'id' })
    itemValue!: string;
    
    @Prop({ required: false, default: 'text' })
    itemText!: string;
    
    @Prop({ required: false, default: null})
    error!: string | null;
    
    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: false })
    readonly!: boolean;
    
    @Prop({ required: false, default: false })
    multiple!: string | boolean;
    
    @Prop({ required: false, default: false })
    nullable!: string | boolean;
    
    @Prop({ required: false, default: false })
    clearable!: string | boolean;
    
    @Prop({ required: false, default: false })
    allowInput!: string | boolean;
    
    @Prop({ required: false, default: false })
    allowCustom!: string | boolean;

    @Prop({ required: false, default: false })
    loading!: string | boolean;
    
    @Ref() readonly inputElement!: HTMLElement;
    @Ref() readonly dropdownElement!: HTMLElement;
    @Ref() readonly wrapperElement!: HTMLElement;

    id: string = IdUtils.generateId();
    selectedValues: Array<string> = [];
    showDropdown: boolean = false;
    filter: string = '';
    
    callbacks: Array<CallbackUnregisterShortcut> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
        this.onValueChanged();
    }

    mounted(): void {
        this.callbacks = [
            EventBus.on("onWindowClick", this.onWindowClick.bind(this))
        ];
    }

    beforeUnmounted(): void {
      this.callbacks.forEach(x => x.unregister());
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get selectedItems(): Array<Item> {
        return this.optionItems.filter(x => this.selectedValues.includes(x.value));
    }

    get filteredOptionItems(): Array<Item> {
        return this.optionItems.filter(x => x.text?.toLowerCase()?.includes(this.filter?.toLowerCase()));
    }

    get optionItems(): Array<Item> {
        let baseItems: Array<Item> = [];
        if (Array.isArray(this.items))
        {
            if (this.items.length == 0) return [];
            const firstValue = this.items[0];
            const isSimpleValue = typeof firstValue === 'string' || firstValue instanceof String;
            baseItems = this.items.map(x => {
                if (isSimpleValue)
                {
                    return {
                        value: x,
                        text: x
                    };
                }
                else
                {
                    return {
                        value: x[this.itemValue],
                        text: x[this.itemText]
                    };
                }
            });
        }
        else
        {
            baseItems = Object.keys(this.items).map(key => {
                return {
                    value: key,
                    text: this.items[key]
                };
            });
        }

        this.selectedValues.forEach(x => {
            if (!baseItems.some(b => b.value == x))
            {
                baseItems.push({
                    text: x,
                    value: x
                });
            }
        });

        return baseItems;
    }

    get placeholderText(): string  {
        return (this.selectedItems.length == 0 && this.placeholder) ? this.placeholder : '';
    }

    get rootClasses(): any {
        return {
             'disabled': this.isDisabled,
             'loading': this.isLoading,
             'multiple': this.isMultiple,
             'clickable': this.allowModify
        };
    }

    get showItemRemoveButton(): boolean {
        if (!this.allowModify) return false;
        return this.isNullable || this.isMultiple;
    }

    get allowModify(): boolean {
        return !this.isDisabled && !this.isLoading;
    }

    get showInput(): boolean {
        return this.isAllowInput || this.isAllowCustom;
    }
    
    get isLoading(): boolean { return ValueUtils.IsToggleTrue(this.loading); }
    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled) || ValueUtils.IsToggleTrue(this.readonly); }
    get isReadonly(): boolean { return ValueUtils.IsToggleTrue(this.readonly) || ValueUtils.IsToggleTrue(this.disabled); }
    get isMultiple(): boolean { return ValueUtils.IsToggleTrue(this.multiple); }
    get isNullable(): boolean { return ValueUtils.IsToggleTrue(this.nullable) || ValueUtils.IsToggleTrue(this.clearable); }
    get isClearable(): boolean { return ValueUtils.IsToggleTrue(this.clearable) || ValueUtils.IsToggleTrue(this.nullable); }
    get isAllowInput(): boolean { return ValueUtils.IsToggleTrue(this.allowInput); }
    get isAllowCustom(): boolean { return ValueUtils.IsToggleTrue(this.allowCustom); }
    
    ////////////////
    //  METHODS  //
    //////////////
    addValue(val: string): void {
        if (!this.isMultiple) this.selectedValues = [];
        if (!this.selectedValues.includes(val))
        {
            this.selectedValues.push(val);
            this.emitValue();
        }
        if (!this.isMultiple) this.showDropdown = false;
    }
    
    removeValue(val: string): void {
        this.selectedValues = this.selectedValues.filter(x => x != val);
        this.emitValue();
    }

    emitValue(): void {
        let emittedValue: string | string[];
        if (this.isMultiple) {
            emittedValue = this.selectedValues;
        }
        else {
            if (this.selectedValues.length == 0 && this.isNullable) emittedValue = null;
            else emittedValue = this.selectedValues[0] || '';
        }
        this.$emit('update:value', emittedValue);
        this.$emit('change', emittedValue);
    }

    valueIsSelected(val: string): boolean { return this.selectedValues.includes(val); }

    tryAddCustomValue(): void {
        if (!this.isAllowCustom) return;
        else if (this.filter.trim().length == 0) return;
        
        const val = this.filter.trim();
        if (this.selectedValues.includes(val)) return;
        this.addValue(val);
        this.filter = '';
    }

    isAllowedToShowDropdown(): boolean {
        return !this.isDisabled && !this.isLoading;
    }

    tryShowDropdown(): void {
        if (this.isAllowedToShowDropdown()) this.setDropdownVisible();
    }

    tryToggleDropdown(): void {
        if (!this.isAllowedToShowDropdown())
        {
            this.showDropdown = false;
            return;
        }
        if (this.showDropdown) this.showDropdown = false;
        else this.setDropdownVisible();
    }

    setDropdownVisible(): void {
        this.showDropdown = true;
        const vh = window.innerHeight;
        const dropdownHeight = Math.min(vh * 0.4, 400) + 12 /* 12=padding+border */;
        const dropdownTopY = this.wrapperElement.getBoundingClientRect().top;
        const dropdownBottomY = dropdownTopY + dropdownHeight;
        if (dropdownBottomY >= vh) {
            this.dropdownElement.style.top = (-dropdownHeight + this.wrapperElement.offsetTop) + 'px';
        }
        else
        {
            this.dropdownElement.style.top = null;
        }
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onInputClicked(): void {
        // this.tryToggleDropdown();
        this.tryShowDropdown();
    }

    onDropdownItemClicked(item: Item): void {
        if (!this.allowModify) return;
        if (!this.valueIsSelected(item.value)) {
            this.addValue(item.value);
        } else if (this.isNullable || this.isMultiple || this.selectedValues.length > 1) {
            this.removeValue(item.value);
        }
    }

    onFilterEnter(): void {
        this.tryAddCustomValue();
    }
    onFilterFocus(): void {
        this.tryShowDropdown();
    }

    @Watch('value')
    onValueChanged(): void {
        if (Array.isArray(this.value))
        {
            this.selectedValues = this.value;
        }
        else {
            this.selectedValues = !!this.value ? [ this.value ] : [];
        }
    }

    onWindowClick(e: MouseEvent): void {
        this.$nextTick(() => {
            if (this.dropdownElement == null || this.inputElement == null || !(e.target instanceof Element)) return;
            if (this.dropdownElement.contains(e.target) || this.inputElement.contains(e.target)) return;
            this.showDropdown = false;
        });
    }

    @Watch('isLoading')
    onIsLoadingChanged(): void {
        if (this.isLoading) this.showDropdown = false;
    }
}
</script>

<style scoped lang="scss">
</style>

<style lang="scss">
.select-component {
    position: relative;
    &__input {
        display: flex;
        flex-wrap: wrap;
        padding-top: 3px;
    }
    &__input-chip {
        display: flex;
        align-items: center;
        padding-left: 5px;
        margin-right: 5px;
        background-color: var(--color--accent-base);
        border-radius: 2px;
        margin-bottom: 2px;
    }
    &__input-chip-value {
        padding-right: 5px;
        min-height: 28px;
        display: flex;
        align-items: center;
    }
    &__input-chip-remove {
        height: 100%;
        padding-right: 3px;
        border-left: 1px solid var(--color--accent-darken2);
        display: flex;
        align-items: center;
    }
    &__dropdown {
        position: absolute;
        z-index: 99999;
        background-color: var(--color--accent-lighten1);
        padding: 5px 10px;
        border: 1px solid var(--color--accent-base);
        max-height: calc(min(40vh, 400px));
        max-width: 800px;
        overflow-y: auto;
    }
    /* &__dropdown__items {
    } */
    &__dropdown__item {
        padding: 5px;
        display: flex;
        align-items: center;
    }
    &__textInput-wrapper {
        flex: 1;
    }
    &__textInput {
        width: 100%;
        padding: 5px 0 !important;
        min-height: 28px;
        border: none !important;
    }
    /* &__placeholder { } */
    /* &__error { } */
}
</style>