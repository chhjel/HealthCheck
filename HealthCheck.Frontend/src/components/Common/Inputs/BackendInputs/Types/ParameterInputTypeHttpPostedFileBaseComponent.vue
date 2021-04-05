<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeHttpPostedFileBaseComponent.vue -->
<template>
    <div>
        <v-layout style="margin:0">
            <input
                type="file"
                @change="onFileChanged"
                ref="fileinput"
                style="width:100%"
                :disabled="readonly" />
            
            <v-flex xs2
                :xs3="isListItem"
                class="text-sm-right pa-0"
                v-if="localValue != null">
                <v-tooltip bottom>
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
            </v-flex>

        </v-layout>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import { HCBackendInputConfig } from 'generated/Models/Core/HCBackendInputConfig';

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

    localValue: string | null = '';
    
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

        let input: HTMLInputElement = (<HTMLInputElement>this.$refs.fileinput);
        input.value = "";
    }

    get isNullable(): boolean {
        return this.config.Nullable;
    }

    get placeholderText(): string {
        return this.localValue == null ? "null" : "";
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

<style scoped>
</style>
