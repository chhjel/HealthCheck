<!-- src/components/modules/EndpointControl/CountOverDurationComponent.vue -->
<template>
    <div class="root">
        <div class="field-list horizontal-layout">
            <input-component
                type="number"
                name="Request count"
                v-model="count"
                v-on:change="onDataChanged"
                :disabled="readonly"
                :clearable="false"
            ></input-component>

            <timespan-input-component
                v-model="duration"
                v-on:change="onDataChanged"
                name="Over duration of"
                :allowClear="false"
                :minimal="false"
                :disabled="readonly"
                />

            <v-btn
                dark small flat
                color="error"
                @click="remove()"
                :disabled="readonly">
                Remove
            </v-btn>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { EndpointControlCountOverDuration, EndpointControlRule } from "../../../models/modules/EndpointControl/EndpointControlModels";
import TimespanInputComponent from '../../Common/Basic/TimespanInputComponent.vue';
import InputComponent from '../../Common/Basic/InputComponent.vue';

@Component({
    components: { InputComponent, TimespanInputComponent }
})
export default class CountOverDurationComponent extends Vue {
    @Prop({ required: true })
    value!: EndpointControlCountOverDuration;
    @Prop({ required: false, default: false })
    readonly!: boolean;

    count: number = 10;
    duration: string = '0:1:0';

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    beforeMount(): void {
        this.count = this.value.Count;
        this.duration = this.value.Duration;
    }

    ////////////////
    //  GETTERS  //
    //////////////

    ////////////////
    //  METHODS  //
    //////////////
    remove(): void {
        this.$emit('delete', this.value);
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDataChanged(): void {
        let freshValues = {
            Count: this.count,
            Duration: this.duration,
            _frontendId: (<any>this.value)._frontendId
        }

        this.$emit('input', freshValues);
    }
}
</script>

<style scoped lang="scss">
.root {
    margin-left: 20px;
    padding-left: 20px;

    @media (max-width: 900px) {
        margin-left: 0;
        padding-left: 0;
        margin-bottom: 40px;;
    }

    .horizontal-layout {
        display: flex;
        align-items: center;
        flex-direction: row;
    }

    .field-list {
        @media (max-width: 900px) {
            align-items: start;
            flex-direction: column;
        }

        div {
            margin-right: 10px;
            
            @media (max-width: 900px) {
                width: 100%;
            }
        }
    }
}
</style>