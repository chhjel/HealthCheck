<template>
    <div class="switch-component" :class="rootClasses" tabindex="0" @keydown.enter="tryToggle" :disabled="isDisabled">
        <input type="checkbox" :id="`sw-${id}`"
            v-model="localValue"
            :disabled="disabled" />
        <span class="switch-component__toggle" @click="tryToggle" :class="toggleClasses"></span>
        <label :for="`sw-${id}`" v-if="resolvedLabel">{{ resolvedLabel }}</label>
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
export default class SwitchComponent extends Vue {

    @Prop({ required: true })
    value!: boolean;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: null })
    label!: string;

    @Prop({ required: false, default: null })
    falseLabel!: string;

    @Prop({ required: false, default: null })
    color!: string;
    
    @Prop({ required: false, default: false })
    ensureLabelHeight!: string | boolean;

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
        let classes = {
             'disabled': this.isDisabled,
             'ensure-label-height': this.isEnsureLabelHeight
        };
        return classes;
    }

    get toggleClasses(): any {
        let classes = {
        };
        classes[this.color || 'primary'] = true;
        return classes;
    }

    get resolvedLabel(): string | null {
        if (this.localValue != true) {
            return this.falseLabel || this.label;
        }
        return this.label;
    }

    get isEnsureLabelHeight(): boolean { return ValueUtils.IsToggleTrue(this.ensureLabelHeight); }
    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }

    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    tryToggle(): void {
        if (!this.isDisabled) this.localValue = !this.localValue;
    }
	
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
        this.$emit('change', this.localValue);
        this.$emit('blur', this.localValue);
    }
}
</script>

<style scoped lang="scss">
.switch-component {
    display: flex;
    align-items: center;

    input, label, .switch-component__toggle {
        user-select: none;
    }
    label {
        padding-left: 5px;
        flex: 1;
    }
    &:not(.disabled) {
        input, label, .switch-component__toggle {
            cursor: pointer;
        }
    }
    &.disabled {
        .switch-component__toggle {
            background: var(--color--accent-darken3);
        }
    }
    input {
        display: none;
        &:checked + .switch-component__toggle:before {
            left: 1.6em;
            transform: rotate(45deg);
        }
        &:not(:checked):not(:disabled) + .switch-component__toggle {
            background: var(--color--accent-darken8);
        }
    }
    &__toggle {
        display: block;
        position: relative;
        width: 3em;
        height: 1.6em;
        /* background: #50565a; */
        border-radius: 1em;
        transition: all 0.1s ease-in-out;

        &:before {
            content: "";
            display: block;
            width: 1.2em;
            height: 1.2em;
            border-radius: 1em;
            background: #f7f2f2;
            position: absolute;
            left: 0.2em;
            top: 0.2em;
            transition: all 0.2s ease-in-out;
        }
    }
    &.disabled { }
    &.ensure-label-height {
        margin-top: 19px;
    }
}
</style>
