<!-- src/components/modules/Comparison/Outputs/OutputComponent.vue -->
<template>
    <div class="result-item">
        <h3>{{ title }}</h3>
        <component
            :is="componentType"
            :resultData="resultDataObj" />
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import IdUtils from "@util/IdUtils";
import DiffOutputComponent from "./DiffOutputComponent.vue";
import NoteOutputComponent from "./NoteOutputComponent.vue";
import SideNotesOutputComponent from "./SideNotesOutputComponent.vue";
import HtmlOutputComponent from "./HtmlOutputComponent.vue";
import SideHtmlOutputComponent from "./SideHtmlOutputComponent.vue";
import { HCComparisonDiffOutputType } from "@generated/Enums/Core/HCComparisonDiffOutputType";
import UnknownOutputComponent from "./UnknownOutputComponent.vue";

@Options({
    components: {
        DiffOutputComponent,
        SideNotesOutputComponent,
        NoteOutputComponent,
        UnknownOutputComponent,
        HtmlOutputComponent,
        SideHtmlOutputComponent
    }
})
export default class DiffSelectionComponent extends Vue {
    @Prop({ required: true })
    title!: string;

    @Prop({ required: true })
    dataType!: HCComparisonDiffOutputType;

    @Prop({ required: true })
    resultData!: string;

    id: string = IdUtils.generateId();
    resultDataObj: object = {};

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    created() {
        this.resultDataObj = JSON.parse(this.resultData);
    }

    ////////////////
    //  METHODS  //
    //////////////
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get componentType(): string {
        if (this.dataType == HCComparisonDiffOutputType.Diff) return 'DiffOutputComponent';
        else if (this.dataType == HCComparisonDiffOutputType.Note) return 'NoteOutputComponent';
        else if (this.dataType == HCComparisonDiffOutputType.SideNotes) return 'SideNotesOutputComponent';
        else if (this.dataType == HCComparisonDiffOutputType.Html) return 'HtmlOutputComponent';
        else if (this.dataType == HCComparisonDiffOutputType.SideHtml) return 'SideHtmlOutputComponent';
        else return 'UnknownOutputComponent';
    }
}
</script>

<style scoped lang="scss">
.result-item {
    text-align: left;
    margin-top: 18px;

    h3 {
        text-align: center;
    }
}
</style>
