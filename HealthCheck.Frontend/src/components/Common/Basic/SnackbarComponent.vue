<template>
    <Teleport to="body">
        <div class="snackbar-component" :class="rootClasses" v-if="isVisible">
            <slot></slot>
        </div>
    </Teleport>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
    components: {}
})
export default class SnackbarComponent extends Vue {

    @Prop({ required: true })
    value!: boolean;

    @Prop({ required: false, default: 6000 })
    timeout!: number;

    @Prop({ required: false, default: null })
    color!: string;

    localValue: boolean = false;
    
    isVisible: boolean = false;
    timeoutRefFade: NodeJS.Timeout;
    timeoutRefHide: NodeJS.Timeout;
    hideDelay: number = 1500;

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
            'fading': !this.localValue
        };
        classes[this.color || 'info'] = true;
        return classes;
    }

    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onTimeoutFade(): void {
        this.localValue = false;
    }
    onTimeoutHide(): void {
        this.isVisible = false;
    }
	
    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
		this.localValue = this.value;

        if (this.localValue)
        {
            this.isVisible = true;
            if (this.timeoutRefFade) clearTimeout(this.timeoutRefFade);
            if (this.timeoutRefHide) clearTimeout(this.timeoutRefHide);
            this.timeoutRefFade = setTimeout(this.onTimeoutFade, this.timeout);
            this.timeoutRefHide = setTimeout(this.onTimeoutHide, this.timeout + this.hideDelay);
        }
    }

    @Watch('localValue')
    emitLocalValue(): void
    {
		this.$emit('update:value', this.localValue);
    }
}
</script>

<style scoped lang="scss">
.snackbar-component {
    padding: 5px;
    display: inline-flex;
    align-items: baseline;
    position: fixed;
    bottom: 0;
    transition: 1s all;
    z-index: 99999;
    left: 50%;
    transform: translateX(-50%);

    &.fading {
        bottom: -100px;
    }
}
</style>
