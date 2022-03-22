<!-- src/components/modules/EndpointControl/EndpointControlPageComponent.vue -->
<template>
    <div>
        <div> <!-- PAGE-->
            <div fluid fill-height class="content-root">
            <div>
            <div>
            <div>
                <h1 class="mb-1">Endpoint control rules</h1>

                <!-- LOAD PROGRESS -->
                <progress-linear-component
                    v-if="loadStatus.inProgress"
                    indeterminate color="green"></progress-linear-component>

                <!-- DATA LOAD ERROR -->
                <alert-component :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
                </alert-component>

                <btn-component :disabled="!allowRuleChanges"
                    @click="onAddNewRuleClicked"
                    class="mb-3">
                    <icon-component size="20px" class="mr-2">add</icon-component>
                    Add new
                </btn-component>

                <btn-component v-if="HasAccessToEditEndpointDefinitions"
                    @click="editDefinitionsDialogVisible = true"
                    class="mb-3 ml-2 right">
                    Edit endpoint definitions
                </btn-component>

                <btn-component v-if="HasAccessToLatestRequestsDialog"
                    @click="showLatestRequestsDialog"
                    class="mb-3 ml-2 right">
                    Latest requests
                </btn-component>

                <block-component
                    v-for="(rule, cindex) in rules"
                    :key="`rule-${cindex}-${rule.Id}`"
                    class="rule-list-item"
                    >
                    <div class="rule-list-item--inner">
                        <tooltip-component tooltip="Enable or disable this rule">
                            <switch-component
                                v-model:value="rule.Enabled"
                                color="secondary"
                                style="flex: 0"
                                @click="setRuleEnabled(rule, !rule.Enabled)"
                                :disabled="!HasAccessToEditRules"
                                ></switch-component>
                        </tooltip-component>
                        
                        <div class="rule-list-item--rule"
                            @click="showRule(rule)">
                            <rule-description-component 
                                :rule="rule" 
                                :endpointDefinitions="EndpointDefinitions"
                                :customResultDefinitions="CustomResultDefinitions" />
                        </div>
                        
                        <tooltip-component v-if="getRuleWarning(rule) != null" :tooltip="getRuleWarning(rule)">
                            <icon-component style="cursor: help;" color="warning">warning</icon-component>
                        </tooltip-component>

                        <tooltip-component v-if="ruleIsOutsideLimit(rule)" tooltip="This rules' limits has been reached">
                            <icon-component style="cursor: help;">timer_off</icon-component>
                        </tooltip-component>

                        <tooltip-component :tooltip="`Last modified by '${rule.LastChangedBy}'`">
                            <icon-component style="cursor: help;">person</icon-component>
                            <code style="color: var(--v-primary-base); cursor: help;">{{ rule.LastChangedBy }}</code>
                        </tooltip-component>
                    </div>
                </block-component>

            </div>
            </div>
            </div>
            </div>
            
            <dialog-component v-model:value="ruleDialogVisible"
                scrollable
                persistent
                max-width="1200"
                content-class="current-rule-dialog">
                <card-component v-if="currentRule != null">
                    <toolbar-component>
                        <div class="current-rule-dialog__title">{{ currentDialogTitle }}</div>
                                                <btn-component icon
                            @click="hideCurrentRule()"
                            :disabled="serverInteractionInProgress">
                            <icon-component>close</icon-component>
                        </btn-component>
                    </toolbar-component>

                                        
                    <div>
                        <rule-component
                            :module-id="config.Id"
                            :rule="currentRule"
                            :endpointDefinitions="EndpointDefinitions"
                            :readonly="!allowRuleChanges"
                            :customResultDefinitions="datax.CustomResultDefinitions"
                            v-on:ruleDeleted="onRuleDeleted"
                            v-on:ruleSaved="onRuleSaved"
                            v-on:serverInteractionInProgress="setServerInteractionInProgress"
                            ref="currentRuleComponent"
                            />
                    </div>

                                        <div >
                                                <btn-component color="error" flat
                            v-if="showDeleteRule"
                            :disabled="serverInteractionInProgress"
                            @click="$refs.currentRuleComponent.tryDeleteRule()">Delete</btn-component>
                        <btn-component color="success"
                            :disabled="serverInteractionInProgress || !HasAccessToEditRules"
                            @click="$refs.currentRuleComponent.saveRule()">Save</btn-component>
                    </div>
                </card-component>
            </dialog-component>

            <dialog-component v-model:value="deleteDefinitionDialogVisible"
                @keydown.esc="deleteDefinitionDialogVisible = false"
                max-width="290"
                content-class="confirm-dialog">
                <card-component>
                    <div class="headline">Confirm deletion</div>
                    <div>
                        {{ deleteDefinitionDialogText }}
                    </div>
                                        <div>
                                                <btn-component color="secondary" @click="deleteDefinitionDialogVisible = false">Cancel</btn-component>
                        <btn-component color="error"
                            :loading="loadStatus.inProgress"
                            :disabled="loadStatus.inProgress"
                            @click="confirmDeleteEndpointDefinition()">Delete</btn-component>
                    </div>
                </card-component>
            </dialog-component>
            
            <dialog-component v-model:value="editDefinitionsDialogVisible"
                @keydown.esc="editDefinitionsDialogVisible = false"
                scrollable
                max-width="1200"
                content-class="current-rule-dialog">
                <card-component>
                    <toolbar-component>
                        <div class="current-rule-dialog__title">Edit endpoint definitions</div>
                                                <btn-component icon
                            @click="editDefinitionsDialogVisible = false">
                            <icon-component>close</icon-component>
                        </btn-component>
                    </toolbar-component>

                                        
                    <div>
                        <block-component
                            v-for="(def, dindex) in EndpointDefinitions"
                            :key="`endpointdef-${dindex}-${def.EndpointId}`"
                            class="definition-list-item mb-2">
                            <btn-component
                                :loading="loadStatus.inProgress"
                                :disabled="loadStatus.inProgress"
                                color="error" class="right"
                                @click="showDeleteDefinitionDialog(def.EndpointId)">
                                <icon-component size="20px" class="mr-2">delete</icon-component>
                                Delete
                            </btn-component>

                            <h3>{{ getEndpointDisplayName(def.EndpointId) }}</h3>
                            <div style="clear:both;"></div>
                        </block-component>
                    </div>
                                        <div >
                                                <btn-component
                            :loading="loadStatus.inProgress"
                            :disabled="loadStatus.inProgress"
                            color="error"
                            @click="showDeleteDefinitionDialog(null)">
                            <icon-component size="20px" class="mr-2">delete_forever</icon-component>
                            Delete all definitions
                        </btn-component>
                        <btn-component color="success"
                            @click="editDefinitionsDialogVisible = false">Close</btn-component>
                    </div>
                </card-component>
            </dialog-component>
            
            <dialog-component v-model:value="latestRequestsDialogVisible"
                @keydown.esc="hideLatestRequestsDialog"
                scrollable
                max-width="1200"
                content-class=""
                @input="v => v || hideLatestRequestsDialog()">
                <card-component>
                    <toolbar-component>
                        <div class="current-rule-dialog__title">Latest requests</div>
                                                <btn-component icon @click="hideLatestRequestsDialog">
                            <icon-component>close</icon-component>
                        </btn-component>
                    </toolbar-component>

                                        <div>
                        <block-component class="mb-2">
                            <latest-requests-component 
                                :moduleId="config.Id"
                                :endpointDefinitions="EndpointDefinitions"
                                :options="options.Options"
                                :log="HasAccessToViewLatestRequestData"
                                :charts="HasAccessToViewRequestCharts"
                                />
                        </block-component>
                    </div>
                                        <div >
                                                <btn-component color="success" @click="hideLatestRequestsDialog">Close</btn-component>
                    </div>
                </card-component>
            </dialog-component>
        </div> <!-- /PAGE-->
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Provide } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import LoggedEndpointDefinitionViewModel from '@models/modules/RequestLog/LoggedEndpointDefinitionViewModel';
import LoggedEndpointRequestViewModel from '@models/modules/RequestLog/LoggedEndpointRequestViewModel';
import { EntryState } from '@models/modules/RequestLog/EntryState';
import DateUtils from '@util/DateUtils';
import LinqUtils from '@util/LinqUtils';
import KeyArray from '@util/models/KeyArray';
import KeyValuePair from '@models/Common/KeyValuePair';
import '@lazy-copilot/datetimepicker/dist/datetimepicker.css'
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";
import FilterInputComponent from '@components/Common/FilterInputComponent.vue';
import DataTableComponent from '@components/Common/DataTableComponent.vue';
import { DataTableGroup } from '@components/Common/DataTableComponent.vue.models';
import SimpleDateTimeComponent from '@components/Common/SimpleDateTimeComponent.vue';
import RuleDescriptionComponent from '@components/modules/EndpointControl/RuleDescriptionComponent.vue';
import FilterableListComponent from '@components/Common/FilterableListComponent.vue';
import { FilterableListItem } from '@components/Common/FilterableListComponent.vue.models';
import RuleComponent from '@components/modules/EndpointControl/RuleComponent.vue';
import IdUtils from '@util/IdUtils';
import EndpointControlUtils from '@util/EndpointControl/EndpointControlUtils';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import EndpointControlService from '@services/EndpointControlService';
import ModuleOptions from '@models/Common/ModuleOptions';
import ModuleConfig from '@models/Common/ModuleConfig';
import { EndpointControlCustomResultDefinitionViewModel, EndpointControlDataViewModel, EndpointControlEndpointDefinition, EndpointControlFilterMode, EndpointControlPropertyFilter, EndpointControlRule } from '@models/modules/EndpointControl/EndpointControlModels';
import LatestRequestsComponent from '@components/modules/EndpointControl/LatestRequestsComponent.vue';

import { ModuleFrontendOptions } from '@components/modules/EndpointControl/EndpointControlPageComponent.vue.models';
import { StoreUtil } from "@util/StoreUtil";
import StringUtils from "@util/StringUtils";
@Options({
    components: {
        SimpleDateTimeComponent,
        BlockComponent,
        RuleComponent,
        RuleDescriptionComponent,
        LatestRequestsComponent
    }
})
export default class EndpointControlPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<ModuleFrontendOptions>;

    service: EndpointControlService = new EndpointControlService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);

    // UI STATE
    loadStatus: FetchStatus = new FetchStatus();
    serverInteractionInProgress: boolean = false;
    editDefinitionsDialogVisible: boolean = false;
    deleteDefinitionDialogVisible: boolean = false;
    latestRequestsDialogVisible: boolean = false;
    deleteDefinitionDialogText: string = "";
    endpointDefinitionIdToDelete: string | null = null;

    datax: EndpointControlDataViewModel | null = null;
    currentRule: EndpointControlRule | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.loadData();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get HasAccessToEditEndpointDefinitions(): boolean {
        return this.options.AccessOptions.indexOf("EditEndpointDefinitions") != -1;
    }
    get HasAccessToViewLatestRequestData(): boolean {
        return this.options.AccessOptions.indexOf("ViewLatestRequestData") != -1;
    }
    get HasAccessToViewRequestCharts(): boolean {
        return this.options.AccessOptions.indexOf("ViewRequestCharts") != -1;
    }
    get HasAccessToEditRules(): boolean {
        return this.options.AccessOptions.indexOf("EditRules") != -1;
    }
    get HasAccessToLatestRequestsDialog(): boolean {
        return this.HasAccessToViewLatestRequestData || this.HasAccessToViewRequestCharts;
    }
    
    get showDeleteRule(): boolean
    {
        return this.currentRule != null && this.currentRule.Id != null && this.HasAccessToEditRules;
    }

    get allowRuleChanges(): boolean
    {
        return !this.serverInteractionInProgress && this.HasAccessToEditRules;
    };

    get currentDialogTitle(): string
    {
        return (this.currentRule != null && this.currentRule.Id != null)
            ? 'Edit endpoint rule'
            : 'Create new endpoint rule';
    }

    get ruleDialogVisible(): boolean
    {
        return this.currentRule != null;
    }
    set ruleDialogVisible(v: boolean) {}

    get EndpointDefinitions(): Array<EndpointControlEndpointDefinition>
    {
        return (this.datax == null) ? [] : this.datax.EndpointDefinitions;
    };

    get CustomResultDefinitions(): Array<EndpointControlCustomResultDefinitionViewModel>
    {
        return (this.datax == null) ? [] : this.datax.CustomResultDefinitions;
    }

    get rules(): Array<EndpointControlRule>
    {
        let rules = (this.datax == null) ? [] : this.datax.Rules;
        return rules;
    };

    ////////////////
    //  METHODS  //
    //////////////
    getEndpointDisplayName(endpointId: string) : string {
        if (this.datax == null) return endpointId;

        return EndpointControlUtils.getEndpointDisplayName(endpointId, this.datax.EndpointDefinitions);
    }

    updateUrl(): void {
        let routeParams: any = {};
        if (this.currentRule != null && this.currentRule.Id != null)
        {
            routeParams['id'] = this.currentRule.Id;
        }
        else if (this.latestRequestsDialogVisible)
        {
            routeParams['id'] = 'latest-requests';
        }

        this.$router.push({ name: this.config.Id, params: routeParams })
    }

    updateSelectionFromUrl(): void {
        const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.id) || null;
        
        if (idFromHash) {
            let ruleFromUrl = this.rules.filter(x => x.Id != null && x.Id == idFromHash)[0];
            if (ruleFromUrl != null)
            {
                this.showRule(ruleFromUrl, false);
            }
            else if (idFromHash == 'latest-requests' && this.HasAccessToLatestRequestsDialog)
            {
                this.latestRequestsDialogVisible = true;
            }
        }
    }

    loadData(): void {
        this.service.GetData(this.loadStatus, { onSuccess: (data) => this.onDataRetrieved(data) });
    }

    onDataRetrieved(data: EndpointControlDataViewModel): void {
        this.datax = data;
        this.datax.Rules.forEach(rule => {
            EndpointControlUtils.postProcessRule(rule);
        });

        this.updateSelectionFromUrl();
        
        this.datax.Rules = this.datax.Rules.sort(
            (a, b) => LinqUtils.SortByThenBy(a, b,
                x => x.Enabled ? 1 : 0,
                x => (x.LastChangedAt == null) ? 32503676400000 : x.LastChangedAt.getTime(),
                false, false)
            );
    }

    setRuleEnabled(rule: EndpointControlRule, enabled: boolean): void {
        if (this.serverInteractionInProgress)
        {
            return;
        }

        this.serverInteractionInProgress = true;
        
        this.service.SetRuleEnabled(rule, enabled, this.loadStatus, {
            onSuccess: (data) => {
                if (data.Success == true) {
                    rule.Enabled = enabled;
                }
            },
            onDone: () => this.serverInteractionInProgress = false
        });
    }

    onRuleSaved(rule: EndpointControlRule): void {
        if (this.datax == null)
        {
            return;
        }
        EndpointControlUtils.postProcessRule(rule);

        const position = this.datax.Rules.findIndex(x => x.Id == rule.Id);
        //this.datax.Rules = this.datax.Rules.filter(x => x.Id != rule.Id);

        if (position == -1)
        {
            this.datax.Rules.push(rule);
        }
        else {
            this.datax.Rules[position] = rule;
            // this.datax.Rules.unshift(rule);
        }
        // this.$forceUpdate();

        this.hideCurrentRule();
    }

    onRuleDeleted(rule: EndpointControlRule): void {
        if (this.datax == null)
        {
            return;
        }

        this.datax.Rules = this.datax.Rules.filter(x => x.Id != rule.Id);
        this.hideCurrentRule();
    }

    showRule(rule: EndpointControlRule, updateRoute: boolean = true): void {
        this.currentRule = rule;

        if (updateRoute)
        {
            this.updateUrl();
        }
    }

    hideCurrentRule(): void {
        this.currentRule = null;
        this.updateUrl();
    }
    
    setServerInteractionInProgress(inProgress: boolean): void
    {
        this.serverInteractionInProgress = inProgress;
    }

    getRuleWarning(rule: EndpointControlRule): string | null
    {
        if (!rule.AlwaysTrigger
            && rule.TotalRequestCountLimits.length == 0 
            && rule.CurrentEndpointRequestCountLimits.length == 0)
        {
            return 'No limits have been defined, the rule won\'t have any effect.';
        }
        return null;
    }

    ruleIsOutsideLimit(rule: EndpointControlRule): boolean
    {
        // if (rule.ToTime != null && rule.ToTime.getTime() > new Date().getTime())
        // {
        //     return true;
        // }
        // else if (rule.FromTime != null && rule.FromTime.getTime() < new Date().getTime())
        // {
        //     return true;
        // }
        // else if (rule.NotificationCountLimit != null && rule.NotificationCountLimit <= 0)
        // {
        //     return true;
        // }

        return false;
    }

    showLatestRequestsDialog(): void {
        this.latestRequestsDialogVisible = true;
        this.updateUrl();
    }

    hideLatestRequestsDialog(): void {
        this.latestRequestsDialogVisible = false;
        this.updateUrl();
    }

    showDeleteDefinitionDialog(endpointId: string | null): void
    {
        this.deleteDefinitionDialogVisible = true;
        this.endpointDefinitionIdToDelete = endpointId;
        this.deleteDefinitionDialogText = (endpointId == null)
            ? `Delete all endpoint definitions?`
            : `Delete endpoint definition '${endpointId}'?`;
    }

    confirmDeleteEndpointDefinition(): void {
        this.deleteDefinitionDialogVisible = false;

        if (this.endpointDefinitionIdToDelete != null)
        {
            this.service.DeleteEndpointDefinition(this.endpointDefinitionIdToDelete, this.loadStatus, {
                onSuccess: (r) => {
                    if (this.datax != null)
                    {
                        this.datax.EndpointDefinitions = this.datax.EndpointDefinitions
                            .filter(x => x.EndpointId != this.endpointDefinitionIdToDelete);
                    }
                }
            });
        }
        else
        {
            this.service.DeleteAllEndpointDefinitions(this.loadStatus, {
                onSuccess: (r) => {
                    if (this.datax != null)
                    {
                        this.datax.EndpointDefinitions = [];
                    }
                }
            });
        }
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////    
    onAddNewRuleClicked(): void {
        if (this.datax == null)
        {
            return;
        }

        let rule: EndpointControlRule = {
            Id: '00000000-0000-0000-0000-000000000000',
            AlwaysTrigger: false,
            LastChangedBy: 'You',
            Enabled: true,
            LastChangedAt: new Date(),
            EndpointIdFilter: this.createDefaultFilter(),
            UserLocationIdFilter: this.createDefaultFilter(),
            UserAgentFilter: this.createDefaultFilter(),
            UrlFilter: this.createDefaultFilter(),
            TotalRequestCountLimits: [],
            CurrentEndpointRequestCountLimits: [],
            BlockResultTypeId: '',
            CustomBlockResultProperties: {}
        };

        this.showRule(rule);
    }

    createDefaultFilter(): EndpointControlPropertyFilter {
        return {
            Enabled: false,
            Filter: '',
            FilterMode: EndpointControlFilterMode.Matches,
            Inverted: false,
            CaseSensitive: false
        };
    }
}
</script>

<style scoped lang="scss">
.current-rule-dialog__title {
    font-size: 34px;
    font-weight: 600;
}
.rule-list-item {
    margin-bottom: 20px;
    
    .rule-list-item--inner {
        display: flex;
        align-items: center;
        flex-direction: row;
        flex-wrap: nowrap;

        .rule-list-item--rule {
            flex: 1;
            cursor: pointer;
            font-size: 16px;
            margin-left: 20px;
            margin-right: 20px;
            
            .rule-list-item--operator {
                font-weight: 600;
            }
            .rule-list-item--condition {
                color: var(--v-primary-base);
            }
            .rule-list-item--action {
                color: var(--v-secondary-base);
            }
            /* .rule-list-item--condition,
            .rule-list-item--action {
                font-weight: 600;
            } */
        }
    }
}
</style>