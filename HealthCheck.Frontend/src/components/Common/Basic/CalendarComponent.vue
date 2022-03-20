<template>
    <div class="calendar-component" :class="rootClasses">
		<h3>TODO: CalendarComponent</h3>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>weekdays:</b>' {{ weekdays }}'</div>
        <div><b>type:</b>' {{ type }}'</div>
        <div><b>color:</b>' {{ color }}'</div>

		<slot></slot>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
    components: {}
})
export default class CalendarComponent extends Vue {

    @Prop({ required: true })
    value!: string;

    @Prop({ required: false, default: null })
    weekdays!: string;

    @Prop({ required: false, default: null })
    type!: string;

    @Prop({ required: false, default: null })
    color!: string;

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
.calendar-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;

}
</style>
