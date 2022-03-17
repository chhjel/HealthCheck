<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeGuidComponent.vue -->
<template>
    <div>
        <div>
            <div :xs10="isNullable" :xs12="!isNullable">
                <text-field-component
                    class="pt-0"
                    v-model:value="localValue"
                    :placeholder="placeholderText"
                    :disabled="readonly"
                    required />
            </div>

            <div xs2
                :xs3="isListItem"
                class="text-sm-right"
                v-if="isNullable">
                <tooltip-component tooltip="Sets value to null">
                    <btn-component flat icon color="primary" class="ma-0 pa-0"
                        @click="setValueToNull"
                        :disabled="localValue == null || readonly">
                        <icon-component>clear</icon-component>
                    </btn-component>
                </tooltip-component>
            </div>

        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { HCBackendInputConfig } from '@generated/Models/Core/HCBackendInputConfig';

@Options({
    components: {
    }
})
export default class ParameterInputTypeGuidComponent extends Vue {
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
    }
    
    setValueToNull(): void {
        this.localValue = null;
    }

    validateValue(): void {
        if (this.localValue == null) {
            this.localValue = "";
        }
    }

    get isNullable(): boolean {
        return this.config.Nullable;
    }

    get placeholderText(): string {
        if (this.isNullable)
        {
            return this.localValue == null || this.localValue.length == 0 ? this.nullName : "";
        }
        return (this.localValue == null || this.localValue.length == 0)
            ? "00000000-0000-0000-0000-000000000000" : "";
    }

    get nullName(): string {
        return this.config.NullName || 'null';
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
        this.$emit('update:value', this.localValue);
    }
}
</script>

<style scoped>
</style>
