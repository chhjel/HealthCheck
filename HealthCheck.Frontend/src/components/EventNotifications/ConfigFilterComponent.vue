<!-- src/components/Common/ConfigFilterComponent.vue -->
<template>
    <div class="root">
        <div class="horizontal-layout">
            
            <v-tooltip bottom>
                <template v-slot:activator="{ on }">
                    <v-btn v-on="on"
                        dark icon outline small
                        color="primary"
                        @click="isMatchingOnStringified = !isMatchingOnStringified"
                        :disabled="readonly">
                        <v-icon>code</v-icon>
                    </v-btn>
                </template>
                <span>
                    Toggle between filtering on a <b>property</b> or the <b>whole stringified event payload</b> itself.
                </span>
            </v-tooltip>
            
            <v-text-field type="text"
                v-if="showPropertyName"
                label="Property name"
                v-model="propertyName"
                v-on:change="onDataChanged"
                :disabled="readonly"
            ></v-text-field>

            <v-select
                v-model="matchType"
                :items="matchTypeOptions"
                item-text="text" item-value="value" color="secondary"
                v-on:change="onDataChanged"
                :disabled="readonly"
                >
            </v-select>

            <v-text-field type="text"
                label="Value to search for"
                v-model="filter"
                v-on:change="onDataChanged"
                :disabled="readonly"
            ></v-text-field>

            <v-switch
                v-model="caseSensitive" 
                label="Case sensitive"
                color="secondary"
                v-on:change="onDataChanged"
                :disabled="readonly"
            ></v-switch>
            
            <v-btn v-if="allowDelete"
                dark outline small
                color="error"
                @click="remove()"
                :disabled="readonly">
                <v-icon>delete</v-icon>
            </v-btn>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { EventSinkNotificationConfigFilter, FilterMatchType } from "../../models/EventNotifications/EventNotificationModels";

@Component({
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
        return [
            { text: 'Contains', value: FilterMatchType.Contains},
            { text: 'Matches', value: FilterMatchType.Matches},
            { text: 'RegEx', value: FilterMatchType.RegEx}
        ];
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

    .horizontal-layout {
        display: flex;
        align-items: center;
        flex-direction: row;
        flex-wrap: wrap;

        div {
            margin-right: 10px;
        }
    }
}
</style>