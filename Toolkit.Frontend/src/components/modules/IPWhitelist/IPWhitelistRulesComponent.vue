<!-- src/components/modules/IPWhitelist/IPWhitelistPageComponent.vue -->
<template>
    <div class="ip-whitelist-rules">
        <div v-if="!rules || rules.length == 0">- No whitelist rules created yet -</div>
        <div v-else class="ip-whitelist-rules__list">
            <div v-for="rule in sortedRules" :key="`rule-${rule.Id}-${id}`"
                @click="onRuleClicked(rule)"
                class="ip-whitelist-rules__item hoverable-lift-light"
                :class="getRuleClasses(rule)">
                <div class="ip-whitelist-rules__item__left">
                    <icon-component>{{ getRuleIcon(rule) }}</icon-component>
                </div>
                <div class="ip-whitelist-rules__item__right">
                    <div class="ip-whitelist-rules__item__name">{{ rule.Name }}</div>
                    <div class="ip-whitelist-rules__item__meta" v-if="getRuleMetaText(rule) ">{{ getRuleMetaText(rule) }}</div>
                    <div class="ip-whitelist-rules__item__note" v-if="rule.Note"><p>{{ rule.Note }}</p></div>
                </div>
                <hr/>
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
import IconComponent from "@components/Common/Basic/IconComponent.vue";
import LinqUtils from "@util/LinqUtils";

@Options({
    components: {
        FilterableListComponent,
        IconComponent
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

    getRuleIcon(rule: TKIPWhitelistRule): string {
        if (!rule.Enabled) return 'do_not_disturb_on';
        else if (this.ruleIsExpired(rule)) return 'timer_off';
        else return 'playlist_add_check';
    }

    getRuleMetaText(rule: TKIPWhitelistRule): string {
        if (!rule.Enabled) return 'Disabled';
        else if (this.ruleIsExpired(rule)) return 'Expired';
        else return '';
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

    get sortedRules(): Array<TKIPWhitelistRule> {
        let sorted = this.rules.map(x => x);
        sorted.sort((a, b) =>  LinqUtils.SortByThenBy(a, b,
            x => (!x.Enabled || this.ruleIsExpired(x)) ? 0 : 1,
            x => x.Name,
            false, true
            ));
        return sorted;
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
        display: flex;
        align-items: center;
        margin-bottom: 10px;
        margin-top: 10px;
        border: 2px solid var(--color--accent-darken2);
        padding: 5px;
        min-height: 38px;

        &__left {
            width: 40px;
            padding-right: 5px;
            display: flex;
            justify-content: center;
            align-items: center;
        }
        &__right {
            flex: 1;
            max-width: calc(100% - 50px);
        }
        &__name {
            font-weight: 600;
        }
        &__meta {
            font-weight: 600;
            font-size: 12px;
            color: var(--color--error-darken1);
        }
        &__note {
            padding: 4px 0;
            p {
                margin: 0;
                font-size: 13px;
                white-space: nowrap;
                overflow: hidden;
                text-overflow: ellipsis;
            }
        }
    }
}
</style>
