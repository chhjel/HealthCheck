<!-- src/components/Common/Basic/TimespanInputComponent.vue -->
<template>
    <div class="input-component timespan">
        <input-header-component :name="name" :description="description" :showDescriptionOnStart="showDescriptionOnStart" />
        
        <div class="input-component--inputs" :style="style">
            <div class="input-component--input-prefix" v-if="minimal">
                <tooltip-component tooltip="Number of hours">
                    <span>H:</span>
                </tooltip-component>
            </div>
            <text-field-component
                v-model:value="hourValue"
                @input="onInput()"
                :disabled="disabled"
                :placeholder="isNull ? 'null' : ''"
                type="number"
                class="spacer">
            </text-field-component>
            <div class="input-component--input-prefix" v-if="!minimal">hours, </div>
            
            <div class="input-component--input-prefix" v-if="minimal">
                <tooltip-component tooltip="Number of minutes">
                    <span>M:</span>
                </tooltip-component>
            </div>
            <text-field-component
                v-model:value="minuteValue"
                @input="onInput()"
                :disabled="disabled"
                :placeholder="isNull ? 'null' : ''"
                type="number"
                class="spacer">
            </text-field-component>
            <div class="input-component--input-prefix" v-if="!minimal">minutes, </div>

            <div class="input-component--input-prefix" v-if="minimal">
                <tooltip-component tooltip="Number of seconds">
                    <span>S:</span>
                </tooltip-component>
            </div>
            <text-field-component
                v-model:value="secondValue"
                @input="onInput()"
                :disabled="disabled"
                :placeholder="isNull ? 'null' : ''"
                type="number"
                class="spacer">
            </text-field-component>
            <div class="input-component--input-prefix" v-if="!minimal">seconds.</div>

            <btn-component 
                v-if="allowClear"
                class="clear-button"
                @click="onClearClicked"
                :disabled="disabled"
                flat icon small>
                <icon-component color="#757575">clear</icon-component>
            </btn-component>
        </div>

        <div class="input-component--error" v-if="error != null && error.length > 0">{{ error }}</div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import InputHeaderComponent from "./InputHeaderComponent.vue";

@Options({
    components: { InputHeaderComponent }
})
export default class TimespanInputComponent extends Vue
{
    @Prop({ required: true })
    value!: string;
    
    @Prop({ required: false, default: '' })
    name!: string;
    
    @Prop({ required: false, default: '' })
    description!: string;
    
    @Prop({ required: false, default: null})
    error!: string | null;
    
    @Prop({ required: false, default: false })
    disabled!: boolean;
    
    @Prop({ required: false, default: true })
    allowClear!: boolean;
    
    @Prop({ required: false, default: false })
    showDescriptionOnStart!: boolean;
    
    @Prop({ required: false, default: false })
    minimal!: boolean;
    
    @Prop({ required: false, default: false })
    fill!: boolean;
    
    @Prop({ required: false, default: 23 })
    maxHour!: number | null;

    showDescription: boolean = false;
    currentValue: string = '0:0:0';

    hourValue: string = '0';
    minuteValue: string = '0';
    secondValue: string = '0';

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
        this.onValueChanged();
    }

    mounted(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get isNull(): boolean {
        return this.value == null || this.value == undefined;
    }

    get style(): any {
        if (this.fill)
        {
            return {
                'justify-content': 'space-between'
            }
        }
        return {};
    }

    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch('value')
    onValueChanged(): void {
        const parts = (this.value || '').split(':');

        if (parts.length < 3) {
            this.currentValue = '0:0:0';
            this.hourValue = this.minuteValue = this.secondValue = (this.value == null ? '' : '0');
            return;
        }

        this.hourValue = Number(parts[0]).toString();
        this.minuteValue = Number(parts[1]).toString();
        this.secondValue = Number(parts[2]).toString();
        this.currentValue = `${this.hourValue}:${this.minuteValue}:${this.secondValue}`;
    }

    onInput(): void {
        if (this.maxHour != null && this.maxHour != undefined && Number(this.hourValue) > this.maxHour) {
            this.hourValue = `${this.maxHour}`;
        }
        if (Number(this.minuteValue) > 59) {
            this.minuteValue = '59';
        }
        if (Number(this.secondValue) > 59) {
            this.secondValue = '59';
        }
        
        this.currentValue = `${this.hourValue}:${this.minuteValue}:${this.secondValue}`;
        this.$emit('update:value', this.currentValue);
        this.$emit('change', this.currentValue);
    }

    onClearClicked(): void {
        if (this.disabled) return;
        this.$emit('update:value', '');
        this.$emit('change', '');
    }
}
</script>

<style scoped lang="scss">
</style>

<style lang="scss">
.input-component {
    input {
        font-size: 18px;
        color: #000 !important;
    }

    .input-component--error {
        margin-top: -21px;
        margin-left: 2px;
        font-weight: 600;
        color: var(--color--error-base) !important;
    }

    &--inputs {
        display: flex;
        flex-direction: row;
        flex-wrap: nowrap;
        align-items: baseline;
    }

    &--input-prefix {
        font-size: 18px;
        margin-right: 5px;
    }

    .clear-button {
        margin: 0 !important;
        padding: 0;
        width: 28px;
    }
}
</style>