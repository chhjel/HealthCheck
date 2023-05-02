<!-- src/components/modules/IPWhitelist/IPWhitelistPageComponent.vue -->
<template>
    <div class="ip-whitelist">
        <tabs-component :labels="['Whitelist', 'Config', 'Log']" v-model:value="currentTab" class="mt-3" :disabled="isLoading">
            <template #Whitelist>
                <h2>Whitelist</h2>
                <code>{{ rules }}</code>
                <IPWhitelistRuleComponent :config="config" :rule="currentRule" v-if="currentRule"/>
            </template>
            <!-- <template #Bypass><h2>Bypass</h2></template> -->
            <template #Config>
                <h2>Config</h2>
                <IPWhitelistConfigComponent :config="config" />
            </template>
            <template #Log>
                <h2>Log</h2>
                <code>{{ log }}</code>
            </template>
        </tabs-component>
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
import { TKIPWhitelistRule } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistRule";
import IPWhitelistRuleComponent from "./IPWhitelistRuleComponent.vue";
import TabsComponent from "@components/Common/Basic/TabsComponent.vue";
import StringUtils from "@util/StringUtils";
import { RouteLocationNormalized } from "vue-router";
import IPWhitelistConfigComponent from "./IPWhitelistConfigComponent.vue";

@Options({
    components: {
        FilterableListComponent,
        IPWhitelistRuleComponent,
        IPWhitelistConfigComponent,
        TabsComponent
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
    rules: Array<TKIPWhitelistRule> = [];
    log: Array<TKIPWhitelistLogItem> = [];

    currentRule: TKIPWhitelistRule | null = null;
    currentTab: string = '';
    routeListener: Function | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadRules();
        this.loadLog();
        this.setInitialTab();
        this.routeListener = this.$router.afterEach((t, f, err) => this.onRouteChanged(t, f));

        this.currentRule = {
            Id: '00000000-0000-0000-0000-000000000000',
            Enabled: true,
            EnabledUntil: null,
            Ips: [],
            Name: 'New rule',
            Note: ''
        };

    }

    ////////////////
    //  METHODS  //
    //////////////
    setInitialTab(): void {
        const tabFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.tab) || null;
        this.currentTab = tabFromHash || 'Whitelist';
    }

    loadLog(): void {
        this.service.GetLog(this.dataLoadStatus, {
            onSuccess: (d) => this.log = d
        })
    }
    loadRules(): void {
        this.service.GetRules(this.dataLoadStatus, {
            onSuccess: (d) => this.rules = d
        })
    }

    updateUrl(): void {
        let routeParams: any = {};
        routeParams['tab'] = this.currentTab;

        if (this.currentTab == 'Whitelist' && this.currentRule != null && this.currentRule.Id != null)
        {
            routeParams['id'] = this.currentRule.Id;
        }

        this.$router.push({ name: this.config.Id, params: routeParams })
    }
    
    onRouteChanged(to: RouteLocationNormalized, from: RouteLocationNormalized): void {
        if (!to.path.toLowerCase().startsWith('/ipwhitelist/')) return;

        this.currentTab = StringUtils.stringOrFirstOfArray(to.params.tab) || 'Whitelist';

        const oldIdFromHash = StringUtils.stringOrFirstOfArray(from.params.id) || null;
        const newIdFromHash = StringUtils.stringOrFirstOfArray(to.params.id) || null;
        const idChanged = oldIdFromHash != newIdFromHash;

        if (idChanged)
        {
            // this.setActiveType(matchingStream, false);
        }
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
    @Watch("currentTab")
    onTabChanged(): void {
        if (this.$route.params.tab != this.currentTab) {
            this.updateUrl();
        }
    }
}
</script>

<style scoped lang="scss">
.ip-whitelist {
    h2 {
        margin-top: 10px;
        margin-bottom: 10px;
    }
}
</style>