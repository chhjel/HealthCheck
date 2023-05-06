<!-- src/components/modules/IPWhitelist/IPWhitelistTestComponent.vue -->
<template>
    <div class="ip-whitelist-test">
        <BlockComponent>
            <p>Test a given IP and url against the current whitelist rules.</p>

            <text-field-component v-model:value="ipToTest" label="IP address to check" class="mb-3" />
            <text-field-component v-model:value="pathToTest" label="Path and querystring to check" class="mb-3" />
            
            <btn-component @disabled="isLoading"
                color="primary" class="mt-2"
                @click="testIp">Test</btn-component>

            <div v-if="result" class="result">
                <div v-if="result.Blocked" class="blocked">
                    Request was blocked with status code '{{ result.HttpStatusCode }}'.
                </div>
                <div v-if="!result.Blocked" class="allowed">
                    <div>{{ result.AllowedReason }}</div>
                    <div v-if="result.AllowingRule">
                        <btn-component @click="gotoRule(result.AllowingRule)">{{ result.AllowingRule.Name }}</btn-component>
                    </div>
                </div>
            </div>
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
import BlockComponent from "@components/Common/Basic/BlockComponent.vue";
import BtnComponent from "@components/Common/Basic/BtnComponent.vue";
import TextFieldComponent from "@components/Common/Basic/TextFieldComponent.vue";
import TextareaComponent from "@components/Common/Basic/TextareaComponent.vue";
import ValueUtils from "@util/ValueUtils";
import { TKIPWhitelistCheckResult } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistCheckResult";
import { TKIPWhitelistRule } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistRule";

@Options({
    components: {
        BlockComponent,
        BtnComponent,
        TextareaComponent,
        TextFieldComponent
    }
})
export default class IPWhitelistTestComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: false, default: false })
    loading!: string | boolean;

    // Service
    service: IPWhitelistService = new IPWhitelistService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    ipToTest: string = '';
    pathToTest: string = '/';
    
    result: TKIPWhitelistCheckResult | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
    }

    ////////////////
    //  METHODS  //
    //////////////
    testIp(): void {
        this.service.IsRequestAllowed({
            RawIP: this.ipToTest,
            Path: this.pathToTest
        }, this.dataLoadStatus, {
            onSuccess: (d) => this.result = d
        });
    }

    gotoRule(rule: TKIPWhitelistRule): void {
        this.$emit('ruleSelected', rule);
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
.ip-whitelist-test {
    .result {
        font-weight: 600;
        .allowed, .blocked {
            margin-top: 30px;
            padding: 20px;
            display: inline-block;
        }

        .allowed {
            border: 2px solid var(--color--success-lighten2);
        }
        .blocked {
            border: 2px solid var(--color--error-lighten2);
        }
    }
}
</style>