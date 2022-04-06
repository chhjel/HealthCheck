<template>
    <div class="toolbar-component" :class="rootClasses">
        <div class="toolbar-component__prefix">
            <slot name="prefix"></slot>
        </div>
        <div class="toolbar-component__content">
		    <slot></slot>
        </div>
        <div class="toolbar-component__suffix">
            <slot name="suffix"></slot>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from "@util/ValueUtils";

@Options({
    components: {}
})
export default class ToolbarComponent extends Vue {
    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: false })
    fixed!: string | boolean;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {

    }

    ////////////////
    //  GETTERS  //
    //////////////
    get rootClasses(): any {
        let classes = {
            'fixed': this.isFixed
        };
        classes[this.color || 'accent'] = true;
        return classes;
    }

    get isFixed(): boolean { return ValueUtils.IsToggleTrue(this.fixed); }

    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
	
    /////////////////
    //  WATCHERS  //
    ///////////////

}
</script>

<style scoped lang="scss">
.toolbar-component {
    background-color: var(--color--background-bright);
    box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.02), 0 3px 2px 0 rgba(0, 0, 0, 0.02), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
    width: 100%;
    position: relative;
    z-index: 99;
    display: flex;

    &.fixed {
        position: fixed;
        top: 0;
    }

    &__content {
        display: flex;
        flex-direction: row;
        flex-wrap: nowrap;
        overflow-y: hidden;
        overflow-x: auto;
        overflow: overlay hidden;
        -ms-overflow-style: none;
        height: 56px;

        &::-webkit-scrollbar {
            display: none;
        }
         
        @media (min-width: 960px) {
            height: 64px;
        }
    }
}
</style>

<style lang="scss">
.toolbar-component {
    .btn-component {
        border: none;
        box-shadow: none !important;
        padding: 5px 16px;
        flex-shrink: 0;
        margin: 0;
    }
}
</style>
