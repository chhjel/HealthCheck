<!-- src/components/modules/IPWhitelist/IPWhitelistPageComponent.vue -->
<template>
    <div class="ip-whitelist-rule">
        <block-component>
            <text-field-component v-model:value="rule.Name"
                label="Name"
                class="mb-3"
                :disabled="isLoading" />
            <textarea-component v-model:value="rule.Note"
                label="Note"
                class="mb-3"
                :disabled="isLoading" />
            <switch-component
                v-model:value="rule.Enabled"
                label="Rule enabled"
                falseLabel="Rule disabled"
                color="secondary"
                :disabled="isLoading"
                class="mb-3" />
            <input-header-component name="Disabled after" />
            <date-picker-component v-model:value="rule.EnabledUntil"
                :disabled="isLoading" 
                class="mb-3"/>
        </block-component>

        <block-component class="mt-4 ip-list" title="IP Addresses">
            <ul>
                <li v-for="(ip, x) in rule.Ips" :key="`ip-${id}-${ip}-${x}`">
                    <code>{{ ip }}</code>
                    <btn-component @click="onRemoveIPClicked(ip)" :disabled="isLoading" color="error" class="ml-3">Remove</btn-component>
                </li>
            </ul>
            <div v-if="!rule.Ips || rule.Ips.length == 0">- no addresses added yet -</div>
            <btn-component @click="showAddIPDialog" :disabled="isLoading" color="primary">Add new IP</btn-component>
            <btn-component @click="cidrTestDialogVisible = true" :disabled="isLoading" color="secondary">CIDR test</btn-component>
        </block-component>

        <block-component class="mt-4" title="Links">
            <code>{{ links }}</code>
        </block-component>
        
        <dialog-component v-model:value="addIpDialogVisible"
            max-width="620"
            :persistent="dataLoadStatus.inProgress">
            <template #header>Add new IP Address</template>
            <template #footer>
                <btn-component color="primary"
                    :disabled="isLoading || newIpAddressInputValue.trim().length == 0"
                    :loading="isLoading"
                    @click="onAddIPClicked">Add</btn-component>
                <btn-component color="secondary"
                    :disabled="isLoading"
                    :loading="isLoading"
                    @click="addIpDialogVisible = false">Cancel</btn-component>
            </template>
            <div>
                <p>Add any number of addresses, one per line. Supports CIDR format.</p>
                <editor-component
                    class="editor"
                    :language="'text'"
                    v-model:value="newIpAddressInputValue"
                    :read-only="isLoading"
                    ref="editor" />
            </div>
        </dialog-component>
        
        <dialog-component v-model:value="cidrTestDialogVisible"
            max-width="620"
            :persistent="dataLoadStatus.inProgress">
            <template #header>CIDR test</template>
            <template #footer>
                <btn-component color="primary" :disabled="isLoading" :loading="isLoading" @click="onCidrTestClicked">Test</btn-component>
                <btn-component color="secondary" :disabled="isLoading" :loading="isLoading" @click="cidrTestDialogVisible = false">Close</btn-component>
            </template>
            <div>
                <text-field-component v-model:value="cidrTestIp"
                    label="IP" class="mb-3" :disabled="isLoading" />
                <text-field-component v-model:value="cidrTestCidr"
                    label="Rule IP/CIDR" class="mb-3" :disabled="isLoading" />
                <div class="mb-3">{{ cidrTestResult }}</div>
            </div>
        </dialog-component>
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
import { TKIPWhitelistRule } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistRule";
import { TKIPWhitelistLink } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistLink";
import ModuleConfig from "@models/Common/ModuleConfig";
import ValueUtils from "@util/ValueUtils";
import BtnComponent from "@components/Common/Basic/BtnComponent.vue";
import SwitchComponent from "@components/Common/Basic/SwitchComponent.vue";
import CheckboxComponent from "@components/Common/Basic/CheckboxComponent.vue";
import TextFieldComponent from "@components/Common/Basic/TextFieldComponent.vue";
import TextareaComponent from "@components/Common/Basic/TextareaComponent.vue";
import BlockComponent from "@components/Common/Basic/BlockComponent.vue";
import DatePickerComponent from "@components/Common/Basic/DatePickerComponent.vue";
import InputHeaderComponent from "@components/Common/Basic/InputHeaderComponent.vue";
import DialogComponent from "@components/Common/Basic/DialogComponent.vue";
import EditorComponent from "@components/Common/EditorComponent.vue";

@Options({
    components: {
        BlockComponent,
        BtnComponent,
        SwitchComponent,
        CheckboxComponent,
        TextFieldComponent,
        TextareaComponent,
        DatePickerComponent,
        InputHeaderComponent,
        DialogComponent,
        EditorComponent
    }
})
export default class IPWhitelistRuleComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    rule!: TKIPWhitelistRule;
    
    @Prop({ required: false, default: false })
    loading!: string | boolean;

    @Ref() readonly editor!: EditorComponent;

    // Service
    service: IPWhitelistService = new IPWhitelistService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    links: Array<TKIPWhitelistLink> = [];
    addIpDialogVisible: boolean = false;
    newIpAddressInputValue: string = '';
    cidrTestDialogVisible: boolean = false;
    cidrTestIp: string = '';
    cidrTestCidr: string = '';
    cidrTestResult: string = '';

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadLinks();
        
        this.refreshEditorSize();
        this.$nextTick(() => this.refreshEditorSize());
        setTimeout(() => {
            this.refreshEditorSize();
        }, 100);
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadLinks(): void {
        if (!this.rule.Id || this.rule.Id == '00000000-0000-0000-0000-000000000000') return;
        this.service.GetRuleLinks(this.rule.Id, this.dataLoadStatus, {
            onSuccess: (d) => this.links = d
        });
    }

    refreshEditorSize(): void {
        if (this.editor)
        {
            this.editor.refreshSize();
        }
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
    showAddIPDialog(): void {
        this.addIpDialogVisible = true;
    }

    onAddIPClicked(): void {
        const ips = this.newIpAddressInputValue.split('\n')
            .map(x => x.replace(/\r/g, '').trim())
            .filter(x => x.length > 0);

        ips.forEach(ip => {
            if (!this.rule.Ips.some(x => x.toLowerCase() == ip.toLowerCase())) {
                this.rule.Ips.push(ip);
            }
        });

        this.newIpAddressInputValue = '';
        this.addIpDialogVisible = false;
    }

    onRemoveIPClicked(ip: string): void {
        if (!confirm(`Remove IP '${ip}'?`)) return;
        this.rule.Ips = this.rule.Ips.filter(x => x != ip);
    }

    onCidrTestClicked(): void {
        this.cidrTestResult = 'Checking..';
        this.service.IpMatchesCidr({
                IP: this.cidrTestIp,
                IPWithOptionalCidr: this.cidrTestCidr
            }, this.dataLoadStatus, {
                onSuccess: matches => this.cidrTestResult = matches ? 'Matches!' : 'Does not match.'
            });
    }
}
</script>

<style scoped lang="scss">
.ip-whitelist-rule {
    .ip-list {
        code {
            color: var(--color--primary-darken1) !important;
        }
    }
}

.editor {
    width: 100%;
    height: 400px;
}
</style>
