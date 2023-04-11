<!-- src/components/modules/EndpointControl/RuleComponent.vue -->
<template>
    <div class="root">
        <alert-component
            :value="serverInteractionError != null && serverInteractionError.length > 0"
            type="error" >
            {{ serverInteractionError }}
        </alert-component>

        <div class="header-data">
            <switch-component
                v-model:value="internalRule.Enabled" 
                :disabled="!allowChanges"
                label="Enabled"
                falseLabel="Disabled"
                color="secondary"
                class="left mr-2"
                style="flex: 1"
            ></switch-component>
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
                :endpointDefinitions="endpointDefinitions"
                :customResultDefinitions="customResultDefinitions"
                :conditionDefinitions="conditionDefinitions"
                 />
        </div>

        <block-component class="mb-4 pt-0" title="Filters">
            <h3 class="mt-4">IP address / user location id</h3>
            <rule-filter-component class="payload-filter" :readonly="!allowChanges"
                v-model:value="internalRule.UserLocationIdFilter" />
            <h3 class="mt-4">Endpoint Id</h3>
            <rule-filter-component class="payload-filter" :readonly="!allowChanges"
                v-model:value="internalRule.EndpointIdFilter"
                :filterOptions="endpointFilterOptions" />
            <h3 class="mt-4">Url</h3>
            <rule-filter-component class="payload-filter" :readonly="!allowChanges"
                v-model:value="internalRule.UrlFilter" />
            <h3 class="mt-4">User-Agent</h3>
            <rule-filter-component class="payload-filter" :readonly="!allowChanges"
                v-model:value="internalRule.UserAgentFilter" />
        </block-component>
        
        <block-component class="mb-4" v-if="internalRule != null" title="Conditions">
            <switch-component
                v-model:value="internalRule.AlwaysTrigger" 
                label="Always trigger for all matching requests"
                color="secondary"
                :disabled="!allowChanges"
            ></switch-component>

            <div v-if="!internalRule.AlwaysTrigger">
                <h3 class="mt-4 mb-2">After request count per IP per endpoint</h3>
                <count-over-duration-component
                    class="mt-2"
                    v-for="(item, index) in internalRule.CurrentEndpointRequestCountLimits"
                    :key="`endpoint-limit-${item._frontendId}`"
                    :value="item"
                    :readonly="!allowChanges"
                    @update:value="(val) => onCoDChanged(internalRule.CurrentEndpointRequestCountLimits, index, val)"
                    @delete="(val) => onCoDDelete(internalRule.CurrentEndpointRequestCountLimits, index)"
                    />
                <btn-component class="ml-4"
                    :disabled="!allowChanges" 
                    @click.stop="addCodItem(internalRule.CurrentEndpointRequestCountLimits)">
                    <icon-component size="20px" class="mr-2">add</icon-component>
                    Add
                </btn-component>

                <h3 class="mt-4 mb-2">After total request count per IP</h3>
                <count-over-duration-component
                    class="mt-2"
                    v-for="(item, index) in internalRule.TotalRequestCountLimits"
                    :key="`total-limit-${item._frontendId}`"
                    :value="item"
                    :readonly="!allowChanges"
                    @update:value="(val) => onCoDChanged(internalRule.TotalRequestCountLimits, index, val)"
                    @delete="(val) => onCoDDelete(internalRule.TotalRequestCountLimits, index)"
                    />
                <btn-component class="ml-4"
                    :disabled="!allowChanges" 
                    @click.stop="addCodItem(internalRule.TotalRequestCountLimits)">
                    <icon-component size="20px" class="mr-2">add</icon-component>
                    Add
                </btn-component>
            </div>

            <div v-if="!internalRule.AlwaysTrigger && conditionDefinitions != null && conditionDefinitions.length > 0">
                <h3 class="mt-4">Custom conditions</h3>

                <div v-if="internalRule.Conditions && internalRule.Conditions.length > 0"
                    class="ruleconditions">
                    <div v-for="(cond, cIndex) in internalRule.Conditions"
                        :key="`condition-selected-${id}-${cond.Id}-${cIndex}`"
                        class="rulecondition">
                        <btn-component
                            dark small flat
                            color="error"
                            class="right"
                            @click="removeCondition(cIndex)"
                            :disabled="!allowChanges">
                            Remove
                        </btn-component>
                        <div class="rulecondition__name">{{ getConditionDefinition(cond.ConditionId).Name }}</div>
                        <div class="rulecondition__desc">{{ getConditionDefinition(cond.ConditionId).Description }}</div>

                        <div class="rulecondition__props" v-if="getConditionDefinition(cond.ConditionId) != null">
                            <div>
                                <backend-input-component
                                    v-for="(parameterDef, pIndex) in getConditionDefinition(cond.ConditionId).CustomProperties"
                                    :key="`condition-selected-prop-${id}-${cond.Id}-${cIndex}-${pIndex}-${parameterDef.Id}`"
                                    v-model:value="cond.Parameters[parameterDef.Id]"
                                    :config="parameterDef"
                                    :readonly="!allowChanges"
                                    />
                            </div>
                        </div>
                    </div>
                </div>

                <btn-component class="ml-4"
                    :disabled="!allowChanges" 
                    @click.stop="onAddCustomConditionClicked">
                    <icon-component size="20px" class="mr-2">add</icon-component>
                    Add
                </btn-component>
            </div>
        </block-component>
        
        <block-component class="mb-4" v-if="internalRule != null" title="Resulting action">
            <h3 class="mt-4 mb-2">Select what happens when a result is overridden</h3>
            
            <select-component
                class="mode-select"
                v-model:value="internalRule.BlockResultTypeId"
                :items="blockResultOptions"
                item-text="text" item-value="value"
                :disabled="!allowChanges"
                >
            </select-component>

            <p class="mt-4 mb-2">{{ selectedBlockResultDescription }}</p>

            <backend-input-component
                v-for="(def, defIndex) in selectedBlockResultPropertyDefinitions"
                :key="`defx-${defIndex}`"
                class="mb-2"
                v-model:value="internalRule.CustomBlockResultProperties[def.Id]"
                :config="def"
                :readonly="!allowChanges"
                />
        </block-component>

        <dialog-component v-model:value="deleteDialogVisible" max-width="500">
            <template #header>Confirm deletion</template>
            <template #footer>
                <btn-component color="error" @click="deleteRule()">Delete it</btn-component>
                <btn-component color="secondary" @click="deleteDialogVisible = false">Cancel</btn-component>
            </template>
            <div>
                Are you sure you want to delete this rule?
            </div>
        </dialog-component>

        <dialog-component v-model:value="customConditionDialogVisible" max-width="600">
            <template #header>Select condition to add</template>
            <template #footer>
                <btn-component color="secondary" @click="customConditionDialogVisible = false">Cancel</btn-component>
            </template>
            <div class="conditiondefs">
                <div v-for="cond in conditionDefinitions"
                    :key="`condition-${id}-${cond.Id}`"
                    @click="addCustomCondition(cond)"
                    class="conditiondef clickable hoverable-light">
                    <div class="conditiondef__name">{{ cond.Name }}</div>
                    <div class="conditiondef__desc">{{ cond.Description }}</div>
                </div>
            </div>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Inject } from "vue-property-decorator";
import { Options } from "vue-class-component";
import SimpleDateTimeComponent from '@components/Common/SimpleDateTimeComponent.vue';
import RuleFilterComponent from '@components/modules/EndpointControl/RuleFilterComponent.vue';
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import DateUtils from '@util/DateUtils';
import IdUtils from '@util/IdUtils';
import EndpointControlUtils from '@util/EndpointControl/EndpointControlUtils';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import InputHeaderComponent from '@components/Common/Basic/InputHeaderComponent.vue';
import TimespanInputComponent from '@components/Common/Basic/TimespanInputComponent.vue';
import CountOverDurationComponent from '@components/modules/EndpointControl/CountOverDurationComponent.vue';
import RuleDescriptionComponent from '@components/modules/EndpointControl/RuleDescriptionComponent.vue';
import EndpointControlService from '@services/EndpointControlService';
import { EndpointControlCountOverDuration, EndpointControlCustomResultDefinitionViewModel, EndpointControlEndpointDefinition, EndpointControlPropertyFilter, EndpointControlRule } from '@models/modules/EndpointControl/EndpointControlModels';
import { TKBackendInputConfig } from "@generated/Models/Core/TKBackendInputConfig";
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import { StoreUtil } from "@util/StoreUtil";
import { TKEndpointControlConditionDefinitionViewModel } from "@generated/Models/Module/EndpointControl/TKEndpointControlConditionDefinitionViewModel";

@Options({
    components: {
        RuleFilterComponent,
        SimpleDateTimeComponent,
        BlockComponent,
        InputHeaderComponent,
        TimespanInputComponent,
        CountOverDurationComponent,
        RuleDescriptionComponent,
        BackendInputComponent
    }
})
export default class RuleComponent extends Vue {
    @Prop({ required: true })
    moduleId!: string;

    @Prop({ required: true })
    rule!: EndpointControlRule;

    @Prop({ required: true })
    customResultDefinitions!: Array<EndpointControlCustomResultDefinitionViewModel> | null;

    @Prop({ required: true })
    conditionDefinitions!: Array<TKEndpointControlConditionDefinitionViewModel> | null;

    @Prop({ required: false, default: null})
    endpointDefinitions!: Array<EndpointControlEndpointDefinition>;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    // @ts-ignore
    internalRule: EndpointControlRule = null;
    service: EndpointControlService = new EndpointControlService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.moduleId);
    notifierDialogVisible: boolean = false;
    deleteDialogVisible: boolean = false;
    customConditionDialogVisible: boolean = false;
    isSaving: boolean = false;
    isDeleting: boolean = false;
    serverInteractionError: string | null = null;
    serverInteractionInProgress: boolean = false;
    id: string = IdUtils.generateId();

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
        return StoreUtil.store.state.globalOptions;
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

    get blockResultOptions(): any {
        let items = [ { text: 'Block request', value: '', description: 'Blocks the request with a 409 response.' } ];
        if (this.customResultDefinitions)
        {
            items = [...items, ...this.customResultDefinitions.map(x => {
                return {
                    text: x.Name,
                    value: x.Id,
                    description: x.Description
                }
            })];
        }
        return items;
    }

    get selectedBlockResultDescription(): string {
        const option = this.blockResultOptions.filter((x:any) => x.value == this.internalRule.BlockResultTypeId)[0];
        return (option) ? option.description : '';
    }

    get selectedBlockResultPropertyDefinitions(): Array<TKBackendInputConfig> {
        if (!this.customResultDefinitions)
        {
            return [];
        }

        const def = this.customResultDefinitions.filter(x => x.Id == this.internalRule.BlockResultTypeId)[0];
        if (!def)
        {
            return [];
        }

        return def.CustomProperties;
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

    conditionDefCache: { [key: string]: TKEndpointControlConditionDefinitionViewModel } = {};
    getConditionDefinition(id: string): TKEndpointControlConditionDefinitionViewModel {
        if (this.conditionDefCache[id] == null)
        this.conditionDefCache[id] = this.conditionDefinitions.find(x => x.Id == id);
        return this.conditionDefCache[id];
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
        list[index] = newVal;
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

    onAddCustomConditionClicked(): void {
        this.customConditionDialogVisible = true;
    }

    addCustomCondition(cond: TKEndpointControlConditionDefinitionViewModel): void {
        if (this.internalRule.Conditions == null) this.internalRule.Conditions = [];
        this.internalRule.Conditions.push({
            ConditionId: cond.Id,
            Parameters: {}
        });
        this.customConditionDialogVisible = false;
    }

    removeCondition(index: number): void {
        this.internalRule.Conditions.splice(index, 1);
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
.conditiondef {
    margin-bottom: 10px;
    border: 2px solid var(--color--accent-darken1);
    padding: 7px;

    &__name {
        font-weight: 600;
    }
}
.rulecondition {
    margin-top: 16px;
    margin-left: 12px;
    border-left: 4px solid var(--color--accent-darken1);
    padding-left: 12px;
    &__name {
        font-weight: 600;
        font-size: 17px;
    }
    &__desc {
        font-size: 14px;
        color: var(--color--accent-darken8);
        margin-bottom: 10px;
    }
}
</style>
