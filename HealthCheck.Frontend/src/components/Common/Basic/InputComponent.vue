<!-- src/components/Common/Basic/InputComponent.vue -->
<template>
    <div class="input-component">
        <div class="input-component--header" v-if="showHeader">
            <div class="input-component--header-name">{{ name }}</div>
            <v-icon small v-if="hasDescription"
                color="gray" class="input-component--help-icon"
                @click="toggleDescription">help</v-icon>
        </div>

        <div v-show="showDescription" class="input-component--description" v-html="description"></div>
        
        <v-text-field
            v-model="currentValue"
            @input="onInput($event)"
            @click:clear="onClearClicked()"
            :disabled="disabled"
            :type="type"
            clearable>

            <v-tooltip slot="append-outer" bottom v-if="showActionIcon">
                <v-icon slot="activator" @click="onActionIconClicked">{{ actionIcon }}</v-icon>
                Insert placeholder
            </v-tooltip>
        </v-text-field>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";

@Component({
    components: {}
})
export default class InputComponent extends Vue
{
    @Prop({ required: true })
    value!: string;
    
    @Prop({ required: false, default: '' })
    name!: string;
    
    @Prop({ required: false, default: '' })
    description!: string;
    
    @Prop({ required: false, default: '' })
    actionIcon!: string;
    
    @Prop({ required: false, default: 'text' })
    type!: string;
    
    @Prop({ required: false, default: false })
    disabled!: boolean;

    showDescription: boolean = false;
    currentValue: string = '';

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created(): void {
        this.currentValue = this.value;
    }

    mounted(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get showActionIcon(): boolean {
        return this.actionIcon != null && this.actionIcon.length > 0;
    }

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
        this.currentValue = this.value;
    }

    onInput(newValue: string): void {
        this.$emit('input', newValue);
    }

    onClearClicked(): void {
        this.$emit('input', '');
    }

    onActionIconClicked(): void {
        this.$emit('actionIconClicked');
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
}
</style>