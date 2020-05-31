<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeHttpPostedFileBaseComponent.vue -->
<template>
    <div>
        <v-layout style="margin:0">
            <input
                type="file"
                @change="onFileChanged"
                ref="fileinput"
                style="width:100%" />
            
            <v-flex xs2
                :xs3="isListItem"
                class="text-sm-right pa-0"
                v-if="parameter.Value != null">
                <v-tooltip bottom>
                    <template v-slot:activator="{ on }">
                        <span v-on="on">
                            <v-btn flat icon color="primary" class="ma-0 pa-0"
                                @click="setValueToNull"
                                :disabled="parameter.Value == null">
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
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from  '../../../../../models/modules/TestSuite/TestParameterViewModel';

@Component({
    components: {
    }
})
export default class ParameterInputTypeHttpPostedFileBaseComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;

    @Prop({ required: false })
    isListItem!: boolean;

    mounted(): void {
    }

    setValueToNull(): void {
        this.parameter.Value = null;

        let input: HTMLInputElement = (<HTMLInputElement>this.$refs.fileinput);
        input.value = "";
    }

    get placeholderText(): string {
        return this.parameter.Value == null ? "null" : "";
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
                this.parameter.Value = `${file.type}|${file.name}|${base64String}`;
            };
        })(file);
        
        reader.readAsBinaryString(file);
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
}
</script>

<style scoped>
</style>
