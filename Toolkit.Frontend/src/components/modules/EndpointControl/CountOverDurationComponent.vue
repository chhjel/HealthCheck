<!-- src/components/modules/EndpointControl/CountOverDurationComponent.vue -->
<template>
    <div class="count-over-duration-component">
        <div class="field-list horizontal-layout">
            <input-component
                type="number"
                name="Request count"
                v-model:value="count"
                v-on:change="onDataChanged"
                :disabled="readonly"
                :clearable="false"
                class="mb-2"
            ></input-component>

            <timespan-input-component
                v-model:value="duration"
                v-on:change="onDataChanged"
                name="Over duration of"
                :allowClear="false"
                :minimal="false"
                :disabled="readonly"
                class="mb-2"
                />

            <btn-component
                dark flat
                color="error"
                class="mt-3"
                @click="remove()"
                :disabled="readonly">
                Remove
            </btn-component>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { EndpointControlCountOverDuration, EndpointControlRule } from '@models/modules/EndpointControl/EndpointControlModels';
import TimespanInputComponent from '@components/Common/Basic/TimespanInputComponent.vue';
import InputComponent from '@components/Common/Basic/InputComponent.vue';

@Options({
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

        this.$emit('update:value', freshValues);
    }
}
</script>

<style scoped lang="scss">
.count-over-duration-component {
    margin-left: 20px;
    padding-left: 20px;

    @media (max-width: 900px) {
        margin-left: 0;
        padding-left: 0;
        margin-bottom: 20px;;
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