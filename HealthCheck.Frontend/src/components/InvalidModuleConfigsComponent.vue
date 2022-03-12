<!-- src/components/InvalidModuleConfigsComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex>
            <!-- CONTENT BEGIN -->
            <v-alert
                v-for="(module, mindex) in invalidConfigs"
                :key="`invalid-module-${mindex}`"
                :value="true" color="error"
                icon="info" outline>

                <div v-if="module.LoadErrors.length > 0">
                    Module '{{ module.Name }}' has some configuration issues:
                    <ul>
                        <li
                            v-for="(err, errindex) in module.LoadErrors"
                            :key="`invalid-module-${mindex}-err-${errindex}`"
                        >
                            <code>{{ err }}</code>
                        </li>
                        <code v-if="module.LoadErrorStacktrace != null">{{ module.LoadErrorStacktrace }}</code>
                    </ul>
                </div>
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
import ModuleConfig from '@models/Common/ModuleConfig';

@Options({
    components: {
    }
})
export default class InvalidModuleConfigsComponent extends Vue {
    @Prop({ required: true })
    invalidConfigs!: Array<ModuleConfig>;

}
</script>

<style scoped>
</style>

<style>
</style>