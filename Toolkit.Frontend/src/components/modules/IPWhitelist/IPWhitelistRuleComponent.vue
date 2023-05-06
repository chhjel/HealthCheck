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
                description="Internal note only shown in this interface."
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
                :clearable="true"
                placeholder="No expiration"
                class="mb-3"/>
        </block-component>

        <block-component class="mt-4 ip-list" title="IP Addresses">
            <div v-if="isNewUnsaved">IPs can be added after the rule has been saved.</div>
            <div v-else class="mt-3">
                <ul>
                    <li v-for="(ip, x) in ips" :key="`ip-${id}-${ip.Id}-${x}`">
                        <code>{{ ip.IP }}</code>
                        <small v-if="ip.Note">{{ ip.Note }}</small>
                        <btn-component @click="onRemoveIPClicked(ip)" :disabled="isLoading" color="error" class="ml-3" small>Remove</btn-component>
                    </li>
                </ul>
                <div v-if="!ips || ips.length == 0">- no addresses added yet -</div>
                <btn-component @click="showAddIPDialog" :disabled="isLoading" color="primary">Add new IP</btn-component>
                <btn-component @click="cidrTestDialogVisible = true" :disabled="isLoading" color="secondary">CIDR test</btn-component>
            </div>
        </block-component>

        <block-component class="mt-4" title="Links">
            <div class="ip-whitelist-rule__links">
                <div v-for="link in sortedLinks" :key="`link-${id}-${link.Id}`"
                    @click="showAddLinkDialog(link)"
                    class="ip-whitelist-rule__link hoverable-lift-light"
                    :class="getLinkClasses(link)">
                    <div class="ip-whitelist-rule__link__name">{{ link.Name }}</div>
                    <div class="ip-whitelist-rule__link__note" v-if="link.Note"><p>{{ link.Note }}</p></div>
                </div>
            </div>
            
            <div v-if="isNewUnsaved">Links can be added after the rule has been saved.</div>
            <div v-else class="mt-3">
                <btn-component @click="showAddLinkDialog(null)" :disabled="isLoading" color="primary">Add new link</btn-component>
            </div>
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
                <FeedbackComponent ref="saveIpFeedback" />
            </template>
            <div>
                <p>Add any number of addresses, one per line. Supports IPv4 CIDR notation.</p>
                <editor-component
                    class="editor"
                    :language="'text'"
                    v-model:value="newIpAddressInputValue"
                    :read-only="isLoading"
                    ref="editor" />
                
                <select-component
                    label="Add recent ip"
                    placeholder="- select ip to add -"
                    noDataText="- no recent ips -"
                    v-model:value="selectedRecentIp"
                    :items="recentIps"
                    :disabled="isLoading"
                    allowInput
                    v-on:change="onRecentIpSelected"
                    class="mt-3">
                </select-component>
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
                <p>Check if a given IP matches a given IPv4 CIDR notation.</p>
                <text-field-component v-model:value="cidrTestIp"
                    label="IP" class="mb-3" :disabled="isLoading" />
                <text-field-component v-model:value="cidrTestCidr"
                    label="Rule IP/CIDR" class="mb-3" :disabled="isLoading" />
                <div class="mb-3">{{ cidrTestResult }}</div>
            </div>
        </dialog-component>
        
        <dialog-component v-model:value="addLinkDialogVisible"
            max-width="620"
            :persistent="dataLoadStatus.inProgress">
            <template #header>Add new link</template>
            <template #footer>
                <btn-component color="primary" :disabled="isLoading" :loading="isLoading"
                    @click="onSaveLinkClicked">Save</btn-component>
                <btn-component color="error" v-if="isLinkSaved" :disabled="isLoading" :loading="isLoading"
                    @click="onDeleteLinkClicked">Delete</btn-component>
                <btn-component color="secondary" :disabled="isLoading" :loading="isLoading"
                    @click="hideLinkDialog">Cancel</btn-component>
                <FeedbackComponent ref="saveLinkFeedback" />
            </template>
            <div v-if="currentLink">
                <p>Create a link that grants access to adding IPs to this rule.</p>
                <text-field-component v-model:value="currentLink.Name"
                    label="Name"
                    class="mb-3"
                    :disabled="isLoading" />
                <textarea-component v-model:value="currentLink.Note"
                    label="Note shown on link open"
                    class="mb-3"
                    :disabled="isLoading" />
                <text-field-component v-model:value="currentLink.Password"
                    label="Password (optional)"
                    description="Optional password required to use the link. Password can be seen here later."
                    class="mb-3"
                    :disabled="isLoading" />
                <input-header-component name="Link expires at" />
                <date-picker-component v-model:value="currentLink.InvitationExpiresAt" :disabled="isLoading" :clearable="true"
                    placeholder="No expiration"
                    class="mb-3"/>

                <h4>Link</h4>
                <div class="wl-link-wrapper">
                    <div class="wl-link">{{ currentLinkUrl }}</div>
                    <btn-component @click="copyLinkToClipboard" color="secondary" round small title="Copy to clipboard">
                        <icon-component>content_copy</icon-component>
                    </btn-component>
                </div>
                <FeedbackComponent ref="linkClipboardFeedback" reserve />
            </div>
        </dialog-component>
        
        <textarea style="display:none;" ref="copyValue" :value="currentLinkUrl" />
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
import FeedbackComponent from "@components/Common/Basic/FeedbackComponent.vue";
import IPWhitelistLinkUtils from "@util/IPWhitelist/IPWhitelistLinkUtils";
import { TKIPWhitelistIP } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistIP";
import SelectComponent from "@components/Common/Basic/SelectComponent.vue";
import { TKIPWhitelistLogItem } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistLogItem";
import LinqUtils from "@util/LinqUtils";
import ClipboardUtil from "@util/ClipboardUtil";
import IconComponent from "@components/Common/Basic/IconComponent.vue";

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
        EditorComponent,
        FeedbackComponent,
        SelectComponent,
        IconComponent
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
    @Ref() readonly saveLinkFeedback!: FeedbackComponent;
    @Ref() readonly saveIpFeedback!: FeedbackComponent;
    @Ref() readonly linkClipboardFeedback!: FeedbackComponent;

    // Service
    service: IPWhitelistService = new IPWhitelistService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    links: Array<TKIPWhitelistLink> = [];
    ips: Array<TKIPWhitelistIP> = [];
    addIpDialogVisible: boolean = false;
    newIpAddressInputValue: string = '';
    cidrTestDialogVisible: boolean = false;
    cidrTestIp: string = '';
    cidrTestCidr: string = '';
    cidrTestResult: string = '';
    addLinkDialogVisible: boolean = false;
    currentLink: TKIPWhitelistLink | null = null;
    selectedRecentIp: string = '';
    log: Array<TKIPWhitelistLogItem> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadLinks();
        this.loadIps();
        this.loadLog();
        
        this.refreshEditorSize();
        this.$nextTick(() => this.refreshEditorSize());
        setTimeout(() => {
            this.refreshEditorSize();
        }, 100);

        // this.showAddLinkDialog(null);
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

    loadIps(): void {
        if (!this.rule.Id || this.rule.Id == '00000000-0000-0000-0000-000000000000') return;
        this.service.GetRuleIPs(this.rule.Id, this.dataLoadStatus, {
            onSuccess: (d) => this.ips = d
        });
    }

    loadLog(): void {
        this.service.GetLog(this.dataLoadStatus, {
            onSuccess: (d) => this.log = d
        })
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

    get isNewUnsaved(): boolean {
        return this.rule.Id == '00000000-0000-0000-0000-000000000000';
    }

    get isLinkSaved(): boolean {
        return this.currentLink && this.currentLink.Id != '00000000-0000-0000-0000-000000000000';
    }

    get currentLinkUrl(): string {
        if (!this.currentLink) return '';
        else return IPWhitelistLinkUtils.getAbsoluteLinkUrl(this.globalOptions.EndpointBase, this.currentLink.RuleId, this.currentLink.Secret);
    }

    get recentIps(): Array<string> {
        let ips = this.log.map(x => x.IP);
        ips.sort();
        return Array.from(new Set(ips));
    }

    get sortedLinks(): Array<TKIPWhitelistLink> {
        let sorted = this.links.map(x => x);
        sorted.sort((a, b) =>  LinqUtils.SortByThenBy(a, b,
            x => this.linkIsExpired(x) ? 0 : 1,
            x => x.Name,
            false, true
            ));
        return sorted;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    showAddIPDialog(): void {
        this.addIpDialogVisible = true;
    }

    onAddIPClicked(): void {
        this.saveIpFeedback.show('Saving..');
        const ips = this.newIpAddressInputValue.split('\n')
            .map(x => x.replace(/\r/g, '').trim())
            .filter(x => x.length > 0);

        const items: Array<TKIPWhitelistIP> = ips.map(ip => ({
                Id: '00000000-0000-0000-0000-000000000000',
                RuleId: this.rule.Id,
                IP: ip,
                Note: ''
            }));
        
        this.service.StoreRuleIPs(items, this.dataLoadStatus, {
            onSuccess: d => {
                this.saveIpFeedback.show('Saved');
                d.forEach(ip => {
                    const index = this.ips.findIndex(x => x.Id == ip.Id);
                    if (index == -1) {
                        this.ips.push(ip);
                    } else {
                        this.ips[index] = ip;
                    }

                    if (!this.ips.some(x => x.Id.toLowerCase() == ip.Id.toLowerCase())) {
                        this.ips.push(ip);
                    }
                });
            }
        });

        this.newIpAddressInputValue = '';
        this.addIpDialogVisible = false;
    }

    onRemoveIPClicked(ip: TKIPWhitelistIP): void {
        if (!confirm(`Remove IP '${ip.IP}'?`)) return;
        this.service.DeleteRuleIP(ip.Id, this.dataLoadStatus, {
            onSuccess: d => this.ips = this.ips.filter(x => x.Id != ip.Id)
        });
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

    showAddLinkDialog(link: TKIPWhitelistLink | null): void {
        this.addLinkDialogVisible = true;
        this.currentLink = JSON.parse(JSON.stringify(link)) || {
            Id: '00000000-0000-0000-0000-000000000000',
            RuleId: this.rule.Id,
            Secret: IdUtils.generateId(),
            InvitationExpiresAt: null,
            Name: 'New link',
            Note: '',
            Password: ''
        };
    }

    hideLinkDialog(): void {
        this.currentLink = null;
        this.addLinkDialogVisible = false;
    }

    onSaveLinkClicked(): void {
        this.saveLinkFeedback.show('Saving..');
        this.service.StoreRuleLink(this.currentLink, this.dataLoadStatus, {
            onSuccess: (d) => {
                this.saveLinkFeedback.show('Saved');
                const index = this.links.findIndex(x => x.Id == d.Id);
                if (index == -1) {
                    this.links.push(d);
                } else {
                    this.links[index] = d;
                }
                this.currentLink = JSON.parse(JSON.stringify(d));
            }
        });
    }

    onDeleteLinkClicked(): void {
        if (!confirm(`Delete link '${this.currentLink.Name}'?`)) return;
        this.service.DeleteRuleLink(this.currentLink.Id, this.dataLoadStatus, {
            onSuccess: (d) => {
                this.links = this.links.filter(x => x.Id != this.currentLink.Id);
                this.hideLinkDialog();
            }
        });
    }

    onRecentIpSelected(ip: string): void {
        if (typeof ip != 'string') return;
        if (!this.newIpAddressInputValue.includes(ip)) {
            this.newIpAddressInputValue += "\n" + ip;
        }
    }

    getLinkClasses(link: TKIPWhitelistLink): any {
        let classes: any = {};
        classes['disabled'] = this.linkIsExpired(link);
        return classes;
    }

    linkIsExpired(link: TKIPWhitelistLink): boolean {
        if (link.InvitationExpiresAt == null) return false;
        
        const expirationDate = new Date(link.InvitationExpiresAt);
        return expirationDate.getTime() < new Date().getTime();
    }

    copyLinkToClipboard(): void {
        let copySourceElement = this.$refs.copyValue as HTMLTextAreaElement;
        const err = ClipboardUtil.putDataOnClipboard(copySourceElement);
        if (err) {
            this.linkClipboardFeedback.show('Something failed');
        } else {
            this.linkClipboardFeedback.show('Link put on clipboard', 3000);
        }
    }
}
</script>

<style scoped lang="scss">
.ip-whitelist-rule {
    .ip-list {
        code {
            color: var(--color--primary-darken1) !important;
        }
        small {
            margin-left: 13px;
            color: #565656;
            font-size: 13px;
        }
    }

    &__links {

    }
    &__link {
        cursor: pointer;
        margin-bottom: 5px;
        margin-top: 5px;
        border: 1px solid var(--color--accent-darken1);
        padding: 5px 10px;
        min-height: 30px;
        display: flex;
        flex-direction: column;
        justify-content: center;

        &__name {
            font-weight: 600;
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

.wl-link-wrapper {
    display: flex;
    align-items: center;
}

.wl-link {
    padding: 10px;
    font-family: monospace;
    font-weight: 600;
    margin-top: 5px;
    display: block;
    border: 2px solid #d1d1d1;
    background-color: #eee;
}

.editor {
    width: 100%;
    height: 400px;
}
</style>
