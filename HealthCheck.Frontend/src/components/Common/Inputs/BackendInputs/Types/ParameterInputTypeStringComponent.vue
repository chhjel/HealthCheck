<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeStringComponent.vue -->
<!-- todo: flex 1 & max-width: npx -->
<template>
    <div class="flex">
        <text-field-component
            v-if="isTextField"
            class="pt-0 spacer"
            v-model:value="localValue"
            :placeholder="placeholderText"
            :disabled="readonly" />
        <textarea-component
            v-if="isTextArea"
            class="pt-0 spacer"
            v-model:value="localValue"
            :placeholder="placeholderText"
            :disabled="readonly" />
        <editor-component
            v-if="isCodeArea"
            class="editor spacer"
            :language="'json'"
            v-model:value="localValue"
            :read-only="readonly"
            ref="editor" />

        <div v-if="!config.NotNull">
            <tooltip-component tooltip="Sets value to null">
                <btn-component flat icon color="primary" class="ma-0 pa-0"
                    @click="setValueToNull"
                    :disabled="localValue == null || readonly">
                    <icon-component>clear</icon-component>
                </btn-component>
            </tooltip-component>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { HCBackendInputConfig } from '@generated/Models/Core/HCBackendInputConfig';
import EditorComponent from '@components/Common/EditorComponent.vue'

@Options({
    components: {
        EditorComponent
    }
})
export default class ParameterInputTypeStringComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: false })
    isListItem!: boolean;

    @Prop({ required: true })
    config!: HCBackendInputConfig;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    localValue: string | null = '';
    
    created(): void {
        this.updateLocalValue();
        
        this.refreshEditorSize();
        this.$nextTick(() => this.refreshEditorSize());
        setTimeout(() => {
            this.refreshEditorSize();
        }, 100);
    }
    
    setValueToNull(): void {
        this.localValue = null;
    }

    get placeholderText(): string {
        return this.localValue == null ? this.nullName : "";
    }

    get nullName(): string {
        return this.config.NullName || 'null';
    }

    get isTextArea(): boolean {
        return this.config.Flags.includes('TextArea');
    }

    get isCodeArea(): boolean {
        return this.config.Flags.includes('CodeArea');
    }

    get isTextField(): boolean {
        return !this.isTextArea && !this.isCodeArea;
    }
    
    refreshEditorSize(): void {
        const editor: EditorComponent = <EditorComponent>this.$refs.editor;
        if (editor)
        {
            editor.refreshSize();
        }
    }
    
    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
        this.localValue = this.value;
        this.validateValue();
    }

    @Watch('localValue')
    onLocalValueChanged(): void
    {
        this.validateValue();
        this.$emit('update:value', this.localValue);
    }

    validateValue(): void {
        if (this.localValue == null && this.config.NotNull) {
            this.localValue = "";
        }
    }
}
</script>

<style scoped>
.editor {
  width: 100%;
  height: 200px;
  border: 1px solid #949494;
}
</style>
