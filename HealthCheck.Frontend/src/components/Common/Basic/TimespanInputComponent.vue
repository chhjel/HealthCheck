<!-- src/components/Common/Basic/TimespanInputComponent.vue -->
<template>
    <div class="input-component timespan">
        <div class="input-component--header" v-if="showHeader">
            <div class="input-component--header-name">{{ name }}</div>
            <v-icon small v-if="hasDescription"
                color="gray" class="input-component--help-icon"
                @click="toggleDescription">help</v-icon>
        </div>

        <div v-show="showDescription" class="input-component--description" v-html="description"></div>
        
        <div class="input-component--inputs">
            <div class="input-component--input-prefix" v-if="minimal">H:</div>
            <v-text-field
                v-model="hourValue"
                @input="onInput()"
                :disabled="disabled"
                type="number">
            </v-text-field>
            <div class="input-component--input-prefix" v-if="!minimal">hours, </div>
            
            <div class="input-component--input-prefix" v-if="minimal">M:</div>
            <v-text-field
                v-model="minuteValue"
                @input="onInput()"
                :disabled="disabled"
                type="number">
            </v-text-field>
            <div class="input-component--input-prefix" v-if="!minimal">minutes, </div>

            <div class="input-component--input-prefix" v-if="minimal">S:</div>
            <v-text-field
                v-model="secondValue"
                @input="onInput()"
                :disabled="disabled"
                type="number">
            </v-text-field>
            <div class="input-component--input-prefix" v-if="!minimal">seconds.</div>

            <v-btn 
                v-if="allowClear"
                class="clear-button"
                @click:clear="onClearClicked()"
                :disabled="disabled"
                flat icon small>
                <v-icon color="#757575">cancel</v-icon>
            </v-btn>
        </div>

        <div class="input-component--error" v-if="error != null && error.length > 0">{{ error }}</div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";

@Component({
    components: {}
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

    showDescription: boolean = false;
    currentValue: string = '0:0:0';

    hourValue: string = '0';
    minuteValue: string = '0';
    secondValue: string = '0';

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
        this.showDescription = this.hasDescription && this.showDescriptionOnStart;
        this.onValueChanged();
    }

    mounted(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get showHeader(): boolean {
        return this.name != null && this.name.length > 0;
    }

    get hasDescription(): boolean {
        return this.description != null && this.description.length > 0;
    }

    ////////////////
    //  METHODS  //
    //////////////
    toggleDescription(): void {
        this.showDescription = !this.showDescription;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch('value')
    onValueChanged(): void {
        const parts = this.value.split(':');

        if (parts.length < 3) {
            this.currentValue = '0:0:0';
            this.hourValue = this.minuteValue = this.secondValue;
            return;
        }

        this.hourValue = Number(parts[0]).toString();
        this.minuteValue = Number(parts[1]).toString();
        this.secondValue = Number(parts[2]).toString();
        this.currentValue = `${this.hourValue}:${this.minuteValue}:${this.secondValue}`;
    }

    onInput(): void {
        if (Number(this.hourValue) > 23) {
            this.hourValue = '23';
        }
        if (Number(this.minuteValue) > 59) {
            this.minuteValue = '59';
        }
        if (Number(this.secondValue) > 59) {
            this.secondValue = '59';
        }
        
        this.currentValue = `${this.hourValue}:${this.minuteValue}:${this.secondValue}`;
        this.$emit('input', this.currentValue);
        this.$emit('change', this.currentValue);
    }

    onClearClicked(): void {
        if (this.disabled) return;
        this.$emit('input', '');
    }
}
</script>

<style scoped lang="scss">
.input-component {
    .input-component--header {
        text-align: left;

        .input-component--header-name {
            display: inline-block;
            font-size: 16px;
            color: var(--v-secondary-base);
            font-weight: 600;
        }

        .input-component--help-icon {
            user-select: none;
            font-size: 20px !important;
            &:hover {
                color: #1976d2;
            }
        }
    }

    .input-component--description {
        text-align: left;
        padding: 10px;
        border-radius: 10px;
        background-color: #ebf1fb;
    }

    .clickable {
        cursor: pointer;
    }
}
</style>

<style lang="scss">
.input-component {
    input {
        font-size: 18px;
        color: #000 !important;
    }

    .v-input {
        padding-top: 0;
    }

    .input-component--error {
        margin-top: -21px;
        margin-left: 2px;
        font-weight: 600;
        color: var(--v-error-base) !important;
    }

    &--inputs {
        display: flex;
        flex-direction: row;
        flex-wrap: nowrap;
        align-items: baseline;

        .v-input {
            max-width: 80px;
        }
    }

    &--input-prefix {
        font-size: 18px;
        margin-right: 5px;
    }

    .clear-button {
        align-self: end;
    }
}
</style>