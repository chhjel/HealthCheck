<!-- src/components/Common/SimpleDateTimeComponent.vue -->
<template>
    <div class="root input-component">
        <div class="input-component--header" v-if="showHeader">
            <div class="input-component--header-name">{{ name }}</div>
            <v-icon small v-if="hasDescription"
                color="gray" class="input-component--help-icon"
                @click="toggleDescription">help</v-icon>
        </div>

        <div v-show="showDescription" class="input-component--description" v-html="description"></div>
        
        <v-text-field
            class="filter-input" type="datetime-local"
            v-model="content"
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
    showDescription: boolean = false;

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

    get showHeader(): boolean {
        return this.name != null && this.name.length > 0;
    }

    get hasDescription(): boolean {
        return this.description != null && this.description.length > 0;
    }

    toggleDescription(): void {
        this.showDescription = !this.showDescription;
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
.input-component {
    .input-component--header {
        text-align: left;

        .input-component--header-name {
            display: inline-block;
            font-size: 16px;
            color: var(--v-secondary-base);
            font-weight: 600;
        }

        .input-component--help-icon {
            user-select: none;
            font-size: 20px !important;
            &:hover {
                color: #1976d2;
            }
        }
    }

    .input-component--description {
        text-align: left;
        padding: 10px;
        border-radius: 10px;
        background-color: #ebf1fb;
    }
}
</style>

<style lang="scss">
.input-component {
    input {
        font-size: 18px;
        color: #000 !important;
    }

    .v-input {
        padding-top: 0;
    }
}
</style>