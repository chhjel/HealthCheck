<!-- src/components/modules/IPWhitelist/IPWhitelistLogComponent.vue -->
<template>
    <div class="ip-whitelist-log">
        <btn-component @click="clearRequestLog" :disabled="isLoading" color="error">Clear</btn-component>
        
        <h3>Latest blocked requests</h3>
        <code>{{ blockedLog }}</code>
        
        <h3>Latest allowed requests</h3>
        <code>{{ allowedLog }}</code>
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
import BlockComponent from "@components/Common/Basic/BlockComponent.vue";
import BtnComponent from "@components/Common/Basic/BtnComponent.vue";
import TextFieldComponent from "@components/Common/Basic/TextFieldComponent.vue";
import TextareaComponent from "@components/Common/Basic/TextareaComponent.vue";
import ValueUtils from "@util/ValueUtils";
import { TKIPWhitelistRule } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistRule";
import { TKIPWhitelistLogItem } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistLogItem";

@Options({
    components: {
        BlockComponent,
        BtnComponent,
        TextareaComponent,
        TextFieldComponent
    }
})
export default class IPWhitelistLogComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: false, default: false })
    loading!: string | boolean;

    // Service
    service: IPWhitelistService = new IPWhitelistService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    log: Array<TKIPWhitelistLogItem> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadLog();
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadLog(): void {
        this.service.GetLog(this.dataLoadStatus, {
            onSuccess: (d) => this.log = d
        })
    }

    clearRequestLog(): void {
        this.service.ClearRequestLog(this.dataLoadStatus, {
            onSuccess: d => this.loadLog()
        });
    }

    gotoRule(rule: TKIPWhitelistRule): void {
        this.$emit('ruleSelected', rule);
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get allowedLog(): Array<TKIPWhitelistLogItem> {
        if (!this.log) return [];
        else return this.log.filter(x => !x.WasBlocked);
    }

    get blockedLog(): Array<TKIPWhitelistLogItem> {
        if (!this.log) return [];
        else return this.log.filter(x => x.WasBlocked);
    }

    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get isLoading(): boolean {
        return this.dataLoadStatus.inProgress || ValueUtils.IsToggleTrue(this.loading);
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
.ip-whitelist-log {
}
</style>