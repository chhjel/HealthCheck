<!-- src/components/Common/Basic/SimplePagingComponent.vue -->
<template>
    <div class="simple-paging-component" v-if="count > 1">
        <div v-for="num in pageNumbers"
            :key="`page-${num}`"
            @click="onClickedPage(num)"
            class="page-button"
            :class="{ 'active': num == currentValue }">
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
    items!: Array<any>;

    @Prop({ required: false, default: 100 })
    pageSize!: number;

    currentValue: number = 1;
    
    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
        this.currentValue = this.value;
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get count(): number {
        return (this.items != null) ? this.items.length : 0;
    }

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

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onClickedPage(num: number): void {
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
.simple-paging-component {
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