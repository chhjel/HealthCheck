<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputAnyJsonComponent.vue -->
<template>
    <div>
        <v-btn @click="showDialog">{{ buttonText }}</v-btn>
        
        <v-dialog v-model="editorDialogVisible"
            @keydown.esc="editorDialogVisible = false"
            scrollable
            fullscreen
            content-class="edit-json-value-dialog">
            <v-card style="background-color: #f4f4f4">
                <v-toolbar class="elevation-0">
                    <v-toolbar-title>Edit value of parameter '{{ name }}' of type '{{ type}}'</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-btn icon
                        @click="editorDialogVisible = false">
                        <v-icon>close</v-icon>
                    </v-btn>
                </v-toolbar>

                <v-divider></v-divider>
                
                <v-card-text>
            
                    <editor-component
                        class="editor"
                        :language="'json'"
                        v-model="localValue"
                        :read-only="false"
                        ref="editor"/>
                        
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions >
                    <v-btn color="error"
                        @click="setValueToNull">Set value to null</v-btn>
                    <v-btn color="secondary"
                        v-if="hasTemplate"
                        @click="setValueToTemplate">Reset value</v-btn>
                    <v-spacer></v-spacer>
                    <v-alert :value="error.length > 0" color="error">{{ error }}</v-alert>
                    <v-spacer></v-spacer>
                    <v-btn color="primary"
                        @click="editorDialogVisible = false">Close</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import TestParameterTemplateViewModel from "../../../../../models/modules/TestSuite/TestParameterTemplateViewModel";
import EditorComponent from  '../../../../Common/EditorComponent.vue';
import { HCBackendInputConfig } from 'generated/Models/Core/HCBackendInputConfig';

@Component({
    components: {
        EditorComponent
    }
})
export default class ParameterInputAnyJsonComponent extends Vue {
    @Prop({ required: true })
    name!: string;

    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    type!: string;

    @Prop({ required: true })
    config!: HCBackendInputConfig;

    localValue: string | null = '';

    @Prop({ required: false })
    isListItem!: boolean;

    editorDialogVisible: boolean = false;
    
    mounted(): void {
        if (this.localValue == null) {
            this.localValue = '';
        }
    }

    refreshSize(): void {
        const editor: EditorComponent = <EditorComponent>this.$refs.editor;
        editor.refreshSize();
    }

    showDialog(): void {
        this.editorDialogVisible = true;
    }

    setValueToNull(): void {
        this.localValue = '';
    }

    setValueToTemplate(): void {
        this.localValue = this.templateValue;
    }

    get hasTemplate(): boolean {
        try {
            const _ = (this.$store.state.tests.templateValues as Array<TestParameterTemplateViewModel>)
                .filter(x => x.Type == this.type)[0];
            return true;
        } catch(e)
        {
            return false;
        }
    }
    
    get templateValue(): string {
        const templateData = (this.$store.state.tests.templateValues as Array<TestParameterTemplateViewModel>)
            .filter(x => x.Type == this.type)[0];
        return (templateData) ? templateData.Template : '{\n}';
    }

    get isNull(): boolean {
        return this.localValue === null || this.localValue === undefined || this.localValue === '';
    }

    get error(): string {
        if (this.isNull)
        {
            return '';
        }

        try {
            JSON.parse(this.localValue ?? '');
            return '';
        } catch (e) {
            return `${e}`;
        }
    }

    get buttonText(): string {
        if (this.isNull)
        {
            return `NULL`;
        }
        else
        {
            const json = this.localValue ?? '';
            try {
                const obj = JSON.parse(json);
                let title: string = obj.name ?? obj.Name 
                    ?? obj.title ?? obj.Title
                    ?? obj.id ?? obj.Id
                    ?? '';

                title = `${title}`;
                if (title.length > 0)
                {
                    title = `'${title}'`;
                }
                return `${title}...`;
            } catch {
                return 'Invalid Json';
            }
        }
    }
    
    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch("editorDialogVisible")
    onFullscreenChanged(): void {
        this.refreshSize();
        this.$nextTick(() => this.refreshSize());
        setTimeout(() => {
            this.refreshSize();
        }, 100);
    }

    @Watch('value')
    updateLocalValue(): void
    {
        this.localValue = this.value;
    }

    @Watch('localValue')
    onLocalValueChanged(): void
    {
        this.$emit('input', this.localValue);
    }
}
</script>

<style scoped>
.editor {
  width: 100%;
  height: calc(100vh - 200px);
}
.code-editor-loader {
    display: flex;
    align-items: center;
    justify-content: center;
}
</style>
