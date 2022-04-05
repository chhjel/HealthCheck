<!-- src/components/Common/FilterInputComponent.vue -->
<template>
    <div class="root">
        <text-field-component
            class="filter-input"
            v-model:value="content"
            @input="handleInput" ref="input"
            appendIcon="search" />
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
    components: {}
})
export default class FilterInputComponent extends Vue {
    @Prop({ required: true })
    value!: string;
    content: string = this.value;

    handleInput(): void {
      this.$emit('update:value', this.content);
    }
}
</script>

<style scoped>
.root {
    display: flex;
    align-items: center;
}
.filter-input {
    padding: 5px;
    flex: 1;
}
.icon {
    position: absolute;
    right: 0;
    top: 0;
}
.label {
    margin-left: 5px;
}
</style>