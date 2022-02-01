<!-- src/components/modules/Metrics/MetricsBlockComponent.vue -->
<template>
    <div :style="styleBlockComponent">
        <div :style="styleBlockComponentHeader">
            <h4 :style="styleBlockComponentHeaderTitle" v-if="title.length > 0">{{ title }}</h4>
        </div>
        
        <div :style="styleBlockComponentContent">
            <slot/>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, } from "vue-property-decorator";

@Component({})
export default class MetricsBlockComponent extends Vue {
    @Prop({ required: false, default: '' })
    title!: string;

    //////////////////
    //  LIFECYCLE  //
    ////////////////

    ////////////////
    //  GETTERS  //
    //////////////

    //////////////
    //  STYLE  //
    ////////////
    get styleBlockComponent() : any {
        return {
            "border": "8px solid rgb(225 225 225)",
            "background-color": "rgb(255, 255, 255)",
            'padding': '20px 10px'
        }
    }
    get styleBlockComponentHeader() : any {
        return {
            "display": "flex",
            "padding-left": "20px",
            "border-radius": "0 25px 0 25px"
        }
    }
    get styleBlockComponentHeaderTitle() : any {
        return {
            "flex-grow": "1",
            "font-size": "22px",
            "margin-top": "10px",
            "margin-bottom": "10px"
        }
    }
    get styleBlockComponentContent() : any {
        return {
            "padding": "0px 48px 24px 24px",
            "padding-bottom": "0"
        }
    }
    
    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
</style>
