<template>
    <div class="feedback-component" :class="rootClasses" v-if="shouldBeVisible">
        <icon-component v-if="icon">{{ icon }}</icon-component>
        <div class="feedback-component___content">{{ text }}</div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from "@util/ValueUtils";

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

    @Prop({ required: false, default: false })
    reserve!: string | boolean;

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
            'icon': this.hasIcon,
            'reserve': this.isReserved
        };
        if (this.type) classes[this.type] = true;
        return classes;
    }

    get hasIcon(): boolean { return this.icon && this.icon.length > 0; }
    get shouldBeVisible(): boolean { return this.isReserved || this.forceShow === true || this.visible; }
    get isReserved(): boolean { return ValueUtils.IsToggleTrue(this.reserve); }

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
            this.text = '';
        }, durationMs);
    }
}
</script>

<style scoped lang="scss">
.feedback-component {
    display: flex;
    flex-wrap: nowrap;
    align-items: center;
    display: inline-block;
    box-sizing: border-box;

    &:not(.reserve) {
        padding: 10px;
    }

    &.icon {
        .feedback-component__content {
            margin-left: 5px;
        }
    }
}
</style>
