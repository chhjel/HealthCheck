<template>
    <span class="badge-component">
        <div class="badge-component__badge" :class="badgeClasses">
            <slot></slot>
        </div>
    </span>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
    components: {}
})
export default class BadgeComponent extends Vue {

    @Prop({ required: false, default: null })
    color!: string;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {

    }

    ////////////////
    //  GETTERS  //
    //////////////
    get badgeClasses(): any {
        let classes = {
        };
        classes[this.color || 'primary'] = true;
        return classes;
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
    &__badge {
        width: 22px;
        height: 22px;
        font-size: 14px;
        border-radius: 50%;
        display: flex;
        justify-content: center;
        align-items: center;
        flex-direction: row;
        flex-wrap: wrap;
    }
}
</style>
