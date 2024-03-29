<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeTimeSpanComponent.vue -->
<template>
    <div class="flex">
        <timespan-input-component
            class="pt-0 spacer"
            v-model:value="localValue"
            :disabled="readonly"
            :minimal="true"
            :allowClear="false"
            :fill="true"
            :maxHour="null"
            />

        <div v-if="!config.NotNull">
            <tooltip-component :tooltip="clearTooltip">
                <btn-component flat icon color="primary" class="ma-0 pa-0"
                    @click="setValueToNull"
                    :disabled="localValue == null || readonly">
                    <icon-component>clear</icon-component>
                </btn-component>
            </tooltip-component>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TKBackendInputConfig } from '@generated/Models/Core/TKBackendInputConfig';
import TimespanInputComponent from '@components/Common/Basic/TimespanInputComponent.vue'
import { TKUIHint } from "@generated/Enums/Core/TKUIHint";

@Options({
    components: {
        TimespanInputComponent
    }
})
export default class ParameterInputTypeTimeSpanComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: false })
    isListItem!: boolean;

    @Prop({ required: true })
    config!: TKBackendInputConfig;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    localValue: string | null = '';
    
    created(): void {
        this.updateLocalValue();
    }
    
    setValueToNull(): void {
        this.localValue = this.isNullable ? null : '0:0:0';
    }

    get placeholderText(): string {
        return this.localValue == null ? this.nullName : "";
    }

    get nullName(): string {
        return this.config.NullName || 'null';
    }
    
    get isNullable(): boolean {
        return this.config.Nullable;
    }
    
    get clearTooltip(): string {
        return this.isNullable ? 'Sets value to null' : 'Clears value';
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

    validateValue(): void {
        if (this.localValue == null && this.config.UIHints.includes(TKUIHint.NotNull)) {
            this.localValue = "0:0:0";
        }
        if (this.localValue != null && !this.localValue.includes(':'))
        {
            this.localValue = this.isNullable ? null : "0:0:0";
        }
    }
}
</script>

<style scoped>
.editor {
  width: 100%;
  height: 200px;
  border: 1px solid #949494;
}
</style>
