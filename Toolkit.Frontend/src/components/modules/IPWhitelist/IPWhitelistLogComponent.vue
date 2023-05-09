<!-- src/components/modules/IPWhitelist/IPWhitelistLogComponent.vue -->
<template>
    <div class="ip-whitelist-log">
        <btn-component @click="loadLog" :disabled="isLoading" color="primary">Refresh</btn-component>
        <btn-component @click="clearRequestLog" :disabled="isLoading" color="error">Clear</btn-component>

        <fetch-status-progress-component :status="dataLoadStatus" class="mt-2" />
        <text-field-component v-model:value="filterText" label="Filter" class="mb-3" />
        
        <h3 class="mt-2">Latest blocked requests ({{ blockedLog.length }})</h3>
        <div class="request-log-list">
            <div v-for="item in blockedLog"
                :key="`request-item-${id}-${item.Timestamp}-${item.Path}-${item.WasBlocked}`"
                class="request-log-item blocked">
                <div class="request-log-item__row">
                    <div class="request-log-item__timestamp">{{ formatDate(item.Timestamp) }}</div>
                    <div class="request-log-item__method">{{ item.Method }}</div>
                    <div class="request-log-item__path">{{ item.Path }}</div>
                </div>
                <div class="request-log-item__row">
                    <div class="request-log-item__ip">{{ item.IP }}</div>
                </div>
            </div>
        </div>
        
        <h3 class="mt-4">Latest allowed requests ({{ allowedLog.length }})</h3>
        <div class="request-log-list">
            <div v-for="item in allowedLog"
                :key="`request-item-${id}-${item.Timestamp}-${item.Path}-${item.WasBlocked}`"
                class="request-log-item allowed">
                <div class="request-log-item__row">
                    <div class="request-log-item__timestamp">{{ formatDate(item.Timestamp) }}</div>
                    <div class="request-log-item__method">{{ item.Method }}</div>
                    <div class="request-log-item__path">{{ item.Path }}</div>
                </div>
                <div class="request-log-item__row">
                    <div class="request-log-item__ip">{{ item.IP }}</div>
                    <div class="request-log-item__note">{{ item.Note }}</div>
                </div>
            </div>
        </div>
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
import FetchStatusProgressComponent from "@components/Common/Basic/FetchStatusProgressComponent.vue";
import DateUtils from "@util/DateUtils";

@Options({
    components: {
        BlockComponent,
        BtnComponent,
        TextareaComponent,
        TextFieldComponent,
        FetchStatusProgressComponent
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
    filterText: string = '';

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
        if (!confirm('Delete all log entries?')) return;
        this.service.ClearRequestLog(this.dataLoadStatus, {
            onSuccess: d => this.loadLog()
        });
    }

    gotoRule(rule: TKIPWhitelistRule): void {
        this.$emit('ruleSelected', rule);
    }
    
    public formatDate(val: string | Date): string | null {
        return DateUtils.FormatDate(val, 'dd.MM HH:mm:ss');;
    }

    itemMatchesFilter(item: TKIPWhitelistLogItem): boolean {
        if (this.filterText.trim().length == 0) return true;
        else return item.IP?.toLowerCase()?.includes(this.filterText.toLowerCase())
            || item.Path?.toLowerCase()?.includes(this.filterText.toLowerCase());
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get allowedLog(): Array<TKIPWhitelistLogItem> {
        if (!this.log) return [];
        else return this.log.filter(x => !x.WasBlocked && this.itemMatchesFilter(x));
    }

    get blockedLog(): Array<TKIPWhitelistLogItem> {
        if (!this.log) return [];
        else return this.log.filter(x => x.WasBlocked && this.itemMatchesFilter(x));
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
    .request-log-item {
        border-left: 4px solid;
        padding-left: 10px;
        margin-left: 2px;
        padding-top: 3px;
        padding-bottom: 3px;
        border-bottom: 1px solid var(--color--accent-darken1) !important;
        font-family: monospace;

        &.allowed {
            border-color: var(--color--success-lighten1);
        }
        &.blocked {
            border-color: var(--color--error-lighten4);
        }

        &__row {
            display: flex;
            flex-wrap: nowrap;
        }

        &__timestamp {
            width: 120px;
            font-weight: 600;
        }
        &__method {
            font-weight: 600;
            margin-right: 4px;
        }
        /* &__path { } */
        &__ip {
            min-width: 100px;
            margin-right: 20px;
            margin-top: 2px;
            font-size: 12px;
            color: var(--color--primary-base);
        }
        &__note {
            margin-top: 2px;
            font-size: 11px;
            color: var(--color--primary-base);
        }
    }
}
</style>
