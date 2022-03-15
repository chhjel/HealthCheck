<template>
    <div class="list-component" :class="rootClasses">
		<h3>TODO: ListComponent</h3>
        <div><b>expand:</b>' {{ expand }}'</div>
        <div><b>dense:</b>' {{ dense }}'</div>

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
export default class ListComponent extends Vue {

    @Prop({ required: false, default: false })
    expand!: string | boolean;

    @Prop({ required: false, default: false })
    dense!: string | boolean;


    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {

    }

    ////////////////
    //  GETTERS  //
    //////////////
    get rootClasses(): any {
        return {
             'expand': this.isExpand,
             'dense': this.isDense
        };
    }

    get isExpand(): boolean { return ValueUtils.IsToggleTrue(this.expand); }
    get isDense(): boolean { return ValueUtils.IsToggleTrue(this.dense); }

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
.list-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.expand { }
    &.dense { }
}
</style>
