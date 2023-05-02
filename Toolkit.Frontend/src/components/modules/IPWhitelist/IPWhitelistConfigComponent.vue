<!-- src/components/modules/IPWhitelist/IPWhitelistPageComponent.vue -->
<template>
    <div class="ip-whitelist-config">
        <BlockComponent>
            <div v-if="wlconfig">
                <textarea-component v-model:value="wlconfig.DefaultResponse"
                    label="Default blocked response content"
                    class="mb-3" />
                <text-field-component v-model:value="wlconfig.DefaultHttpStatusCode"
                    label="Default blocked response status code"
                    type="number"
                    class="mb-3" />
            </div>
            
            <btn-component @disabled="isLoading"
                color="primary" class="mt-2"
                @click="saveConfig">Save</btn-component>
        </BlockComponent>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { FetchStatus } from '@services/abstractions/TKServiceBase';
import { StoreUtil } from "@util/StoreUtil";
import IPWhitelistService from "@services/IPWhitelistService";
import IdUtils from "@util/IdUtils";
import ModuleConfig from "@models/Common/ModuleConfig";
import { TKIPWhitelistConfig } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistConfig";
import BlockComponent from "@components/Common/Basic/BlockComponent.vue";
import BtnComponent from "@components/Common/Basic/BtnComponent.vue";
import TextFieldComponent from "@components/Common/Basic/TextFieldComponent.vue";
import TextareaComponent from "@components/Common/Basic/TextareaComponent.vue";

@Options({
    components: {
        BlockComponent,
        BtnComponent,
        TextareaComponent,
        TextFieldComponent
    }
})
export default class IPWhitelistConfigComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    // Service
    service: IPWhitelistService = new IPWhitelistService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    wlconfig: TKIPWhitelistConfig | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadConfig();
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadConfig(): void {
        this.service.GetConfig(this.dataLoadStatus, {
            onSuccess: (d) => this.wlconfig = d
        })
    }

    saveConfig(): void {
        this.service.SaveConfig(this.wlconfig, this.dataLoadStatus);
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get isLoading(): boolean {
        return this.dataLoadStatus.inProgress;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
.ip-whitelist-config {

}
</style>