<!-- src/components/result_data/data_types/TestResultImageUrlsDataComponent.vue -->
<template>
    <div>
      <v-carousel 
        :height="'100%'"
        :cycle="false"
        :hide-delimiters="!showControls"
        :hide-controls="!showControls">
        <v-carousel-item
          v-for="(url,index) in urls"
          :key="`result-data-item-${index}-${url}`"
          :src="url"
          @click.native="showControls = !showControls"
        >
          <div class="details" v-if="showControls">
            <div class="details-title">
              <a :href="url" target="_blank">Image {{index+1}}/{{urls.length}}: {{url}}</a>
            </div>
            <div style="clear: both;"></div>
          </div>
          
        </v-carousel-item>
      </v-carousel>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestResultDataDumpViewModel from "../../../../models/TestSuite/TestResultDataDumpViewModel";

@Component({
    components: {
    }
})
export default class TestResultImageUrlsDataComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;

    showControls: boolean = true;

    mounted(): void {
    }

    get urls(): Array<string> {
      return this.data.Data.split(/\r\n|\r|\n/)
        .filter(x => x.trim().length > 0);
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
</style>

<style>
.v-dialog .data-dump-imageurls {
    height: calc(100vh - 64px);
}
</style>