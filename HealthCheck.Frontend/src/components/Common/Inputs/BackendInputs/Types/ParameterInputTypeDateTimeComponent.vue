<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeDateTimeComponent.vue -->
<template>
    <div>
        <v-text-field
            class="pt-0"
            v-model="localValue"
            required />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import DateUtils from  '../../../../../util/DateUtils';

@Component({
    components: {
    }
})
export default class ParameterInputTypeDateTimeComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    localValue: string = '';
    
    mounted(): void {
        this.updateLocalValue();
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
        this.localValue = this.value;
        this.validateValue();
    }

    @Watch('localValue')
    onLocalValueChanged(): void
    {
        this.validateValue();
        this.$emit('input', this.localValue);
    }

    validateValue(): void {
        if (this.localValue == null || this.localValue === '') {
            this.setValueToNow();
        }
    }

    setValueToNow(): void {
        this.localValue = DateUtils.FormatDate(new Date(), 'dd-MM-yy HH:mm:ss');
    }
}
</script>

<style scoped>
</style>
