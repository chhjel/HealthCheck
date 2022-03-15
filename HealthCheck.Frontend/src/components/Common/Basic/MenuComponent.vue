<template>
    <div class="menu-component" :class="rootClasses">
		<h3>TODO: MenuComponent</h3>

		<slot></slot>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
    components: {}
})
export default class MenuComponent extends Vue {



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
.menu-component {
	border: 2px solid red;
	padding: 5px;
	margin: 5px;

}
</style>
