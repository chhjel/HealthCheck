<template>
    <div class="pagination-component" :class="rootClasses">
		<h3>TODO: PaginationComponent</h3>
        <div><b>value:</b>' {{ value }}'</div>
        <div><b>length:</b>' {{ length }}'</div>
        <div><b>disabled:</b>' {{ disabled }}'</div>

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
export default class PaginationComponent extends Vue {

    @Prop({ required: true })
    value!: string;

    @Prop({ required: false, default: null })
    length!: string;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

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
             'disabled': this.isDisabled
        };
    }

    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }

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
.pagination-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.disabled { }
}
</style>
