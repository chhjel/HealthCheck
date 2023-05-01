<!-- src/components/modules/IPWhitelist/IPWhitelistPageComponent.vue -->
<template>
    <div class="ip-whitelist">
        // ToDo
        <hr>
        [Rules] [Config] [Log]
        <hr>
        Rule edit: inputs + option to allow for links that append current ip.
        <hr>
        <code>{{ wlconfig }}</code>
        <code>{{ rules }}</code>
        <code>{{ log }}</code>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import FilterableListComponent from '@components/Common/FilterableListComponent.vue';
import { FetchStatus } from '@services/abstractions/TKServiceBase';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import { StoreUtil } from "@util/StoreUtil";
import IPWhitelistService from "@services/IPWhitelistService";
import IdUtils from "@util/IdUtils";
import { TKIPWhitelistLogItem } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistLogItem";
import { TKIPWhitelistConfig } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistConfig";
import { TKIPWhitelistRule } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistRule";

@Options({
    components: {
        FilterableListComponent
    }
})
export default class IPWhitelistPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;

    // Service
    service: IPWhitelistService = new IPWhitelistService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    wlconfig: TKIPWhitelistConfig | null = null;
    rules: Array<TKIPWhitelistRule> = [];
    log: Array<TKIPWhitelistLogItem> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadConfig();
        this.loadRules();
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
    loadConfig(): void {
        this.service.GetConfig(this.dataLoadStatus, {
            onSuccess: (d) => this.wlconfig = d
        })
    }
    loadRules(): void {
        this.service.GetRules(this.dataLoadStatus, {
            onSuccess: (d) => this.rules = d
        })
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
.ip-whitelist {

}
</style>