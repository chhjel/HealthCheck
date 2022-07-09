<!-- src/components/Common/Basic/InputComponent.vue -->
<template>
    <div class="input-component" :class="{ 'loading': loading }">
        <text-field-component
            v-if="!isTextArea"
            :label="name" :description="description" :showDescriptionOnStart="showDescriptionOnStart"
            v-model:value="currentValue"
            @input="onInput"
            v-on:change="onDataChanged"
            @click:clear="onClearClicked()"
            :errorMessages="error"
            :disabled="disabled"
            :type="type"
            :clearable="clearable"
            :appendIcon="actionIcon"
            :appendIconTooltip="actionIconTooltip"
            @click:append="onActionIconClicked">
        </text-field-component>
        
        <textarea-component
            v-if="isTextArea"
            :label="name" :description="description" :showDescriptionOnStart="showDescriptionOnStart"
            v-model:value="currentValue"
            @input="onInput"
            v-on:change="onDataChanged"
            @click:clear="onClearClicked()"
            :errorMessages="error"
            :disabled="disabled"
            :clearable="clearable"
            :appendIcon="actionIcon"
            :appendIconTooltip="actionIconTooltip"
            @click:append="onActionIconClicked">
        </textarea-component>
        
        <progress-linear-component v-if="loading"
            class="mt-0"
            indeterminate
            :color="loadingColor"
            :height="loadingHeight"
        ></progress-linear-component>
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
    
    @Prop({ required: false, default: '' })
    actionIconTooltip!: string;
    
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
    
    @Prop({ required: false, default: 'success' })
    loadingColor!: string;
    
    @Prop({ required: false, default: 4 })
    loadingHeight!: number;

    currentValue: string = '';

    get isTextArea(): boolean { return this.uiHints.indexOf('TextArea') != -1; }

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
    get showActionIcon(): boolean {
        return this.actionIcon != null && this.actionIcon.length > 0;
    }

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

    onInput(newValue: string | InputEvent): void {
        const eventValue = (<any>newValue)?.target?.value;
        this.$emit('update:value', eventValue ?? newValue);
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
</style>
