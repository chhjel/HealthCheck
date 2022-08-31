<!-- src/components/Common/Basic/SelectComponent.vue -->
<template>
    <div class="select-component" :class="rootClasses">
        <input-header-component :name="label" :description="description" :showDescriptionOnStart="showDescriptionOnStart" :ensureHeight="isEnsureLabelHeight" />
        
        <div class="input-wrapper" ref="wrapperElement">
            <div class="select-component__input input input-padding-2"
                @click="onInputClicked"
                ref="inputElement"
                tabindex="0"
                @keypress="onSelectKeyPress">
                <div v-for="(item, iIndex) in selectedItems"
                    :key="`${id}-item-${iIndex}`"
                    class="select-component__input-chip">
                    <span class="select-component__input-chip-value"
                        @click.stop.prevent="onChipClicked(item)">{{ item.text }}</span>
                    <div class="select-component__input-chip-remove accent"
                        :class="removeButtonClasses"
                        tabindex="0"
                        @click.stop.prevent="onRemoveValueClicked(item.value)"
                        v-if="showItemRemoveButton">
                        <icon-component>clear</icon-component>
                    </div>
                </div>
                <span class="select-component__placeholder input-placeholder"
                    v-if="placeholderText && !showInput">{{ placeholderText }}</span>
                <span class="spacer" v-if="!showInput"></span>
                <div v-if="showInput" class="select-component__textInput-wrapper">
                    <input type="text" class="select-component__textInput input" :disabled="!allowModify"
                        v-model="filter"
                        :placeholder="placeholderText"
                        @keyup.enter="onFilterEnter"
                        @keydown="onFilterKeyDown"
                        @keydown.esc="hideDropdown"
                        @blur="onFilterBlur"
                        @focus="onFilterFocus"
                        @input="onFilterInput"
                        ref="filterInputElement" />
                    <icon-component v-if="isClearable" class="input-icon" :class="clearableIconClasses"
                        title="Clear"
                        @click.stop.prevent="clear">clear</icon-component>
                </div>
                <span class="select-component__expandIcon" @click.stop.prevent="tryToggleDropdown">
                    <icon-component medium color="--color--accent-darken7">expand_more</icon-component>
                </span>
            </div>
        </div>
        <TeleportFix to="body" :disabled="!showDropdown">
            <div class="select-component__dropdown box-shadow" v-show="showDropdown" ref="dropdownElement">
                <div class="select-component__dropdown__items">
                    <virtual-list
                        class="select-component__dropdown-scroller"
                        :data-key="'value'"
                        :data-sources="filteredOptionItems"
                        :data-component="dropdownItemType"
                        :extra-props="extraItemProps"
                        :estimate-size="34"
                    />
                </div>                
                <div v-if="noDataText && filteredOptionItems.length == 0" class="select-component__statusText">{{ noDataText }}</div>
            </div>
        </TeleportFix>

        <progress-linear-component v-if="isLoading" indeterminate height="3" />

        <div class="select-component__error input-error" v-if="resolvedError != null && resolvedError.length > 0">{{ resolvedError }}</div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { shallowRef, Teleport as teleport_, TeleportProps, VNodeProps } from 'vue';
import IdUtils from "@util/IdUtils";
import InputHeaderComponent from "./InputHeaderComponent.vue";
import ValueUtils from "@util/ValueUtils";
import ElementUtils from "@util/ElementUtils";
import EventBus, { CallbackUnregisterShortcut } from "@util/EventBus";
import { StoreUtil } from "@util/StoreUtil";
import VirtualList from 'vue3-virtual-scroll-list';
import Item from './Items.vue';
import SelectDropdownItemComponent from "./SelectDropdownItemComponent.vue";

interface Item {
    value: string;
    text: string;
}

const TeleportFix = teleport_ as {
  new (): {
    $props: VNodeProps & TeleportProps
  }
}
@Options({
    components: {
        InputHeaderComponent,
        SelectDropdownItemComponent,
        TeleportFix,
        VirtualList
    }
})
export default class SelectComponent extends Vue
{
    asd = [{id: 'unique_1', text: 'abc'}, {id: 'unique_2', text: 'xyz'}];
    dropdownItemType = shallowRef(SelectDropdownItemComponent);

    @Prop({ required: false })
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
    
    @Prop({ required: false, default: '- No matches -' })
    noDataText!: string;
    
    @Prop({ required: false, default: 'value' })
    itemValue!: string;
    
    @Prop({ required: false, default: 'text' })
    itemText!: string;
    
    @Prop({ required: false, default: null})
    error!: string | null;
    
    @Prop({ required: false, default: false })
    ensureLabelHeight!: string | boolean;
    
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

    @Prop({ required: false, default: null })
    validateCustomInput: null | ((val: string) => string | null | boolean);

    @Prop({ required: false, default: false })
    loading!: string | boolean;
    
    @Ref() readonly inputElement!: HTMLElement;
    @Ref() readonly dropdownElement!: HTMLElement;
    @Ref() readonly wrapperElement!: HTMLElement;
    @Ref() readonly filterInputElement!: HTMLInputElement;

    id: string = IdUtils.generateId();
    selectedValues: Array<string> = [];
    showDropdown: boolean = false;
    filter: string = '';
    resolvedError: string = '';
    
    callbacks: Array<CallbackUnregisterShortcut> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
        this.resolvedError = this.error;
        this.onValueChanged();
    }

    mounted(): void {
        this.callbacks = [
            EventBus.on("onModuleSwitched", this.onModuleSwitched.bind(this)),
            EventBus.on("onWindowClick", this.onWindowClick.bind(this)),
            EventBus.on("onWindowScroll", this.onWindowScroll.bind(this)),
            EventBus.on("onWindowResize", this.onWindowResize.bind(this)),
        ];
    }

    beforeUnmount(): void {
      this.hideDropdown();
      this.callbacks.forEach(x => x.unregister());
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get selectedItems(): Array<Item> {
        if (this.useInputOnly) return [];
        return this.optionItems.filter(x => this.selectedValues.includes(x.value));
    }

    get filteredOptionItems(): Array<Item> {
        return this.optionItems.filter(x => x.text?.toLowerCase()?.includes(this.filter?.toLowerCase()));
    }

    get extraItemProps(): object {
        return {
            isMultiple: this.isMultiple,
            valueIsSelected: this.valueIsSelected,
            onDropdownItemClicked: this.onDropdownItemClicked,
            hideDropdown: this.hideDropdown
        };
    }

    get optionItems(): Array<Item> {
        let baseItems: Array<Item> = [];
        if (Array.isArray(this.items) && this.items.length > 0)
        {
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
        else if (!Array.isArray(this.items))
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
             'open': this.showDropdown,
             'disabled': this.isDisabled,
             'loading': this.isLoading,
             'multiple': this.isMultiple,
             'clickable': this.allowModify
        };
    }

    get removeButtonClasses(): any {
        return {
             'disabled': !this.allowModify,
             'clickable': this.allowModify,
             'hoverable': this.allowModify
        };
    }

    get showItemRemoveButton(): boolean {
        return this.isNullable || this.isMultiple;
    }

    get allowModify(): boolean {
        return !this.isDisabled && !this.isLoading;
    }

    get showInput(): boolean {
        return this.isAllowInput || this.isAllowCustom;
    }

    get useInputOnly(): boolean {
        return this.showInput && (!this.items || (Array.isArray(this.items) && this.items.length == 0));
    }
    
    get clearableIconClasses(): any {
        return {
            'clickable': !this.isDisabled
        };
    }

    get allowNullValue(): boolean { return this.optionItems.some(x => x.value == null); }
    get allowEmptyValue(): boolean { return this.optionItems.some(x => x.value === ''); }

    get isEnsureLabelHeight(): boolean { return ValueUtils.IsToggleTrue(this.ensureLabelHeight); }
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
    clear(): void {
        if (this.isReadonly || this.isDisabled
            || ((!this.selectedValues || this.selectedValues.length == 0) && !this.filter)) return;
        this.selectedValues = [];
        this.filter = '';
        this.$emit('click:clear');
        this.emitValue();
    }

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

    onRemoveValueClicked(val: string): void {
        if (!this.allowModify) return;
        this.removeValue(val);
        this.hideDropdown();
    }

    emitValue(): void {
        let emittedValue: string | string[];
        if (this.isMultiple) {
            emittedValue = this.selectedValues;
        }
        else {
            if (this.selectedValues.length == 0 && this.isNullable) emittedValue = null;
            else if (this.allowNullValue) emittedValue = this.selectedValues[0];
            else if (this.selectedValues[0] as any === 0) emittedValue = this.selectedValues[0];
            else emittedValue = this.selectedValues[0] || '';
        }
        this.$emit('update:value', emittedValue);
        this.$emit('change', emittedValue);
        this.$emit('blur', emittedValue);
    }

    valueIsSelected(val: string): boolean { return this.selectedValues.includes(val); }

    tryAddCustomValue(): void {
        if (!this.isAllowCustom) return;
        else if (this.filter.trim().length == 0) return;
        
        const val = this.filter.trim();
        const validationResult = this.tryValidateCustomValue(val);
        let allowed = true;
        let validationMessage = '';
        if (typeof validationResult === 'string') { allowed = !validationResult; validationMessage = validationResult; }
        else if (typeof validationResult == "boolean") { allowed = validationResult; }

        if (!allowed) {
            this.resolvedError = validationMessage;
            return;
        } else {
            this.resolvedError = '';
        }

        if (this.selectedValues.includes(val)) return;
        this.addValue(val);
        
        if (!this.useInputOnly) this.filter = '';
    }

    // Returning:
    //  - a string = false w/ validation message.
    //  - true = allow
    //  - false = disallow w/o validation message
    tryValidateCustomValue(val: string): string | boolean {
        if (this.validateCustomInput == null) return null;
        const result = this.validateCustomInput(val);
        if (typeof result === 'string') return result;
        else if (typeof result == "boolean") return result;
        else return true;
    }

    isAllowedToShowDropdown(): boolean {
        if (this.useInputOnly) return false;
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

    hideDropdown(): void {
        this.showDropdown = false;
    }

    setDropdownVisible(): void {
        this.showDropdown = true;
        this.$nextTick(() => this.setDropdownPosition());
    }

    setDropdownPosition(): void {
        if (!this.showDropdown) return;
        
        const yOffset = !!this.resolvedError ? 24 : 0;
        const pos = ElementUtils.calcDropdownPosition(this.wrapperElement, this.dropdownElement, yOffset);
        this.dropdownElement.style.top = pos.top;
        this.dropdownElement.style.left = pos.left;
    }

    setFilterFromShortcutIfSuitable(item: Item): boolean {
        if (StoreUtil.store.state.input.ctrlIsHeldDown && this.isAllowCustom && this.allowModify)
        {
            this.filter = item.text;
            this.hideDropdown();
            return true;
        }
        return false;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onChipClicked(item: Item): void {
        if (this.setFilterFromShortcutIfSuitable(item)) return;
        this.onInputClicked();
    }

    onInputClicked(): void {
        // this.tryToggleDropdown();
        this.tryShowDropdown();
    }

    onDropdownItemClicked(item: Item): void {
        if (!this.allowModify) return;
        else if (this.setFilterFromShortcutIfSuitable(item)) return;

        if (!this.valueIsSelected(item.value)) {
            this.addValue(item.value);
            this.filter = '';
        } else if (this.isNullable || this.isMultiple || this.selectedValues.length > 1) {
            this.removeValue(item.value);
        }
    }

    onSelectKeyPress(e: KeyboardEvent): void {
        // todo: allows opening dropdown while others are open as well
        // if (e.code == 'Space' && e.currentTarget == e.target) {
        //     e.preventDefault();
        //     this.tryToggleDropdown();
        // }
    }

    onFilterEnter(): void {
        this.tryAddCustomValue();
    }
    onFilterBlur(e: FocusEvent): void {
        this.resolvedError = this.error;
        const isDropDown = ElementUtils.isChildOf(e.relatedTarget as HTMLElement, this.dropdownElement);
        if (!isDropDown) {
            this.tryAddCustomValue();
        }
    }
    onFilterFocus(): void {
        this.tryShowDropdown();
    }
    onFilterKeyDown(e: KeyboardEvent): void {
        if (!this.isAllowInput) return;
        if (e.key == 'Backspace'
            && this.selectedItems && this.selectedItems.length > 0
            && this.filter.length == 0) {
            this.removeValue(this.selectedItems[this.selectedItems.length-1].value);
        }
    }
    onFilterInput(): void {
        this.setDropdownPosition();
    }

    @Watch('value')
    onValueChanged(): void {
        this.resolvedError = this.error;

        if (Array.isArray(this.value))
        {
            this.selectedValues = this.value;
        }
        else {
            // Value is empty, ignore if not allowed
            if (this.value === '' && !this.allowEmptyValue) {
                this.selectedValues = [];
            }
            // Value is not null, set as selected
            else if (this.value != null) {
                this.selectedValues = [ this.value ];
            // Value is null, set as value if a possible choice
            } else {
                this.selectedValues = this.allowNullValue ? [ this.value ] : [];
            }
        }

        if (this.useInputOnly) {
            if (Array.isArray(this.value) && this.value.length > 0 && this.value[0]) {
                this.filter = this.value[0];
            }
            else if (!Array.isArray(this.value) && this.value) {
                this.filter = this.value;
            }
        }
    }
    
    // Hack until vue fixes teleported kids being orphaned
    onModuleSwitched(): void {
        this.hideDropdown();
    }

    onWindowClick(e: MouseEvent): void {
        this.$nextTick(() => {
            if (this.dropdownElement == null || this.inputElement == null || !(e.target instanceof Element)) return;
            if (this.dropdownElement.contains(e.target) || this.inputElement.contains(e.target)) return;
            this.showDropdown = false;
        });
    }

    onWindowScroll(e: Event): void {
        this.setDropdownPosition();
        if (!ElementUtils.isScrolledIntoView(this.wrapperElement)) {
            this.hideDropdown();
        }
    }
    onWindowResize(e: UIEvent): void { this.setDropdownPosition(); }

    @Watch('isLoading')
    onIsLoadingChanged(): void {
        if (this.isLoading) this.showDropdown = false;
    }

    @Watch('error')
    onErrorChanged(): void {
        this.resolvedError = this.error;
    }

    @Watch('resolvedError')
    onResolvedErrorChanged(): void {
        this.$nextTick(() => this.setDropdownPosition());
    }
}
</script>

<style scoped lang="scss">
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
        white-space: nowrap;
        max-width: calc(100% - 38px);
    }
    &__input-chip-value {
        padding-right: 5px;
        min-height: 28px;
        display: flex;
        align-items: center;
        max-width: 100%;
        overflow: hidden;
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
        border: 1px solid var(--color--accent-base);
        max-width: 800px;
    }
    &__dropdown-scroller {
        max-height: calc(min(40vh, 400px));
        overflow-y: auto;
    }
    /* &__dropdown__items {
    } */
    &__statusText {
        padding: 5px 10px;
    }
    &__textInput-wrapper {
        flex: 1;
        display: flex;
    }
    &__textInput {
        width: 100%;
        padding: 5px 0 !important;
        min-height: 28px;
        min-width: 30px;
        border: none !important;
    }
    &.open {
        .select-component__expandIcon {
            transform: rotate(180deg);
        }
    }
    &__expandIcon {
        margin-bottom: -4px;
        transform-origin: 50% 40%;
        transition: transform 0.25s;
    }
    /* &__placeholder { } */
    /* &__error { } */
}
</style>