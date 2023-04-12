<!-- src/components/modules/Comparison/Outputs/SideNotesOutputComponent.vue -->
<template>
    <div class="side-notes">
        <p class="left-note">{{ resultData.Left }}</p>
        <p class="right-note">{{ resultData.Right }}</p>
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
export default class SideNotesOutputComponent extends Vue {
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
.side-notes {
    display: flex;
    p {
        flex: 1;
        padding: 10px;
    }
    .left-note {
        text-align: right;
        padding-right: 22px;
        border-right: 2px solid var(--color--accent-darken3);
    }
    .right-note {
        text-align: left;
        padding-left: 20px;
    }
}
</style>
