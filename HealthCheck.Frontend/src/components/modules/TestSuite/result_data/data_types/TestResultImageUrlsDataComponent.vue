<!-- src/components/modules/TestSuite/result_data/data_types/TestResultImageUrlsDataComponent.vue -->
<template>
    <div>
      <progress-linear-component
        v-if="showLoader"
        indeterminate color="primary"></progress-linear-component>

      <carousel-component>
        <div
          class="image-result-data-item"
          v-for="(url,index) in urls"
          :key="`result-data-item-${index}-${url}`"
          :src="url"
          @click="showControls = !showControls"
          v-on:load="onImageLoaded"
        >
          <div class="details" v-if="showControls">
            <div class="details-title">
              <a :href="url" target="_blank">Image {{index+1}}/{{urls.length}}: {{url}}</a>
            </div>
            <div style="clear: both;"></div>
          </div>
        </div>
      </carousel-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestResultDataDumpViewModel } from '@generated/Models/Core/TestResultDataDumpViewModel';

@Options({
    components: {
    }
})
export default class TestResultImageUrlsDataComponent extends Vue {
    @Prop({ required: true })
    resultData!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;

    showControls: boolean = true;
    showLoader: boolean = true;

    mounted(): void {
      this.showLoader = true;
    }

    get urls(): Array<string> {
      return this.resultData.Data.split(/\r\n|\r|\n/)
        .filter(x => x.trim().length > 0);
    }

    onImageLoaded(url: string): void {
      if (url == this.urls[0]) {
        this.showLoader = false;
      }
    }
}
</script>

<style scoped>
.details {
  background-color: #00000088;
}
.details-title {
  float: left;
  color: #fff;
  padding: 5px;
}
.details-title a {
  color: #fff;
}
.details-title a:visited {
  color: #fff;
}
.details-icon {
  float: right;
}
.image-result-data-item {
  min-height: 100px;
}
</style>
