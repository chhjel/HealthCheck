<!-- src/components/paremeter_inputs/input_types/ParameterInputTypeStringComponent.vue -->
<template>
    <div>
        <v-layout>
            <v-flex :xs10="parameter.NotNull" :xs12="!parameter.NotNull">
                <v-text-field
                    class="pt-0"
                    v-model="parameter.Value"
                    :placeholder="placeholderText"
                    required />
            </v-flex>

            <v-flex xs2 class="text-sm-right" v-if="!parameter.NotNull">
                <v-tooltip bottom>
                    <template v-slot:activator="{ on }">
                        <span v-on="on">
                            <v-btn flat icon color="primary" class="ma-0 pa-0"
                                @click="setValueToNull"
                                :disabled="parameter.Value == null">
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
import { Vue, Component, Prop } from "vue-property-decorator";
import TestParameterViewModel from '../../../../models/TestSuite/TestParameterViewModel';

@Component({
    components: {
    }
})
export default class ParameterInputTypeStringComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;
    
    mounted(): void {
        if (this.parameter.Value == null && this.parameter.NotNull) {
            this.parameter.Value = "";
        }
    }

    setValueToNull(): void {
        this.parameter.Value = null;
    }

    get placeholderText(): string {
        return this.parameter.Value == null ? "null" : "";
    }
}
</script>

<style scoped>
</style>
