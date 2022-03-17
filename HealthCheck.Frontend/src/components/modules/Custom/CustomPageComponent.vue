<!-- src/components/modules/Custom/CustomPageComponent.vue -->
<template>
    <div>
        <content-component class="pl-0">
        <div fluid fill-height class="content-root">
        <div>
        <div class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
          <div v-html="config.RawHtml"></div>
          <!-- CONTENT END -->
        </div>
        </div>
        </div>
        </content-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
// @ts-ignore
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';

@Options({ components: {} })
export default class CustomPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;
}
</script>

<style scoped lang="scss">
</style>