<template>
    <div class="btn-component" :class="rootClasses">
		<!-- <h3>TODO: BtnComponent</h3>
        <div><b>flat:</b>' {{ flat }}'</div>
        <div><b>href:</b>' {{ href }}'</div>
        <div><b>small:</b>' {{ small }}'</div>
        <div><b>color:</b>' {{ color }}'</div>
        <div><b>title:</b>' {{ title }}'</div>
        <div><b>icon:</b>' {{ icon }}'</div>
        <div><b>round:</b>' {{ round }}'</div>
        <div><b>large:</b>' {{ large }}'</div>
        <div><b>disabled:</b>' {{ disabled }}'</div>
        <div><b>loading:</b>' {{ loading }}'</div>
        <div><b>depressed:</b>' {{ depressed }}'</div>
        <div><b>outline:</b>' {{ outline }}'</div>
        <div><b>xSmall:</b>' {{ xSmall }}'</div>
        <div><b>target:</b>' {{ target }}'</div> -->
        <a v-if="href" :href="(href || '')">
		    <span class="btn-component__contents"><slot></slot></span>
        </a>
        <span v-else class="btn-component__contents"><slot></slot></span>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'

@Options({
    components: {}
})
export default class BtnComponent extends Vue {

    @Prop({ required: false, default: false })
    flat!: string | boolean;

    @Prop({ required: false, default: null })
    href!: string;

    @Prop({ required: false, default: false })
    small!: string | boolean;

    @Prop({ required: false, default: null })
    color!: string;

    @Prop({ required: false, default: null })
    title!: string;

    @Prop({ required: false, default: false })
    icon!: string | boolean;

    @Prop({ required: false, default: false })
    round!: string | boolean;

    @Prop({ required: false, default: false })
    large!: string | boolean;

    @Prop({ required: false, default: false })
    disabled!: string | boolean;

    @Prop({ required: false, default: false })
    loading!: string | boolean;

    @Prop({ required: false, default: false })
    depressed!: string | boolean;

    @Prop({ required: false, default: false })
    outline!: string | boolean;

    @Prop({ required: false, default: false })
    xSmall!: string | boolean;

    @Prop({ required: false, default: null })
    target!: string;


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
             'flat': this.isFlat,
             'small': this.isSmall,
             'icon': this.isIcon,
             'round': this.isRound,
             'large': this.isLarge,
             'disabled': this.isDisabled,
             'loading': this.isLoading,
             'depressed': this.isDepressed,
             'outline': this.isOutline,
             'x-small': this.isXSmall
        };
    }

    get isFlat(): boolean { return ValueUtils.IsToggleTrue(this.flat); }
    get isSmall(): boolean { return ValueUtils.IsToggleTrue(this.small); }
    get isIcon(): boolean { return ValueUtils.IsToggleTrue(this.icon); }
    get isRound(): boolean { return ValueUtils.IsToggleTrue(this.round); }
    get isLarge(): boolean { return ValueUtils.IsToggleTrue(this.large); }
    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }
    get isLoading(): boolean { return ValueUtils.IsToggleTrue(this.loading); }
    get isDepressed(): boolean { return ValueUtils.IsToggleTrue(this.depressed); }
    get isOutline(): boolean { return ValueUtils.IsToggleTrue(this.outline); }
    get isXSmall(): boolean { return ValueUtils.IsToggleTrue(this.xSmall); }

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
.btn-component {
    padding: 5px;
    background-color: #eee;
    border: 1px solid gray;
    display: flex;
    align-content: center;
    justify-content: center;
    align-items: center;
    flex-direction: row;
    cursor: pointer;

    &__contents {
        color: #333;
        display: flex;
        align-content: center;
        justify-content: center;
        align-items: center;
        flex-direction: row;
        white-space: nowrap;

        &:hover {
            text-decoration: none;
        }
    }

    &.icon { }
    &.disabled {
        cursor: default;
    }
    &.loading { }

    // Styles
    &.outline { }
    &.round { }
    &.flat { }
    &.depressed { }

    // Sizes
    &.large { }
    &.small { }
    &.x-small { }
}
</style>
