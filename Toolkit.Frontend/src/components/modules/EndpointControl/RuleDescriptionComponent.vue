<!-- src/components/modules/EndpointControl/RuleDescriptionComponent.vue -->
<template>
    <div class="root">
        <b>When</b>
        <ul>
            <li v-if="description.filters.length == 0">Always</li>
            <li v-for="(filter, fltIndex) in description.filters"
                :key="`rule-${rule.Id}-filter-${fltIndex}`"
                class="anywrap">
                <span v-if="fltIndex > 0">And </span>
                {{ filter }}
            </li>
        </ul>

        <div v-if="!rule.AlwaysTrigger">
            <b>And any of these limits are reached</b>
            <ul>
                <li v-if="description.limits.length == 0">- no limits defined -</li>
                <li v-for="(limit, limIndex) in description.limits"
                    :key="`rule-${rule.Id}-limit-${limIndex}`">
                    <span v-if="limIndex > 0">Or </span>
                    {{ limit }}
                </li>
            </ul>
        </div>

        <div v-if="!rule.AlwaysTrigger && description.conditions && description.conditions.length > 0">
            <b>And all the following conditions are met</b>
            <ul>
                <li v-for="(conditions, condIndex) in description.conditions"
                    :key="`rule-${rule.Id}-condition-${condIndex}`">
                    <span v-if="condIndex > 0">And </span>
                    {{ conditions }}
                </li>
            </ul>
        </div>

        <div>
            <b>Then</b>
            <ul>
                <li>{{ description.action }}</li>
            </ul>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { EndpointControlCustomResultDefinitionViewModel, EndpointControlEndpointDefinition, EndpointControlRule } from '@models/modules/EndpointControl/EndpointControlModels';
import EndpointControlUtils, { RuleDescription } from '@util/EndpointControl/EndpointControlUtils';
import { TKEndpointControlConditionDefinitionViewModel } from "@generated/Models/Module/EndpointControl/TKEndpointControlConditionDefinitionViewModel";

@Options({
    components: {  }
})
export default class RuleDescriptionComponent extends Vue {
    @Prop({ required: true })
    rule!: EndpointControlRule;

    @Prop({ required: false, default: null })
    endpointDefinitions!: Array<EndpointControlEndpointDefinition>;

    @Prop({ required: false, default: null })
    customResultDefinitions!: Array<EndpointControlCustomResultDefinitionViewModel>;
                    
    @Prop({ required: false, default: null })
    conditionDefinitions!: Array<TKEndpointControlConditionDefinitionViewModel> | null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////

    ////////////////
    //  GETTERS  //
    //////////////
    get description(): RuleDescription
    {
        return EndpointControlUtils.describeRuleExt(this.rule, this.endpointDefinitions, this.customResultDefinitions, this.conditionDefinitions);
    }

    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
</style>