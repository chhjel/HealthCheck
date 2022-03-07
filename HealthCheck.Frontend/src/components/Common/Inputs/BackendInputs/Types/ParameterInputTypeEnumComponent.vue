<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeEnumComponent.vue -->
<template>
    <div class="fixed-height-select">
        <v-select
            v-model="localValue"
            :items="items"
            :multiple="multiple"
            :chips="multiple"
            :disabled="readonly"
            v-on:change="onChanged"
            color="secondary"
            class="parameter-select pt-0">
        </v-select>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import { HCBackendInputConfig } from 'generated/Models/Core/HCBackendInputConfig';

@Component({
    components: {
    }
})
export default class ParameterInputTypeEnumComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    config!: HCBackendInputConfig;

    @Prop({ required: false, default: false })
    multiple!: boolean;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    localValue: string | string[] = "";
    
    mounted(): void {
        this.$nextTick(() => {
            this.updateLocalValue();
            this.onChanged();
        });
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    updateLocalValue(): void
    {
        this.localValue = this.value;

        if (this.multiple) {
            if (this.config.DefaultValue != null) {
                this.localValue = this.config.DefaultValue.split(", ");
            } else {
                this.localValue = [];
            }
        } else {
            if (this.localValue == null || this.localValue.length == 0) {
                if (this.isNullable && this.config.DefaultValue == null)
                {
                    this.localValue = this.config.DefaultValue || "[null]";
                }
                else
                {
                    this.localValue = this.config.DefaultValue || this.config.PossibleValues[0];
                }
            }
        }
        this.onChanged();
    }
    
    get items(): Array<string> {
        if (this.isNullable)
        {
            return ['[null]', ...this.config.PossibleValues];
        }
        return this.config.PossibleValues;
    }

    get isNullable(): boolean {
        return this.config.Nullable;
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

<style lang="scss">
.parameter-checkbox label {
    color: #000 !important;
}
.fixed-height-select {
    .v-select--chips .v-input__slot {
        height: 32px;
    }
}
</style>
