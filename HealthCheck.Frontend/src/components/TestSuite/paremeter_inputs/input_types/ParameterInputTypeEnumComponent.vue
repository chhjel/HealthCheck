<!-- src/components/paremeter_inputs/input_types/ParameterInputTypeEnumComponent.vue -->
<template>
    <div>
        <v-select
            v-model="value"
            :items="items"
            :multiple="multiple"
            :chips="multiple"
            v-on:change="onChanged"
            color="secondary"
            class="parameter-select pt-0 mt-2">
        </v-select>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from '../../../../models/TestSuite/TestParameterViewModel';

@Component({
    components: {
    }
})
export default class ParameterInputTypeEnumComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;

    @Prop({ required: false, default: false })
    multiple!: boolean;
    
    value: string | string[] = "";

    get items(): Array<string> {
        return this.parameter.PossibleValues;
    }

    mounted(): void {
        if (this.multiple) {
            if (this.parameter.DefaultValue != null) {
                this.value = this.parameter.DefaultValue.split(", ");
            } else {
                this.value = [];
            }
        } else {
            this.value = this.parameter.DefaultValue || this.parameter.PossibleValues[0];
        }
        this.onChanged();
    }

    onChanged(): void {
        if (this.multiple) {
            let selected = <Array<string>>this.value;
            this.parameter.Value = selected.join(", ");
        } else {
            this.parameter.Value = <string>this.value;
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
