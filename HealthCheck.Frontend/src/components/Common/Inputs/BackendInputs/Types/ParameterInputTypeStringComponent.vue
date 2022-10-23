<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeStringComponent.vue -->
<!-- todo: flex 1 & max-width: npx -->
<template>
    <div class="flex">
        <text-field-component
            v-if="isTextField"
            class="pt-0 spacer"
            v-model:value="localValue"
            @blur="tryValidatePattern"
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
            :language="config?.CodeLanguage || 'json'"
            v-model:value="localValue"
            :read-only="readonly"
            :allowFullscreen="true"
            :title="config?.Name"
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
import { HCUIHint } from "@generated/Enums/Core/HCUIHint";
import RegexUtils from "@util/RegexUtils";
import { nextTick } from "@vue/runtime-core";

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
        this.localValue = '';
        this.localValue = null;
    }

    get placeholderText(): string {
        return this.localValue == null ? this.nullName : "";
    }

    get nullName(): string {
        return this.config.NullName || 'null';
    }

    get isTextArea(): boolean {
        return this.config.UIHints.includes(HCUIHint.TextArea);
    }

    get isCodeArea(): boolean {
        return this.config.UIHints.includes(HCUIHint.CodeArea);
    }

    get isTextField(): boolean {
        return !this.isTextArea && !this.isCodeArea;
    }

    get hasPattern(): boolean {
        return this.isTextField && !!this.config.TextPattern;
    }
    
    refreshEditorSize(): void {
        const editor: EditorComponent = <EditorComponent>this.$refs.editor;
        if (editor)
        {
            editor.refreshSize();
        }
    }

    tryValidatePattern(): void {
        if (!!this.localValue && this.hasPattern) {
            this.tryEnforcePattern();
        }
    }

    tryEnforcePattern(): void {
        try {
            if (!this.config.TextPattern.startsWith('/')) return;

            let pattern = this.config.TextPattern.substring(1);
            pattern = pattern.substring(0, pattern.lastIndexOf('/'));
            const flags = this.config.TextPattern.substring(this.config.TextPattern.lastIndexOf('/') + 1);
            const regex = new RegExp(pattern, flags);
            nextTick(() => {
                this.localValue = RegexUtils.inverseReplaceRegex(regex, this.localValue, '');
            });
        } catch(e) {
            console.warn(`[${this.config.Name}] Failed to enforce input pattern '${this.config.TextPattern}' with the following error:`);
            console.warn(e);
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
        if (this.localValue == null && this.config.UIHints.includes(HCUIHint.NotNull)) {
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
