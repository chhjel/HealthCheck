<!-- src/components/modules/Documentation/DocumentationPageComponent.vue -->
<template>
	<div class="module-root">
		<!-- NAVIGATION DRAWER -->
		<Teleport to="#module-nav-menu"> Nav here </Teleport>

		<!-- CONTENT -->
		<div class="content-root">
			<h3>data-table-component</h3>
			<!-- DataTableComponent -->
			<data-table-component
				:groups="DataTableComponent_groups"
				:headers="DataTableComponent_headers"
			></data-table-component>
			<hr />

			<h3>editor-component</h3>
			<!-- EditorComponent -->
			<editor-component style="height:200px"></editor-component>
			<hr />

			<h3>filterable-list-component</h3>
			<!-- FilterableListComponent -->
			<filterable-list-component
				:items="FilterableListComponent_items"
			></filterable-list-component>
			<hr />

			<h3>filter-input-component</h3>
			<!-- FilterInputComponent -->
			<filter-input-component
				v-model:value="FilterInputComponent_value"
			></filter-input-component>
			<hr />

			<h3>flow-diagram-component</h3>
			<!-- FlowDiagramComponent -->
			<flow-diagram-component
				:title="FlowDiagramComponent_title"
				:steps="FlowDiagramComponent_steps"
			></flow-diagram-component>
			<hr />

            <!-- 
			<h3>loading-screen-component</h3>
			<loading-screen-component></loading-screen-component>
			<hr /> -->

			<h3>progress-bar-component</h3>
			<!-- ProgressBarComponent -->
			<progress-bar-component
				:max="ProgressBarComponent_max"
				:success="ProgressBarComponent_success"
				:error="ProgressBarComponent_error"
			></progress-bar-component>
			<hr />

			<h3>sequence-diagram-component</h3>
			<!-- SequenceDiagramComponent -->
			<sequence-diagram-component
				:steps="SequenceDiagramComponent_steps"
			></sequence-diagram-component>
			<hr />

			<h3>simple-date-time-component</h3>
			<!-- SimpleDateTimeComponent -->
			<simple-date-time-component
				v-model:value="SimpleDateTimeComponent_value"
			></simple-date-time-component>
			<hr />

			<h3>alert-component</h3>
			<!-- AlertComponent -->
			<alert-component v-model:value="AlertComponent_value">Some alert here!</alert-component>
			<hr />

			<h3>autocomplete-component</h3>
			<!-- AutocompleteComponent -->
			<autocomplete-component
				v-model:value="AutocompleteComponent_value"
			></autocomplete-component>
			<hr />

			<h3>badge-component</h3>
			<!-- BadgeComponent -->
			<badge-component>42</badge-component>
			<hr />

			<h3>block-component</h3>
			<!-- BlockComponent -->
			<block-component>Block content</block-component>
			<hr />

			<h3>btn-component</h3>
			<!-- BtnComponent -->
			<btn-component>Button</btn-component>
			<hr />

			<h3>calendar-component</h3>
			<!-- CalendarComponent -->
			<calendar-component
				v-model:value="CalendarComponent_value"
			></calendar-component>
			<hr />

			<h3>card-component</h3>
			<!-- CardComponent -->
			<card-component>Card?</card-component>
			<hr />

			<h3>carousel-component</h3>
			<!-- CarouselComponent -->
			<carousel-component></carousel-component>
			<hr />

			<h3>checkbox-component</h3>
			<!-- CheckboxComponent -->
			<checkbox-component
				v-model:value="CheckboxComponent_value"
			></checkbox-component>
			<hr />

			<h3>chip-component</h3>
			<!-- ChipComponent -->
			<chip-component></chip-component>
			<hr />

			<h3>combobox-component</h3>
			<!-- ComboboxComponent -->
			<combobox-component
				v-model:value="ComboboxComponent_value"
			></combobox-component>
			<hr />

			<h3>date-picker-component</h3>
			<!-- DatePickerComponent -->
			<date-picker-component
				v-model:value="DatePickerComponent_value"
			></date-picker-component>
			<hr />

			<h3>dialog-component</h3>
			<!-- DialogComponent -->
			<dialog-component v-model:value="DialogComponent_value"></dialog-component>
			<hr />

			<h3>expansion-panel-component</h3>
			<!-- ExpansionPanelComponent -->
			<expansion-panel-component
				v-model:value="ExpansionPanelComponent_value"
			></expansion-panel-component>
			<hr />

			<h3>fetch-status-progress-component</h3>
			<!-- FetchStatusProgressComponent -->
			<fetch-status-progress-component
				:status="FetchStatusProgressComponent_status"
			></fetch-status-progress-component>
			<hr />

			<h3>icon-component</h3>
			<!-- IconComponent -->
			<icon-component></icon-component>
			<hr />

			<h3>input-component</h3>
			<!-- InputComponent -->
			<input-component v-model:value="InputComponent_value"></input-component>
			<hr />

			<h3>input-header-component</h3>
			<!-- InputHeaderComponent -->
			<input-header-component></input-header-component>
			<hr />

			<h3>menu-component</h3>
			<!-- MenuComponent -->
			<menu-component></menu-component>
			<hr />

			<h3>pagination-component</h3>
			<!-- PaginationComponent -->
			<pagination-component
				v-model:value="PaginationComponent_value"
			></pagination-component>
			<hr />

			<h3>paging-component</h3>
			<!-- PagingComponent -->
			<paging-component
				v-model:value="PagingComponent_value"
				:count="PagingComponent_count"
			></paging-component>
			<hr />

			<h3>progress-circular-component</h3>
			<!-- ProgressCircularComponent -->
			<progress-circular-component></progress-circular-component>
			<hr />

			<h3>progress-linear-component</h3>
			<!-- ProgressLinearComponent -->
			<progress-linear-component></progress-linear-component>
			<hr />

			<h3>select-component</h3>
			<!-- SelectComponent -->
			<select-component
				v-model:value="SelectComponent_value"
				:items="SelectComponent_items"
			></select-component>
			<hr />

			<h3>simple-paging-component</h3>
			<!-- SimplePagingComponent -->
			<simple-paging-component
				v-model:value="SimplePagingComponent_value"
				:items="SimplePagingComponent_items"
			></simple-paging-component>
			<hr />

			<h3>snackbar-component</h3>
			<!-- SnackbarComponent -->
			<snackbar-component
				v-model:value="SnackbarComponent_value"
			></snackbar-component>
			<hr />

			<h3>stepper-component</h3>
			<!-- StepperComponent -->
			<stepper-component></stepper-component>
			<hr />

			<h3>switch-component</h3>
			<!-- SwitchComponent -->
			<switch-component v-model:value="SwitchComponent_value"></switch-component>
			<hr />

			<h3>textarea-component</h3>
			<!-- TextareaComponent -->
			<textarea-component
				v-model:value="TextareaComponent_value"
			></textarea-component>
			<hr />

			<h3>text-field-component</h3>
			<!-- TextFieldComponent -->
			<text-field-component
				v-model:value="TextFieldComponent_value"
			></text-field-component>
			<hr />

			<h3>timeline-component</h3>
			<!-- TimelineComponent -->
			<timeline-component></timeline-component>
			<hr />

			<h3>timespan-input-component</h3>
			<!-- TimespanInputComponent -->
			<timespan-input-component
				v-model:value="TimespanInputComponent_value"
			></timespan-input-component>
			<hr />

			<h3>toolbar-component</h3>
			<!-- ToolbarComponent -->
			<toolbar-component></toolbar-component>
			<hr />

			<h3>tooltip-component</h3>
			<!-- TooltipComponent -->
			<tooltip-component></tooltip-component>
			<hr />

			<h3>bar-chart-component</h3>
			<!-- BarChartComponent -->
			<bar-chart-component
				:sets="BarChartComponent_sets"
				:bars="BarChartComponent_bars"
			></bar-chart-component>
			<hr />

			<h3>data-over-time-chart-component</h3>
			<!-- DataOverTimeChartComponent -->
			<data-over-time-chart-component
				:entries="DataOverTimeChartComponent_entries"
				:sets="DataOverTimeChartComponent_sets"
			></data-over-time-chart-component>
			<hr />

			<h3>floating-squares-effect-component</h3>
			<!-- FloatingSquaresEffectComponent -->
			<floating-squares-effect-component></floating-squares-effect-component>
			<hr />

			<h3>backend-input-component</h3>
			<!-- BackendInputComponent -->
			<backend-input-component
				:config="BackendInputComponent_config"
				v-model:value="BackendInputComponent_value"
			></backend-input-component>
			<hr />

			<h3>unknown-backend-input-component</h3>
			<!-- UnknownBackendInputComponent -->
			<unknown-backend-input-component
				v-model:value="UnknownBackendInputComponent_value"
				:type="UnknownBackendInputComponent_type"
			></unknown-backend-input-component>
			<hr />

			<h3>parameter-input-any-json-component</h3>
			<!-- ParameterInputAnyJsonComponent -->
			<parameter-input-any-json-component
				:name="ParameterInputAnyJsonComponent_name"
				v-model:value="ParameterInputAnyJsonComponent_value"
				:type="ParameterInputAnyJsonComponent_type"
				:config="ParameterInputAnyJsonComponent_config"
			></parameter-input-any-json-component>
			<hr />

			<h3>parameter-input-pick-reference-component</h3>
			<!-- ParameterInputPickReferenceComponent -->
			<parameter-input-pick-reference-component
				v-model:value="ParameterInputPickReferenceComponent_value"
				:config="ParameterInputPickReferenceComponent_config"
			></parameter-input-pick-reference-component>
			<hr />

			<h3>parameter-input-type-boolean-component</h3>
			<!-- ParameterInputTypeBooleanComponent -->
			<parameter-input-type-boolean-component
				v-model:value="ParameterInputTypeBooleanComponent_value"
				:config="ParameterInputTypeBooleanComponent_config"
			></parameter-input-type-boolean-component>
			<hr />

			<h3>parameter-input-type-date-time-component</h3>
			<!-- ParameterInputTypeDateTimeComponent -->
			<parameter-input-type-date-time-component
				v-model:value="ParameterInputTypeDateTimeComponent_value"
				:config="ParameterInputTypeDateTimeComponent_config"
			></parameter-input-type-date-time-component>
			<hr />

			<h3>parameter-input-type-date-time-offset-component</h3>
			<!-- ParameterInputTypeDateTimeOffsetComponent -->
			<parameter-input-type-date-time-offset-component
				v-model:value="ParameterInputTypeDateTimeOffsetComponent_value"
				:config="ParameterInputTypeDateTimeOffsetComponent_config"
			></parameter-input-type-date-time-offset-component>
			<hr />

			<h3>parameter-input-type-decimal-component</h3>
			<!-- ParameterInputTypeDecimalComponent -->
			<parameter-input-type-decimal-component
				v-model:value="ParameterInputTypeDecimalComponent_value"
				:config="ParameterInputTypeDecimalComponent_config"
			></parameter-input-type-decimal-component>
			<hr />

			<h3>parameter-input-type-double-component</h3>
			<!-- ParameterInputTypeDoubleComponent -->
			<parameter-input-type-double-component
				v-model:value="ParameterInputTypeDoubleComponent_value"
				:config="ParameterInputTypeDoubleComponent_config"
			></parameter-input-type-double-component>
			<hr />

			<h3>parameter-input-type-enum-component</h3>
			<!-- ParameterInputTypeEnumComponent -->
			<parameter-input-type-enum-component
				v-model:value="ParameterInputTypeEnumComponent_value"
				:config="ParameterInputTypeEnumComponent_config"
			></parameter-input-type-enum-component>
			<hr />

			<h3>parameter-input-type-flagged-enum-component</h3>
			<!-- ParameterInputTypeFlaggedEnumComponent -->
			<parameter-input-type-flagged-enum-component
				v-model:value="ParameterInputTypeFlaggedEnumComponent_value"
				:config="ParameterInputTypeFlaggedEnumComponent_config"
			></parameter-input-type-flagged-enum-component>
			<hr />

			<h3>parameter-input-type-generic-list-component</h3>
			<!-- ParameterInputTypeGenericListComponent -->
			<parameter-input-type-generic-list-component
				:name="ParameterInputTypeGenericListComponent_name"
				v-model:value="ParameterInputTypeGenericListComponent_value"
				:type="ParameterInputTypeGenericListComponent_type"
				:listType="ParameterInputTypeGenericListComponent_listType"
				:config="ParameterInputTypeGenericListComponent_config"
			></parameter-input-type-generic-list-component>
			<hr />

			<h3>parameter-input-type-guid-component</h3>
			<!-- ParameterInputTypeGuidComponent -->
			<parameter-input-type-guid-component
				v-model:value="ParameterInputTypeGuidComponent_value"
				:config="ParameterInputTypeGuidComponent_config"
			></parameter-input-type-guid-component>
			<hr />

			<h3>parameter-input-type-http-posted-file-base-component</h3>
			<!-- ParameterInputTypeHttpPostedFileBaseComponent -->
			<parameter-input-type-http-posted-file-base-component
				v-model:value="ParameterInputTypeHttpPostedFileBaseComponent_value"
				:config="ParameterInputTypeHttpPostedFileBaseComponent_config"
			></parameter-input-type-http-posted-file-base-component>
			<hr />

			<h3>parameter-input-type-int32-component</h3>
			<!-- ParameterInputTypeInt32Component -->
			<parameter-input-type-int32-component
				v-model:value="ParameterInputTypeInt32Component_value"
				:config="ParameterInputTypeInt32Component_config"
			></parameter-input-type-int32-component>
			<hr />

			<h3>parameter-input-type-int64-component</h3>
			<!-- ParameterInputTypeInt64Component -->
			<parameter-input-type-int64-component
				v-model:value="ParameterInputTypeInt64Component_value"
				:config="ParameterInputTypeInt64Component_config"
			></parameter-input-type-int64-component>
			<hr />

			<h3>parameter-input-type-single-component</h3>
			<!-- ParameterInputTypeSingleComponent -->
			<parameter-input-type-single-component
				v-model:value="ParameterInputTypeSingleComponent_value"
				:config="ParameterInputTypeSingleComponent_config"
			></parameter-input-type-single-component>
			<hr />

			<h3>parameter-input-type-string-component</h3>
			<!-- ParameterInputTypeStringComponent -->
			<parameter-input-type-string-component
				v-model:value="ParameterInputTypeStringComponent_value"
				:config="ParameterInputTypeStringComponent_config"
			></parameter-input-type-string-component>
			<hr />

			<h3>parameter-input-type-time-span-component</h3>
			<!-- ParameterInputTypeTimeSpanComponent -->
			<parameter-input-type-time-span-component
				v-model:value="ParameterInputTypeTimeSpanComponent_value"
				:config="ParameterInputTypeTimeSpanComponent_config"
			></parameter-input-type-time-span-component>
			<hr />
		</div>
	</div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";

import DataTableComponent from "@components/Common/DataTableComponent.vue";
import EditorComponent from "@components/Common/EditorComponent.vue";
import FilterableListComponent from "@components/Common/FilterableListComponent.vue";
import FilterInputComponent from "@components/Common/FilterInputComponent.vue";
import FlowDiagramComponent from "@components/Common/FlowDiagramComponent.vue";
import LoadingScreenComponent from "@components/Common/LoadingScreenComponent.vue";
import ProgressBarComponent from "@components/Common/ProgressBarComponent.vue";
import SequenceDiagramComponent from "@components/Common/SequenceDiagramComponent.vue";
import SimpleDateTimeComponent from "@components/Common/SimpleDateTimeComponent.vue";
import AlertComponent from "@components/Common/Basic/AlertComponent.vue";
import AutocompleteComponent from "@components/Common/Basic/AutocompleteComponent.vue";
import BadgeComponent from "@components/Common/Basic/BadgeComponent.vue";
import BlockComponent from "@components/Common/Basic/BlockComponent.vue";
import BtnComponent from "@components/Common/Basic/BtnComponent.vue";
import CalendarComponent from "@components/Common/Basic/CalendarComponent.vue";
import CardComponent from "@components/Common/Basic/CardComponent.vue";
import CarouselComponent from "@components/Common/Basic/CarouselComponent.vue";
import CheckboxComponent from "@components/Common/Basic/CheckboxComponent.vue";
import ChipComponent from "@components/Common/Basic/ChipComponent.vue";
import ComboboxComponent from "@components/Common/Basic/ComboboxComponent.vue";
import DatePickerComponent from "@components/Common/Basic/DatePickerComponent.vue";
import DialogComponent from "@components/Common/Basic/DialogComponent.vue";
import ExpansionPanelComponent from "@components/Common/Basic/ExpansionPanelComponent.vue";
import FetchStatusProgressComponent from "@components/Common/Basic/FetchStatusProgressComponent.vue";
import IconComponent from "@components/Common/Basic/IconComponent.vue";
import InputComponent from "@components/Common/Basic/InputComponent.vue";
import InputHeaderComponent from "@components/Common/Basic/InputHeaderComponent.vue";
import MenuComponent from "@components/Common/Basic/MenuComponent.vue";
import PaginationComponent from "@components/Common/Basic/PaginationComponent.vue";
import PagingComponent from "@components/Common/Basic/PagingComponent.vue";
import ProgressCircularComponent from "@components/Common/Basic/ProgressCircularComponent.vue";
import ProgressLinearComponent from "@components/Common/Basic/ProgressLinearComponent.vue";
import SelectComponent from "@components/Common/Basic/SelectComponent.vue";
import SimplePagingComponent from "@components/Common/Basic/SimplePagingComponent.vue";
import SnackbarComponent from "@components/Common/Basic/SnackbarComponent.vue";
import StepperComponent from "@components/Common/Basic/StepperComponent.vue";
import SwitchComponent from "@components/Common/Basic/SwitchComponent.vue";
import TextareaComponent from "@components/Common/Basic/TextareaComponent.vue";
import TextFieldComponent from "@components/Common/Basic/TextFieldComponent.vue";
import TimelineComponent from "@components/Common/Basic/TimelineComponent.vue";
import TimespanInputComponent from "@components/Common/Basic/TimespanInputComponent.vue";
import ToolbarComponent from "@components/Common/Basic/ToolbarComponent.vue";
import TooltipComponent from "@components/Common/Basic/TooltipComponent.vue";
import BarChartComponent from "@components/Common/Charts/BarChartComponent.vue";
import DataOverTimeChartComponent from "@components/Common/Charts/DataOverTimeChartComponent.vue";
import FloatingSquaresEffectComponent from "@components/Common/Effects/FloatingSquaresEffectComponent.vue";
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import UnknownBackendInputComponent from "@components/Common/Inputs/BackendInputs/UnknownBackendInputComponent.vue";
import ParameterInputAnyJsonComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputAnyJsonComponent.vue";
import ParameterInputPickReferenceComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputPickReferenceComponent.vue";
import ParameterInputTypeBooleanComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeBooleanComponent.vue";
import ParameterInputTypeDateTimeComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeDateTimeComponent.vue";
import ParameterInputTypeDateTimeOffsetComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeDateTimeOffsetComponent.vue";
import ParameterInputTypeDecimalComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeDecimalComponent.vue";
import ParameterInputTypeDoubleComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeDoubleComponent.vue";
import ParameterInputTypeEnumComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeEnumComponent.vue";
import ParameterInputTypeFlaggedEnumComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeFlaggedEnumComponent.vue";
import ParameterInputTypeGenericListComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeGenericListComponent.vue";
import ParameterInputTypeGuidComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeGuidComponent.vue";
import ParameterInputTypeHttpPostedFileBaseComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeHttpPostedFileBaseComponent.vue";
import ParameterInputTypeInt32Component from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeInt32Component.vue";
import ParameterInputTypeInt64Component from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeInt64Component.vue";
import ParameterInputTypeSingleComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeSingleComponent.vue";
import ParameterInputTypeStringComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeStringComponent.vue";
import ParameterInputTypeTimeSpanComponent from "@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeTimeSpanComponent.vue";
import { FetchStatusWithProgress } from "@services/abstractions/HCServiceBase";
import { HCBackendInputConfig } from "@generated/Models/Core/HCBackendInputConfig";

@Options({
	components: {
		DataTableComponent,
		EditorComponent,
		FilterableListComponent,
		FilterInputComponent,
		FlowDiagramComponent,
		LoadingScreenComponent,
		ProgressBarComponent,
		SequenceDiagramComponent,
		SimpleDateTimeComponent,
		AlertComponent,
		AutocompleteComponent,
		BadgeComponent,
		BlockComponent,
		BtnComponent,
		CalendarComponent,
		CardComponent,
		CarouselComponent,
		CheckboxComponent,
		ChipComponent,
		ComboboxComponent,
		DatePickerComponent,
		DialogComponent,
		ExpansionPanelComponent,
		FetchStatusProgressComponent,
		IconComponent,
		InputComponent,
		InputHeaderComponent,
		MenuComponent,
		PaginationComponent,
		PagingComponent,
		ProgressCircularComponent,
		ProgressLinearComponent,
		SelectComponent,
		SimplePagingComponent,
		SnackbarComponent,
		StepperComponent,
		SwitchComponent,
		TextareaComponent,
		TextFieldComponent,
		TimelineComponent,
		TimespanInputComponent,
		ToolbarComponent,
		TooltipComponent,
		BarChartComponent,
		DataOverTimeChartComponent,
		FloatingSquaresEffectComponent,
		BackendInputComponent,
		UnknownBackendInputComponent,
		ParameterInputAnyJsonComponent,
		ParameterInputPickReferenceComponent,
		ParameterInputTypeBooleanComponent,
		ParameterInputTypeDateTimeComponent,
		ParameterInputTypeDateTimeOffsetComponent,
		ParameterInputTypeDecimalComponent,
		ParameterInputTypeDoubleComponent,
		ParameterInputTypeEnumComponent,
		ParameterInputTypeFlaggedEnumComponent,
		ParameterInputTypeGenericListComponent,
		ParameterInputTypeGuidComponent,
		ParameterInputTypeHttpPostedFileBaseComponent,
		ParameterInputTypeInt32Component,
		ParameterInputTypeInt64Component,
		ParameterInputTypeSingleComponent,
		ParameterInputTypeStringComponent,
		ParameterInputTypeTimeSpanComponent,
	},
})
export default class DevPageComponent extends Vue {
	DataTableComponent_groups: Array<any>= [];
	DataTableComponent_headers: Array<string> = [
		"Some string here",
		"Some string here",
		"Some string here",
	];
	FilterableListComponent_items: Array<any>= [];
	FilterInputComponent_value: string = "Some string here";
	FlowDiagramComponent_title: string = "Some string here";
	FlowDiagramComponent_steps: Array<any>= [];
	ProgressBarComponent_max: number = 88;
	ProgressBarComponent_success: number = 88;
	ProgressBarComponent_error: number = 88;
	SequenceDiagramComponent_steps: Array<any>= [];
	SimpleDateTimeComponent_value: Date | null = new Date();
	AlertComponent_value: boolean = true;
	AutocompleteComponent_value: string = "Some string here";
	CalendarComponent_value: string = "Some string here";
	CheckboxComponent_value: boolean = true;
	ComboboxComponent_value: string = "Some string here";
	DatePickerComponent_value: string = "Some string here";
	DialogComponent_value: boolean = true;
	ExpansionPanelComponent_value: boolean = true;
	FetchStatusProgressComponent_status: FetchStatusWithProgress | null = { inProgress: true, progress: 50, failed: false, errorMessage: null };
	InputComponent_value: string = "Some string here";
	PaginationComponent_value: string = "Some string here";
	PagingComponent_value: number = 88;
	PagingComponent_count: number = 88;
	SelectComponent_value: string = "Some string here";
	SelectComponent_items: Array<string> = ['A', 'B', 'C'];
	SimplePagingComponent_value: number = 88;
	SimplePagingComponent_items: Array<any>= [];
	SnackbarComponent_value: boolean = true;
	SwitchComponent_value: boolean = true;
	TextareaComponent_value: string = "Some string here";
	TextFieldComponent_value: string = "Some string here";
	TimespanInputComponent_value: string = "Some string here";
	BarChartComponent_sets: Array<any>= [];
	BarChartComponent_bars: Array<any>= [];
	DataOverTimeChartComponent_entries: Array<any>= [];
	DataOverTimeChartComponent_sets: Array<any>= [];
	BackendInputComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	BackendInputComponent_value: string = "Some string here";
	UnknownBackendInputComponent_value: string = "Some string here";
	UnknownBackendInputComponent_type: string = "Some string here";
	ParameterInputAnyJsonComponent_name: string = "Some string here";
	ParameterInputAnyJsonComponent_value: string = "Some string here";
	ParameterInputAnyJsonComponent_type: string = "Some string here";
	ParameterInputAnyJsonComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputPickReferenceComponent_value: string = "Some string here";
	ParameterInputPickReferenceComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeBooleanComponent_value: boolean = true;
	ParameterInputTypeBooleanComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeDateTimeComponent_value: string = "Some string here";
	ParameterInputTypeDateTimeComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeDateTimeOffsetComponent_value: string = "Some string here";
	ParameterInputTypeDateTimeOffsetComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeDecimalComponent_value: string = "Some string here";
	ParameterInputTypeDecimalComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeDoubleComponent_value: string = "Some string here";
	ParameterInputTypeDoubleComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeEnumComponent_value: string = "Some string here";
	ParameterInputTypeEnumComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeFlaggedEnumComponent_value: string = "Some string here";
	ParameterInputTypeFlaggedEnumComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeGenericListComponent_name: string = "Some string here";
	ParameterInputTypeGenericListComponent_value: string = "Some string here";
	ParameterInputTypeGenericListComponent_type: string = "Some string here";
	ParameterInputTypeGenericListComponent_listType: string = "Some string here";
	ParameterInputTypeGenericListComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeGuidComponent_value: string = "Some string here";
	ParameterInputTypeGuidComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeHttpPostedFileBaseComponent_value: string = "Some string here";
	ParameterInputTypeHttpPostedFileBaseComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeInt32Component_value: string = "Some string here";
	ParameterInputTypeInt32Component_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeInt64Component_value: string = "Some string here";
	ParameterInputTypeInt64Component_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeSingleComponent_value: string = "Some string here";
	ParameterInputTypeSingleComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeStringComponent_value: string = "Some string here";
	ParameterInputTypeStringComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeTimeSpanComponent_value: string = "Some string here";
	ParameterInputTypeTimeSpanComponent_config: HCBackendInputConfig = this.createBackendInputConfig();

    createBackendInputConfig(type: string = 'type'): HCBackendInputConfig {
        return {
            Type: type,
            Id: 'id',
            Name: 'Name',
            Description: 'Description',
            NotNull: false,
            Nullable: true,
            FullWidth: false,
            DefaultValue: null,
            NullName: '[null]',
            Flags: [],
            PossibleValues: ['A', 'B', 'C'],
            ParameterIndex: 0,
            ExtraValues: { },
            PropertyInfo: null
        };
    }
}
</script>

<style scoped lang="scss"></style>
