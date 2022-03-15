<template>
    <div class="alert-component" :class="rootClasses">
		<h3>TODO: AlertComponent</h3>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>color:</b>' {{ color }}'</div>
        <div><b>icon:</b>' {{ icon }}'</div>
        <div><b>outline:</b>' {{ outline }}'</div>
        <div><b>type:</b>' {{ type }}'</div>
        <div><b>elevation:</b>' {{ elevation }}'</div>

		<div v-if="value">
            <slot></slot>
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
export default class AlertComponent extends Vue {

    @Prop({ required: true })
    value!: boolean;

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: false })
    icon!: string | boolean;

    @Prop({ required: false, default: false })
    outline!: string | boolean;

    @Prop({ required: false, default: null })
    type!: string;

    @Prop({ required: false, default: null })
    elevation!: string;

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
             'icon': this.isIcon,
             'outline': this.isOutline
        };
    }

    get isIcon(): boolean { return ValueUtils.IsToggleTrue(this.icon); }
    get isOutline(): boolean { return ValueUtils.IsToggleTrue(this.outline); }

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
.alert-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.icon { }
    &.outline { }
}
</style>
