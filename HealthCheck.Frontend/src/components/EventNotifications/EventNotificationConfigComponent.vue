<!-- src/components/Common/EventNotificationConfigComponent.vue -->
<template>
    <div class="root">
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { EventSinkNotificationConfigFilter, FilterMatchType } from "../../models/EventNotifications/EventNotificationModels";

@Component({
    components: {}
})
export default class EventNotificationConfigComponent extends Vue {
    @Prop({ required: true })
    value!: EventSinkNotificationConfigFilter;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    beforeMount(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////

    ////////////////
    //  METHODS  //
    //////////////


    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDataChanged(): void {
        // let freshValues: Partial<EventSinkNotificationConfigFilter> ={
        //     MatchType: this.matchType,
        //     Filter: this.filter,
        //     PropertyName: this.propertyName,
        //     CaseSensitive: this.caseSensitive
        // }

        // let updatedObject = {...this.value, ...freshValues };
        // this.$emit('input', updatedObject);
    }
}
</script>

<style scoped lang="scss">
.root {
}
</style>