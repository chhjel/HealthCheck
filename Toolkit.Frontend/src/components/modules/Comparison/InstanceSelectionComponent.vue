<!-- src/components/modules/Comparison/ComparisonPageComponent.vue -->
<template>
    <div class="instance-selection clickable hoverable-lift-light" :class="classes">
        <div class="instance-selection__title" v-if="name">
            {{ name }}
        </div>
        <div class="instance-selection__description" v-if="description">
            {{ description }}
        </div>
        <div class="instance-selection__empty" v-if="!hasSelection">
            {{ placeholder }}
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import IdUtils from "@util/IdUtils";

@Options({
    components: {
    }
})
export default class InstanceSelectionComponent extends Vue {
    @Prop({ required: false, default: null })
    name!: string | null;

    @Prop({ required: true, default: null })
    description!: string | null;

    @Prop({ required: false, default: '- Click to select data -' })
    placeholder!: string;

    id: string = IdUtils.generateId();

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted()
    {
    }

    ////////////////
    //  METHODS  //
    //////////////
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get hasSelection(): boolean {
        return !!this.name;
    }

    get classes(): any {
        let classes = {};
        classes['has-selection'] = this.hasSelection;
        return classes;
    }
}
</script>

<style scoped lang="scss">
.instance-selection {
    padding: 20px;
    margin: 20px;
    text-align: left;
    border: 4px solid var(--color--accent-base);
    user-select: none;

    &__title {
        font-size: 20px;
        font-weight: 600;
    }
    &__description {
        margin-top: 10px;
        font-size: 15px;
        color: #6a6a6a;
    }
    &__empty {
        text-align: center;
        font-size: 14px;
        color: var(--color--primary-darken1);
        font-weight: 600;
        white-space: pre;
    }

    &.has-selection {
        color: var(--color--primary-darken1);
        border: 2px solid var(--color--primary-darken1);
    }
}
</style>
