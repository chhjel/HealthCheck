<!-- src/components/Common/ConfigFilterComponent.vue -->
<template>
    <div class="root">
        <v-switch
            v-if="allowPropertyName"
            v-model="isMatchingOnStringified" 
            :label="payloadMatchTypeToggleLable"
            color="secondary"
            v-on:change="onDataChanged"
        ></v-switch>

        <div class="horizontal-layout">
            <v-text-field type="text"
                v-if="showPropertyName"
                label="Target payload property name"
                v-model="propertyName"
                v-on:change="onDataChanged"
            ></v-text-field>

            <v-text-field type="text"
                label="Filter"
                v-model="filter"
                v-on:change="onDataChanged"
            ></v-text-field>

            <v-select
                v-model="matchType"
                :items="matchTypeOptions"
                item-text="text" item-value="value" color="secondary"
                v-on:change="onDataChanged"
                >
            </v-select>

            <v-switch
                v-model="caseSensitive" 
                label="Case sensitive"
                color="secondary"
                v-on:change="onDataChanged"
            ></v-switch>
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
    value!: EventSinkNotificationConfigFilter;
    @Prop({ required: true })
    allowPropertyName!: boolean;

    filter!: string;
    propertyName!: string | null;
    matchType!: FilterMatchType;
    caseSensitive!: boolean;
    isMatchingOnStringified: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    beforeMount(): void {
        this.matchType = this.value.MatchType;
        this.filter = this.value.Filter || '';
        this.propertyName = this.value.PropertyName;
        this.caseSensitive = this.value.CaseSensitive;
        this.isMatchingOnStringified = this.showPropertyName && this.propertyName == null;
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get showPropertyName(): boolean
    {
        return this.allowPropertyName && !this.isMatchingOnStringified;
    }

    get payloadMatchTypeToggleLable(): string
    {
        return this.isMatchingOnStringified
            ? "Now matching on stringified payload"
            : "Now matching on payload property values";
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

        let updatedObject = {...this.value, ...freshValues };
        this.$emit('input', updatedObject);
    }
}
</script>

<style scoped lang="scss">
.root {
    margin-left: 20px;
    padding-left: 20px;
    border-left: 2px solid gray;

    .horizontal-layout {
        display: flex;
        align-items: center;
    }
}
</style>