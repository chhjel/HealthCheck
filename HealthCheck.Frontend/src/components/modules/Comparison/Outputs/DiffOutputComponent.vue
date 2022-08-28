<!-- src/components/modules/Comparison/Outputs/DiffOutputComponent.vue -->
<template>
    <div>
        <diff-component
            class="codeeditor codeeditor__input"
            :allowFullscreen="true"
            :originalName="resultData.originalName"
            :originalContent="resultData.originalContent"
            :modifiedName="resultData.modifiedName"
            :modifiedContent="resultData.modifiedContent"
            :readOnly="true"
            />
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { StoreUtil } from "@util/StoreUtil";
import IdUtils from "@util/IdUtils";
import DiffComponent from "@components/Common/DiffComponent.vue";

interface ResultData {
    originalName: string;
    originalContent: string;
    modifiedName: string;
    modifiedContent: string;
}
@Options({
    components: {
        DiffComponent
    }
})
export default class DiffOutputComponent extends Vue {
    @Prop({ required: true })
    resultData!: ResultData;

    id: string = IdUtils.generateId();

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted()
    {
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
}
</script>

<style scoped lang="scss">
.codeeditor {
    box-shadow: 0 2px 4px 1px rgba(0, 0, 0, 0.15), 0 3px 2px 0 rgba(0,0,0,.02), 0 1px 2px 0 rgba(0,0,0,.06);
    &__input {
        position: relative;
        height: 600px;
    }
}
</style>
