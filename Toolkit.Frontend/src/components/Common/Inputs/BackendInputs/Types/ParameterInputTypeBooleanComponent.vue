<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeBooleanComponent.vue -->
<template>
    <div>
        <switch-component v-if="!isNullable"
            v-model:value="localValue" 
            :label="label"
            color="primary"
            class="parameter-checkbox pt-0"
            :disabled="readonly"
        ></switch-component>

        <checkbox-component v-if="isNullable"
            v-model:value="localValue"
            :allowIndeterminate="true" 
            :label="label"
            :disabled="readonly"
            color="primary"
            class="parameter-checkbox pt-0"
        ></checkbox-component>
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
export default class ParameterInputTypeBooleanComponent extends Vue {
    @Prop({ required: true })
    value!: any;

    @Prop({ required: true })
    config!: TKBackendInputConfig;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    localValue: any = false;
    
    created(): void {
        this.$nextTick(() => {
            this.updateLocalValue();
            this.onLocalValueChanged();
        });
    }

    validateValue(): void {
        if (this.isNullable)
        {
            if (this.localValue == null || this.localValue === '') {
                this.localValue = null;
            }
            else {
                this.localValue = this.valueIsTrue(this.localValue);
            }
        }
        else
        {
            this.localValue = this.valueIsTrue(this.localValue);
        }
    }

    valueIsTrue(value: any): boolean {
        return (typeof value === "boolean" && value)
            || (typeof value === "string" && value.toLowerCase() == "true");
    }

    get label(): string {
        if (this.isNullable && this.localValue == null)
        {
            return this.nullName;
        }
        return this.isTrue ? "Yes" : "No";
    }

    get nullName(): string {
        return this.config.NullName || 'null';
    }

    get isNullable(): boolean {
        return this.config.Nullable;
    }

    get isTrue(): boolean {
        return this.valueIsTrue(this.localValue);
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

<style lang="scss">
.parameter-checkbox label {
    color: #000 !important;
}
.parameter-checkbox {
    margin-top: 8px;
}
.parameter-list-input {
    .parameter-checkbox {
        margin-top: 0;
    }
}
</style>
