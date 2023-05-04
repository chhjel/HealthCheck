<!-- src/components/modules/EventNotifications/ConfigFilterComponent.vue -->
<template>
    <div class="config-filter-component">
        <div class="field-list horizontal-layout">
            <text-field-component type="text"
                v-if="showPropertyName"
                v-model:value="propertyName"
                v-on:change="onDataChanged"
                :disabled="readonly"
                class="mb-2"
            ></text-field-component>

            <select-component
                v-model:value="matchType"
                :items="matchTypeOptions"
                item-text="text" item-value="value"
                v-on:change="onDataChanged"
                :disabled="readonly"
                class="spacer mb-2"
                >
            </select-component>

            <text-field-component type="text"
                v-model:value="filter"
                v-on:change="onDataChanged"
                :disabled="readonly"
                class="mb-2"
            ></text-field-component>
        
            <switch-component
                v-model:value="caseSensitive" 
                label="Case sensitive"
                color="secondary"
                v-on:change="onDataChanged"
                :disabled="readonly"
                class="mb-2"
            ></switch-component>
            
            <btn-component v-if="allowDelete"
                flat
                color="error"
                class="filter-action-button"
                @click="remove()"
                :disabled="readonly">
                Remove
            </btn-component>

            <btn-component
                flat
                color="secondary"
                class="filter-action-button"
                @click="isMatchingOnStringified = !isMatchingOnStringified"
                :disabled="readonly"
                title="Toggle between filtering on a property or the whole stringified event payload itself.">
                Toggle mode
            </btn-component>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { EventSinkNotificationConfigFilter, FilterMatchType } from "@models/modules/EventNotifications/EventNotificationModels";

@Options({
    components: {}
})
export default class ConfigFilterComponent extends Vue {
    @Prop({ required: true })
    config!: EventSinkNotificationConfigFilter;
    @Prop({ required: true })
    allowPropertyName!: boolean;
    @Prop({ required: false, default: false })
    readonly!: boolean;
    @Prop({ required: false, default: false })
    allowDelete!: boolean;

    filter!: string;
    propertyName!: string | null;
    matchType!: FilterMatchType;
    caseSensitive!: boolean;
    isMatchingOnStringified: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    beforeMount(): void {
        this.matchType = this.config.MatchType;
        this.filter = this.config.Filter || '';
        this.propertyName = this.config.PropertyName;
        this.caseSensitive = this.config.CaseSensitive;
        this.isMatchingOnStringified = !this.allowPropertyName || this.propertyName == null;
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get showPropertyName(): boolean
    {
        return this.allowPropertyName && !this.isMatchingOnStringified;
    }

    get matchTypeOptions(): any {
        let items = [
            { text: 'Contains', value: FilterMatchType.Contains},
            { text: 'Matches', value: FilterMatchType.Matches},
            { text: 'Starts with', value: FilterMatchType.StartsWith},
            { text: 'Ends with', value: FilterMatchType.EndsWith},
            { text: 'Matches RegEx', value: FilterMatchType.RegEx}
        ];

        if (this.isMatchingOnStringified)
        {
            items.forEach(x => {
                x.text = `Stringified payload ${x.text.toLowerCase()}`;
            });
        }
        return items;
    }

    ////////////////
    //  METHODS  //
    //////////////
    remove(): void {
        this.$emit('delete', this.config);
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDataChanged(): void {
        if (this.isMatchingOnStringified)
        {
            this.propertyName = null;
        }
        else if(this.propertyName == null)
        {
            this.propertyName = '';
        }

        let freshValues: Partial<EventSinkNotificationConfigFilter> ={
            MatchType: this.matchType,
            Filter: this.filter,
            PropertyName: this.propertyName,
            CaseSensitive: this.caseSensitive
        }

        let updatedObject = {...this.config, ...freshValues };
        this.$emit('change', updatedObject);
    }
}
</script>

<style scoped lang="scss">
.config-filter-component {
    margin-left: 20px;
    padding-left: 20px;

    @media (max-width: 900px) {
        margin-left: 0;
        padding-left: 0;
        margin-bottom: 40px;;
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
/*             
            @media (max-width: 900px) {
                width: 100%;
            } */
        }
    }
    .filter-action-button {
        position: relative;
        top: -2px;
    }
}
</style>

<style lang="scss">
.config-filter-component {
    .field-list {
        .btn-component.icon {
            margin-left: 0;
            width: 36px !important;
        }
    }
}
</style>
