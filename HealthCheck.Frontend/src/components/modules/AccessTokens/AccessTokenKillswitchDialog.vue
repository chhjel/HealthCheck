<!-- src/components/profile/AccessTokenKillswitchDialog.vue -->
<template>
    <div>
        <v-dialog v-model="dialogOpen"
            @keydown.esc="closeDialog"
            scrollable
            max-width="800"
            content-class="root-profile-dialog">
            <v-card style="background-color: #f4f4f4">
                <v-toolbar class="elevation-0">
                    <v-toolbar-title>Delete currently used token</v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-btn icon @click="closeDialog">
                        <v-icon>close</v-icon>
                    </v-btn>
                </v-toolbar>

                <v-divider></v-divider>
                
                <v-card-text>
                    <p>Delete the currently used token if needed.</p>
                    <p><b>This action is irreversible.</b></p>
                </v-card-text>

                <v-divider></v-divider>
                <v-card-actions >
                    <v-spacer></v-spacer>
                    <v-btn color="error"
                        :loading="loadStatus.inProgress"
                        :disabled="loadStatus.inProgress"
                        @click="killswitchToken()">Delete token</v-btn>
                    <v-btn @click="closeDialog">Close</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import { HCFrontEndOptions } from "generated/Models/WebUI/HCFrontEndOptions";
import AccessTokensService from "services/AccessTokensService";
import { FetchStatus } from "services/abstractions/HCServiceBase";

@Component({
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
        return this.$store.state.globalOptions;
    }

    ////////////////
    //  METHODS  //
    //////////////
    closeDialog(): void {
        this.$emit('input', false);
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
        this.$emit('input', this.dialogOpen);
    }
}
</script>

<style scoped lang="scss">
</style>

<style lang="scss">

</style>