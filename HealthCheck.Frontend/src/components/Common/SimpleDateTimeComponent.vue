<!-- src/components/Common/SimpleDateTimeComponent.vue -->
<template>
    <div class="root">
        <v-text-field
            class="filter-input" type="text"
            v-model="content"
            :label="label"
            v-on:change="onTextChanged"
            v-on:click:clear="onTextChanged"
            :error-messages="error"
            :disabled="readonly"
            clearable />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import DateUtils from "../../util/DateUtils";

@Component({
    components: {}
})
export default class SimpleDateTimeComponent extends Vue {
    @Prop({ required: true })
    value!: Date | null;

    @Prop({ required: false })
    label!: string;

    @Prop({ required: false, default: 'yyyy/MM/dd HH:mm:ss' })
    dateFormat!: string;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    content!: string;
    error: string = "";

    beforeMount(): void {
        if (this.value == null)
        {
            this.content = '';
        }
        else
        {
            this.content = DateUtils.FormatDate(this.value, this.dateFormat);
        }
    }

    onTextChanged(): void {
        this.$nextTick(() => this.notifyTextChanged());
    }

    notifyTextChanged(): void {
        this.error = "";

        if (this.content == null || this.content.length == 0)
        {
            this.$emit('input', null);
            return;
        }

        try 
        {
            const parsedDate = new Date(this.content);
            if (isNaN(parsedDate.getTime()))
            {
                this.error = `Invalid date. Must be empty or on on the format '${this.dateFormat}'.`;
                this.$emit('input', null);
            } else {
                this.$emit('input', parsedDate);
            }
            return;
        }
        catch(ex) {
            this.error = ex;
        }
        this.$emit('input', null);
    }
}
</script>

<style scoped lang="scss">
/* .root {
} */
</style>