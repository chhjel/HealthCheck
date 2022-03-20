<template>
    <div class="progress-circular-component" :class="rootClasses">
		<h3>TODO: ProgressCircularComponent</h3>
        <div><b>size:</b>' {{ size }}'</div>
        <div><b>width:</b>' {{ width }}'</div>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>color:</b>' {{ color }}'</div>
        <div><b>indeterminate:</b>' {{ indeterminate }}'</div>

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
export default class ProgressCircularComponent extends Vue {

    @Prop({ required: false, default: 0 })
    value!: number;

    @Prop({ required: false, default: null })
    size!: string;

    @Prop({ required: false, default: 0 })
    width!: number;

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: false })
    indeterminate!: string | boolean;

    localValue: number = 0;

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
             'indeterminate': this.isIndeterminate
        };
    }

    get isIndeterminate(): boolean { return ValueUtils.IsToggleTrue(this.indeterminate); }

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
.progress-circular-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.indeterminate { }
}
</style>
