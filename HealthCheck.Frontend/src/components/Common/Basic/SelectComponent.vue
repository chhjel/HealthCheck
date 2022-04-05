<!-- src/components/Common/Basic/SelectComponent.vue -->
<template>
    <div class="select-component" :class="rootClasses">
        <input-header-component :name="label" :description="description" :showDescriptionOnStart="showDescriptionOnStart" />
        
        <div class="input-wrapper">
            <div class="select-component__input input input-padding-4" @click="onInputClicked" ref="inputElement" tabindex="0">
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
                <span class="select-component__placeholder input-placeholder"
                    v-if="selectedItems.length == 0 && placeholder">{{ placeholder }}</span>
            </div>
        </div>
        <div class="select-component__dropdown" v-show="showDropdown" ref="dropdownElement">
            <!-- <div class="select-component__dropdown__search">
                Search here
            </div> -->
            <div class="select-component__dropdown__items">
                <div v-for="(item, iIndex) in optionItems"
                    :key="`${id}-item-${iIndex}`"
                    class="select-component__dropdown__item" tabindex="0"
                    @click.stop.prevent="onDropdownItemClicked(item)">
                    <icon-component v-if="valueIsSelected(item.value)" class="mr-1">check_box</icon-component>
                    <icon-component v-if="!valueIsSelected(item.value)" class="mr-1">check_box_outline_blank</icon-component>
                    {{ item.text }}
                </div>
            </div>
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
    
    @Prop({ required: false, default: 'id' })
    itemValue!: string;
    
    @Prop({ required: false, default: 'text' })
    itemText!: string;
    
    @Prop({ required: false, default: null})
    error!: string | null;
    
    @Prop({ required: false, default: false })
    disabled!: string | boolean;
    
    @Prop({ required: false, default: false })
    multiple!: string | boolean;
    
    @Prop({ required: false, default: false })
    nullable!: string | boolean;

    @Prop({ required: false, default: false })
    loading!: string | boolean;
    
    @Ref() readonly inputElement!: HTMLElement;
    @Ref() readonly dropdownElement!: HTMLElement;

    id: string = IdUtils.generateId();
    selectedValues: Array<string> = [];
    showDropdown: boolean = false;
    
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

    get optionItems(): Array<Item> {
        if (Array.isArray(this.items))
        {
            if (this.items.length == 0) return [];
            const firstValue = this.items[0];
            const isSimpleValue = typeof firstValue === 'string' || firstValue instanceof String;
            return this.items.map(x => {
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
        return Object.keys(this.items).map(key => {
            return {
                value: key,
                text: this.items[key]
            };
        });
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
    
    get isLoading(): boolean { return ValueUtils.IsToggleTrue(this.loading); }
    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }
    get isMultiple(): boolean { return ValueUtils.IsToggleTrue(this.multiple); }
    get isNullable(): boolean { return ValueUtils.IsToggleTrue(this.nullable); }
    
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
    // todo: nullable, if not remove clears
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
            if (this.selectedValues.length == 0 && this.isNullable) return null;
            emittedValue = this.selectedValues[0] || '';
        }
        this.$emit('update:value', emittedValue);
        this.$emit('change', emittedValue);
    }

    valueIsSelected(val: string): boolean { return this.selectedValues.includes(val); }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onInputClicked(): void {
        if (this.isDisabled || this.isLoading)
        {
            this.showDropdown = false;
            return;
        }
        this.showDropdown = !this.showDropdown;
    }

    onDropdownItemClicked(item: Item): void {
        if (!this.allowModify) return;
        if (!this.valueIsSelected(item.value)) {
            this.addValue(item.value);
        } else if (this.isNullable || this.isMultiple || this.selectedValues.length > 1) {
            this.removeValue(item.value);
        }
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
    &__input {
        display: flex;
        flex-wrap: wrap;
    }
    &__input-chip {
        display: flex;
        align-items: center;
        padding-left: 5px;
        margin-right: 5px;
        background-color: var(--color--accent-base);
        border-radius: 2px;
    }
    &__input-chip-value {
        padding-right: 5px;
        min-height: 27px;
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
    }
    /* &__dropdown__items {
    } */
    &__dropdown__item {
        padding: 5px;
        display: flex;
        align-items: center;
    }
    /* &__placeholder { } */
    /* &__error { } */
}
</style>