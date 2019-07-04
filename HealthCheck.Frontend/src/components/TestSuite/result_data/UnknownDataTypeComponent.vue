<!-- src/components/result_data/UnknownDataTypeComponent.vue -->
<template>
    <div>
        <v-alert
        :value="true"
        color="warning"
        icon="priority_high"
        outline
        >
            Unknown data type '{{data.Type}}'. No data component created for this type yet.
        </v-alert>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestResultDataDumpViewModel from "../../../models/TestResultDataDumpViewModel";

@Component({
    components: {
    }
})
export default class UnknownDataTypeComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;

    mounted(): void {
    }
}
</script>

<style scoped>
</style>
