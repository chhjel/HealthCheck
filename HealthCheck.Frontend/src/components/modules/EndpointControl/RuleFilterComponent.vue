<!-- src/components/modules/EndpointControl/RuleFilterComponent.vue -->
<template>
    <div class="rule-filter-component">
        <div class="field-list horizontal-layout">
            <switch-component
                v-model:value="enabled" 
                falseLabel="Filter disabled"
                color="secondary"
                v-on:change="onDataChanged"
                :disabled="readonly"
            ></switch-component>
            
            <div class="horizontal-layout" v-if="enabled">
                <select-component
                    class="mode-select"
                    v-model:value="filterMode"
                    :items="filterModeOptions"
                    item-text="text" item-value="value"
                    v-on:change="onDataChanged"
                    :disabled="readonly"
                    >
                </select-component>
            </div>

            <select-component v-if="enabled"
                v-model:value="filterValue"
                :items="filterContentOptions"
                :readonly="readonly"
                no-data-text="Value required"
                placeholder="Value to search for"
                v-on:change="onDataChanged"
                :disabled="readonly"
                allowInput allowCustom
                >
            </select-component>
            <div class="horizontal-layout" v-if="enabled">
                <switch-component
                    v-model:value="caseSensitive" 
                    label="Case sensitive"
                    color="secondary"
                    v-on:change="onDataChanged"
                    :disabled="readonly"
                ></switch-component>

                <switch-component
                    v-model:value="inverted" 
                    label="Inverted"
                    color="secondary"
                    v-on:change="onDataChanged"
                    :disabled="readonly"
                ></switch-component>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { EndpointControlFilterMode, EndpointControlPropertyFilter, EndpointControlRule } from '@models/modules/EndpointControl/EndpointControlModels';

@Options({
    components: {}
})
export default class RuleFilterComponent extends Vue {
    @Prop({ required: true })
    value!: EndpointControlPropertyFilter;
    @Prop({ required: false, default: false })
    readonly!: boolean;
    @Prop({ required: false, default: () => [] })
    filterOptions!: Array<string>;

    enabled: boolean = false;
    inverted: boolean = false;
    filterValue: string = '';
    filterMode: EndpointControlFilterMode = EndpointControlFilterMode.Matches;
    caseSensitive: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    beforeMount(): void {
        this.enabled = this.value.Enabled;
        this.inverted = this.value.Inverted;
        this.filterValue = this.value.Filter;
        this.filterMode = this.value.FilterMode;
        this.caseSensitive = this.value.CaseSensitive;
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get filterContentOptions(): Array<string> {
        return this.filterOptions ?? [];
    }

    get filterModeOptions(): any {
        let items = [
            { text: 'Contains', value: EndpointControlFilterMode.Contains},
            { text: 'Matches', value: EndpointControlFilterMode.Matches},
            { text: 'Starts with', value: EndpointControlFilterMode.StartsWith},
            { text: 'Ends with', value: EndpointControlFilterMode.EndsWith},
            { text: 'Matches RegEx', value: EndpointControlFilterMode.RegEx}
        ];
        return items;
    }

    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDataChanged(): void {
        let freshValues: Partial<EndpointControlPropertyFilter> = {
            Enabled: this.enabled,
            FilterMode: this.filterMode,
            Filter: this.filterValue,
            Inverted: this.inverted,
            CaseSensitive: this.caseSensitive
        }

        let updatedObject = {...this.value, ...freshValues };
        this.$emit('update:value', updatedObject);
    }
}
</script>

<style scoped lang="scss">
.rule-filter-component {
    margin-left: 20px;
    padding-left: 20px;

    @media (max-width: 900px) {
        margin-left: 0;
        padding-left: 0;
        margin-bottom: 40px;;
    }

    .mode-select {
        max-width: 230px;
    }

    .horizontal-layout {
        display: flex;
        align-items: center;
        flex-direction: row;
    }

    .field-list {
        @media (max-width: 900px) {
            align-items: start;
            flex-direction: column;
        }

        div {
            margin-right: 10px;
            
            @media (max-width: 900px) {
                width: 100%;
            }
        }
    }
}
</style>