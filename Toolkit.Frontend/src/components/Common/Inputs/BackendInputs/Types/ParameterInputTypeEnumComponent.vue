<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeEnumComponent.vue -->
<template>
    <div class="fixed-height-select">
        <select-component
            v-model:value="localValue"
            :items="items"
            :multiple="multiple"
            :chips="multiple"
            :disabled="readonly"
            v-on:change="onChanged"
            class="parameter-select pt-0">
        </select-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TKBackendInputConfig } from '@generated/Models/Core/TKBackendInputConfig';

@Options({
    components: {
    }
})
export default class ParameterInputTypeEnumComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    config!: TKBackendInputConfig;

    @Prop({ required: false, default: false })
    multiple!: boolean;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    localValue: string | string[] = "";
    
    created(): void {
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
                    this.localValue = this.config.DefaultValue || this.nullName;
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
            return [this.nullName, ...this.config.PossibleValues];
        }
        return this.config.PossibleValues;
    }

    get isNullable(): boolean {
        return this.config.Nullable;
    }

    get nullName(): string {
        return this.config.NullName || '[null]';
    }

    onChanged(): void {
        if (this.multiple) {
            let selected = <Array<string>>this.localValue;
            this.$emit('update:value', selected.join(", "));
        } else {
            this.$emit('update:value', <string>this.localValue);
        }
    }
}
</script>

<style lang="scss">
.parameter-checkbox label {
    color: #000 !important;
}
</style>
