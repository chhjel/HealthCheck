<!-- src/components/modules/Comparison/Outputs/NoteOutputComponent.vue -->
<template>
    <div class="notes">
        <p>{{ resultData.Note }}</p>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";

interface ResultData {
    Note: string;
}
@Options({
    components: {
    }
})
export default class NoteOutputComponent extends Vue {
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
.notes {
    p {
        text-align: center;
        padding: 10px;
    }
}
</style>
