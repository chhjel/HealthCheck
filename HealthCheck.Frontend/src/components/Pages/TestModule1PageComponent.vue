<!-- src/components/Pages/TestModule1PageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            
        <v-container grid-list-md>
            Success :-)
        </v-container>

          <!-- CONTENT END -->
        </v-flex>
        </v-layout>
        </v-container>
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Page/FrontEndOptionsViewModel';

@Component({
    components: {
    }
})
export default class TestModule1PageComponent extends Vue {
    @Prop({ required: true })
    globalOptions!: FrontEndOptionsViewModel;

    @Prop({ required: true })
    options!: any;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        console.log({
            Global: this.globalOptions,
            Local: this.options
        });
    }
}
</script>

<style scoped>
</style>

<style>
</style>