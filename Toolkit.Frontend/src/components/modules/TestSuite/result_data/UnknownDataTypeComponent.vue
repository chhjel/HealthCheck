<!-- src/components/modules/TestSuite/result_data/UnknownDataTypeComponent.vue -->
<template>
    <div>
        <alert-component
        :value="true"
        color="warning"
        icon="priority_high"
        outline
        >
            Unknown data type '{{datax.Type}}'. No data component created for this type yet.
        </alert-component>
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
    datax!: TestResultDataDumpViewModel;

    mounted(): void {
    }
}
</script>

<style scoped>
</style>
