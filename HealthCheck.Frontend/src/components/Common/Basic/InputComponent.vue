<!-- src/components/Common/Basic/InputComponent.vue -->
<template>
    <div class="input-component" :class="{ 'loading': loading }">
        <div class="input-component--header" v-if="showHeader">
            <div class="input-component--header-name">{{ name }}</div>
            <icon-component small v-if="hasDescription"
                color="gray" class="input-component--help-icon"
                @click="toggleDescription">help</icon-component>
        </div>

        <div v-show="showDescription" class="input-component--description" v-html="description"></div>
        
        <text-field-component
            v-if="!isTextArea"
            v-model:value="currentValue"
            @input="onInput($event.target.value)"
            v-on:change="onDataChanged"
            @click:clear="onClearClicked()"
            :disabled="disabled"
            :type="type"
            :clearable="clearable">
            <tooltip-component v-if="showActionIcon" tooltip="Insert placeholder">
                <icon-component @click="onActionIconClicked">{{ actionIcon }}</icon-component>
            </tooltip-component>
        </text-field-component>
        
        <textarea-component
            v-if="isTextArea"
            v-model:value="currentValue"
            @input="onInput($event.target.value)"
            v-on:change="onDataChanged"
            @click:clear="onClearClicked()"
            :disabled="disabled"
            :clearable="clearable">
            <tooltip-component v-if="showActionIcon" tooltip="Insert placeholder">
                <icon-component @click="onActionIconClicked">{{ actionIcon }}</icon-component>
            </tooltip-component>
        </textarea-component>
        
        <progress-linear-component v-if="loading"
            class="mt-0"
            :value="loadingProgress"
            :color="loadingColor"
            :height="loadingHeight"
        ></progress-linear-component>

        <div class="input-component--error" v-if="error != null && error.length > 0">{{ error }}</div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import TextareaComponent from '@components/Common/Basic/TextareaComponent.vue';
import TextFieldComponent from '@components/Common/Basic/TextFieldComponent.vue';

@Options({
    components: { TextareaComponent, TextFieldComponent }
})
export default class InputComponent extends Vue
{
    @Prop({ required: true })
    value!: string;
    
    @Prop({ required: false, default: '' })
    name!: string;
    
    @Prop({ required: false, default: '' })
    description!: string;
    
    @Prop({ required: false, default: '' })
    actionIcon!: string;
    
    @Prop({ required: false, default: 'text' })
    type!: string;
    
    @Prop({ required: false, default: null})
    error!: string | null;
    
    @Prop({ required: false, default: '' })
    uiHints!: string;
    
    @Prop({ required: false, default: false })
    disabled!: boolean;
    
    @Prop({ required: false, default: true })
    clearable!: boolean;
    
    @Prop({ required: false, default: false })
    showDescriptionOnStart!: boolean;
    
    @Prop({ required: false, default: false })
    loading!: boolean;
    
    @Prop({ required: false, default: 0 })
    loadingProgress!: number;
    
    @Prop({ required: false, default: 'success' })
    loadingColor!: string;
    
    @Prop({ required: false, default: 4 })
    loadingHeight!: number;

    showDescription: boolean = false;
    currentValue: string = '';

    get isTextArea(): boolean { return this.uiHints.indexOf('TextArea') != -1; }

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
    get showActionIcon(): boolean {
        return this.actionIcon != null && this.actionIcon.length > 0;
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
        this.$emit('update:value', newValue);
    }

    onClearClicked(): void {
        this.$emit('update:value', '');
    }

    onActionIconClicked(): void {
        this.$emit('actionIconClicked');
    }

    onDataChanged(): void {
        this.$emit('change', this.currentValue);
    }
}
</script>

<style scoped lang="scss">
.input-component {
    .input-component--header {
        text-align: left;

        .input-component--header-name {
            display: inline-block;
            font-size: 16px;
            color: var(--color--secondary-base);
            font-weight: 600;
        }

        .input-component--help-icon {
            user-select: none;
            font-size: 20px !important;
            &:hover {
                color: #1976d2;
            }
        }
    }

    .input-component--description {
        text-align: left;
        padding: 10px;
        border-radius: 10px;
        background-color: #ebf1fb;
    }
}
</style>

<style lang="scss">
.input-component {
    input {
        font-size: 18px;
        color: #000 !important;
    }

    .input-component--error {
        margin-top: -21px;
        margin-left: 2px;
        font-weight: 600;
        color: var(--color--error-base) !important;
    }
}
</style>