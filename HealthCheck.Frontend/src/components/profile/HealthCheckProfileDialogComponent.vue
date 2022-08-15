<!-- src/components/profile/HealthCheckProfileDialogComponent.vue -->
<template>
    <div>
        <dialog-component v-model:value="dialogOpen" max-width="800">
            <template #header>Profile</template>
            <template #footer>
                <btn-component color="secondary" @click="closeDialog">Close</btn-component>
            </template>
            <health-check-profile-component />
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
