<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeHttpPostedFileBaseComponent.vue -->
<template>
    <div>
        <v-layout class="parameter-input-file">
            <!-- <input
                type="file"
                @change="onFileChanged"
                ref="fileinput"
                style="width:100%"
                :disabled="readonly" /> -->

            <input type="file"
                :id="`file-parameter-${id}`"
                style="display: none;"
                ref="fileinput"
                @change="onFileChanged"
                :disabled="readonly" />
            <div class="upload-label-wrapper">
                <v-tooltip bottom>
                    <template v-slot:activator="{ on }">
                        <label :for="`file-parameter-${id}`"
                            v-on="on"
                            class="v-btn v-btn--small theme--light upload-label"
                            :class="{ 'disabled': readonly, 'v-btn--disabled': readonly }">
                            <div>{{ label }}</div>
                        </label>
                    </template>
                    <span>{{ tooltip }}</span>
                </v-tooltip>
            </div>
            <v-tooltip bottom v-if="allowClear">
                <template v-slot:activator="{ on }">
                    <span v-on="on">
                        <v-btn flat icon color="primary" class="ma-0 pa-0"
                            @click="setValueToNull"
                            :disabled="localValue == null">
                            <v-icon>clear</v-icon>
                        </v-btn>
                    </span>
                </template>
                <span>Clear file</span>
            </v-tooltip>
        </v-layout>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import { HCBackendInputConfig } from 'generated/Models/Core/HCBackendInputConfig';
import IdUtils from "util/IdUtils";

@Component({
    components: {
    }
})
export default class ParameterInputTypeHttpPostedFileBaseComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    config!: HCBackendInputConfig;

    @Prop({ required: false })
    isListItem!: boolean;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    @Prop({ required: false, default: true })
    allowClear!: boolean;

    localValue: string | null = '';
    id: string = IdUtils.generateId();
    selectedFile: File | null = null;
    
    mounted(): void {
        this.updateLocalValue();
        this.onLocalValueChanged();
    }

    getReadableFileSize(bytes: number, si: boolean = true): string
    {
        var thresh = si ? 1000 : 1024;
        if(Math.abs(bytes) < thresh) {
            return bytes + ' B';
        }
        var units = si
            ? ['kB','MB','GB','TB','PB','EB','ZB','YB']
            : ['KiB','MiB','GiB','TiB','PiB','EiB','ZiB','YiB'];
        var u = -1;
        do {
            bytes /= thresh;
            ++u;
        } while(Math.abs(bytes) >= thresh && u < units.length - 1);
        return bytes.toFixed(1)+' '+units[u];
    }

    setValueToNull(): void {
        this.localValue = null;
        this.selectedFile = null;

        let input: HTMLInputElement = (<HTMLInputElement>this.$refs.fileinput);
        input.value = "";
    }

    get label(): string {
        const fileName = this.selectedFileName;
        if (fileName)
        {
            return `'${fileName}'`;
        }
        return 'Select file';
    }

    get tooltip(): string {
        const fileName = this.selectedFileName;
        if (fileName)
        {
            return `'${fileName}'`;
        }
        return 'Click to select a file';
    }

    get selectedFileName(): string | null {
        if (this.localValue && this.localValue.includes('|'))
        {
            const parts = this.localValue.split('|');
            return parts[1];
        }
        return null;
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
        this.localValue = this.value;
        if (this.localValue && !this.localValue.includes('|'))
        {
            this.localValue = null;
        }
    }

    @Watch('localValue')
    onLocalValueChanged(): void
    {
        this.$emit('input', this.localValue);
        this.$emit('onFileChanged', this.selectedFile);
    }

    onFileChanged(): void {
        let input: HTMLInputElement = (<HTMLInputElement>this.$refs.fileinput);
        let files = input.files;
        if (files == null || files.length == 0)
        {
            this.setValueToNull();
            return;
        }
        
        let file = files[0];
        this.selectedFile = file;
        var reader = new FileReader();
        reader.onload = ((theFile: File) => {
            return (e: any) => {
                var binaryData = e.target.result;
                var base64String = window.btoa(binaryData);
                this.localValue = `${file.type}|${file.name}|${base64String}`;
            };
        })(file);
        
        reader.readAsBinaryString(file);
    }
}
</script>

<style scoped lang="scss">
.parameter-input-file {
    position: relative;
    align-items: baseline;
    margin-top: 0 !important;
}
.upload-label-wrapper {
    overflow: hidden;
    max-width: 100%;
}
.upload-label {
    cursor: pointer;
    white-space: nowrap;
    max-width: 100%;

    max-width: calc(100% - 10px);
    overflow: hidden;
    justify-content: flex-start;

    div {
        overflow: hidden;
        text-overflow: ellipsis;
    }
}
</style>
