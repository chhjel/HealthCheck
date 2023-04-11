<!-- src/components/profile/AccessTokenKillswitchDialog.vue -->
<template>
    <div>
        <dialog-component v-model:value="dialogOpen" max-width="800">
            <template #header>Delete currently used token</template>
            <template #footer>
                <btn-component color="error"
                    :loading="loadStatus.inProgress"
                    :disabled="loadStatus.inProgress"
                    @click="killswitchToken()">Delete token</btn-component>
                <btn-component color="secondary" @click="closeDialog">Close</btn-component>
            </template>

            <div>
                <p>Delete the currently used token if needed.</p>
                <p><b>This action is irreversible.</b></p>
            </div>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TKFrontEndOptions } from "@generated/Models/WebUI/TKFrontEndOptions";
import AccessTokensService from "@services/AccessTokensService";
import { FetchStatus } from "@services/abstractions/TKServiceBase";
import { StoreUtil } from "@util/StoreUtil";

@Options({
    components: {
    }
})
export default class AccessTokenKillswitchDialog extends Vue
{
    @Prop({ required: true })
    value!: boolean;

    service: AccessTokensService = new AccessTokensService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, '');
    loadStatus: FetchStatus = new FetchStatus();
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

    ////////////////
    //  METHODS  //
    //////////////
    closeDialog(): void {
        this.$emit('update:value', false);
    }

    killswitchToken(): void {
        this.service.KillswitchToken(this.loadStatus, {
            onSuccess: (d) => {
                if (d && d.success) {
                    alert('Token deleted successfully.');
                    window.location.reload();
                }
            }
        });
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

</style>