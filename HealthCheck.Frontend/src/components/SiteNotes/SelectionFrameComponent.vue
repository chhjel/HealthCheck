<!-- src/components/sitenotes/InjectedSiteNotesComponent.vue -->
<template>
    <div>
        <div class="injected-site-notes-selection-frame-component"
            v-if="targetRect != null"
            :style="{ left: `${left}px`, top: `${top}px`, width: `${width}px`, height: `${height}px` }">
            <slot />
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import HtmlElementUtils from '../../util/HtmlElementUtils';

@Component({
    components: {
    }
})
export default class SelectionFrameComponent extends Vue {
    @Prop({ required: true })
    targetRect!: ClientRect | DOMRect | null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get left(): number
    {
        return (this.targetRect == null) ? 0 : this.targetRect.left + window.scrollX;
    }
    get top(): number
    {
        return (this.targetRect == null) ? 0 : this.targetRect.top + window.scrollY;
    }
    get width(): number
    {
        return (this.targetRect == null) ? 0 : this.targetRect.right - this.targetRect.left;
    }
    get height(): number
    {
        return (this.targetRect == null) ? 0 : this.targetRect.bottom - this.targetRect.top;
    }
    
    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped>
.injected-site-notes-selection-frame-component {
    position: absolute;
    border: 5px solid yellow;
    pointer-events: none;
}
</style>