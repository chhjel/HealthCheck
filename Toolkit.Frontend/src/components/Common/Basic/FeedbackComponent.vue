<template>
    <div class="feedback-component" :class="rootClasses" v-if="isVisible">
        <icon-component v-if="icon">{{ icon }}</icon-component>
        <div class="feedback-component___content">{{ text }}</div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
    components: {}
})
export default class FeedbackComponent extends Vue {
    @Prop({ required: false, default: null })
    forceShow!: boolean | null;

    @Prop({ required: false, default: '' })
    icon!: string;

    @Prop({ required: false, default: null })
    type!: 'error' | 'info' | 'warning';

    @Prop({ required: false, default: 2000 })
    duration!: number;

    text: string = '';
    visible: boolean = false;
    timeoutId: NodeJS.Timeout | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get rootClasses(): any {
        let classes = {
            'icon': this.hasIcon
        };
        classes[this.type] = true;
        return classes;
    }

    get hasIcon(): boolean { return this.icon && this.icon.length > 0; }
    get isVisible(): boolean { return this.forceShow === true || this.visible; }

    ////////////////
    //  METHODS  //
    //////////////
    public show(text: string, durationMs: number | null = null): void {
        this.text = text || '';
        durationMs = durationMs || this.duration;

        this.visible = true;

        if (this.timeoutId) clearTimeout(this.timeoutId);
        this.timeoutId = setTimeout(() => {
            this.visible = false;
        }, durationMs);
    }
}
</script>

<style scoped lang="scss">
.feedback-component {
    padding: 10px;
    display: flex;
    flex-wrap: nowrap;
    align-items: center;
    display: inline-block;

    &.icon {
        .feedback-component__content {
            margin-left: 5px;
        }
    }
}
</style>
