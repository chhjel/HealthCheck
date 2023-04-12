<!-- src/components/Common/SimpleDateTimeComponent.vue -->
<template>
    <div class="root input-component">
        <input-header-component :name="name" :description="description" />
        
        <text-field-component
            class="filter-input" type="datetime-local"
            v-model:value="content"
            v-on:change="onTextChanged"
            v-on:click:clear="onTextChanged"
            :error-messages="error"
            :disabled="readonly"
            clearable />
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import DateUtils from '@util/DateUtils';
import InputHeaderComponent from "./Basic/InputHeaderComponent.vue";

@Options({
    components: { InputHeaderComponent }
})
export default class SimpleDateTimeComponent extends Vue {
    @Prop({ required: true })
    value!: Date | null;

    @Prop({ required: false, default: '' })
    name!: string;

    @Prop({ required: false, default: '' })
    description!: string;

    @Prop({ required: false, default: 'yyyy-MM-ddTHH:mm' })
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
            this.$emit('update:value', null);
            return;
        }

        try 
        {
            const parsedDate = new Date(this.content);
            if (isNaN(parsedDate.getTime()))
            {
                this.error = `Invalid date. Must be empty or on on the format '${this.dateFormat}'.`;
                this.$emit('update:value', null);
            } else {
                this.$emit('update:value', parsedDate);
            }
            return;
        }
        catch(ex: any) {
            this.error = ex;
        }
        this.$emit('update:value', null);
    }
}
</script>

<style scoped lang="scss">
</style>
