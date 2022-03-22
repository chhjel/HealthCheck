<!-- src/components/Common/Basic/SelectComponent.vue -->
<template>
    <div class="select-component">
        <div class="select-component--header" v-if="showHeader">
            <div class="select-component--header-name">{{ name }}</div>
            <icon-component small v-if="hasDescription"
                color="gray" class="select-component--help-icon"
                @click="toggleDescription">help</icon-component>
        </div>

        <div v-show="showDescription" class="select-component--description" v-html="description"></div>
        
        <select v-model="currentValue" @input="onInput($event.target.value)" :disabled="disabled">
            <!-- <option disabled value="">Please select one</option> -->
            <option v-for="(item, iIndex) in optionItems"
                :key="`${id}-item-${iIndex}`"
                :value="item.value"
                >{{ item.text }}</option>
        </select>
        <div v-if="loading">Loading...</div>

        <div class="select-component--error" v-if="error != null && error.length > 0">{{ error }}</div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import IdUtils from "@util/IdUtils";

@Options({
    components: {}
})
export default class SelectComponent extends Vue
{
    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    items!: any;
    
    @Prop({ required: false, default: '' })
    name!: string;
    
    @Prop({ required: false, default: 'primary' })
    color!: string;
    
    @Prop({ required: false, default: '' })
    itemText!: string;
    
    @Prop({ required: false, default: '' })
    itemValue!: string;
    
    @Prop({ required: false, default: '' })
    description!: string;
    
    @Prop({ required: false, default: null})
    error!: string | null;
    
    @Prop({ required: false, default: false })
    disabled!: boolean;
    
    @Prop({ required: false, default: false })
    multiple!: boolean;
    
    @Prop({ required: false, default: false })
    showDescriptionOnStart!: boolean;
    
    @Prop({ required: false, default: false })
    loading!: boolean;

    id: string = IdUtils.generateId();
    showDescription: boolean = false;
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
        this.showDescription = this.hasDescription && this.showDescriptionOnStart;
    }

    mounted(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get optionItems(): Array<any> {
        if (Array.isArray(this.items))
        {
            return this.items.map(x => {
                return {
                    value: x,
                    text: x
                };
            });
        }
        return Object.keys(this.items).map(key => {
            return {
                value: key,
                text: this.items[key]
            };
        });
    }

    get showHeader(): boolean {
        return this.name != null && this.name.length > 0;
    }

    get hasDescription(): boolean {
        return this.description != null && this.description.length > 0;
    }

    ////////////////
    //  METHODS  //
    //////////////
    toggleDescription(): void {
        this.showDescription = !this.showDescription;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch('value')
    onValueChanged(): void {
        this.currentValue = this.value;
    }

    onInput(newValue: string): void {
        let emittedValue: string | string[] = newValue;
        if (this.multiple) {
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