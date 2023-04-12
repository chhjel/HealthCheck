<!-- src/components/NoPageAvailablePageComponent.vue -->
<template>
    <div>
        <div>
            
            <alert-component
                :value="true" color="info"
                icon="info" outline>
            It seems you do not have access to any of these pages.
            </alert-component>
            
        </div>
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