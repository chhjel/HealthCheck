<!-- src/components/Common/Inputs/BackendInputs/UnknownBackendInputComponent.vue -->
<template>
    <div>
        Unknown parameter type '{{type}}'. No input component created for this type yet.<br />
        Default value {{valueName}} will be used.
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
    components: {
    }
})
export default class UnknownBackendInputComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    type!: string;

    mounted(): void {
        if (this.type.startsWith("Nullable<")) {
            this.$emit('input', null);
        }
    }

    get valueName(): string {
        if (this.value == null) return "null";
        else return `'${this.value}'`;
    }
}
</script>

<style scoped>
</style>
