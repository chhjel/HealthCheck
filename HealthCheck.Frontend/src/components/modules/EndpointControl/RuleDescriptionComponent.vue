<!-- src/components/modules/EndpointControl/RuleDescriptionComponent.vue -->
<template>
    <div class="root">
        <b>When</b>
        <ul>
            <li v-if="description.filters.length == 0">Always</li>
            <li v-for="(filter, fltIndex) in description.filters"
                :key="`rule-${rule.Id}-filter-${fltIndex}`"
                class="anywrap">
                {{ filter }}
            </li>
        </ul>

        <div v-if="!rule.AlwaysTrigger">
            <b>And any of these limits are reached</b>
            <ul>
                <li v-if="description.limits.length == 0">- no limits defined -</li>
                <li v-for="(limit, limIndex) in description.limits"
                    :key="`rule-${rule.Id}-limit-${limIndex}`">
                    {{ limit }}
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

    //////////////////
    //  LIFECYCLE  //
    ////////////////

    ////////////////
    //  GETTERS  //
    //////////////
    get description(): RuleDescription
    {
        return EndpointControlUtils.describeRuleExt(this.rule, this.endpointDefinitions, this.customResultDefinitions);
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