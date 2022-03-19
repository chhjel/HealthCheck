<!-- src/components/NoPageAvailablePageComponent.vue -->
<template>
    <div>
        <div> <!-- PAGE-->
            <!-- CONTENT BEGIN -->
            <alert-component
                :value="true" color="info"
                icon="info" outline>
            It seems you do not have access to any of these pages.
            </alert-component>
            <!-- CONTENT END -->
        </div> <!-- /PAGE-->
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
    components: {
    }
})
export default class NoPageAvailablePageComponent extends Vue {
}
</script>

<style scoped>
</style>

<style>
</style>