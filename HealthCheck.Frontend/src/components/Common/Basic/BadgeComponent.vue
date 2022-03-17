<template>
    <div class="badge-component" :class="rootClasses">
		<slot></slot>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
    components: {}
})
export default class BadgeComponent extends Vue {

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {

    }

    ////////////////
    //  GETTERS  //
    //////////////
    get rootClasses(): any {
        return {

        };
    }


    ////////////////
    //  METHODS  //
    //////////////

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
	
    /////////////////
    //  WATCHERS  //
    ///////////////

}
</script>

<style scoped lang="scss">
.badge-component {
    border: 1px solid #fff;
    background-color: gray;
    color: #fff;
    border-radius: 20px;
    min-width: 19px;
    display: inline-block;
    padding: 2px;
    text-align: center;
}
</style>
