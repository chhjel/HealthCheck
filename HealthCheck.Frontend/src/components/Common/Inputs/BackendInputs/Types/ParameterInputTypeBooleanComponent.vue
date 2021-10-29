<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeBooleanComponent.vue -->
<template>
    <div>
        <v-switch v-if="!isNullable"
            v-model="localValue" 
            :label="label"
            color="secondary"
            class="parameter-checkbox pt-0"
            :disabled="readonly"
        ></v-switch>

        <v-checkbox v-if="isNullable"
            v-model="nullableCheckboxState"
            :indeterminate="localValue == null" 
            :label="label"
            :disabled="readonly"
            @click="setNextState"
            color="secondary"
            class="parameter-checkbox pt-0"
        ></v-checkbox>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import { HCBackendInputConfig } from 'generated/Models/Core/HCBackendInputConfig';

@Component({
    components: {
    }
})
export default class ParameterInputTypeBooleanComponent extends Vue {
    @Prop({ required: true })
    value!: any;

    @Prop({ required: true })
    config!: HCBackendInputConfig;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    localValue: any = false;
    nullableCheckboxState: boolean = false;
    
    mounted(): void {
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

    setNextState(): void {
        if (this.readonly)
        {
            return;
        }
        
        if (this.localValue == null) {
            this.localValue = "true";
        } else if (this.isTrue) {
            this.localValue = "false";
        } else {
            this.localValue = null
        }
        this.updateCheckboxState();
    }

    updateCheckboxState(): void {
        this.nullableCheckboxState = (this.localValue == null) ? false : this.isTrue;
    }

    valueIsTrue(value: any): boolean {
        return (typeof value === "boolean" && value)
            || (typeof value === "string" && value.toLowerCase() == "true");
    }

    get label(): string {
        if (this.isNullable && this.localValue == null)
        {
            return "null";
        }
        return this.isTrue ? "Yes" : "No";
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
        this.updateCheckboxState();
    }

    @Watch('localValue')
    onLocalValueChanged(): void
    {
        this.validateValue();
        this.$emit('input', this.localValue);
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
        margin-top: 24px;
        margin-left: 8px;
    }
}
</style>
