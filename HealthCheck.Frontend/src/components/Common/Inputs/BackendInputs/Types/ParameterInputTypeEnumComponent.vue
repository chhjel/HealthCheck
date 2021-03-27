<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeEnumComponent.vue -->
<template>
    <div>
        <v-select
            v-model="localValue"
            :items="items"
            :multiple="multiple"
            :chips="multiple"
            v-on:change="onChanged"
            color="secondary"
            class="parameter-select pt-0">
        </v-select>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import BackendInputConfig from "../BackendInputConfig";

@Component({
    components: {
    }
})
export default class ParameterInputTypeEnumComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    config!: BackendInputConfig;

    @Prop({ required: false, default: false })
    multiple!: boolean;

    localValue: string | string[] = "";
    
    mounted(): void {
        this.updateLocalValue();
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    updateLocalValue(): void
    {
        this.localValue = this.value;

        if (this.multiple) {
            if (this.config.defaultValue != null) {
                this.localValue = this.config.defaultValue.split(", ");
            } else {
                this.localValue = [];
            }
        } else {
            this.localValue = this.config.defaultValue || this.config.possibleValues[0];
        }
        this.onChanged();
    }
    
    get items(): Array<string> {
        return this.config.possibleValues;
    }

    onChanged(): void {
        if (this.multiple) {
            let selected = <Array<string>>this.localValue;
            this.$emit('input', selected.join(", "));
        } else {
            this.$emit('input', <string>this.localValue);
        }
    }
}
</script>

<style>
.parameter-checkbox label {
    color: #000 !important;
}
.v-select--chips .v-input__slot {
    height: 32px;
}
</style>
