<!-- src/components/modules/EventNotifications/ConfigFilterComponent.vue -->
<template>
    <div class="root">
        <div class="field-list horizontal-layout">
            
            <div class="horizontal-layout">
                <tooltip-component bottom>
                    <template v-slot:activator="{ on }">
                        <btn-component v-on="on"
                            dark icon small
                            :color="!isMatchingOnStringified ? `primary` : 'secondary'"
                            :class="{ 'lighten-5': isMatchingOnStringified }"
                            @click="isMatchingOnStringified = !isMatchingOnStringified"
                            :disabled="readonly">
                            <icon-component>code</icon-component>
                        </btn-component>
                    </template>
                    <span>
                        Toggle between filtering on a <b>property</b> or the <b>whole stringified event payload</b> itself.
                    </span>
                </tooltip-component>
                
                <text-field-component type="text"
                    v-if="showPropertyName"
                    label="Property name"
                    v-model:value="propertyName"
                    v-on:change="onDataChanged"
                    :disabled="readonly"
                ></text-field-component>

                <v-select
                    v-model:value="matchType"
                    :items="matchTypeOptions"
                    item-text="text" item-value="value" color="secondary"
                    v-on:change="onDataChanged"
                    :disabled="readonly"
                    >
                </v-select>
            </div>

            <text-field-component type="text"
                label="Value to search for"
                v-model:value="filter"
                v-on:change="onDataChanged"
                :disabled="readonly"
            ></text-field-component>

            <div class="horizontal-layout">
                <v-switch
                    v-model:value="caseSensitive" 
                    label="Case sensitive"
                    color="secondary"
                    v-on:change="onDataChanged"
                    :disabled="readonly"
                ></v-switch>
                
                <!-- <tooltip-component bottom>
                    <template v-slot:activator="{ on }">
                        <btn-component v-on="on"
                            dark icon small
                            :color="caseSensitive ? `primary` : 'secondary'"
                            :class="{ 'lighten-5': !caseSensitive }"
                            @click="caseSensitive = !caseSensitive; onDataChanged();"
                            :disabled="readonly">
                            <icon-component small>text_fields</icon-component>
                        </btn-component>
                    </template>
                    <span>
                        Toggle case-sensitive filtering.
                    </span>
                </tooltip-component> -->
                
                <btn-component v-if="allowDelete"
                    dark small flat
                    color="error"
                    @click="remove()"
                    :disabled="readonly">
                    Remove
                    <!-- <icon-component>delete</icon-component> -->
                </btn-component>
            </div>
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
.root {
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
            
            @media (max-width: 900px) {
                width: 100%;
            }
        }
    }
}
</style>