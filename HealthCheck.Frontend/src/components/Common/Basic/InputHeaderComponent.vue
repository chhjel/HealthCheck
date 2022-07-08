<!-- src/components/Common/Basic/InputHeaderComponent.vue -->
<template>
    <div class="input-header-component" v-if="showHeader || showDescription || isEnsureHeight">
        <div class="input-component--header" v-if="showHeader">
            <div class="input-component--header-name">{{ name }}</div>
            <icon-component small v-if="hasDescription"
                color="gray" class="input-component--help-icon clickable"
                @click="toggleDescription">help</icon-component>
            <icon-component v-if="showActionIcon"
                color="gray" class="input-component--action-icon clickable"
                @click="onActionIconClicked">{{ actionIcon }}</icon-component>
        </div>

        <div v-show="showDescription" class="input-component--description" v-html="description"></div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from "@util/ValueUtils";

@Options({
    components: {}
})
export default class InputHeaderComponent extends Vue
{
    @Prop({ required: false, default: '' })
    name!: string;
    
    @Prop({ required: false, default: '' })
    description!: string;
    
    @Prop({ required: false, default: false })
    showDescriptionOnStart!: boolean;

    @Prop({ required: false, default: false })
    ensureHeight!: string | boolean;
    
    @Prop({ required: false, default: '' })
    actionIcon!: string;

    showDescription: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
        this.showDescription = this.hasDescription && this.showDescriptionOnStart;
    }

    mounted(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////

    get showHeader(): boolean {
        return this.name != null && this.name.length > 0;
    }

    get hasDescription(): boolean {
        return this.description != null && this.description.length > 0;
    }

    get isEnsureHeight(): boolean { return ValueUtils.IsToggleTrue(this.ensureHeight); }

    get showActionIcon(): boolean {
        return !!this.actionIcon && this.actionIcon.length > 0;
    }

    ////////////////
    //  METHODS  //
    //////////////
    toggleDescription(): void {
        this.showDescription = !this.showDescription;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onActionIconClicked(): void {
        this.$emit('actionIconClicked');
    }
}
</script>

<style scoped lang="scss">
.input-header-component {
    min-height: 19px;
    
    .input-component--header {
        text-align: left;
        display: flex;
        flex-wrap: nowrap;
        align-items: center;
        
        .input-component--header-name {
            display: inline-block;
            font-size: 16px;
            color: var(--color--secondary-base);
            font-weight: 600;
        }

        .input-component--help-icon {
            user-select: none;
            font-size: 20px !important;
            margin-left: 2px;
            &:hover {
                color: var(--color--info-base) !important;
            }
        }
        .input-component--action-icon {
            user-select: none;
            font-size: 20px !important;
            margin-left: 4px;
            &:hover {
                color: var(--color--info-base) !important;
            }
        }
    }

    .input-component--description {
        text-align: left;
        padding: 10px;
        border-radius: 10px;
        color: var(--color--text) !important;
        background-color: #ebf1fb;
    }
}
</style>
