<template>
    <div class="expansion-panel-component" :class="rootClasses">
        <div class="expansion-panel_header" @click="localValue = !localValue" v-if="showHeader">
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

    @Prop({ required: false, default: true })
    showHeader!: boolean;

    @Prop({ required: false, default: false })
    cleanMode!: boolean;

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
             'open': this.value,
             'clean': this.cleanMode
        };
    }

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
    border-radius: 4px;
    font-size: 14px;
    overflow: hidden;
    min-height: 42px;

    &:not(.clean) {
        box-shadow: 0 3px 1px -2px rgba(0,0,0,.2),0 2px 2px 0 rgba(0,0,0,.14),0 1px 5px 0 rgba(0,0,0,.12);
    }

    .expansion-panel_header {
        cursor: pointer;
        padding: 12px 24px;
        background-color: #f5f5f5;
    }

    .expansion-panel_content {
        padding: 0px 24px;
    }
    &:not(.open) {
        .expansion-panel_content {
            display: none;
        }
    }
    &.open {
        .expansion-panel_content {
            animation: fade-in .3s ease-in-out;
            padding: 12px 24px;
        }
    }
}

@keyframes fade-in {
  0% {
    display: none;
    opacity: 0;
  }
  1% {
    display: block;
    opacity: 0;
  }
  100% {
    opacity: 1;
  }
}
</style>
