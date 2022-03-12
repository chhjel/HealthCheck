<!-- src/components/modules/TestSuite/result_data/UnknownDataTypeComponent.vue -->
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
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestResultDataDumpViewModel } from '@generated/Models/Core/TestResultDataDumpViewModel';

@Options({
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
