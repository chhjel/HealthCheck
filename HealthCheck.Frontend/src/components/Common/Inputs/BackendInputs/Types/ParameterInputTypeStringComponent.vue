<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeStringComponent.vue -->
<!-- todo: flex 1 & max-width: npx -->
<template>
    <div>
        <v-layout>
            <v-flex :xs10="!config.NotNull" :xs12="config.NotNull">
                <v-text-field
                    v-if="isTextField"
                    class="pt-0"
                    v-model="localValue"
                    :placeholder="placeholderText"
                    :disabled="readonly"
                    required />
                <v-textarea
                    v-if="isTextArea"
                    class="pt-0"
                    v-model="localValue"
                    :placeholder="placeholderText"
                    :disabled="readonly"
                    required />
                <editor-component
                    v-if="isCodeArea"
                    class="editor"
                    :language="'json'"
                    v-model="localValue"
                    :read-only="readonly"
                    ref="editor" />
            </v-flex>

            <v-flex xs2
                :xs3="isListItem"
                class="text-sm-right"
                v-if="!config.NotNull">
                <v-tooltip bottom>
                    <template v-slot:activator="{ on }">
                        <span v-on="on">
                            <v-btn flat icon color="primary" class="ma-0 pa-0"
                                @click="setValueToNull"
                                :disabled="localValue == null || readonly">
                                <v-icon>clear</v-icon>
                            </v-btn>
                        </span>
                    </template>
                    <span>Sets value to null</span>
                </v-tooltip>
            </v-flex>

        </v-layout>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import { HCBackendInputConfig } from 'generated/Models/Core/HCBackendInputConfig';
import EditorComponent from '../../../EditorComponent.vue'

@Component({
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
    
    mounted(): void {
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
        this.$emit('input', this.localValue);
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
