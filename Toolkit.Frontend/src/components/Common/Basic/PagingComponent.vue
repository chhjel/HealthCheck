<!-- src/components/Common/Basic/PagingComponent.vue -->
<template>
    <div class="paging-component" v-if="visible">
        
        <div class="page-button" @click="onNextPrevClick(-1)"><icon-component>chevron_left</icon-component></div>
        <div v-for="(btn, bIndex) in buttons"
            :key="`page-btn-${bIndex}-${btn.number}-${id}`"
            @click="onClickedButton(btn)"
            class="page-button"
            :class="{ 'active': isActive(btn), 'middle': btn.isDialogButton, 'wide': (btn.isDialogButton && !hasExtraButton), 'disabled': disabled }">
            <span v-if="btn.isPage">{{ btn.number }}</span>
            <span v-if="btn.isDialogButton">...</span>
        </div>
        <div class="page-button" @click="onNextPrevClick(1)"><icon-component>chevron_right</icon-component></div>
        
        <!-- DIALOGS -->
        <dialog-component v-model:value="dialogVisible" max-width="480">
            <template #header>Jump to page</template>
            <template #footer>
                <btn-component color="primary"
                    @click="navigateToPage(dialogNumber)">Go to page {{ dialogNumber }}</btn-component>
                <btn-component color="secondary"
                    @click="dialogVisible = false">Cancel</btn-component>
            </template>

            <div>
                <text-field-component
                    label="Page number"
                    v-model:value="dialogNumber"
                    type="number"
                    ref="dialogNumberInput"></text-field-component>
            </div>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import IdUtils from "@util/IdUtils";
import LinqUtils from "@util/LinqUtils";
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import TextFieldComponent from "./TextFieldComponent.vue";

interface PageinationButton {
    number: number;
    isPage: boolean;
    isDialogButton: boolean;
}

@Options({
    components: {}
})
export default class PagingComponent extends Vue
{
    @Prop({ required: true })
    value!: number;

    @Prop({ required: false, default: 0 })
    count!: number;

    @Prop({ required: false, default: null })
    pagesCount!: number | null;

    @Prop({ required: false, default: 100 })
    pageSize!: number;

    @Prop({ required: false, default: false })
    asIndex!: boolean;

    @Prop({ required: false, default: false })
    disabled!: boolean;

    @Ref() readonly dialogNumberInput!: TextFieldComponent;

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
    get visible(): boolean {
        if (this.pagesCount != null) return this.pagesCount > 0;
        return this.count > 1 && this.pageCount > 1;
    }
    get pageCount(): number {
        if (this.pagesCount != null) return this.pagesCount;
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
        return this.pageCount > this.maxButtonCount + 1;
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

    refreshSizeDelayed(): void {
        setTimeout(() => {
            this.refreshSize();
        }, 10);
    }

    navigateToPage(num: number): void  {
        if (this.asIndex) {
            num--;
        }

        const min = this.asIndex ? 0 : 1;
        const max = this.asIndex ? this.pageCount - 1 : this.pageCount;
        if (num < min) num = min;
        else if (num > max) num = max;

        this.$emit('update:value', num);
        this.$emit('change', num);
        this.currentValue = num;
        this.dialogVisible = false;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onClickedButton(btn: PageinationButton): void {
        if (this.disabled) return;
        if (btn.isPage)
        {
            this.navigateToPage(btn.number);
        }
        else if (btn.isDialogButton)
        {
            this.dialogVisible = true;
            const self = this;
            this.$nextTick(() => {
                self.dialogNumberInput.focus();
            });
        }
    }

    onNextPrevClick(mod: number): void {
        if (this.disabled) return;
        this.navigateToPage(this.currentValue + mod + (this.asIndex ? 1 : 0));
    }

    @Watch("value")
    onValueChanged(): void {
        this.currentValue = this.value;
        this.refreshSizeDelayed();
    }

    @Watch("pageCount")
    onPageCountChanged(): void {
        this.refreshSizeDelayed();
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
        display: inline-flex;
        align-items: center;
        padding: 5px 15px;
        border-radius: 5px;
        background-color: #eee;
        cursor: pointer;
        user-select: none;

        &.active {
            font-weight: 600;
            color: #fff;
            background-color: var(--color--primary-base);
        }

        &.disabled {
            color: #bbb;
            cursor: default;
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