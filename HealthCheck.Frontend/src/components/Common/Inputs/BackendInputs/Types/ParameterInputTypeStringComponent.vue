<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeStringComponent.vue -->
<template>
    <div>
        <v-layout>
            <v-flex :xs10="config.notNull" :xs12="!config.notNull">
                <v-text-field
                    v-if="!isTextArea"
                    class="pt-0"
                    v-model="localValue"
                    :placeholder="placeholderText"
                    required />
                <v-textarea
                    v-if="isTextArea"
                    class="pt-0"
                    v-model="localValue"
                    :placeholder="placeholderText"
                    required />
            </v-flex>

            <v-flex xs2
                :xs3="isListItem"
                class="text-sm-right"
                v-if="!config.notNull">
                <v-tooltip bottom>
                    <template v-slot:activator="{ on }">
                        <span v-on="on">
                            <v-btn flat icon color="primary" class="ma-0 pa-0"
                                @click="setValueToNull"
                                :disabled="localValue == null">
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
import BackendInputConfig from "../BackendInputConfig";

@Component({
    components: {
    }
})
export default class ParameterInputTypeStringComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: false })
    isListItem!: boolean;

    @Prop({ required: true })
    config!: BackendInputConfig;

    localValue: string | null = '';
    
    mounted(): void {
        this.updateLocalValue();
    }
    
    setValueToNull(): void {
        this.localValue = null;
    }

    get placeholderText(): string {
        return this.localValue == null ? "null" : "";
    }

    get isTextArea(): boolean {
        return this.config.flags.includes('TextArea');
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

    validateValue(): void {
        if (this.localValue == null && this.config.notNull) {
            this.localValue = "";
        }
    }
}
</script>

<style scoped>
</style>
