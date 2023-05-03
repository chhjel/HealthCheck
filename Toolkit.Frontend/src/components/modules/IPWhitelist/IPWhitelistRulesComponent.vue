<!-- src/components/modules/IPWhitelist/IPWhitelistPageComponent.vue -->
<template>
    <div class="ip-whitelist-rules">
        <div v-if="!rules || rules.length == 0">- No whitelist rules created yet -</div>
        <div v-else>
            <div v-for="rule in rules" :key="`rule-${rule.Id}-${id}`"
                @click="onRuleClicked(rule)">
                <div>{{ rule.Name }}</div>
                <code>{{ rule }}</code>
            </div>
        </div>
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

}
</style>