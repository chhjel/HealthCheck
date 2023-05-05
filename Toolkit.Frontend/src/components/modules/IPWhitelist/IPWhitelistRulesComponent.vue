<!-- src/components/modules/IPWhitelist/IPWhitelistPageComponent.vue -->
<template>
    <div class="ip-whitelist-rules">
        <div v-if="!rules || rules.length == 0">- No whitelist rules created yet -</div>
        <div v-else class="ip-whitelist-rules__list">
            <div v-for="rule in rules" :key="`rule-${rule.Id}-${id}`"
                @click="onRuleClicked(rule)"
                class="ip-whitelist-rules__item"
                :class="getRuleClasses(rule)">
                <div>{{ rule.Name }}</div>
                <div>{{ rule.Note }}</div>
                <div>Enabled: {{ rule.Enabled }}</div>
                <div>EnabledUntil: {{ rule.EnabledUntil }}</div>
                <hr/>
            </div>
        </div>
        <hr/>
        <div>todo: show note w/ ellipsis</div>
        <div>todo: show expired/when it expires</div>
        <div>todo: show as disabled if disabled or expired</div>
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
import ModuleConfig from "@models/Common/ModuleConfig";
import ValueUtils from "@util/ValueUtils";

@Options({
    components: {
        FilterableListComponent
    }
})
export default class IPWhitelistRuleComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    rules!: Array<TKIPWhitelistRule>;

    @Prop({ required: false, default: false })
    loading!: string | boolean;
    
    // Service
    service: IPWhitelistService = new IPWhitelistService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
    }

    ////////////////
    //  METHODS  //
    //////////////
    onRuleClicked(rule: TKIPWhitelistRule): void {
        this.$emit('ruleClicked', rule);
    }

    getRuleClasses(rule: TKIPWhitelistRule): any {
        let classes: any = {};
        classes['disabled'] = !rule.Enabled || this.ruleIsExpired(rule);
        return classes;
    }

    ruleIsExpired(rule: TKIPWhitelistRule): boolean {
        if (rule.EnabledUntil == null) return false;
        
        const expirationDate = new Date(rule.EnabledUntil);
        return expirationDate.getTime() < new Date().getTime();
    }

    ////////////////
    //  GETTERS  //
    //////////////
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
.ip-whitelist-rules {
    &__list {

    }
    &__item {
        cursor: pointer;
    }
}
</style>