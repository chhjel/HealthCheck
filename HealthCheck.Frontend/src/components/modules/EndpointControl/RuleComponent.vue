<!-- src/components/modules/EndpointControl/RuleComponent.vue -->
<template>
    <div class="root">
        <v-alert
            :value="serverInteractionError != null && serverInteractionError.length > 0"
            type="error" >
            {{ serverInteractionError }}
        </v-alert>

        <div class="header-data">
            <v-switch
                v-model="internalRule.Enabled" 
                :disabled="!allowChanges"
                label="Enabled"
                color="secondary"
                class="left mr-2"
                style="flex: 1"
            ></v-switch>
            <div>
                <div class="metadata-chip"
                    v-if="internalRule.LastChangedBy != null && internalRule.LastChangedBy.length > 0">
                    Last changed at {{ formatDate(internalRule.LastChangedAt) }} by '{{ internalRule.LastChangedBy }}'
                </div>
                <div class="metadata-chip"
                    v-if="internalRule.LastNotifiedAt != null">
                    Last notified at {{ formatDate(internalRule.LastNotifiedAt) }}
                </div>
            </div>
        </div>

        <div class="rule-summary">
            <rule-description-component
                :rule="internalRule"
                :endpointDefinitions="endpointDefinitions" />
        </div>

        <block-component class="mb-4" title="Filters">
            <h3 class="mt-4">IP address / user location id</h3>
            <rule-filter-component class="payload-filter" :readonly="!allowChanges"
                v-model="internalRule.UserLocationIdFilter" />
            <h3 class="mt-4">Endpoint Id</h3>
            <rule-filter-component class="payload-filter" :readonly="!allowChanges"
                v-model="internalRule.EndpointIdFilter"
                :filterOptions="endpointFilterOptions" />
            <h3 class="mt-4">Url</h3>
            <rule-filter-component class="payload-filter" :readonly="!allowChanges"
                v-model="internalRule.UrlFilter" />
            <h3 class="mt-4">User-Agent</h3>
            <rule-filter-component class="payload-filter" :readonly="!allowChanges"
                v-model="internalRule.UserAgentFilter" />
        </block-component>
        
        <block-component class="mb-4" v-if="internalRule != null" title="Limits">
            <h3 class="mt-4 mb-2">Limits request count per IP per endpoint</h3>
            <count-over-duration-component
                class="mt-2"
                v-for="(item, index) in internalRule.CurrentEndpointRequestCountLimits"
                :key="`endpoint-limit-${item._frontendId}`"
                :value="item"
                :readonly="!allowChanges"
                @input="(val) => onCoDChanged(internalRule.CurrentEndpointRequestCountLimits, index, val)"
                @delete="(val) => onCoDDelete(internalRule.CurrentEndpointRequestCountLimits, index)"
                />
            <v-btn class="ml-4"
                :disabled="!allowChanges" 
                @click.stop="addCodItem(internalRule.CurrentEndpointRequestCountLimits)">
                <v-icon size="20px" class="mr-2">add</v-icon>
                Add new limit
            </v-btn>

            <h3 class="mt-4 mb-2">Limit total request count per IP</h3>
            <count-over-duration-component
                class="mt-2"
                v-for="(item, index) in internalRule.TotalRequestCountLimits"
                :key="`total-limit-${item._frontendId}`"
                :value="item"
                :readonly="!allowChanges"
                @input="(val) => onCoDChanged(internalRule.TotalRequestCountLimits, index, val)"
                @delete="(val) => onCoDDelete(internalRule.TotalRequestCountLimits, index)"
                />
            <v-btn class="ml-4"
                :disabled="!allowChanges" 
                @click.stop="addCodItem(internalRule.TotalRequestCountLimits)">
                <v-icon size="20px" class="mr-2">add</v-icon>
                Add new limit
            </v-btn>
        </block-component>

        <v-dialog v-model="deleteDialogVisible"
            @keydown.esc="deleteDialogVisible = false"
            max-width="290"
            content-class="confirm-dialog">
            <v-card>
                <v-card-title class="headline">Confirm deletion</v-card-title>
                <v-card-text>
                    Are you sure you want to delete this rule?
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary" @click="deleteDialogVisible = false">Cancel</v-btn>
                    <v-btn color="error" @click="deleteRule()">Delete it</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import SimpleDateTimeComponent from  '../../Common/SimpleDateTimeComponent.vue';
import RuleFilterComponent from './RuleFilterComponent.vue';
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import DateUtils from  '../../../util/DateUtils';
import IdUtils from  '../../../util/IdUtils';
import EndpointControlUtils from  '../../../util/EndpointControl/EndpointControlUtils';
import BlockComponent from  '../../Common/Basic/BlockComponent.vue';
import InputComponent from  '../../Common/Basic/InputComponent.vue';
import TimespanInputComponent from  '../../Common/Basic/TimespanInputComponent.vue';
import CountOverDurationComponent from './CountOverDurationComponent.vue';
import RuleDescriptionComponent from  './RuleDescriptionComponent.vue';
import EndpointControlService from "../../../services/EndpointControlService";
import { EndpointControlCountOverDuration, EndpointControlEndpointDefinition, EndpointControlPropertyFilter, EndpointControlRule } from "../../../models/modules/EndpointControl/EndpointControlModels";

@Component({
    components: {
        RuleFilterComponent,
        SimpleDateTimeComponent,
        BlockComponent,
        InputComponent,
        TimespanInputComponent,
        CountOverDurationComponent,
        RuleDescriptionComponent
    }
})
export default class RuleComponent extends Vue {
    @Prop({ required: true })
    moduleId!: string;

    @Prop({ required: true })
    rule!: EndpointControlRule;

    @Prop({ required: false, default: null})
    endpointDefinitions!: Array<EndpointControlEndpointDefinition>;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    // @ts-ignore
    internalRule: EndpointControlRule = null;
    service: EndpointControlService = new EndpointControlService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.moduleId);
    notifierDialogVisible: boolean = false;
    deleteDialogVisible: boolean = false;
    isSaving: boolean = false;
    isDeleting: boolean = false;
    serverInteractionError: string | null = null;
    serverInteractionInProgress: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
        this.onRuleChanged();
    }

    @Watch("rule")
    onRuleChanged(): void {
        let intRule = JSON.parse(JSON.stringify(this.rule));
        EndpointControlUtils.postProcessRule(intRule);
        this.internalRule = intRule;
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }
    
    get allowChanges(): boolean {
        return !this.readonly && !this.serverInteractionInProgress;
    }

    get EndpointDefinitionIds(): Array<string> {
        if (this.endpointDefinitions == null) return [];
        else return this.endpointDefinitions.map(x => x.EndpointId);
    }

    get endpointFilterOptions(): Array<string> {
        return this.EndpointDefinitionIds;
    }

    ////////////////
    //  METHODS  //
    //////////////
    setServerInteractionInProgress(inProgress: boolean, err: string | null = null): void
    {
        this.serverInteractionError = err;
        this.serverInteractionInProgress = inProgress;
        this.$emit('serverInteractionInProgress', inProgress);
    }

    public saveRule(): void {
        this.isSaving = true;
        this.setServerInteractionInProgress(true);

        // Need timeout to first apply any changes from currently selected field.
        setTimeout(() => {
            this.saveRuleInternal();
        }, 50);
    }

    saveRuleInternal(): void {
        this.isSaving = true;
        this.setServerInteractionInProgress(true);

        this.service.CreateOrUpdateRule(this.internalRule, null, {
            onSuccess: (data) => this.onRuleSaved(data),
            onError: (message) => this.setServerInteractionInProgress(false, message),
            onDone: () => { this.isSaving = false }
        });
    }

    onRuleSaved(newRule: EndpointControlRule): void {
        this.isSaving = false;
        this.setServerInteractionInProgress(false);
        this.$emit('ruleSaved', newRule);
    }

    public tryDeleteRule(): void {
        this.deleteDialogVisible = true;
    }

    public deleteRule(): void {
        if (this.internalRule.Id == null)
        {
            return;
        }

        this.deleteDialogVisible = false;
        this.isDeleting = true;
        this.setServerInteractionInProgress(true);

        this.service.DeleteRule(this.internalRule, null, {
            onSuccess: (data) => this.onRuleDeleted(this.internalRule),
            onError: (message) => this.setServerInteractionInProgress(false, message),
            onDone: () => { this.isDeleting = false }
        });
    }

    onRuleDeleted(rule: EndpointControlRule): void {
        this.isDeleting = false;
        this.setServerInteractionInProgress(false);
        this.$emit('ruleDeleted', this.rule);
    }

    formatDate(date: Date): string
    {
        return DateUtils.FormatDate(date, 'yyyy MMM d HH:mm:ss');
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onRuleFilterChanged(filter: EndpointControlPropertyFilter, newValues: EndpointControlPropertyFilter): void {
        filter.Enabled = newValues.Enabled;
        filter.Filter = newValues.Filter;
        filter.FilterMode = newValues.FilterMode;
        filter.Inverted = newValues.Inverted;
        filter.CaseSensitive = newValues.CaseSensitive;
    }

    onCoDChanged(list: Array<EndpointControlCountOverDuration>, index: number, newVal: EndpointControlCountOverDuration): void {
        Vue.set(list, index, newVal);
    }

    onCoDDelete(list: Array<EndpointControlCountOverDuration>, index: number): void {
        list.splice(index, 1);
    }

    addCodItem(list: Array<EndpointControlCountOverDuration>): void {
        let item: EndpointControlCountOverDuration = {
            Count: 10,
            Duration: '0:1:0'
        };
        EndpointControlUtils.postProcessCountLimit(item);
        list.push(item);
    }
}
</script>

<style scoped lang="scss">
.metadata-chip {
    display: inline-block;
    border: 1px solid gray;
    padding: 5px;
    margin: 5px;
    font-size: 12px;
}
.without-label {
    margin-top: 0;
    padding-top: 0;
}
.payload-filter {
    border-bottom: solid 1px #ccc;
}
.rule-summary {
    padding: 10px;
    margin-top: 20px;
    margin-bottom: 20px;
    font-size: 18px;
}
.header-data {
    display: flex;
    align-items: center;
    flex-direction: row;
    flex-wrap: wrap-reverse;
}
.notifier-title {
    font-size: 18px;
    margin-bottom: 10px;
    margin-right: 10px;
}
</style>
