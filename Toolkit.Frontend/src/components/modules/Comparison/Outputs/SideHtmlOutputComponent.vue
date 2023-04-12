<!-- src/components/modules/Comparison/Outputs/SideHtmlOutputComponent.vue -->
<template>
    <div class="side-html-output">
        <div class="left-html" v-html="resultData.Left"></div>
        <div class="right-html" v-html="resultData.Right"></div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";

interface ResultData {
    Left: string;
    Right: string;
}
@Options({
    components: {
    }
})
export default class SideHtmlOutputComponent extends Vue {
    @Prop({ required: true })
    resultData!: ResultData;

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
}
</script>

<style scoped lang="scss">
.side-html-output {
    display: flex;
    .left-html {
        flex: 1;
        padding: 10px;
        text-align: right;
        padding-right: 22px;
        border-right: 2px solid var(--color--accent-darken3);
    }
    .right-html {
        flex: 1;
        padding: 10px;
        text-align: left;
        padding-left: 20px;
    }
}
</style>
