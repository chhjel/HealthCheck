<template>
    <div class="alert-component" :class="rootClasses" v-if="value">
        <icon-component v-if="icon">{{ icon }}</icon-component>
        <div class="alert-component_content"><slot></slot></div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'

@Options({
    components: {}
})
export default class AlertComponent extends Vue {

    @Prop({ required: true })
    value!: boolean;

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: '' })
    icon!: string;

    @Prop({ required: false, default: false })
    outline!: string | boolean;

    @Prop({ required: false, default: null })
    type!: 'error' | 'info' | 'warning';

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
            'icon': this.hasIcon,
            'outline': this.isOutline
        };
        classes[this.type] = true;
        return classes;
    }

    get hasIcon(): boolean { return this.icon && this.icon.length > 0; }
    get isOutline(): boolean { return ValueUtils.IsToggleTrue(this.outline); }

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
.alert-component {
    padding: 10px;
    display: flex;
    flex-wrap: nowrap;
    align-items: center;

    &.icon {
        .alert-component_content {
            margin-left: 5px;
        }
    }
}
</style>
