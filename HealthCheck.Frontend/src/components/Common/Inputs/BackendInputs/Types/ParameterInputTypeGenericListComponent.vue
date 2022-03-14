<!-- src/components/modules/TestSuite/paremeter_inputs/input_types/ParameterInputTypeGenericListComponent.vue -->
<template>
    <div>
        <v-list dense class="parameter-list-input" v-if="items.length > 0">
            <draggable
                v-model="items"
                group="grp"
                style="min-height: 10px"
                @end="onChanged">
                <template v-for="(item, itemIndex) in items" :key="`${id}-item-${item.id}`">
                    <v-list-tile class="parameter-list-input-tile">
                        <v-list-tile-action v-if="items.length > 1">
                            <v-icon class="handle-icon">drag_handle</v-icon>
                        </v-list-tile-action>

                        <v-tooltip bottom v-if="!isReadOnlyList" >
                            <template v-slot:activator="{ on }">
                                <v-list-tile-action v-if="!isReadOnlyList" @click="removeItem(itemIndex)" v-on="on">
                                    <v-btn flat icon color="error" :disabled="readonly">
                                        <v-icon>remove</v-icon>
                                    </v-btn>
                                </v-list-tile-action>
                            </template>
                            <span>Remove</span>
                        </v-tooltip>

                        <v-list-tile-content style="max-width: 100%;">
                            <backend-input-component v-if="!isReadOnlyList"
                                :key="`${id}-item-input-${item.id}`"
                                :forceType="listType"
                                forceName=""
                                v-model="item.value"
                                :config="config"
                                :isListItem="true"
                                :isCustomReferenceType="isCustomReferenceType"
                                :parameterDetailContext="`${parameterDetailContext}_${item.id}`"
                                :referenceValueFactoryConfig="referenceValueFactoryConfig"
                                @isAnyJson="notifyIsAnyJson()"
                                style="max-width: 100%;" />
                            <span v-if="isReadOnlyList">{{ item.value }}</span>
                        </v-list-tile-content>
                    </v-list-tile>
                </template>
            </draggable>
        </v-list>
        <v-btn v-if="!isReadOnlyList" small color="primary" @click="addNewItem()" :disabled="readonly" class="ml-0">
            <v-icon>add</v-icon>
        </v-btn>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Inject } from "vue-property-decorator";
import { Options } from "vue-class-component";

//@ts-ignore
import draggable from 'vuedraggable'
import DateUtils from '@util/DateUtils';
import IdUtils from "@util/IdUtils";
import { HCBackendInputConfig } from '@generated/Models/Core/HCBackendInputConfig';
// import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import TestsUtils from "@util/TestsModule/TestsUtils";
import { ReferenceValueFactoryConfigViewModel } from "@generated/Models/Core/ReferenceValueFactoryConfigViewModel";

interface ListItem {
    id: string;
    value: string | null;
}

@Options({
    name: "BackendInputComponent",
    components: {
        draggable,
        BackendInputComponent: () => import("@components/Common/Inputs/BackendInputs/BackendInputComponent.vue")
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
    config!: HCBackendInputConfig;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    @Prop({ required: false, default: false })
    isCustomReferenceType!: boolean;

    @Prop({ required: false, default: '' })
    parameterDetailContext!: string;

    @Prop({ required: false, default: null })
    referenceValueFactoryConfig!: ReferenceValueFactoryConfigViewModel | null;

    localValue: string | null = '';
    items: Array<ListItem> = [];
    id: string = IdUtils.generateId();

    beforeCreate(): void {
        //@ts-ignore
        this.$options.components.BackendInputComponent = require('../BackendInputComponent.vue').default
    }

    mounted(): void {
        const loadedValue = this.getParameterDetail('selection');
        if (loadedValue) { this.items = loadedValue as Array<ListItem>; }

        if (this.config.DefaultValue != null) {
            let defaultItems = JSON.parse(this.config.DefaultValue);
            defaultItems.forEach((x: any) => {
                this.addNewItem((x == null) ? null : x.toString());
            });
        }
        this.$nextTick(() => {
            this.onChanged();
        });
    }

    get isReadOnlyList(): boolean {
        return this.config.Flags.includes("ReadOnlyList");
    }

    notifyIsAnyJson(): void {
        this.$emit('isAnyJson');
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
        this.localValue = JSON.stringify(this.items.map(x => this.prepareValue(x.value)));
        this.updateParameterDetails();
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
        this.items.push({
            id: IdUtils.generateId(),
            value: value
        });
        
        this.$nextTick(() => {
            this.onChanged();
        });
    }

    removeItem(itemIndex: number): void {
        this.items.splice(itemIndex, 1);
        this.updateParameterDetails();
    }

    updateParameterDetails(): void {
        this.setParameterDetail('selection', this.items);
    }

    //////////////////////////
    //  PARAMETER DETAILS  //
    ////////////////////////
    createParameterDetailKey(): string {
        return this.parameterDetailContext + "_" + this.config.Id;
    }
    setParameterDetail<T>(key: string, value: T): void {
        return TestsUtils.setParameterDetail<T>(this.$store, this.createParameterDetailKey(), key, value);
    }
    getParameterDetail<T>(key: string): T | null {
        return TestsUtils.getParameterDetail<T>(this.$store, this.createParameterDetailKey(), key);
    }
}
</script>

<style scoped>
.handle-icon {
    cursor: grab;
}
.parameter-list-input {
    background: none;
}
</style>

<style>
.parameter-list-input .v-list__tile {
    padding: 0;
}
.parameter-list-input .v-list__tile__action {
    min-width: 32px;
}
.parameter-list-input-tile {
    overflow: hidden;
}
</style>
