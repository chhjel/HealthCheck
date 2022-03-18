<template>
    <div class="checkbox-component" :class="rootClasses">
        <input type="checkbox" :id="`cb-${id}`" v-model="value" />
        <label :for="`cb-${id}`">{{ label }}</label>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'
import IdUtils from "@util/IdUtils";

@Options({
    components: {}
})
export default class CheckboxComponent extends Vue {

    @Prop({ required: true })
    value!: boolean;

    @Prop({ required: false, default: null })
    label!: string;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: null })
    inputValue!: string;

    id: string = IdUtils.generateId();
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
		this.$emit('update:input', this.localValue);
    }
}
</script>

<style scoped lang="scss">
.checkbox-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;
    &.disabled { }
}
</style>
