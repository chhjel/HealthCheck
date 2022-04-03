<!-- src/components/Common/Basic/SelectComponent.vue -->
<template>
    <div class="select-component">
        <input-header-component :name="label" :description="description" :showDescriptionOnStart="showDescriptionOnStart" />
        
        <select v-model="currentValue" @input="onInput($event.target.value)" :disabled="disabled">
            <!-- <option disabled value="">Please select one</option> -->
            <option v-for="(item, iIndex) in optionItems"
                :key="`${id}-item-${iIndex}`"
                :value="item.value"
                >{{ item.text }}</option>
        </select>

        <progress-linear-component v-if="isLoading" indeterminate height="3" />

        <div class="select-component--error" v-if="error != null && error.length > 0">{{ error }}</div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import IdUtils from "@util/IdUtils";
import InputHeaderComponent from "./InputHeaderComponent.vue";
import ValueUtils from "@util/ValueUtils";

@Options({
    components: { InputHeaderComponent }
})
export default class SelectComponent extends Vue
{
    @Prop({ required: true })
    value!: string;
    
    @Prop({ required: false, default: '' })
    label!: string;
    
    @Prop({ required: false, default: '' })
    description!: string;
    
    @Prop({ required: false, default: false })
    showDescriptionOnStart!: boolean;

    @Prop({ required: true })
    items!: any;
    
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
    loading!: string | boolean;

    id: string = IdUtils.generateId();
    currentValue: string = '';

    // v-model:value="currentValue"
    // :items="items"
    // :disabled="disabled"
    // :loading="loading"
    // @input="onInput($event)"
    // item-text="text"
    // item-value="value"

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
        this.currentValue = this.value;
    }

    mounted(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get optionItems(): Array<any> {
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
    
    get isLoading(): boolean { return ValueUtils.IsToggleTrue(this.loading); }
    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }
    get isMultiple(): boolean { return ValueUtils.IsToggleTrue(this.multiple); }

    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch('value')
    onValueChanged(): void {
        this.currentValue = this.value;
    }

    onInput(newValue: string): void {
        let emittedValue: string | string[] = newValue;
        if (this.isMultiple) {
            emittedValue = [ newValue ] // todo;
        }
        this.$emit('update:value', emittedValue);
        this.$emit('change', emittedValue);
    }
}
</script>

<style scoped lang="scss">
.select-component {
    .select-component--header {
        text-align: left;

        .select-component--header-name {
            display: inline-block;
            font-size: 16px;
            color: var(--color--secondary-base);
            font-weight: 600;
        }

        .select-component--help-icon {
            user-select: none;
            font-size: 20px !important;
            &:hover {
                color: #1976d2;
            }
        }
    }

    .select-component--description {
        text-align: left;
        padding: 10px;
        border-radius: 10px;
        background-color: #ebf1fb;
    }
}
</style>

<style lang="scss">
.select-component {
    input {
        font-size: 18px;
        color: #000 !important;
    }
    .select-component--error {
        margin-top: -21px;
        margin-left: 2px;
        font-weight: 600;
        color: var(--color--error-base) !important;
    }
}
</style>