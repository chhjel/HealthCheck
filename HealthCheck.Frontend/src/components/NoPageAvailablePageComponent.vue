<!-- src/components/NoPageAvailablePageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex>
            <!-- CONTENT BEGIN -->
            <v-alert
                :value="true" color="info"
                icon="info" outline>
            It seems you do not have access to any of these pages.
            </v-alert>
            <!-- CONTENT END -->
        </v-flex>
        </v-layout>
        </v-container>
        </v-content>
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