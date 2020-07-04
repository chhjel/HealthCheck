<!-- src/components/Common/Basic/SelectComponent.vue -->
<template>
    <div class="select-component">
        <div class="select-component--header" v-if="showHeader">
            <div class="select-component--header-name">{{ name }}</div>
            <v-icon small v-if="hasDescription"
                color="gray" class="select-component--help-icon"
                @click="toggleDescription">help</v-icon>
        </div>

        <div v-show="showDescription" class="select-component--description" v-html="description"></div>
        
        <v-select
            v-model="currentValue"
            :items="items"
            :disabled="disabled"
            :loading="loading"
            @input="onInput($event)"
            item-text="text"
            item-value="value"
            color="secondary">
        </v-select>

        <div class="select-component--error" v-if="error != null && error.length > 0">{{ error }}</div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";

@Component({
    components: {}
})
export default class SelectComponent extends Vue
{
    @Prop({ required: true })
    value!: string;
    
    @Prop({ required: false, default: '' })
    name!: string;

    @Prop({ required: true })
    items!: any;
    
    @Prop({ required: false, default: '' })
    description!: string;
    
    @Prop({ required: false, default: null})
    error!: string | null;
    
    @Prop({ required: false, default: false })
    disabled!: boolean;
    
    @Prop({ required: false, default: false })
    showDescriptionOnStart!: boolean;
    
    @Prop({ required: false, default: false })
    loading!: boolean;

    showDescription: boolean = false;
    currentValue: string = '';

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
        this.$emit('input', newValue);
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
            color: var(--v-secondary-base);
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

    .v-input {
        padding-top: 0;
    }

    .select-component--error {
        margin-top: -21px;
        margin-left: 2px;
        font-weight: 600;
        color: var(--v-error-base) !important;
    }
}
</style>