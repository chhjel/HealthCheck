<!-- src/components/modules/EndpointControl/RuleDescriptionComponent.vue -->
<template>
    <div class="root">
        <b>When</b>
        <ul>
            <li v-if="description.filters.length == 0">Always</li>
            <li v-for="(filter, fltIndex) in description.filters"
                :key="`rule-${rule.Id}-filter-${fltIndex}`">
                {{ filter }}
            </li>
        </ul>

        <div>
            <b>Limit to</b>
            <ul>
                <li v-if="description.limits.length == 0">- no limits defined -</li>
                <li v-for="(limit, limIndex) in description.limits"
                    :key="`rule-${rule.Id}-limit-${limIndex}`">
                    {{ limit }}
                </li>
            </ul>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { EndpointControlRule } from "../../../models/modules/EndpointControl/EndpointControlModels";
import EndpointControlUtils, { RuleDescription } from "../../../util/EndpointControl/EndpointControlUtils";

@Component({
    components: {  }
})
export default class RuleDescriptionComponent extends Vue {
    @Prop({ required: true })
    rule!: EndpointControlRule;

    //////////////////
    //  LIFECYCLE  //
    ////////////////

    ////////////////
    //  GETTERS  //
    //////////////
    get description(): RuleDescription
    {
        return EndpointControlUtils.describeRuleExt(this.rule);
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
/* .root {
} */
</style>