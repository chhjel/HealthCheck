<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeGenericListComponent.vue -->
<template>
    <div>
        <v-list dense class="parameter-list-input">
            <draggable
                v-model="items"
                group="grp"
                style="min-height: 10px"
                @end="onChanged">
                <template v-for="(item, itemIndex) in items">
                    <v-list-tile :key="`${id}-item-${itemIndex}`">
                        <v-list-tile-action v-if="items.length > 1">
                            <v-icon class="handle-icon">drag_handle</v-icon>
                        </v-list-tile-action>
                        <v-list-tile-action v-if="!isReadOnlyList" @click="removeItem(itemIndex)">
                            <v-btn flat icon color="error">
                                <v-icon>remove</v-icon>
                            </v-btn>
                        </v-list-tile-action>
                        <v-list-tile-content style="overflow: visible">
                            <backend-input-component v-if="!isReadOnlyList" 
                                :type="listType"
                                name=""
                                v-model="items[itemIndex]"
                                :config="config"
                                :isListItem="true" />
                            <span v-if="isReadOnlyList">{{ item.Value }}</span>
                        </v-list-tile-content>
                    </v-list-tile>
                </template>
            </draggable>
        </v-list>
        <v-btn v-if="!isReadOnlyList" small color="primary" @click="addNewItem()">
            <v-icon>add</v-icon>
        </v-btn>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";

//@ts-ignore
import draggable from 'vuedraggable'
import DateUtils from  '../../../../../util/DateUtils';
import IdUtils from "../../../../../util/IdUtils";
import BackendInputConfig from "../BackendInputConfig";

@Component({
    name: "BackendInputComponent",
    components: {
        draggable
    }
})
export default class ParameterInputTypeGenericListComponent extends Vue {
    @Prop({ required: true })
    name!: string;

    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    type!: string;

    @Prop({ required: true })
    listType!: string;

    @Prop({ required: false })
    isListItem!: boolean;

    @Prop({ required: true })
    config!: BackendInputConfig;

    localValue: string | null = '';
    items: Array<string | null> = [];
    id: string = IdUtils.generateId();

    beforeCreate(): void {
        //@ts-ignore
        this.$options.components.BackendInputComponent = require('../BackendInputComponent.vue').default
    }

    mounted(): void {
        if (this.config.defaultValue != null) {
            let defaultItems = JSON.parse(this.config.defaultValue);
            defaultItems.forEach((x: any) => {
                this.addNewItem((x == null) ? null : x.toString());
            });
        }
        this.$nextTick(() => {
            this.onChanged();
        });
    }

    get isReadOnlyList(): boolean {
        return this.config.flags.includes("ReadOnlyList");
    }
    
    @Watch('items', {
        deep: true
    })
    onItemsChanged() {
        this.onChanged();
    }

    @Watch('localValue')
    onLocalValueChanged(): void
    {
        this.$emit('input', this.localValue);
    }

    onChanged(): void {
        this.localValue = JSON.stringify(this.items.map(x => this.prepareValue(x)));
    }

    prepareValue(val: string | null | undefined ): string | null {
        if (val == null || val == undefined) {
            return null;
        }
        else if (this.listType == "DateTime" || this.listType == "DateTimeOffset") {
            let parts = val.split('-', 3);
            val = `${parts[1]}-${parts[0]}-${parts[2]}`;
            
            let date = new Date(Date.parse(val));
            let formattedDate = `${date.getTime()}${DateUtils.FormatDate(date, 'zzz').replace(':', '')}`;
            return `\/Date(${formattedDate})\/`;
        }

        return val;
    }

    addNewItem(value: string | null = null): void {
        // (<any>item).Id = this.uuidv4();
        this.items.push(value);
        
        this.$nextTick(() => {
            this.onChanged();
        });
    }

    removeItem(itemIndex: number): void {
        this.items.splice(itemIndex, 1);
    }
}
</script>

<style scoped>
.handle-icon {
    cursor: grab;
}
</style>

<style>
.parameter-list-input .v-list__tile {
    padding: 0;
}
.parameter-list-input .v-list__tile__action {
    min-width: 32px;
}
</style>
