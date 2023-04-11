<!-- src/components/modules/TestSuite/result_data/data_types/TestResultImageUrlsDataComponent.vue -->
<template>
	<div>
		<carousel-component :items="items" :height="height" />
	</div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestResultDataDumpViewModel } from "@generated/Models/Core/TestResultDataDumpViewModel";
import { CarouselItem } from "@components/Common/Basic/CarouselComponent.vue.models";

@Options({
	components: {},
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

	get items(): Array<CarouselItem> {
		const urls = this.resultData.Data.split(/\r\n|\r|\n/)
      .filter((x) => x.trim().length > 0);
    return urls.map((url, index) => {
      const encodedUrl = encodeURI(url);
      const html = `<a href="${encodedUrl}" target="_blank">Image ${(index + 1)}/${urls.length}: ${url}</a>`;
      return {
        url: url,
        detailsHtml: html
      }
    });
	}

	get height(): string {
		if (this.fullscreen) return 'calc(100vh - 160px)';
		else return "400px";
	}
}
</script>

<style scoped>
</style>
