<template>
    <div class="expansion-panel-component" :class="rootClasses">
		<h3>TODO: ExpansionPanelComponent</h3>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>popout:</b>' {{ popout }}'</div>

		<slot></slot>
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
    value!: string;

    @Prop({ required: false, default: false })
    popout!: string | boolean;

    localValue: string = "";

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
             'popout': this.isPopout
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
		this.$emit('update:input', this.localValue);
    }
}
</script>

<style scoped lang="scss">
.expansion-panel-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.popout { }
}
</style>
