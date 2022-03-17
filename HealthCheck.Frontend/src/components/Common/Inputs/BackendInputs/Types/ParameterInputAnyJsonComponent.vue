<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputAnyJsonComponent.vue -->
<template>
    <div>
        <btn-component @click="showDialog" :disabled="readonly">{{ buttonText }}</btn-component>
        
        <dialog-component v-model:value="editorDialogVisible"
            @keydown.esc="editorDialogVisible = false"
            scrollable
            fullscreen
            content-class="edit-json-value-dialog">
            <card-component>
                <toolbar-component>
                    <div>Edit value of parameter '{{ name }}' of type '{{ type}}'</div>
                                        <btn-component icon
                        @click="editorDialogVisible = false">
                        <icon-component>close</icon-component>
                    </btn-component>
                </toolbar-component>

                                
                <div>
            
                    <editor-component
                        class="editor"
                        :language="'json'"
                        v-model:value="localValue"
                        :read-only="readonly"
                        ref="editor"/>
                        
                </div>
                                <div >
                    <btn-component color="error"
                        @click="setValueToNull">Set value to null</btn-component>
                    <btn-component color="secondary"
                        v-if="hasTemplate"
                        @click="setValueToTemplate">Reset value</btn-component>
                                        <alert-component :value="error.length > 0" color="error">{{ error }}</alert-component>
                                        <btn-component color="primary"
                        @click="editorDialogVisible = false">Close</btn-component>
                </div>
            </card-component>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestParameterTemplateViewModel } from '@generated/Models/Core/TestParameterTemplateViewModel';
import EditorComponent from '@components/Common/EditorComponent.vue';
import { HCBackendInputConfig } from '@generated/Models/Core/HCBackendInputConfig';
import { StoreUtil } from "@util/StoreUtil";

@Options({
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

    @Prop({ required: false, default: false })
    readonly!: boolean;

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
            const _ = (StoreUtil.store.state.tests.templateValues as Array<TestParameterTemplateViewModel>)
                .filter(x => x.Type == this.type)[0];
            return true;
        } catch(e)
        {
            return false;
        }
    }
    
    get templateValue(): string {
        const templateData = (StoreUtil.store.state.tests.templateValues as Array<TestParameterTemplateViewModel>)
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

    get nullName(): string {
        return this.config.NullName || 'NULL';
    }

    get buttonText(): string {
        if (this.isNull)
        {
            return this.nullName;
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
        // if (this.isListItem)
        // {
        //     this.$emit('update:value', JSON.parse(this.localValue || ''));
        // }
        this.$emit('update:value', this.localValue);
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
