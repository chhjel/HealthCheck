<!-- src/components/modules/EndpointControl/input/CustomBlockResultPropertyInputComponent.vue -->
<template>
    <div class="root">
        <input-component
            v-if="type == 'String' || type == 'Int32'"
            :type="textType"
            v-model="localValue"
            :name="definition.Name"
            :disabled="readonly"
            :description="definition.Description"
            :clearable="false"
        ></input-component>
        
        <input-header-component
            v-if="type == 'Boolean'"
            :name="definition.Name"
            :description="definition.Description"
            />
        <v-switch
            class="mt-0"
            v-if="type == 'Boolean'"
            :label="localValue ? 'Enabled' : 'Disabled'"
            v-model="localValue"
            :disabled="readonly" />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import InputComponent from '../../../Common/Basic/InputComponent.vue';
import InputHeaderComponent from '../../../Common/Basic/InputHeaderComponent.vue';
import { EndpointControlCustomResultPropertyDefinitionViewModel } from "../../../../models/modules/EndpointControl/EndpointControlModels";

@Component({
    components: { InputComponent, InputHeaderComponent }
})
export default class CustomBlockResultPropertyInputComponent extends Vue {
    @Prop({ required: true })
    value!: string;
    @Prop({ required: false, default: false })
    readonly!: boolean;
    @Prop({ required: true })
    definition!: EndpointControlCustomResultPropertyDefinitionViewModel;

    localValue: any = '';

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
        if (this.type == 'Boolean')
        {
            this.localValue = this.value === "true";
        }
        else {
            this.localValue = this.value;
        }
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get type(): string {
        return this.definition.Type;
    }

    get textType(): string {
        return this.type == 'String' ? 'text' : 'number';
    }

    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch('value')
    OnValueChanged(): void {
        this.localValue = this.value;
    }

    @Watch('localValue')
    OnLocalValueChanged(): void {
        this.$emit('input', this.localValue);
    }
}
</script>

<style scoped lang="scss">
</style>