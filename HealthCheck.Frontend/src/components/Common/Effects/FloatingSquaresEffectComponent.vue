<!-- src/components/Common/Effects/FloatingSquaresEffectComponent.vue -->
<template>
    <div class="floating-squares-effect">
        <ul class="bg-bubbles">
            <li></li>
            <li></li>
            <li></li>
            <li></li>
            <li></li>
            <li></li>
            <li></li>
            <li></li>
            <li></li>
            <li></li>
        </ul>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
	components: {}
})
export default class FloatingSquaresEffectComponent extends Vue {}
</script>

<style scoped lang="scss">
</style>
