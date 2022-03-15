<!-- src/components/profile/AccessTokenKillswitchDialog.vue -->
<template>
    <div>
        <dialog-component v-model:value="dialogOpen"
            @keydown.esc="closeDialog"
            scrollable
            max-width="800"
            content-class="root-profile-dialog">
            <card-component style="background-color: #f4f4f4">
                <toolbar-component class="elevation-0">
                    <v-toolbar-title>Delete currently used token</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <btn-component icon @click="closeDialog">
                        <icon-component>close</icon-component>
                    </btn-component>
                </toolbar-component>

                <v-divider></v-divider>
                
                <v-card-text>
                    <p>Delete the currently used token if needed.</p>
                    <p><b>This action is irreversible.</b></p>
                </v-card-text>

                <v-divider></v-divider>
                <v-card-actions >
                    <v-spacer></v-spacer>
                    <btn-component color="error"
                        :loading="loadStatus.inProgress"
                        :disabled="loadStatus.inProgress"
                        @click="killswitchToken()">Delete token</btn-component>
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
import AccessTokensService from "@services/AccessTokensService";
import { FetchStatus } from "@services/abstractions/HCServiceBase";
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
    get globalOptions(): HCFrontEndOptions {
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