<!-- src/components/modules/Custom/CustomPageComponent.vue -->
<template>
    <div>
        <div class="content-root">
          
          <div v-html="config.RawHtml"></div>
          
        </div>
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