<template>
    <div class="snackbar-component" :class="rootClasses">
		  <!-- <h3>TODO: SnackbarComponent</h3>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>timeout:</b>' {{ timeout }}'</div>
        <div><b>color:</b>' {{ color }}'</div> -->

		  <slot></slot>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
    components: {}
})
export default class SnackbarComponent extends Vue {

    @Prop({ required: true })
    value!: boolean;

    @Prop({ required: false, default: null })
    timeout!: string;

    @Prop({ required: false, default: null })
    color!: string;

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
.snackbar-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;

}
</style>
