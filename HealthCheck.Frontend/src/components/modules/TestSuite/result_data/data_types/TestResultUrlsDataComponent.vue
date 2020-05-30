<!-- src/components/modules/TestSuite/result_data/data_types/TestResultUrlsDataComponent.vue -->
<template>
    <div>
      <div v-if="!fullscreen">{{data.Title}}</div>
      <ul>
        <li v-for="(link, index) in links"
          :key="`result-data-item-url-${index}-${link.Url}`">
          <a :href="link.Url" target="_blank">{{link.Text}}</a>
        </li>
      </ul>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestResultDataDumpViewModel from  '../../../../../models/modules/TestSuite/TestResultDataDumpViewModel';
import HyperLinkViewModel from  '../../../../../models/Common/HyperLinkViewModel';

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
    showLoader: boolean = true;

    mounted(): void {
      this.showLoader = true;
    }

    get links(): Array<HyperLinkViewModel> {
      let lines = this.data.Data.split(/\r\n|\r|\n/)
        .filter(x => x.trim().length > 0);
      
      return lines.map(x => {
        let divider = '=>';
        let parts = x.split(divider, 2);
        
        let text = parts[0];
        let url = parts[1];

        if (text == null || text.trim().length == 0) {
          text = url;
        }

        return {
          Text: text,
          Url: url
        };
      });
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

<style>
.v-dialog .data-dump-imageurls {
    height: calc(100vh - 64px);
}
</style>