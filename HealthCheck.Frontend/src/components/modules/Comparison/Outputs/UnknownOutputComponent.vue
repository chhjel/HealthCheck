<!-- src/components/modules/Comparison/Outputs/UnknownOutputComponent.vue -->
<template>
    <div>
        <code>ERR: Unknown diff output type.</code>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import DiffComponent from "@components/Common/DiffComponent.vue";

@Options({
    components: {
        DiffComponent
    }
})
export default class UnknownOutputComponent extends Vue {
    @Prop({ required: true })
    resultData!: any;
}
</script>

<style scoped lang="scss">
</style>
