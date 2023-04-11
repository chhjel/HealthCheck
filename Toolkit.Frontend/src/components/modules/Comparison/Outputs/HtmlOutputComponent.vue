<!-- src/components/modules/Comparison/Outputs/HtmlOutputComponent.vue -->
<template>
    <div class="html-output" v-html="resultData.Html"></div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";

interface ResultData {
    Html: string;
}
@Options({
    components: {
    }
})
export default class HtmlOutputComponent extends Vue {
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
.html-output {
    text-align: center;
    padding: 10px;
}
</style>
