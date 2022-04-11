<template>
    <div class="carousel-component" :class="rootClasses" :style="rootStyle">
        <div class="carousel-component__nav carousel-component__nav--left material-icons" tabindex="0"
            v-if="showControls && !isAtStart" @click="gotoPrev">arrow_back_ios</div>
        <div class="carousel-component__nav carousel-component__nav--right material-icons" tabindex="0"
            v-if="showControls && !isAtEnd" @click="gotoNext">arrow_forward_ios</div>
        <div class="carousel-component__items" :style="itemsStyle">
            <div class="carousel-component__item"
                v-for="(item, index) in items"
                :key="`${id}-${index}-${item.url}`"
                @click.self="showControls = !showControls">
                <img class="carousel-component__item-img" :src="item.url" @click="showControls = !showControls" />
                <div class="carousel-component__item-details" v-if="showControls && item.detailsHtml" v-html="item.detailsHtml"></div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import ValueUtils from '@util/ValueUtils'
import { CarouselItem } from "./CarouselComponent.vue.models";
import IdUtils from "@util/IdUtils";

@Options({
    components: {}
})
export default class CarouselComponent extends Vue {
    @Prop({ required: true, default: () => [] })
    items!: Array<CarouselItem>;

    @Prop({ required: false, default: '400px' })
    height!: string;

    id: string = IdUtils.generateId();
    showControls: boolean = true;
    currentIndex: number = 0;

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

    get rootStyle(): any {
        return {
            'height': `${this.height}`
        };
    }

    get itemsStyle(): any {
        const margin = -(this.currentIndex * 100);
        return {
            'margin-left': `${margin}%`
        }
    }

    get isAtStart(): boolean { return this.currentIndex == 0; }
    get isAtEnd(): boolean { return this.items.length == 0 || this.currentIndex == this.items.length - 1; }

    ////////////////
    //  METHODS  //
    //////////////
    gotoNext(): void { this.setIndex(this.currentIndex+1); }
    gotoPrev(): void { this.setIndex(this.currentIndex-1); }
    setIndex(idx: number): void {
        this.currentIndex = idx;
        if (this.currentIndex < 0) this.currentIndex = 0;
        else if (this.currentIndex >= this.items.length) this.currentIndex = this.items.length - 1;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
	
    /////////////////
    //  WATCHERS  //
    ///////////////

}
</script>

<style scoped lang="scss">
.carousel-component {
    overflow: hidden;
    user-select: none;
    position: relative;
    &__items {
        height: 100%;
        width: 100%;
        position: relative;
        transition: all 0.5s;
        display: flex;
    }
    &__item {
        height: 100%;
        text-align: center;
        position: relative;
        flex: 0 0 100%;
    }
    &__item-img {
        height: 100%;
        max-width: 100%;
    }
    &__item-details {
        position: absolute;
        width: 100%;
        max-width: 100%;
        text-align: center;
        bottom: 0;
        padding: 3px;
        background-color: #00000036;
    }
    &__nav {
        position: absolute;
        font-size: 90px;
        top: 0;
        bottom: 0;
        display: flex;
        align-items: center;
        padding: 5px;
        cursor: pointer;
        z-index: 10;

        &--left {
            left: 5px;
        }

        &--right {
            right: 5px;
        }
    }
}
</style>

<style lang="scss">
    .carousel-component__item-details {
        a, a:visited {
            color: var(--color--text-light);
        }
    }
</style>
