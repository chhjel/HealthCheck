<!-- src/components/Common/Basic/InputHeaderComponent.vue -->
<template>
    <div class="input-header-component">
        <div class="input-component--header" v-if="showHeader">
            <div class="input-component--header-name">{{ name }}</div>
            <icon-component small v-if="hasDescription"
                color="gray" class="input-component--help-icon"
                @click="toggleDescription">help</icon-component>
        </div>

        <div v-show="showDescription" class="input-component--description" v-html="description"></div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";

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

    ////////////////
    //  METHODS  //
    //////////////
    toggleDescription(): void {
        this.showDescription = !this.showDescription;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
.input-header-component {
    .input-component--header {
        text-align: left;

        .input-component--header-name {
            display: inline-block;
            font-size: 16px;
            color: var(--v-secondary-base);
            font-weight: 600;
        }

        .input-component--help-icon {
            user-select: none;
            font-size: 20px !important;
            &:hover {
                color: #1976d2;
            }
        }
    }

    .input-component--description {
        text-align: left;
        padding: 10px;
        border-radius: 10px;
        background-color: #ebf1fb;
    }
}
</style>
