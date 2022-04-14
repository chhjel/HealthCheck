<template>
    <div class="expansion-panel-component" :class="rootClasses" v-set-max-height:c>
        <div class="expansion-panel_header" @click="localValue = !localValue">
            <slot name="header"></slot>
        </div>
        <div class="expansion-panel_content">
		    <slot name="content"></slot>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'

@Options({
    components: {}
})
export default class ExpansionPanelComponent extends Vue {

    @Prop({ required: true })
    value!: boolean;

    @Prop({ required: false, default: false })
    popout!: string | boolean;

    localValue: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
        this.updateLocalValue();
        this.emitLocalValue();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get rootClasses(): any {
        return {
             'popout': this.isPopout,
             'open': this.value
        };
    }

    get isPopout(): boolean { return ValueUtils.IsToggleTrue(this.popout); }

    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
	
    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
		this.localValue = this.value;
    }

    @Watch('localValue')
    emitLocalValue(): void
    {
		this.$emit('update:value', this.localValue);
    }
}
</script>

<style scoped lang="scss">
.expansion-panel-component {
    box-shadow: 0 3px 1px -2px rgba(0,0,0,.2),0 2px 2px 0 rgba(0,0,0,.14),0 1px 5px 0 rgba(0,0,0,.12);
    border-radius: 4px;
    font-size: 14px;
    transition: all .25s ease-in-out;
    overflow: hidden;
    min-height: 42px;

    &.popout { }

    .expansion-panel_header {
        cursor: pointer;
        padding: 12px 24px;
        background-color: #f5f5f5;
    }

    .expansion-panel_content {
        padding: 12px 24px;
    }
    &:not(.open) {
        max-height: 0 !important;
        /* max-height: inherit !important; */
        .expansion-panel_content {
            padding: 0px 24px;
        }
    }
}
</style>
