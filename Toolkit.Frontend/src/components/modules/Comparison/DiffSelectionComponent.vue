<!-- src/components/modules/Comparison/DiffSelectionComponent.vue -->
<template>
    <div class="diff-selection clickable hoverable-lift-light" :class="classes">
        <div class="diff-selection__header">
            <icon-component class="diff-selection__icon mr-1">{{ icon }}</icon-component>
            <div class="diff-selection__title" v-if="name">
                {{ name }}
            </div>
        </div>
        <div class="diff-selection__description" v-if="description">
            {{ description }}
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
export default class DiffSelectionComponent extends Vue {
    @Prop({ required: false, default: false })
    enabled!: boolean;

    @Prop({ required: false, default: null })
    name!: string | null;

    @Prop({ required: true, default: null })
    description!: string | null;

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

    get classes(): any {
        let classes = {};
        classes['enabled'] = this.enabled;
        return classes;
    }

    get icon(): string {
        return this.enabled ? 'check_circle' : 'radio_button_unchecked';
    }
}
</script>

<style scoped lang="scss">
.diff-selection {
    padding: 10px;
    margin: 10px;
    text-align: left;
    border: 2px solid var(--color--accent-base);
    color: var(--color--accent-darken10);
    user-select: none;

    &__header {
        display: flex;
        flex-wrap: nowrap;
    }
    &__title {
        font-size: 20px;
        font-weight: 600;
        user-select: text;
    }
    &__description {
        margin-top: 10px;
        font-size: 15px;
    }

    &.enabled {
        border: 2px solid var(--color--primary-darken1);
        .diff-selection__title {
            color: var(--color--primary-darken1);
        }
        .diff-selection__icon {
            color: var(--color--primary-base);
        }
    }
}
</style>
