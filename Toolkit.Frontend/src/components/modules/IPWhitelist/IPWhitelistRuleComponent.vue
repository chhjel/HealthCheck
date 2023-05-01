<!-- src/components/modules/IPWhitelist/IPWhitelistPageComponent.vue -->
<template>
    <div class="ip-whitelist-rule">
        <code>{{ rule }}</code>
        <hr>
        <code>{{ links }}</code>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import FilterableListComponent from '@components/Common/FilterableListComponent.vue';
import { FetchStatus } from '@services/abstractions/TKServiceBase';
import { StoreUtil } from "@util/StoreUtil";
import IPWhitelistService from "@services/IPWhitelistService";
import IdUtils from "@util/IdUtils";
import { TKIPWhitelistRule } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistRule";
import { TKIPWhitelistLink } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistLink";
import ModuleConfig from "@models/Common/ModuleConfig";

@Options({
    components: {
        FilterableListComponent
    }
})
export default class IPWhitelistRuleComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    rule!: TKIPWhitelistRule;
    
    // Service
    service: IPWhitelistService = new IPWhitelistService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    links: Array<TKIPWhitelistLink> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadLinks();
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadLinks(): void {
        if (!this.rule.Id || this.rule.Id == '00000000-0000-0000-0000-000000000000') return;
        this.service.GetRuleLinks(this.rule.Id, this.dataLoadStatus, {
            onSuccess: (d) => this.links = d
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
.ip-whitelist-rule {

}
</style>