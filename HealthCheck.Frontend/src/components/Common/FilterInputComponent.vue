<!-- src/components/Common/FilterInputComponent.vue -->
<template>
    <div class="root">
        <input
            class="filter-input" type="text"
            v-model="content"
            @input="handleInput" ref="input" />
        <v-icon class="icon">search</v-icon>
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
      this.$emit('input', this.content);
    }
}
</script>

<style scoped>
.root {
    display: flex;
    align-items: center;
}
.filter-input {
    border-bottom: 1px solid #e4e4e4;
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