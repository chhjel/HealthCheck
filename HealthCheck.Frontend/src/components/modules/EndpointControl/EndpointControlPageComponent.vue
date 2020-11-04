<!-- src/components/modules/EndpointControl/EndpointControlPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
            <v-container fluid fill-height class="content-root">
            <v-layout>
            <v-flex>
            <v-container>
                <h1 class="mb-1">Endpoint control rules</h1>
                
                <data-over-time-chart-component
                    :entries="testEntries"
                    :sets="testSets"
                    ylabel="Requests" />

                <h2>Todo:</h2>
                <ul>
                    <li>show endpoints (definitions) w/ stats &amp; latest requets</li>
                    <li>Show trafic &amp; request count per endpoint</li>
                    <li>Store blocked count per endpoint? Percentage?</li>
                    <li>Notify event system on trigger.</li>
                    <li>Show in fancy map or something with animated lines with trafic.</li>
                </ul>
                <h3>Since [oldest request date]</h3>
                <ul>
                    <li>n requests total</li>
                    <li>n (or "n+"" if max) different IP's invoked endpoints</li>
                    <li>IPs with most traffic:</li>
                    <ul>
                        <li>127.0.0.1: n1 requests</li>
                        <li>127.0.0.2: n2 requests</li>
                        <li>127.0.0.3: n3 requests</li>
                    </ul>
                </ul>

                <code v-if="data != null">{{ data.EndpointDefinitions }}</code><br />

                <!-- LOAD PROGRESS -->
                <v-progress-linear
                    v-if="loadStatus.inProgress"
                    indeterminate color="green"></v-progress-linear>

                <!-- DATA LOAD ERROR -->
                <v-alert :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
                </v-alert>

                <v-btn :disabled="!allowRuleChanges"
                    @click="onAddNewRuleClicked"
                    class="mb-3">
                    <v-icon size="20px" class="mr-2">add</v-icon>
                    Add new
                </v-btn>

                <v-btn v-if="HasAccessToEditEndpointDefinitions"
                    @click="editDefinitionsDialogVisible = true"
                    class="mb-3 ml-2 right">
                    Edit endpoint definitions
                </v-btn>

                <block-component
                    v-for="(rule, cindex) in rules"
                    :key="`rule-${cindex}-${rule.Id}`"
                    class="rule-list-item"
                    >
                    <div class="rule-list-item--inner">
                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                            <v-switch v-on="on"
                                v-model="rule.Enabled"
                                color="secondary"
                                style="flex: 0"
                                @click="setRuleEnabled(rule, !rule.Enabled)"
                                ></v-switch>
                                <!-- :disabled="!allowRuleChanges" -->
                            </template>
                            <span>Enable or disable this rule</span>
                        </v-tooltip>
                        
                        <div class="rule-list-item--rule"
                            @click="showRule(rule)">
                            <rule-description-component :rule="rule" :endpointDefinitions="EndpointDefinitions" />
                        </div>
                        
                        <v-tooltip bottom v-if="getRuleWarning(rule) != null">
                            <template v-slot:activator="{ on }">
                                <v-icon style="cursor: help;" color="warning" v-on="on">warning</v-icon>
                            </template>
                            <span>{{getRuleWarning(rule)}}</span>
                        </v-tooltip>

                        <v-tooltip bottom v-if="ruleIsOutsideLimit(rule)">
                            <template v-slot:activator="{ on }">
                                <v-icon v-on="on" style="cursor: help;">timer_off</v-icon>
                            </template>
                            <span>This rules' limits has been reached</span>
                        </v-tooltip>

                        <v-tooltip bottom>
                            <template v-slot:activator="{ on }">
                                <v-icon style="cursor: help;" v-on="on">person</v-icon>
                                <code style="color: var(--v-primary-base); cursor: help;" v-on="on">{{ rule.LastChangedBy }}</code>
                            </template>
                            <span>Last modified by '{{ rule.LastChangedBy }}'</span>
                        </v-tooltip>
                    </div>
                </block-component>

            </v-container>
            </v-flex>
            </v-layout>
            </v-container>
            
            <v-dialog v-model="ruleDialogVisible"
                scrollable
                persistent
                max-width="1200"
                content-class="current-rule-dialog">
                <v-card v-if="currentRule != null" style="background-color: #f4f4f4">
                    <v-toolbar class="elevation-0">
                        <v-toolbar-title class="current-rule-dialog__title">{{ currentDialogTitle }}</v-toolbar-title>
                        <v-spacer></v-spacer>
                        <v-btn icon
                            @click="hideCurrentRule()"
                            :disabled="serverInteractionInProgress">
                            <v-icon>close</v-icon>
                        </v-btn>
                    </v-toolbar>

                    <v-divider></v-divider>
                    
                    <v-card-text>
                        <rule-component
                            :module-id="config.Id"
                            :rule="currentRule"
                            :endpointDefinitions="EndpointDefinitions"
                            :readonly="!allowRuleChanges"
                            v-on:ruleDeleted="onRuleDeleted"
                            v-on:ruleSaved="onRuleSaved"
                            v-on:serverInteractionInProgress="setServerInteractionInProgress"
                            ref="currentRuleComponent"
                            />
                    </v-card-text>

                    <v-divider></v-divider>
                    <v-card-actions >
                        <v-spacer></v-spacer>
                        <v-btn color="error" flat
                            v-if="showDeleteRule"
                            :disabled="serverInteractionInProgress"
                            @click="$refs.currentRuleComponent.tryDeleteRule()">Delete</v-btn>
                        <v-btn color="success"
                            :disabled="serverInteractionInProgress"
                            @click="$refs.currentRuleComponent.saveRule()">Save</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>

            <v-dialog v-model="deleteDefinitionDialogVisible"
                @keydown.esc="deleteDefinitionDialogVisible = false"
                max-width="290"
                content-class="confirm-dialog">
                <v-card>
                    <v-card-title class="headline">Confirm deletion</v-card-title>
                    <v-card-text>
                        {{ deleteDefinitionDialogText }}
                    </v-card-text>
                    <v-divider></v-divider>
                    <v-card-actions>
                        <v-spacer></v-spacer>
                        <v-btn color="secondary" @click="deleteDefinitionDialogVisible = false">Cancel</v-btn>
                        <v-btn color="error"
                            :loading="loadStatus.inProgress"
                            :disabled="loadStatus.inProgress"
                            @click="confirmDeleteEndpointDefinition()">Delete</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            
            <v-dialog v-model="editDefinitionsDialogVisible"
                @keydown.esc="editDefinitionsDialogVisible = false"
                scrollable
                max-width="1200"
                content-class="current-rule-dialog">
                <v-card style="background-color: #f4f4f4">
                    <v-toolbar class="elevation-0">
                        <v-toolbar-title class="current-rule-dialog__title">Edit endpoint definitions</v-toolbar-title>
                        <v-spacer></v-spacer>
                        <v-btn icon
                            @click="editDefinitionsDialogVisible = false">
                            <v-icon>close</v-icon>
                        </v-btn>
                    </v-toolbar>

                    <v-divider></v-divider>
                    
                    <v-card-text>
                        <block-component
                            v-for="(def, dindex) in EndpointDefinitions"
                            :key="`endpointdef-${dindex}-${def.EndpointId}`"
                            class="definition-list-item mb-2">
                            <v-btn
                                :loading="loadStatus.inProgress"
                                :disabled="loadStatus.inProgress"
                                color="error" class="right"
                                @click="showDeleteDefinitionDialog(def.EndpointId)">
                                <v-icon size="20px" class="mr-2">delete</v-icon>
                                Delete
                            </v-btn>

                            <h3>{{ getEndpointDisplayName(def.EndpointId) }}</h3>
                            <div style="clear:both;"></div>
                        </block-component>
                    </v-card-text>
                    <v-divider></v-divider>
                    <v-card-actions >
                        <v-spacer></v-spacer>
                        <v-btn
                            :loading="loadStatus.inProgress"
                            :disabled="loadStatus.inProgress"
                            color="error"
                            @click="showDeleteDefinitionDialog(null)">
                            <v-icon size="20px" class="mr-2">delete_forever</v-icon>
                            Delete all definitions
                        </v-btn>
                        <v-btn color="success"
                            @click="editDefinitionsDialogVisible = false">Close</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import LoggedEndpointDefinitionViewModel from  '../../../models/modules/RequestLog/LoggedEndpointDefinitionViewModel';
import LoggedEndpointRequestViewModel from  '../../../models/modules/RequestLog/LoggedEndpointRequestViewModel';
import { EntryState } from  '../../../models/modules/RequestLog/EntryState';
import DateUtils from  '../../../util/DateUtils';
import LinqUtils from  '../../../util/LinqUtils';
import KeyArray from  '../../../util/models/KeyArray';
import KeyValuePair from  '../../../models/Common/KeyValuePair';
import '@lazy-copilot/datetimepicker/dist/datetimepicker.css'
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";
import FilterInputComponent from  '../../Common/FilterInputComponent.vue';
import DataTableComponent, { DataTableGroup } from  '../../Common/DataTableComponent.vue';
import SimpleDateTimeComponent from  '../../Common/SimpleDateTimeComponent.vue';
import RuleDescriptionComponent from  './RuleDescriptionComponent.vue';
import FilterableListComponent, { FilterableListItem } from  '../../Common/FilterableListComponent.vue';
import RuleComponent from './RuleComponent.vue';
import IdUtils from  '../../../util/IdUtils';
import EndpointControlUtils from  '../../../util/EndpointControl/EndpointControlUtils';
import BlockComponent from  '../../Common/Basic/BlockComponent.vue';
import { FetchStatus } from  '../../../services/abstractions/HCServiceBase';
import EndpointControlService from  '../../../services/EndpointControlService';
import ModuleOptions from  '../../../models/Common/ModuleOptions';
import ModuleConfig from "../../../models/Common/ModuleConfig";
import { EndpointControlDataViewModel, EndpointControlEndpointDefinition, EndpointControlFilterMode, EndpointControlPropertyFilter, EndpointControlRule } from "../../../models/modules/EndpointControl/EndpointControlModels";
import DataOverTimeChartComponent, { ChartEntry, ChartSet } from '../../Common/Charts/DataOverTimeChartComponent.vue';

@Component({
    components: {
        SimpleDateTimeComponent,
        BlockComponent,
        RuleComponent,
        RuleDescriptionComponent,
        DataOverTimeChartComponent
    }
})
export default class EndpointControlPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;

    service: EndpointControlService = new EndpointControlService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);

    // UI STATE
    loadStatus: FetchStatus = new FetchStatus();
    serverInteractionInProgress: boolean = false;
    editDefinitionsDialogVisible: boolean = false;
    deleteDefinitionDialogVisible: boolean = false;
    deleteDefinitionDialogText: string = "";
    endpointDefinitionIdToDelete: string | null = null;

    data: EndpointControlDataViewModel | null = null;
    currentRule: EndpointControlRule | null = null;

	testSets: Array<ChartSet> = [
        { label: 'Allowed', group: 'allowed', color: '#4cff50' },
        { label: 'Blocked', group: 'blocked', color: '#FF0000' }
    ];
    testEntries: Array<ChartEntry> = [
        { date: DateUtils.CreateDateWithMinutesOffset(1), group: 'blocked' },
        { date: DateUtils.CreateDateWithMinutesOffset(1), group: 'allowed' },
        { date: DateUtils.CreateDateWithMinutesOffset(2), group: 'blocked' },
        { date: DateUtils.CreateDateWithMinutesOffset(16), group: 'blocked' },
        { date: DateUtils.CreateDateWithMinutesOffset(4), group: 'allowed' },
        { date: DateUtils.CreateDateWithMinutesOffset(120), group: 'allowed' },
        { date: DateUtils.CreateDateWithMinutesOffset(121), group: 'allowed' },
        { date: DateUtils.CreateDateWithMinutesOffset(122), group: 'allowed' },
        { date: DateUtils.CreateDateWithMinutesOffset(123), group: 'allowed' },
        { date: DateUtils.CreateDateWithMinutesOffset(120), group: 'allowed' },
        { date: DateUtils.CreateDateWithMinutesOffset(125), group: 'allowed' },
        { date: DateUtils.CreateDateWithMinutesOffset(60 * 24 * 14), group: 'allowed' },
        { date: DateUtils.CreateDateWithMinutesOffset(60 * 24 * 12), group: 'blocked' },
        { date: DateUtils.CreateDateWithMinutesOffset(8), group: 'blocked' },
    ];

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
        return this.$store.state.globalOptions;
    }

    get HasAccessToEditEndpointDefinitions(): boolean {
        return this.options.AccessOptions.indexOf("EditEndpointDefinitions") != -1;
    }
    
    get showDeleteRule(): boolean
    {
        return this.currentRule != null && this.currentRule.Id != null;
    }

    get allowRuleChanges(): boolean
    {
        return !this.serverInteractionInProgress;
    };

    get currentDialogTitle(): string
    {
        return (this.currentRule != null && this.currentRule.Id != null)
            ? 'Edit notification rule'
            : 'Create new notification rule';
    }

    get ruleDialogVisible(): boolean
    {
        return this.currentRule != null;
    }

    get EndpointDefinitions(): Array<EndpointControlEndpointDefinition>
    {
        return (this.data == null) ? [] : this.data.EndpointDefinitions;
    };

    get rules(): Array<EndpointControlRule>
    {
        let rules = (this.data == null) ? [] : this.data.Rules;
        return rules;
    };

    ////////////////
    //  METHODS  //
    //////////////
    getEndpointDisplayName(endpointId: string) : string {
        if (this.data == null) return endpointId;

        return EndpointControlUtils.getEndpointDisplayName(endpointId, this.data.EndpointDefinitions);
    }

    updateUrl(): void {
        let routeParams: any = {};
        if (this.currentRule != null && this.currentRule.Id != null)
        {
            routeParams['id'] = this.currentRule.Id;
        }

        this.$router.push({ name: this.config.Id, params: routeParams })
    }

    updateSelectionFromUrl(): void {
        const idFromHash = this.$route.params.id;
        
        if (idFromHash) {
            let ruleFromUrl = this.rules.filter(x => x.Id != null && x.Id == idFromHash)[0];
            if (ruleFromUrl != null)
            {
                this.showRule(ruleFromUrl, false);
            }
        }
    }

    loadData(): void {
        this.service.GetData(this.loadStatus, { onSuccess: (data) => this.onDataRetrieved(data) });
    }

    onDataRetrieved(data: EndpointControlDataViewModel): void {
        this.data = data;
        this.data.Rules.forEach(rule => {
            EndpointControlUtils.postProcessRule(rule);
        });

        this.updateSelectionFromUrl();
        
        this.data.Rules = this.data.Rules.sort(
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
        if (this.data == null)
        {
            return;
        }
        EndpointControlUtils.postProcessRule(rule);

        const position = this.data.Rules.findIndex(x => x.Id == rule.Id);
        //this.data.Rules = this.data.Rules.filter(x => x.Id != rule.Id);

        if (position == -1)
        {
            this.data.Rules.push(rule);
        }
        else {
            Vue.set(this.data.Rules, position, rule);
            // this.data.Rules.unshift(rule);
        }
        // this.$forceUpdate();

        this.hideCurrentRule();
    }

    onRuleDeleted(rule: EndpointControlRule): void {
        if (this.data == null)
        {
            return;
        }

        this.data.Rules = this.data.Rules.filter(x => x.Id != rule.Id);
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
        if (rule.TotalRequestCountLimits.length == 0 && rule.CurrentEndpointRequestCountLimits.length == 0)
        {
            return 'No limits has been defined, the rule won\'t have any effect.';
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
                    if (this.data != null)
                    {
                        this.data.EndpointDefinitions = this.data.EndpointDefinitions
                            .filter(x => x.EndpointId != this.endpointDefinitionIdToDelete);
                    }
                }
            });
        }
        else
        {
            this.service.DeleteAllEndpointDefinitions(this.loadStatus, {
                onSuccess: (r) => {
                    if (this.data != null)
                    {
                        this.data.EndpointDefinitions = [];
                    }
                }
            });
        }
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////    
    onAddNewRuleClicked(): void {
        if (this.data == null)
        {
            return;
        }

        let rule: EndpointControlRule = {
            Id: '00000000-0000-0000-0000-000000000000',
            LastChangedBy: 'You',
            Enabled: true,
            LastChangedAt: new Date(),
            EndpointIdFilter: this.createDefaultFilter(),
            UserLocationIdFilter: this.createDefaultFilter(),
            UserAgentFilter: this.createDefaultFilter(),
            UrlFilter: this.createDefaultFilter(),
            TotalRequestCountLimits: [],
            CurrentEndpointRequestCountLimits: []
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