<!-- src/components/modules/IPWhitelist/IPWhitelistPageComponent.vue -->
<template>
    <div class="ip-whitelist">
        <tabs-component :labels="['Whitelist', 'Test', 'Config', 'Log']" v-model:value="currentTab" class="mt-3" :disabled="isLoading">
            <template #Whitelist>
                <h2>Whitelist</h2>
                <div v-if="currentRule == null">
                    <btn-component @click="onNewRuleClicked" :disabled="isLoading">New rule</btn-component>
                    <IPWhitelistRulesComponent :config="config" :rules="rules" :loading="isLoading"
                        @ruleClicked="r => setCurrentRule(r)" />
                </div>
                <div v-if="currentRule">
                    <a href="#" @click.stop.prevent="onGoBackToRulesClicked" :disabled="isLoading">&lt;&lt;&lt; Back</a>
                    <IPWhitelistRuleComponent :config="config" :rule="currentRule" :loading="isLoading" class="mt-2" />
                    <btn-component @click="onSaveRuleClicked(currentRule)" :disabled="isLoading" color="primary">Save</btn-component>
                </div>
            </template>
            <template #Test>
                <h2>Test</h2>
                <IPWhitelistTestComponent :config="config" :loading="isLoading" @ruleSelected="onRuleSelectedInTest" />
            </template>
            <!-- <template #Bypass><h2>Bypass</h2></template> -->
            <template #Config>
                <h2>Config</h2>
                <IPWhitelistConfigComponent :config="config" :loading="isLoading" />
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
import IPWhitelistRulesComponent from "./IPWhitelistRulesComponent.vue";
import TabsComponent from "@components/Common/Basic/TabsComponent.vue";
import StringUtils from "@util/StringUtils";
import { RouteLocationNormalized } from "vue-router";
import IPWhitelistConfigComponent from "./IPWhitelistConfigComponent.vue";
import BtnComponent from "@components/Common/Basic/BtnComponent.vue";
import IPWhitelistTestComponent from "./IPWhitelistTestComponent.vue";

@Options({
    components: {
        FilterableListComponent,
        IPWhitelistRuleComponent,
        IPWhitelistRulesComponent,
        IPWhitelistConfigComponent,
        IPWhitelistTestComponent,
        TabsComponent,
        BtnComponent
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
        this.routeListener = this.$router.afterEach((t, f, err) => this.onRouteChanged(t, f));
    }

    ////////////////
    //  METHODS  //
    //////////////
    setInitialTab(): void {
        const tabFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.tab) || null;
        this.currentTab = tabFromHash || 'Whitelist';
        
        if (this.currentTab == 'Whitelist') {
            const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.id) || null;
            if (idFromHash && this.rules.some(r => r.Id == idFromHash)) {
                this.setCurrentRule(this.rules.find(r => r.Id == idFromHash));
            }
        }
    }

    loadLog(): void {
        this.service.GetLog(this.dataLoadStatus, {
            onSuccess: (d) => this.log = d
        })
    }
    loadRules(): void {
        this.service.GetRules(this.dataLoadStatus, {
            onSuccess: (d) => {
                this.rules = d;
                this.setInitialTab();
            }
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

        if (idChanged  && this.rules.some(r => r.Id == newIdFromHash)) {
            this.setCurrentRule(this.rules.find(r => r.Id == newIdFromHash), false);
        }
    }

    setCurrentRule(rule: TKIPWhitelistRule, updateUrl: boolean = true): void {
        if (this.currentRule?.Id == rule?.Id) return;
        
        this.currentRule = rule;

        if (updateUrl) this.updateUrl();
    }

    onNewRuleClicked(): void {
        this.setCurrentRule({
            Id: '00000000-0000-0000-0000-000000000000',
            Enabled: true,
            EnabledUntil: null,
            Ips: [],
            Name: 'New rule',
            Note: ''
        }, false);
    }

    onSaveRuleClicked(rule: TKIPWhitelistRule): void {
        this.service.SaveRule(rule, this.dataLoadStatus, {
            onSuccess: (d) => {
                this.rules.push(d);
                this.setCurrentRule(d);
            }
        });
    }

    onGoBackToRulesClicked(): void {
        this.setCurrentRule(null);
    }

    onRuleSelectedInTest(rule: TKIPWhitelistRule): void {
        this.currentTab = 'Whitelist';
        this.setCurrentRule(rule);
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