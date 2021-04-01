<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeGuidComponent.vue -->
<template>
    <div>
        <v-layout>
            <v-flex :xs10="isNullable" :xs12="!isNullable">
                <v-text-field
                    class="pt-0"
                    v-model="localValue"
                    :placeholder="placeholderText"
                    :disabled="readonly"
                    required />
            </v-flex>

            <v-flex xs2
                :xs3="isListItem"
                class="text-sm-right"
                v-if="isNullable">
                <v-tooltip bottom>
                    <template v-slot:activator="{ on }">
                        <span v-on="on">
                            <v-btn flat icon color="primary" class="ma-0 pa-0"
                                @click="setValueToNull"
                                :disabled="localValue == null || readonly">
                                <v-icon>clear</v-icon>
                            </v-btn>
                        </span>
                    </template>
                    <span>Sets value to null</span>
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
            return this.localValue == null || this.localValue.length == 0 ? "null" : "";
        }
        return (this.localValue == null || this.localValue.length == 0)
            ? "00000000-0000-0000-0000-000000000000" : "";
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
        this.$emit('input', this.localValue);
    }
}
</script>

<style scoped>
</style>
