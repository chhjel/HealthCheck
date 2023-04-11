<!-- src/components/profile/ToolkitProfileDialogComponent.vue -->
<template>
    <div>
        <dialog-component v-model:value="dialogOpen" max-width="800">
            <template #header>Profile</template>
            <template #footer>
                <btn-component color="secondary" @click="closeDialog">Close</btn-component>
            </template>
            <toolkit-profile-component />
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TKFrontEndOptions } from "@generated/Models/WebUI/TKFrontEndOptions";
import { TKIntegratedProfileConfig } from "@generated/Models/WebUI/TKIntegratedProfileConfig";
import ToolkitProfileComponent from '@components/profile/ToolkitProfileComponent.vue';
import { StoreUtil } from "@util/StoreUtil";

@Options({
    components: {
        ToolkitProfileComponent
    }
})
export default class ToolkitProfileDialogComponent extends Vue
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
    get globalOptions(): TKFrontEndOptions {
        return StoreUtil.store.state.globalOptions;
    }

    get profileOptions(): TKIntegratedProfileConfig {
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
