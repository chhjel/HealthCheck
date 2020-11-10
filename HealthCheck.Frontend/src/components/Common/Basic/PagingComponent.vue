<!-- src/components/Common/Basic/SimplePagingComponent.vue -->
<template>
    <div class="paging-component" v-if="count > 1 && pageCount > 1">
        <div v-for="num in pageNumbers"
            :key="`page-${num}`"
            @click="onClickedPage(num)"
            class="page-button"
            :class="{ 'active': isActive(num) }">
            {{ num }}
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";

@Component({
    components: {}
})
export default class SimplePagingComponent extends Vue
{
    @Prop({ required: true })
    value!: number;

    @Prop({ required: true })
    count!: number;

    @Prop({ required: false, default: 100 })
    pageSize!: number;

    @Prop({ required: false, default: false })
    asIndex!: boolean;

    currentValue: number = this.asIndex ? 0 : 1;
    
    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
        this.currentValue = this.value;
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get pageCount(): number {
        return Math.ceil(this.count / this.pageSize);
    }

    get pageNumbers(): Array<number> {
        let pages = [];
        for (let i=0;i<this.pageCount;i++)
        {
            pages.push(i+1);
        }
        return pages;
    }

    ////////////////
    //  METHODS  //
    //////////////
    isActive(num: number): boolean {
        return this.asIndex 
            ? num - 1 == this.currentValue
            : num == this.currentValue;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onClickedPage(num: number): void {
        if (this.asIndex) {
            num--;
        }
        this.$emit('input', num);
        this.currentValue = num;
    }

    @Watch("value")
    onValueChanged(): void {
        this.currentValue = this.value;
    }
}
</script>

<style scoped lang="scss">
.paging-component {
    .page-button {
        display: inline-block;
        padding: 5px 15px;
        border-radius: 5px;
        background-color: #eee;
        cursor: pointer;
        user-select: none;

        &.active {
            font-weight: 600;
            color: #fff;
            background-color: var(--v-primary-base);
        }
    }
}
</style>