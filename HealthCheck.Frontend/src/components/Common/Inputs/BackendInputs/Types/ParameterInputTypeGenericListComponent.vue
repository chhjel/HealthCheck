<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeGenericListComponent.vue -->
<template>
    <div>
        <v-list dense class="parameter-list-input">
            <draggable
                v-model="items"
                group="grp"
                style="min-height: 10px"
                @end="onChanged">
                <template v-for="item in items">
                    <v-list-tile :key="item.Id">
                        <v-list-tile-action v-if="items.length > 1">
                            <v-icon class="handle-icon">drag_handle</v-icon>
                        </v-list-tile-action>
                        <v-list-tile-action v-if="!parameter.ReadOnlyList" @click="removeItem(item)">
                            <v-btn flat icon color="error">
                                <v-icon>remove</v-icon>
                            </v-btn>
                        </v-list-tile-action>
                        <v-list-tile-content style="overflow: visible">
                            <parameter-input-component v-if="!parameter.ReadOnlyList" :parameter="item" :isListItem="true" />
                            <span v-if="parameter.ReadOnlyList">{{ item.Value }}</span>
                        </v-list-tile-content>
                    </v-list-tile>
                </template>
            </draggable>
        </v-list>
        <v-btn v-if="!parameter.ReadOnlyList" small color="primary" @click="addNewItem()">
            <v-icon>add</v-icon>
        </v-btn>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import TestParameterViewModel from  '../../../../../models/modules/TestSuite/TestParameterViewModel';

//@ts-ignore
import draggable from 'vuedraggable'
import DateUtils from  '../../../../../util/DateUtils';

@Component({
    name: "ParameterInputComponent",
    components: {
        draggable
    }
})
export default class ParameterInputTypeGenericListComponent extends Vue {
    @Prop({ required: true })
    parameter!: TestParameterViewModel;

    @Prop({ required: true })
    type!: string;

    items: Array<Partial<TestParameterViewModel>> = [];

    beforeCreate(): void {
        //@ts-ignore
        this.$options.components.ParameterInputComponent = require('../ParameterInputComponent.vue').default
    }

    mounted(): void {
        if (this.parameter.DefaultValue != null) {
            let defaultItems = JSON.parse(this.parameter.DefaultValue);
            defaultItems.forEach((x: any) => {
                this.addNewItem((x == null) ? null : x.toString());
            });
        }
        this.$nextTick(() => {
            this.onChanged();
        });
    }
    
    @Watch('items', {
        deep: true
    })
    onItemsChanged(value: Array<Partial<TestParameterViewModel>>, oldValue: Array<Partial<TestParameterViewModel>>) {
        this.onChanged();
    }

    onChanged(): void {
        this.parameter.Value = JSON.stringify(this.items.map(x => this.prepareValue(x.Value)));
    }

    prepareValue(val: string | null | undefined ): string | null {
        if (val == null || val == undefined) {
            return null;
        }
        else if (this.type == "DateTime" || this.type == "DateTimeOffset") {
            let parts = val.split('-', 3);
            val = `${parts[1]}-${parts[0]}-${parts[2]}`;
            
            let date = new Date(Date.parse(val));
            let formattedDate = `${date.getTime()}${DateUtils.FormatDate(date, 'zzz').replace(':', '')}`;
            return `\/Date(${formattedDate})\/`;
        }

        return val;
    }

    addNewItem(value: string | null = null): void {
        let item = {
            Type: this.type,
            PossibleValues: this.parameter.PossibleValues,
            NotNull: this.parameter.NotNull,
            Value: value
        };
        (<any>item).Id = this.uuidv4();
        this.items.push(item);
        
        this.$nextTick(() => {
            this.onChanged();
        });
    }

    removeItem(item: Partial<TestParameterViewModel>): void {
        this.items = this.items.filter(x => (<any>x).Id !== (<any>item).Id);
    }

    uuidv4(): string {
        return (<any>[1e7]+-1e3+-4e3+-8e3+-1e11).replace(/[018]/g, (c:any) =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        )
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
