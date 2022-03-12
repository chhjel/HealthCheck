<!-- src/components/modules/TestSuite/result_data/data_types/TestResultHtmlDataComponent.vue -->
<template>
    <div>
        <shadow-root v-if="useShadowDom">
            <div v-html="data.Data"></div>
        </shadow-root>
        <div v-if="!useShadowDom" v-html="data.Data"></div>
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
export default class TestResultHtmlDataComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;

    useShadowDom: boolean = false;

    mounted(): void {
        this.useShadowDom = this.data && this.data.Flags && this.data.Flags.includes("ShadowDom");
    }
}
</script>

<style>
</style>
