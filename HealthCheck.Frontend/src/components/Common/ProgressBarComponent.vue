<!-- src/components/Common/ProgressBarComponent.vue -->
<template>
    <div>
        <div class="progress-bar-parent">
            <div class="progress-bar">
                <div class="progress-bar-value success"
                    @click="$emit('clickedSuccess')"
                    v-if="showSuccess"
                    :style="{ 'flex-grow': `${successWidth}` }">{{ success }}</div>
                <div class="progress-bar-value error"
                    @click="$emit('clickedError')"
                    v-if="showError"
                    :style="{ 'flex-grow': `${errorWidth}` }">{{ error }}</div>
                <div class="progress-bar-value remaining"
                    @click="$emit('clickedRemaining')"
                    v-if="showRemaining"
                    :style="{ 'flex-grow': `${remainingWidth}` }">{{ remaining }}</div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
    components: {
    }
})
export default class ProgressBarComponent extends Vue {
    @Prop({ required: true })
    max!: number;
    @Prop({ required: true })
    success!: number;
    @Prop({ required: true })
    error!: number;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get remaining(): number {
        return this.max - this.success - this.error;
    }

    get showSuccess(): boolean {
        return (this.success > 0);
    }
    get showError(): boolean {
        return (this.error > 0);
    }
    get showRemaining(): boolean {
        return (this.remaining > 0);
    }

    get successWidth(): number {
        return this.calcPartWidths()[0];
    }
    get errorWidth(): number {
        return this.calcPartWidths()[1];
    }
    get remainingWidth(): number {
        return this.calcPartWidths()[2];
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    calcPartWidths(): Array<number> {
        let successWidth = (this.success == 0) ? 0 : Math.floor((this.success / this.max) * 100);
        let errorWidth = (this.error == 0) ? 0 : Math.floor((this.error / this.max) * 100);
        let remainingWidth = (this.remaining == 0) ? 0 : Math.max(0, 100 - successWidth - errorWidth);

        let parts = [ successWidth, errorWidth, remainingWidth ];
        let totalWidth = successWidth + errorWidth + remainingWidth;
        if (totalWidth != 100)
        {
            let diff = 100 - totalWidth;
            let indexToFix = parts.findIndex(x => x > 0);
            parts[indexToFix] += diff;
        }
        return parts;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    // ToDo click to switch between count/percentage
}
</script>

<style scoped lang="scss">
.progress-bar-parent {
    position: relative;
}
.progress-bar {
    border: 1px solid gray;
    white-space: nowrap;
    font-size: 0px;
    background-color: #c4c4c4;
    display: flex;
}
.progress-bar-value {
    display: inline-block;
    height: 100%;
    font-size: 16px;
    text-align: center;
    padding: 5px;
    font-weight: 600;
    color: white;
    text-shadow: 0 0 5px var(--color--secondary-base);
    min-width: 26px;

    &.success { background-color: var(--color--success-base); }
    &.error { background-color: var(--color--error-base); }
    /* &.remaining { background-color: var(--color--secondary-base); } */
}
</style>