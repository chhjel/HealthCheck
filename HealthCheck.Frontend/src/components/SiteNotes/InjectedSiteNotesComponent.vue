<!-- src/components/sitenotes/InjectedSiteNotesComponent.vue -->
<template>
    <div style="position: absolute; top: 0; left: 0;">
        <v-app light class="injected-site-notes-component">
            <link href="https://cdn.jsdelivr.net/npm/vuetify@1/dist/vuetify.min.css" rel="stylesheet" />

            <div class="injected-site-notes-component-popup"
                ref="popup"
                :style="{ left: `${left}px`, top: `${top}px` }"
                v-if="this.selectedTargetRect != null">
                <div class="injected-site-notes-component-arrow elevation-10"></div>
                
                <v-card class="elevation-10">
                    <v-card-title primary-title>
                        <div>
                            <h3 class="headline mb-0">Notes</h3>
                            <div>Notes here for {{ selectedTargetQuerySelector }}</div>
                        </div>
                    </v-card-title>

                    <v-card-actions>
                        <v-btn flat @click="setSelectedElement(null)">Cancel</v-btn>
                        <v-btn flat color="primary">Add</v-btn>
                    </v-card-actions>
                </v-card>
            </div>

            <selection-frame-component 
                :targetRect="currentFocusTargetRect" 
                v-if="showSelection"
            ></selection-frame-component>
        </v-app>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import HtmlElementUtils from '../../util/HtmlElementUtils';
import SelectionFrameComponent from './SelectionFrameComponent.vue';

@Component({
    components: {
        SelectionFrameComponent
    }
})
export default class InjectedSiteNotesComponent extends Vue {
    // @Prop({ required: true })
    // max!: number;

    selectedTarget: Element | null = null;
    selectedTargetQuerySelector: string = '';
    currentFocusTarget: Element | null = null;
    currentFocusTargetRect: ClientRect | DOMRect | null = null;
    currentFocusTargetQuerySelector: string = '';
    isSelectionEnabled: boolean = false;
    
    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        document.addEventListener("click", this.onDocumentClicked);
        document.addEventListener("keyup", this.onDocumentKeyUp);
        document.addEventListener("keydown", this.onDocumentKeyDown);
        document.addEventListener("mouseover", this.onDocumentMouseOver);
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get showSelection(): boolean
    {
        return this.currentFocusTargetRect != null && this.isSelectionEnabled;
    }

    get selectedTargetRect(): ClientRect | DOMRect | null
    {
        return (this.selectedTarget == null) ? null : this.selectedTarget.getBoundingClientRect();
    }
    get left(): number
    {
        return (this.selectedTargetRect == null) ? 0 
            : Math.min(window.innerWidth - 400, Math.max(50, this.selectedTargetRect.left + window.scrollX));
    }
    get top(): number
    {
        return (this.selectedTargetRect == null) ? 0 : this.selectedTargetRect.bottom + window.scrollY + 5;
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    setFocusedElement(element: Element): void {
        console.log('setFocusedElement', element);
        this.currentFocusTarget = element;
        this.currentFocusTargetRect = this.currentFocusTarget.getBoundingClientRect();
        this.currentFocusTargetQuerySelector = HtmlElementUtils.CreateQuerySelector(this.currentFocusTarget);
    }

    setSelectedElement(element: Element | null): void {
        if (element == null)
        {
            this.selectedTarget = null;
            this.selectedTargetQuerySelector = '';
        }
        else
        {
            this.selectedTarget = this.currentFocusTarget;
            this.selectedTargetQuerySelector = this.currentFocusTargetQuerySelector;
        }

        this.scrollToPopup();
        // console.log('setSelectedElement', element);
    }

    scrollToPopup(): void {
        setTimeout(() => {
            const testElement = this.$refs.popup as Element;
            if (testElement != null && !HtmlElementUtils.IsInViewport(testElement)) {
                window.scrollTo({
                    top: (window.pageYOffset || document.documentElement.scrollTop) 
                        + testElement.getBoundingClientRect().top - 300,
                    behavior: 'smooth'
                });
            }
        }, 10);
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDocumentKeyDown(event: KeyboardEvent): void {
        if (this.isSelectionEnabled) {
            return;
        }

        if (event.shiftKey && event.altKey)
        {
            this.isSelectionEnabled = true;
            this.currentFocusTarget = null;
            this.currentFocusTargetRect = null;
            this.currentFocusTargetQuerySelector = '';
            document.title = "Enabled";
        }
    }

    onDocumentKeyUp(event: KeyboardEvent): void {
        if (!this.isSelectionEnabled) {
            return;
        }

        if (event.shiftKey === false || event.altKey === false)
        {
            this.isSelectionEnabled = false;
            document.title = "Disabled";
        }
    }

    onDocumentMouseOver(event: MouseEvent): void {
        if (!this.isSelectionEnabled || !(event.target instanceof Element))
        {
            return;
        }
        else if (HtmlElementUtils.CreateQuerySelector(event.target).length == 0)
        {
            return;
        }

        let element = event.target as Element;
        if (this.$el.contains(element))
        {
            return;
        }
        
        this.setFocusedElement(element);
    }

    onDocumentClicked(event: MouseEvent): void {
        if (!this.isSelectionEnabled || !(event.target instanceof Element))
        {
            return;
        }

        event.preventDefault();
        
        if (HtmlElementUtils.CreateQuerySelector(event.target).length == 0)
        {
            return;
        }

        this.isSelectionEnabled = false;
        let element = event.target as Element;
        if (this.$el.contains(element))
        {
            return;
        }

        let selector = HtmlElementUtils.CreateQuerySelector(element);
        this.setSelectedElement(element);
    }
}
</script>

<style scoped lang="scss">
.injected-site-notes-component
{
    position: relative;

    .injected-site-notes-component-popup
    {
        position: absolute;
    }

    .injected-site-notes-component-arrow
    {
        background-color: white;
        height: 20px;
        width: 20px;
        transform: rotate(45deg);
        box-shadow: 0 3px 1px -2px rgba(0,0,0,.2), 0 2px 2px 0 rgba(0,0,0,.14), 0 1px 5px 0 rgba(0,0,0,.12);
        margin-bottom: -10px;
        margin-left: 4px;
    }
}
</style>