<!-- src/components/profile/HealthCheckProfileDialogComponent.vue -->
<template>
    <div>
        <v-dialog v-model="dialogOpen"
            @keydown.esc="closeDialog"
            scrollable
            max-width="800"
            content-class="root-profile-dialog">
            <v-card style="background-color: #f4f4f4">
                <v-toolbar class="elevation-0">
                    <v-toolbar-title>Profile</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-btn icon @click="closeDialog">
                        <v-icon>close</v-icon>
                    </v-btn>
                </v-toolbar>

                <v-divider></v-divider>
                
                <v-card-text>
                    <health-check-profile-component />
                </v-card-text>

                <v-divider></v-divider>
                <v-card-actions >
                    <v-spacer></v-spacer>
                    <v-btn @click="closeDialog">Close</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import { HCFrontEndOptions } from "generated/Models/WebUI/HCFrontEndOptions";
import { HCIntegratedProfileConfig } from "generated/Models/WebUI/HCIntegratedProfileConfig";
import HealthCheckProfileComponent from 'components/profile/HealthCheckProfileComponent.vue';

@Component({
    components: {
        HealthCheckProfileComponent
    }
})
export default class HealthCheckProfileDialogComponent extends Vue
{
    @Prop({ required: true })
    value!: boolean;

    dialogOpen: boolean = this.value;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.dialogOpen = this.value;
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): HCFrontEndOptions {
        return this.$store.state.globalOptions;
    }

    get profileOptions(): HCIntegratedProfileConfig {
        return this.globalOptions.IntegratedProfileConfig;
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    closeDialog(): void {
        this.$emit('input', false);
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch("value")
    onValueChanged(): void
    {
        this.dialogOpen = this.value;
    }

    @Watch("dialogOpen")
    onDialogOpenChanged(): void
    {
        this.$emit('input', this.dialogOpen);
    }
}
</script>

<style scoped lang="scss">
</style>

<style lang="scss">
.root-profile-dialog {
    
}
</style>