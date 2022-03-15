<!-- src/components/profile/HealthCheckProfileDialogComponent.vue -->
<template>
    <div>
        <dialog-component v-model:value="dialogOpen"
            @keydown.esc="closeDialog"
            scrollable
            max-width="800"
            content-class="root-profile-dialog">
            <card-component style="background-color: #f4f4f4">
                <toolbar-component class="elevation-0">
                    <v-toolbar-title>Profile</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <btn-component icon @click="closeDialog">
                        <icon-component>close</icon-component>
                    </btn-component>
                </toolbar-component>

                <v-divider></v-divider>
                
                <v-card-text>
                    <health-check-profile-component />
                </v-card-text>

                <v-divider></v-divider>
                <v-card-actions >
                    <v-spacer></v-spacer>
                    <btn-component @click="closeDialog">Close</btn-component>
                </v-card-actions>
            </card-component>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { HCFrontEndOptions } from "@generated/Models/WebUI/HCFrontEndOptions";
import { HCIntegratedProfileConfig } from "@generated/Models/WebUI/HCIntegratedProfileConfig";
import HealthCheckProfileComponent from '@components/profile/HealthCheckProfileComponent.vue';
import { StoreUtil } from "@util/StoreUtil";

@Options({
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
        return StoreUtil.store.state.globalOptions;
    }

    get profileOptions(): HCIntegratedProfileConfig {
        return this.globalOptions.IntegratedProfileConfig;
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    closeDialog(): void {
        this.$emit('update:value', false);
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
        this.$emit('update:value', this.dialogOpen);
    }
}
</script>

<style scoped lang="scss">
</style>

<style lang="scss">
.root-profile-dialog {
    
}
</style>