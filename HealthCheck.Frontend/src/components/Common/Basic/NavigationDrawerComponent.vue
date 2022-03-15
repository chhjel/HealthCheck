<template>
    <div class="navigation-drawer-component" :class="rootClasses">
		<h3>TODO: NavigationDrawerComponent</h3>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>clipped:</b>' {{ clipped }}'</div>
        <div><b>floating:</b>' {{ floating }}'</div>
        <div><b>mobileBreakPoint:</b>' {{ mobileBreakPoint }}'</div>

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
export default class NavigationDrawerComponent extends Vue {

    @Prop({ required: true })
    value!: string;

    @Prop({ required: false, default: false })
    clipped!: string | boolean;

    @Prop({ required: false, default: false })
    floating!: string | boolean;

    @Prop({ required: false, default: null })
    mobileBreakPoint!: string;

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
             'clipped': this.isClipped,
             'floating': this.isFloating
        };
    }

    get isClipped(): boolean { return ValueUtils.IsToggleTrue(this.clipped); }
    get isFloating(): boolean { return ValueUtils.IsToggleTrue(this.floating); }

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
.navigation-drawer-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.clipped { }
    &.floating { }
}
</style>
