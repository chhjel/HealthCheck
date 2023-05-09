<template>
    <div class="tabs-component" :class="rootClasses" :disabled="isDisabled">
        <div class="tabs-component__tabs">
            <div v-for="label in labels" :key="label"
                class="tabs-component__tab"
                :class="tabClasses(label)"
                tabindex="0"
                @click="onTabClicked(label)"
                @keyup.enter="onTabClicked(label)">
                {{ label }}
            </div>
        </div>
        
        <div class="tabs-component__content">
            <slot :name="localValue">{{ localValue }}</slot>
        </div>
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
export default class TabsComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    labels!: Array<string>;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    id: string = IdUtils.generateId();
    localValue: string = '';

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
        };
        return classes;
    }

    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }

    ////////////////
    //  METHODS  //
    //////////////
    tabClasses(label: string): any {
        let classes = {
             'active': this.localValue == label,
        };
        return classes;
    }

    onTabClicked(label: string): void {
        if (this.isDisabled) return;
        this.localValue = label;
    }

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
.tabs-component {
    &__tabs {
        display: flex;
    }
    &__tab {
        font-size: 14px;
        font-weight: 500;
        display: inline-flex;
        flex-direction: row;
        align-content: center;
        justify-content: center;
        align-items: center;
        vertical-align: middle;
        text-transform: uppercase;
        text-decoration: none;
        cursor: pointer;
        border-radius: 2px;
        user-select: none;
        margin: 5px 5px;
        padding: 3px 5px;

        &.active {
            font-weight: 600;
            border-bottom: 2px solid var(--color--primary-base);
        }
    }
    &__content {
    }
}
</style>
