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

    @Prop({ required: false, default: false })
    dark!: string | boolean;

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
            'fixed': this.isFixed,
            'dark': this.isDark
        };
        classes[this.color || 'accent'] = true;
        return classes;
    }

    get isFixed(): boolean { return ValueUtils.IsToggleTrue(this.fixed); }
    get isDark(): boolean { return ValueUtils.IsToggleTrue(this.dark); }

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
    background-color: #f5f5f5;
    width: 100%;
    position: relative;
    z-index: 99;
    display: flex;

    &.fixed {
        position: fixed;
        top: 0;
        left: 0;
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
