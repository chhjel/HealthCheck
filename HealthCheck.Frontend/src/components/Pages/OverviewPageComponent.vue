<!-- src/components/Pages/OverviewPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
            <!-- CONTENT -->
            <v-container fluid fill-height class="content-root">
                <v-layout>
                    <v-flex>
                        stuff here
                    </v-flex>
                </v-layout>
            </v-container>
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/FrontEndOptionsViewModel';

@Component({
    components: {
    }
})
export default class OverviewPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////

    ////////////////
    //  METHODS  //
    //////////////
}
</script>

<style scoped>
</style>

<style>
</style>