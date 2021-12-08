<!-- src/components/Common/Basic/SimplePagingComponent.vue -->
<template>
    <div class="paging-component" v-if="count > 1 && pageCount > 1">
        
        <div class="page-button" @click="onNextPrevClick(-1)"><v-icon>chevron_left</v-icon></div>
        <div v-for="(btn, bIndex) in buttons"
            :key="`page-btn-${bIndex}-${btn.number}-${id}`"
            @click="onClickedButton(btn)"
            class="page-button"
            :class="{ 'active': isActive(btn), 'middle': btn.isDialogButton, 'wide': (btn.isDialogButton && !hasExtraButton) }">
            <span v-if="btn.isPage">{{ btn.number }}</span>
            <span v-if="btn.isDialogButton">...</span>
        </div>
        <div class="page-button" @click="onNextPrevClick(1)"><v-icon>chevron_right</v-icon></div>
        
        <!-- DIALOGS -->
        <v-dialog v-model="dialogVisible"
            @keydown.esc="dialogVisible = false"
            max-width="480"
            content-class="confirm-dialog">
            <v-card>
                <v-card-title class="headline">Jump to page</v-card-title>
                <v-card-text>
                    <v-text-field
                        label="Page number"
                        solo
                        v-model="dialogNumber"
                        type="number"
                        ref="dialogNumberInput"></v-text-field>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary"
                        @click="dialogVisible = false">Cancel</v-btn>
                    <v-btn color="primary"
                        @click="navigateToPage(dialogNumber)">Go to page {{ dialogNumber }}</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script lang="ts">
import IdUtils from "util/IdUtils";
import LinqUtils from "util/LinqUtils";
import { Vue, Component, Prop, Watch } from "vue-property-decorator";

interface PageinationButton {
    number: number;
    isPage: boolean;
    isDialogButton: boolean;
}

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

    id: string = IdUtils.generateId();
    currentValue: number = this.asIndex ? 0 : 1;
    dialogVisible: boolean = false;
    dialogNumber: number = 1;
    
    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
        this.currentValue = this.value;

        window.addEventListener('resize', this.onWindowResize);
        setTimeout(() => {
            this.refreshSize();
        }, 10);
    }

    beforeDestroy(): void {
        window.removeEventListener('resize', this.onWindowResize)
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get pageCount(): number {
        return Math.ceil(this.count / this.pageSize);
    }

    hasExtraButton: boolean = false;
    get buttons(): Array<PageinationButton> {
        let buttons: Array<PageinationButton> = [];
        this.hasExtraButton = false;

        if (this.showDialogButton)
        {
            let sideCount = Math.floor(this.maxButtonCount / 2);
            for (let i=1;i<=sideCount;i++)
            {
                buttons.push({ number: i, isPage: true, isDialogButton: false });
            }
            buttons.push({ number: this.pageCount/2, isPage: false, isDialogButton: true });
            for (let i=this.pageCount - sideCount + 1;i<=this.pageCount;i++)
            {
                buttons.push({ number: i, isPage: true, isDialogButton: false });
            }

            if (!buttons.some(x => this.isActive(x)))
            {
                this.hasExtraButton = true;
                buttons.push({ number: this.asIndex ? this.currentValue + 1 : this.currentValue, isPage: true, isDialogButton: false });
                buttons = buttons.sort((a, b) => LinqUtils.SortBy(a, b, x => x.number, true));
            }
        }
        else
        {
            for (let i=0;i<this.pageCount;i++)
            {
                buttons.push({
                    number: i+1,
                    isPage: true,
                    isDialogButton: false
                });
            }
        }

        return buttons;
    }

    get pageButtonCountToShow(): number {
        if (this.showDialogButton)
        {
            return this.maxButtonCount;
        }
        return this.pageCount;
    }

    get maxButtonCount(): number 
    {
        return Math.max(this.spaceForMaxButtonCount, 5);
    }

    get showDialogButton(): boolean {
        return this.pageCount > this.maxButtonCount;
    }

    ////////////////
    //  METHODS  //
    //////////////
    isActive(btn: PageinationButton): boolean {
        if (btn.isDialogButton) return false;

        let num = btn.number;
        return this.asIndex
            ? num - 1 == this.currentValue
            : num == this.currentValue;
    }

    spaceForMaxButtonCount: number = 11;
    refreshSize(): void {
        this.spaceForMaxButtonCount = Math.floor(this.$el.clientWidth / 60);
        if (this.spaceForMaxButtonCount % 2 != 0)
        {
            this.spaceForMaxButtonCount--;
        }
    }

    navigateToPage(num: number): void  {
        if (this.asIndex) {
            num--;
        }

        const min = this.asIndex ? 0 : 1;
        const max = this.asIndex ? this.pageCount - 1 : this.pageCount;
        if (num < min) num = min;
        else if (num > max) num = max;

        this.$emit('input', num);
        this.$emit('change', num);
        this.currentValue = num;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onClickedButton(btn: PageinationButton): void {
        if (btn.isPage)
        {
            this.navigateToPage(btn.number);
        }
        else if (btn.isDialogButton)
        {
            this.dialogVisible = true;
            this.$nextTick(() => (<HTMLInputElement>this.$refs.dialogNumberInput).focus())
        }
    }

    onNextPrevClick(mod: number): void {
        this.navigateToPage(this.currentValue + mod + (this.asIndex ? 1 : 0));
    }

    @Watch("value")
    onValueChanged(): void {
        this.currentValue = this.value;
    }

    @Watch("pageCount")
    onPageCountChanged(): void {
        this.refreshSize();
    }

    onWindowResize(): void {
        this.refreshSize();
    }

    @Watch("dialogNumber")
    onDialogNumberChanged(): void {
        this.$nextTick(() => {
            if (this.dialogNumber < 1) this.dialogNumber = 1;
            else if (this.dialogNumber > this.pageCount) this.dialogNumber = this.pageCount;
        });
    }
}
</script>

<style scoped lang="scss">
.paging-component {
    display: flex;
    flex-wrap: nowrap;

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

        &.middle {
            &.wide {
                padding-left: 35px;
                padding-right: 35px;
            }
        }
    }
}
</style>