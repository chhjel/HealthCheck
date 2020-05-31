<!-- src/components/modules/Dataflow/EntryProperties/DataflowEntryPropertyValueDictionaryComponent.vue -->
<template>
    <div>
        <b>{{ title }}</b>
        <ul v-if="items.length > 0">
            <li v-for="(item, index) in items"
                :key="`dict-item-${idPrefix}-${index}`">
                <b>{{ item.title }}</b>: {{ item.value }}
            </li>
        </ul>
        <div v-if="text.length > 0"><i>{{ text }}</i></div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { DataFlowPropertyUIHint } from  '../../../../models/modules/Dataflow/DataFlowPropertyDisplayInfo';

@Component({
    components: {
    }
})
export default class DataflowEntryPropertyValueDictionaryComponent extends Vue {
    @Prop({ required: true })
    type!: DataFlowPropertyUIHint;

    @Prop({ required: true })
    title!: string;

    @Prop({ required: true })
    raw!: any;

    idPrefix: string = '';

    get text(): string
    {
        return (this.items.length > 0) ? '' : '<empty>';
    }

    get items(): Array<any>
    {
        try {
            let items: Array<any> = [];
            for (let key in this.raw)
            {
                items.push({ title: key, value: this.raw[key] });
            }
            return items;
        } catch(e) { return []; }
    }

    created(): void
    {
        this.idPrefix = this.generateId();
    }
    
    generateId(): string {
        return (<any>[1e7]+-1e3+-4e3+-8e3+-1e11).replace(/[018]/g, (c:any) =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    }
}
</script>

<style scoped>
</style>
