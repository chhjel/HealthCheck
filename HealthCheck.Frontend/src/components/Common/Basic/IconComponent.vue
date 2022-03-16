<template>
    <span class="icon-component" :class="rootClasses">
		<!-- <h3>TODO: IconComponent</h3>
        <div><b>color:</b>' {{ color }}'</div>
        <div><b>small:</b>' {{ small }}'</div>
        <div><b>large:</b>' {{ large }}'</div>
        <div><b>slot:</b>' {{ slot }}'</div>
        <div><b>size:</b>' {{ size }}'</div> -->
        <span class="material-icons"><slot></slot></span>
    </span>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'

@Options({
    components: {}
})
export default class IconComponent extends Vue {

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: false })
    small!: string | boolean;

    @Prop({ required: false, default: false })
    large!: string | boolean;

    @Prop({ required: false, default: false })
    help!: string | boolean;

    @Prop({ required: false, default: null })
    slot!: string;

    @Prop({ required: false, default: null })
    size!: string;


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
             'small': this.isSmall,
             'large': this.isLarge,
             'help': this.isHelp
        };
    }

    get isSmall(): boolean { return ValueUtils.IsToggleTrue(this.small); }
    get isLarge(): boolean { return ValueUtils.IsToggleTrue(this.large); }
    get isHelp(): boolean { return ValueUtils.IsToggleTrue(this.help); }

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
.icon-component {
    &.small { }
    &.large { }
    &.help { cursor: help; }
}
</style>
