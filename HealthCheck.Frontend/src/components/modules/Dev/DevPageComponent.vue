<!-- src/components/modules/Documentation/DocumentationPageComponent.vue -->
<template>
	<div class="module-root">
		<!-- NAVIGATION DRAWER -->
		<Teleport to="#module-nav-menu"> Nav here </Teleport>

		<div class="content-root">
			<div>
				<h3 class="todo" @click="showLoader = !showLoader">loading-screen-component</h3>
				<loading-screen-component v-if="showLoader" @click="showLoader = !showLoader"></loading-screen-component>
				<hr />
			</div>

			<div>
				<h3 class="ok">editor-component</h3>
				<!-- EditorComponent -->
				<editor-component style="height:200px" :allowFullscreen="true"></editor-component>
				<hr />

				<h3 class="ok">data-table-component</h3>
				<!-- DataTableComponent -->
				<data-table-component
					:groups="DataTableComponent_groups"
					:headers="DataTableComponent_headers">
					<template v-slot:cell="{ value }">
						{{ value.value }}
					</template>
					<template v-slot:expandedItem="{ item }">
						Item: {{ item }}
					</template>
				</data-table-component>
				<hr />

				<h3 class="ok">flow-diagram-component</h3>
				<!-- FlowDiagramComponent -->
				<flow-diagram-component
					:title="FlowDiagramComponent_title"
					:steps="FlowDiagramComponent_steps"
				></flow-diagram-component>
				<hr />

				<h3 class="ok">progress-bar-component</h3>
				<!-- ProgressBarComponent -->
				<progress-bar-component
					:max="ProgressBarComponent_max"
					:success="ProgressBarComponent_success"
					:error="ProgressBarComponent_error"
				></progress-bar-component>
				<hr />

				<h3 class="ok">sequence-diagram-component</h3>
				<!-- SequenceDiagramComponent -->
				<sequence-diagram-component
					:steps="SequenceDiagramComponent_steps"
					ref="sandboxDiagram"
				></sequence-diagram-component>
				<textarea v-model="SequenceDiagramComponent_sandboxScript"></textarea>
				<hr />

				<h3 class="ok">badge-component</h3>
				<!-- BadgeComponent -->
				<badge-component>42</badge-component>
				<hr />

				<h3 class="ok">block-component</h3>
				<!-- BlockComponent -->
				<block-component class="mb-1">Block with only content</block-component>
				<block-component title="Title" class="mb-1">Block with title</block-component>
				<block-component title="Title" details="Details here" class="mb-1">Block with title and details</block-component>
				<block-component title="Title" details="Details here" buttonText="Button here" class="mb-1">Block with title, details and button</block-component>
				<block-component title="Title" details="Details here" buttonText="Button here" buttonIcon="person" class="mb-1">Block with title, details and button icon</block-component>
				<block-component title="Title" details="Details here" buttonText="Button here" buttonIcon="person" status="Success" class="mb-1">Block with title, details, button icon and status</block-component>
				<hr />

				<h3 class="ok" @click="DialogComponent_value = !DialogComponent_value">dialog-component</h3>
				<!-- DialogComponent -->
				<dialog-component v-model:value="DialogComponent_value">
					<template #header>Header here</template>
					<template #footer>Footer here<br /><btn-component @click="DialogComponent_value=false" color="secondary">Close</btn-component></template>
					<div>Contents here</div>
				</dialog-component>
				<code>{{ DialogComponent_value }}</code>
				<hr />

				<h3 class="ok">checkbox-component</h3>
				<!-- CheckboxComponent -->
				<checkbox-component v-model:value="CheckboxComponent_value" label="Checkbox"></checkbox-component>
				<checkbox-component v-model:value="CheckboxComponent_value" label="Checkbox disabled" disabled></checkbox-component>
				<checkbox-component v-model:value="CheckboxComponent_value" label="Checkbox indeterminate" indeterminate></checkbox-component>
				<checkbox-component v-model:value="CheckboxComponent_value" label="Checkbox disabled indeterminate" disabled indeterminate></checkbox-component>
				<code>{{ CheckboxComponent_value }}</code>
				<hr />

				<h3 class="ok">switch-component</h3>
				<!-- SwitchComponent -->
				<switch-component v-model:value="SwitchComponent_value" label="Switch"></switch-component>
				<switch-component v-model:value="SwitchComponent_value" label="Switch disabled" disabled></switch-component>
				<code>{{ SwitchComponent_value }}</code>
				<hr />

				<h3 class="ok">icon-component</h3>
				<!-- IconComponent -->
				<icon-component>person</icon-component>
				<hr />

				<h3 class="ok">paging-component</h3>
				<!-- PagingComponent -->
				<paging-component
					v-model:value="PagingComponent_value"
					:count="PagingComponent_count"
					pageSize="3"
				></paging-component>
				<paging-component
					v-model:value="PagingComponent_value"
					:count="PagingComponent_count"
					pageSize="3"
					:disabled="true"
				></paging-component>
				<code>{{ PagingComponent_value }}</code>
				<hr />

				<h3 class="ok">alert-component</h3>
				<!-- AlertComponent -->
				<alert-component v-model:value="AlertComponent_value" icon="person" type="error" class="mb-1">Some error here!</alert-component>
				<alert-component v-model:value="AlertComponent_value" icon="person" type="warning" class="mb-1">Some warning here!</alert-component>
				<alert-component v-model:value="AlertComponent_value" icon="person" type="info" class="mb-1">Some info here!</alert-component>
				<alert-component v-model:value="AlertComponent_value" icon="person" type="error" outline class="mb-1">Some error outline here!</alert-component>
				<alert-component v-model:value="AlertComponent_value" icon="person" type="warning" outline class="mb-1">Some warning outline here!</alert-component>
				<alert-component v-model:value="AlertComponent_value" icon="person" type="info" outline class="mb-1">Some info outline here!</alert-component>
				<alert-component v-model:value="AlertComponent_value" type="info" class="mb-1">Without icon</alert-component>
				<alert-component v-model:value="AlertComponent_value" type="error" outline class="mb-1">Without icon</alert-component>
				<hr />

				<h3 class="ok">btn-component</h3>
				<!-- BtnComponent -->
				<btn-component class="mr-1 mb-1">Button</btn-component>
				<btn-component class="mr-1" color="primary">Button primary</btn-component>
				<btn-component class="mr-1" color="secondary">Button secondary</btn-component>
				<btn-component class="mr-1" color="error">Button error</btn-component>
				<btn-component class="mr-1" color="warning">Button warning</btn-component>
				<btn-component class="mr-1" color="info">Button info</btn-component><br />
				<btn-component class="mr-1 mb-1" outline>Button outline</btn-component>
				<btn-component class="mr-1" outline color="primary">Button primary outline</btn-component>
				<btn-component class="mr-1" outline color="secondary">Button secondary outline</btn-component>
				<btn-component class="mr-1" outline color="error">Button error outline</btn-component>
				<btn-component class="mr-1" outline color="warning">Button warning outline</btn-component>
				<btn-component class="mr-1" outline color="info">Button info outline</btn-component><br />
				<btn-component class="mr-1 mb-1" flat>Button flat</btn-component>
				<btn-component class="mr-1" flat color="primary">Button primary flat</btn-component>
				<btn-component class="mr-1" flat color="secondary">Button secondary flat</btn-component>
				<btn-component class="mr-1" flat color="error">Button error flat</btn-component>
				<btn-component class="mr-1" flat color="warning">Button warning flat</btn-component>
				<btn-component class="mr-1" flat color="info">Button info flat</btn-component><br />
				<hr />
				
				<h3 class="ok">chip-component</h3>
				<!-- ChipComponent -->
				<chip-component class="mr-1">Chip</chip-component>
				<chip-component class="mr-1" outline>Chip outline</chip-component>
				<chip-component class="mr-1" color="primary" @click="test = 0">Chip primary</chip-component>
				<chip-component class="mr-1" outline color="primary" @click="test = 0">Chip outline primary</chip-component>
				<chip-component class="mr-1" color="error">Chip error</chip-component>
				<chip-component class="mr-1" outline color="error">Chip outline error</chip-component>
				<hr />

				<h3 class="ok">expansion-panel-component</h3>
				<!-- ExpansionPanelComponent -->
				<expansion-panel-component v-model:value="ExpansionPanelComponent_value">
                    <template #header>Header here</template>
                    <template #content>Content here</template>
				</expansion-panel-component>
				<code>{{ ExpansionPanelComponent_value }}</code>
				<hr />

				<h3 class="ok">progress-linear-component</h3>
				<!-- ProgressLinearComponent -->
				<progress-linear-component :value="progressValue - 25" class="mb-2"></progress-linear-component>
				<progress-linear-component :value="progressValue" color="warning" class="mb-2"></progress-linear-component>
				<progress-linear-component indeterminate height="4" class="mb-2"></progress-linear-component>
				<progress-linear-component indeterminate color="error" class="mb-2"></progress-linear-component>
				<hr />

				<h3 class="ok">progress-circular-component</h3>
				<!-- ProgressCircularComponent -->
				<progress-circular-component :value="progressValue - 25" class="mb-2"></progress-circular-component>
				<progress-circular-component :value="progressValue" size="140" color="warning" class="mb-2"></progress-circular-component>
				<progress-circular-component indeterminate size="120" width="20" class="mb-2"></progress-circular-component>
				<progress-circular-component indeterminate color="error" class="mb-2"></progress-circular-component>
				<progress-circular-component :value="progressValueCustom" size="100" color="success" class="mb-2"></progress-circular-component>
  				<input type="range" min="1" max="100" v-model="progressValueCustom">
				<hr />
				
				<h3 class="ok">fetch-status-progress-component</h3>
				<!-- FetchStatusProgressComponent -->
				<fetch-status-progress-component :status="FetchStatusProgressComponent_status_loading" />
				<fetch-status-progress-component :status="FetchStatusProgressComponent_status_error" />
				<hr />

				<h3 class="ok">input-header-component</h3>
				<!-- InputHeaderComponent -->
				<input-header-component name="Name only here"></input-header-component>
				<input-header-component name="Name and desc here" description="Description here"></input-header-component>
				<hr />

				<h3 class="ok">text-field-component</h3>
				<!-- TextFieldComponent -->
				<text-field-component v-model:value="TextFieldComponent_value" />
				<text-field-component v-model:value="TextFieldComponent_value" label="Label here" />
				<text-field-component v-model:value="TextFieldComponent_value" label="Label here" description="Description here." />
				<text-field-component v-model:value="TextFieldComponent_value" description="Description only." />
				<text-field-component v-model:value="TextFieldComponent_value" label="Loading" description="Description here." :loading="true" />
				<text-field-component v-model:value="TextFieldComponent_value" label="Disabled" disabled />
				<text-field-component v-model:value="TextFieldComponent_value" label="Append icon" appendIcon="person" />
				<text-field-component v-model:value="TextFieldComponent_value" label="Prepend icon" prependIcon="person" />
				<text-field-component v-model:value="TextFieldComponent_value" label="Append + prepend icon" appendIcon="person" prependIcon="person" />
				<text-field-component v-model:value="TextFieldComponent_value" label="Append + prepend icon + clearable + loading" clearable appendIcon="person" appendIconTooltip="Some tooltip here" prependIcon="person" loading />
				<text-field-component v-model:value="TextFieldComponent_value" label="Append + prepend icon w listeners + clearable + loading" clearable appendIcon="person" appendIconTooltip="Some tooltip here" prependIcon="person" loading @click:append="notify('append')" @click:prepend="notify('prepend')" />
				<text-field-component v-model:value="TextFieldComponent_value" label="Append + prepend icon + clearable + loading + disabled" clearable appendIcon="person" prependIcon="person" loading disabled />
				<text-field-component v-model:value="TextFieldComponent_value" label="Append + prepend icon w listeners + clearable + loading" clearable appendIcon="person" appendIconTooltip="Some tooltip here" prependIcon="person" loading @click:append="notify('append')" @click:prepend="notify('prepend')" errorMessages="Some error here." />
				<code>{{ TextFieldComponent_value }}</code>
				<hr />

				<h3 class="ok">simple-date-time-component</h3>
				<!-- SimpleDateTimeComponent -->
				<simple-date-time-component
					v-model:value="SimpleDateTimeComponent_value"
				></simple-date-time-component>
				<code>{{ SimpleDateTimeComponent_value }}</code>
				<hr />
				
				<h3 class="ok">textarea-component</h3>
				<!-- TextareaComponent -->
				<textarea-component v-model:value="TextareaComponent_value" autoGrow />
				<textarea-component v-model:value="TextareaComponent_value" label="Label here" rows="5" />
				<textarea-component v-model:value="TextareaComponent_value" label="Label here" description="Description here." />
				<textarea-component v-model:value="TextareaComponent_value" description="Description only." />
				<textarea-component v-model:value="TextareaComponent_value" label="Loading" description="Description here." :loading="true" />
				<textarea-component v-model:value="TextareaComponent_value" label="Disabled" disabled />
				<textarea-component v-model:value="TextareaComponent_value" label="Append icon" appendIcon="person" />
				<textarea-component v-model:value="TextareaComponent_value" label="Prepend icon" prependIcon="person" />
				<textarea-component v-model:value="TextareaComponent_value" label="Append + prepend icon" appendIcon="person" prependIcon="person" />
				<textarea-component v-model:value="TextareaComponent_value" label="Append + prepend icon + clearable + loading" clearable appendIcon="person" appendIconTooltip="Some tooltip here" prependIcon="person" loading />
				<textarea-component v-model:value="TextareaComponent_value" label="Append + prepend icon w listeners + clearable + loading" clearable appendIcon="person" appendIconTooltip="Some tooltip here" prependIcon="person" loading @click:append="notify('append')" @click:prepend="notify('prepend')" />
				<textarea-component v-model:value="TextareaComponent_value" label="Append + prepend icon + clearable + loading + disabled" clearable appendIcon="person" prependIcon="person" loading disabled />
				<textarea-component v-model:value="TextareaComponent_value" label="Append + prepend icon w listeners + clearable + loading" clearable appendIcon="person" appendIconTooltip="Some tooltip here" prependIcon="person" loading @click:append="notify('append')" @click:prepend="notify('prepend')" errorMessages="Some error here."
					rows="5" autoGrow />
				<code>{{ TextareaComponent_value }}</code>
				<hr />

				<h3 class="ok">input-component</h3>
				<!-- InputComponent -->
				<input-component v-model:value="InputComponent_value"></input-component>
				<input-component v-model:value="InputComponent_value" actionIcon="person" @actionIconClicked="notify('actionIconClicked')"></input-component>
				<code>{{ InputComponent_value }}</code>
				<hr />
				
				<h3 class="ok">select-component</h3>
				<!-- SelectComponent -->
				<select-component v-model:value="SelectComponent_value" :items="SelectComponent_items" label="Array" />
				<select-component v-model:value="SelectComponent_value" :items="SelectComponent_items" label="Disabled" disabled />
				<select-component v-model:value="SelectComponent_value" :items="SelectComponent_items" label="Loading" loading />
				<select-component v-model:value="SelectComponent_value" :items="SelectComponent_items" label="Nullable" nullable />
				<select-component v-model:value="SelectComponent_value" :items="SelectComponent_items" label="Loading + disabled" disabled loading />
				<select-component v-model:value="SelectComponent_valueMultiple" :items="SelectComponent_itemsMultiple" multiple label="Complex" description="Not just a simple array + multiple." />
				<select-component v-model:value="SelectComponent_valueMultiple" :items="SelectComponent_itemsMultiple" multiple label="Complex loading" description="Not just a simple array + multiple." loading />
				<select-component v-model:value="SelectComponent_valueMultiple" :items="SelectComponent_itemsMultiple" multiple label="Complex + loading + disabled" description="Not just a simple array + multiple." disabled loading placeholder="[null]" />
				<select-component v-model:value="SelectComponent_value" :items="SelectComponent_items" label="With error" error="Some error here" />
				
				<select-component v-model:value="SelectComponent_value" :items="SelectComponent_items" label="Array input" allowInput />
				<select-component v-model:value="SelectComponent_valueMultiple" :items="SelectComponent_itemsMultiple" multiple label="Complex input" allowInput />
				<select-component v-model:value="SelectComponent_value" :items="SelectComponent_items" label="Array input custom" allowInput allowCustom />
				<select-component v-model:value="SelectComponent_valueMultiple" :items="SelectComponent_itemsMultiple" multiple label="Complex input custom" allowInput allowCustom />
				<select-component v-model:value="SelectComponent_valueMultiple" :items="SelectComponent_itemsMultiple" multiple label="Complex input custom, clearable" allowInput allowCustom clearable />
				
				<select-component v-model:value="SelectComponent_value" :items="SelectComponent_itemsEmpty" label="Custom, no items" no-data-text="Value required" placeholder="Value to search for" allowInput allowCustom />
				<select-component v-model:value="SelectComponent_value" :items="SelectComponent_itemsMultiple" label="Custom, with items" no-data-text="Value required" placeholder="Value to search for" allowInput allowCustom />

				Value: <code>{{ SelectComponent_value }}</code><br />
				Value multiple: <code>{{ SelectComponent_valueMultiple }}</code>
				<hr />

				<h3 class="ok">timespan-input-component</h3>
				<!-- TimespanInputComponent -->
				<timespan-input-component
					v-model:value="TimespanInputComponent_value"
				></timespan-input-component>
				<code>{{ TimespanInputComponent_value }}</code>
				<hr />

				<h3 class="ok">bar-chart-component</h3>
				<!-- BarChartComponent -->
				<bar-chart-component
					title="Some title here"
					ylabel="Y-Label here"
					:sets="BarChartComponent_sets"
					:bars="BarChartComponent_bars"
				></bar-chart-component>
				<hr />

				<h3 class="ok">data-over-time-chart-component</h3>
				<!-- DataOverTimeChartComponent -->
				<data-over-time-chart-component
					:entries="DataOverTimeChartComponent_entries"
					:sets="DataOverTimeChartComponent_sets"
					title="[count] total requests over the last [timespan]"
					ylabel="Requests"
				></data-over-time-chart-component>
				<hr />

				<h3 class="ok">tooltip-component</h3>
				<!-- TooltipComponent -->
				<tooltip-component tooltip="Tooltip here!">Some text here</tooltip-component>
				<hr />

				<h3 class="ok">filterable-list-component</h3>
				<!-- FilterableListComponent -->
				<filterable-list-component class="dev_border"
					:items="FilterableListComponent_items"
                    :groupByKey="`GroupName`"
                    :sortByKey="`GroupName`"
                    :hrefKey="`Href`"
                    :filterKeys="[ 'Name', 'Description' ]"
                    :disabled="false"
                    :showFilter="true"
                    :groupIfSingleGroup="false"
                    />
				<filterable-list-component class="dev_border"
					:items="FilterableListComponent_items"
                    :groupByKey="`GroupName`"
                    :sortByKey="`GroupName`"
                    :hrefKey="`Href`"
                    :filterKeys="[ 'Name', 'Description' ]"
                    :disabled="true"
                    :showFilter="false"
                    :groupIfSingleGroup="false"
                    />
				<filterable-list-component class="dev_border"
					:items="FilterableListComponent_items"
                    :groupByKey="`GroupName`"
                    :sortByKey="`GroupName`"
                    :hrefKey="`Href`"
                    :filterKeys="[ 'Name', 'Description' ]"
                    :loading="true"
                    :disabled="true"
                    :showFilter="false"
                    :groupIfSingleGroup="false"
                    />
				<hr />

				<h3 class="ok">filter-input-component</h3>
				<!-- FilterInputComponent -->
				<filter-input-component
					v-model:value="FilterInputComponent_value"
				></filter-input-component>
				<code>{{ FilterInputComponent_value }}</code>
				<hr />

				<h3 class="ok" @click="showFloatingSquares = !showFloatingSquares">floating-squares-effect-component</h3>
				<!-- FloatingSquaresEffectComponent -->
				<floating-squares-effect-component v-if="showFloatingSquares"></floating-squares-effect-component>
				<hr />

				<h3 class="ok">carousel-component</h3>
				<!-- CarouselComponent -->
				<carousel-component :items="CarouselComponent_items" />
				<hr />

				<h3 class="ok" @click="SnackbarComponent_value = true">snackbar-component</h3>
				<!-- SnackbarComponent -->
				<snackbar-component v-model:value="SnackbarComponent_value" :timeout="2500" color="info">
					Some text here!
					<btn-component flat @click="SnackbarComponent_value = false">Close</btn-component>
				</snackbar-component>
				<code>{{ SnackbarComponent_value }}</code>
				<hr />

				<h3 class="ok">toolbar-component</h3>
				<!-- ToolbarComponent -->
				<toolbar-component>
					<btn-component>Some button</btn-component>
					<btn-component>Another button</btn-component>
				</toolbar-component>
				<toolbar-component dark>
					<btn-component>Some button</btn-component>
					<btn-component>Another button</btn-component>
				</toolbar-component>
				<hr />


				<h3 class="ok">backend-input-component</h3>
				<!-- BackendInputComponent -->
				<backend-input-component
					:config="BackendInputComponent_config"
					v-model:value="BackendInputComponent_value"
				></backend-input-component>
				<code>{{ BackendInputComponent_value }}</code>
				<hr />

				<h3 class="ok">unknown-backend-input-component</h3>
				<!-- UnknownBackendInputComponent -->
				<unknown-backend-input-component
					v-model:value="UnknownBackendInputComponent_value"
					:type="UnknownBackendInputComponent_type"
				></unknown-backend-input-component>
				<code>{{ UnknownBackendInputComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-any-json-component</h3>
				<!-- ParameterInputAnyJsonComponent -->
				<parameter-input-any-json-component
					:name="ParameterInputAnyJsonComponent_name"
					v-model:value="ParameterInputAnyJsonComponent_value"
					:type="ParameterInputAnyJsonComponent_type"
					:config="ParameterInputAnyJsonComponent_config"
				></parameter-input-any-json-component>
				<code>{{ ParameterInputAnyJsonComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-boolean-component</h3>
				<!-- ParameterInputTypeBooleanComponent -->
				<parameter-input-type-boolean-component
					v-model:value="ParameterInputTypeBooleanComponent_value"
					:config="ParameterInputTypeBooleanComponent_config"
				></parameter-input-type-boolean-component>
				<code>{{ ParameterInputTypeBooleanComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-date-time-component</h3>
				<!-- ParameterInputTypeDateTimeComponent -->
				<parameter-input-type-date-time-component
					v-model:value="ParameterInputTypeDateTimeComponent_value"
					:config="ParameterInputTypeDateTimeComponent_config"
				></parameter-input-type-date-time-component>
				<code>{{ ParameterInputTypeDateTimeComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-date-time-offset-component</h3>
				<!-- ParameterInputTypeDateTimeOffsetComponent -->
				<parameter-input-type-date-time-offset-component
					v-model:value="ParameterInputTypeDateTimeOffsetComponent_value"
					:config="ParameterInputTypeDateTimeOffsetComponent_config"
				></parameter-input-type-date-time-offset-component>
				<code>{{ ParameterInputTypeDateTimeOffsetComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-decimal-component</h3>
				<!-- ParameterInputTypeDecimalComponent -->
				<parameter-input-type-decimal-component
					v-model:value="ParameterInputTypeDecimalComponent_value"
					:config="ParameterInputTypeDecimalComponent_config"
				></parameter-input-type-decimal-component>
				<code>{{ ParameterInputTypeDecimalComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-double-component</h3>
				<!-- ParameterInputTypeDoubleComponent -->
				<parameter-input-type-double-component
					v-model:value="ParameterInputTypeDoubleComponent_value"
					:config="ParameterInputTypeDoubleComponent_config"
				></parameter-input-type-double-component>
				<code>{{ ParameterInputTypeDoubleComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-enum-component</h3>
				<!-- ParameterInputTypeEnumComponent -->
				<parameter-input-type-enum-component
					v-model:value="ParameterInputTypeEnumComponent_value"
					:config="ParameterInputTypeEnumComponent_config"
				></parameter-input-type-enum-component>
				<code>{{ ParameterInputTypeEnumComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-flagged-enum-component</h3>
				<!-- ParameterInputTypeFlaggedEnumComponent -->
				<parameter-input-type-flagged-enum-component
					v-model:value="ParameterInputTypeFlaggedEnumComponent_value"
					:config="ParameterInputTypeFlaggedEnumComponent_config"
				></parameter-input-type-flagged-enum-component>
				<code>{{ ParameterInputTypeFlaggedEnumComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-generic-list-component</h3>
				<!-- ParameterInputTypeGenericListComponent -->
				<parameter-input-type-generic-list-component
					:name="ParameterInputTypeGenericListComponent_name"
					v-model:value="ParameterInputTypeGenericListComponent_value"
					:type="ParameterInputTypeGenericListComponent_type"
					:listType="ParameterInputTypeGenericListComponent_listType"
					:config="ParameterInputTypeGenericListComponent_config"
				></parameter-input-type-generic-list-component>
				<code>{{ ParameterInputTypeGenericListComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-guid-component</h3>
				<!-- ParameterInputTypeGuidComponent -->
				<parameter-input-type-guid-component
					v-model:value="ParameterInputTypeGuidComponent_value"
					:config="ParameterInputTypeGuidComponent_config"
				></parameter-input-type-guid-component>
				<code>{{ ParameterInputTypeGuidComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-http-posted-file-base-component</h3>
				<!-- ParameterInputTypeHttpPostedFileBaseComponent -->
				<parameter-input-type-http-posted-file-base-component
					v-model:value="ParameterInputTypeHttpPostedFileBaseComponent_value"
					:config="ParameterInputTypeHttpPostedFileBaseComponent_config"
				></parameter-input-type-http-posted-file-base-component>
				<code>{{ ParameterInputTypeHttpPostedFileBaseComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-int32-component</h3>
				<!-- ParameterInputTypeInt32Component -->
				<parameter-input-type-int32-component
					v-model:value="ParameterInputTypeInt32Component_value"
					:config="ParameterInputTypeInt32Component_config"
				></parameter-input-type-int32-component>
				<code>{{ ParameterInputTypeInt32Component_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-int64-component</h3>
				<!-- ParameterInputTypeInt64Component -->
				<parameter-input-type-int64-component
					v-model:value="ParameterInputTypeInt64Component_value"
					:config="ParameterInputTypeInt64Component_config"
				></parameter-input-type-int64-component>
				<code>{{ ParameterInputTypeInt64Component_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-single-component</h3>
				<!-- ParameterInputTypeSingleComponent -->
				<parameter-input-type-single-component
					v-model:value="ParameterInputTypeSingleComponent_value"
					:config="ParameterInputTypeSingleComponent_config"
				></parameter-input-type-single-component>
				<code>{{ ParameterInputTypeSingleComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-string-component</h3>
				<!-- ParameterInputTypeStringComponent -->
				<parameter-input-type-string-component
					v-model:value="ParameterInputTypeStringComponent_value"
					:config="ParameterInputTypeStringComponent_config"
				></parameter-input-type-string-component>
				<code>{{ ParameterInputTypeStringComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-type-time-span-component</h3>
				<!-- ParameterInputTypeTimeSpanComponent -->
				<parameter-input-type-time-span-component
					v-model:value="ParameterInputTypeTimeSpanComponent_value"
					:config="ParameterInputTypeTimeSpanComponent_config"
				></parameter-input-type-time-span-component>
				<code>{{ ParameterInputTypeTimeSpanComponent_value }}</code>
				<hr />

				<h3 class="ok">parameter-input-pick-reference-component</h3>
				<!-- ParameterInputPickReferenceComponent -->
				<parameter-input-pick-reference-component
					v-model:value="ParameterInputPickReferenceComponent_value"
					:config="ParameterInputPickReferenceComponent_config"
				></parameter-input-pick-reference-component>
				<code>{{ ParameterInputPickReferenceComponent_value }}</code>
				<hr />

				<h3 class="ok">date-picker-component</h3>
				<!-- DatePickerComponent -->
				<date-picker-component
					v-model:value="DatePickerComponent_value"
					:allowed-dates="DatePickerComponent_allowDate"
					rangePresets="past"
				></date-picker-component>
				<code>{{ DatePickerComponent_value }}</code>
				<hr />

				<h3 class="ok">calendar-component</h3>
				<!-- CalendarComponent -->
				<calendar-component :events="CalendarComponent_value"></calendar-component>
				<hr />

				<h3 class="ok">stepper-component</h3>
				<!-- StepperComponent -->
				<stepper-component :steps="Stepper_Steps"></stepper-component>
				<hr />

				<!-- qwe -->
			</div>
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
import BadgeComponent from "@components/Common/Basic/BadgeComponent.vue";
import BlockComponent from "@components/Common/Basic/BlockComponent.vue";
import BtnComponent from "@components/Common/Basic/BtnComponent.vue";
import CalendarComponent from "@components/Common/Basic/CalendarComponent.vue";
import CarouselComponent from "@components/Common/Basic/CarouselComponent.vue";
import { CarouselItem } from "@components/Common/Basic/CarouselComponent.vue.models";
import CheckboxComponent from "@components/Common/Basic/CheckboxComponent.vue";
import ChipComponent from "@components/Common/Basic/ChipComponent.vue";
import DatePickerComponent from "@components/Common/Basic/DatePickerComponent.vue";
import DialogComponent from "@components/Common/Basic/DialogComponent.vue";
import ExpansionPanelComponent from "@components/Common/Basic/ExpansionPanelComponent.vue";
import FetchStatusProgressComponent from "@components/Common/Basic/FetchStatusProgressComponent.vue";
import IconComponent from "@components/Common/Basic/IconComponent.vue";
import InputComponent from "@components/Common/Basic/InputComponent.vue";
import InputHeaderComponent from "@components/Common/Basic/InputHeaderComponent.vue";
import PagingComponent from "@components/Common/Basic/PagingComponent.vue";
import ProgressCircularComponent from "@components/Common/Basic/ProgressCircularComponent.vue";
import ProgressLinearComponent from "@components/Common/Basic/ProgressLinearComponent.vue";
import SelectComponent from "@components/Common/Basic/SelectComponent.vue";
import SnackbarComponent from "@components/Common/Basic/SnackbarComponent.vue";
import StepperComponent from "@components/Common/Basic/StepperComponent.vue";
import SwitchComponent from "@components/Common/Basic/SwitchComponent.vue";
import TextareaComponent from "@components/Common/Basic/TextareaComponent.vue";
import TextFieldComponent from "@components/Common/Basic/TextFieldComponent.vue";
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
import { DataTableGroup } from "@components/Common/DataTableComponent.vue.models";
import { FlowDiagramStep, FlowDiagramStepType } from "@components/Common/FlowDiagramComponent.vue.models";
import { SequenceDiagramStep } from "@components/Common/SequenceDiagramComponent.vue.models";
import { FilterableListItem } from "@components/Common/FilterableListComponent.vue.models";
import { BarChartSet } from "@components/Common/Charts/BarChartComponent.vue.models";
import { StepperComponentStepViewModel } from "@components/Common/Basic/StepperComponent.vue.models";

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
		BadgeComponent,
		BlockComponent,
		BtnComponent,
		CalendarComponent,
		CarouselComponent,
		CheckboxComponent,
		ChipComponent,
		DatePickerComponent,
		DialogComponent,
		ExpansionPanelComponent,
		FetchStatusProgressComponent,
		IconComponent,
		InputComponent,
		InputHeaderComponent,
		PagingComponent,
		ProgressCircularComponent,
		ProgressLinearComponent,
		SelectComponent,
		SnackbarComponent,
		StepperComponent,
		SwitchComponent,
		TextareaComponent,
		TextFieldComponent,
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
	DataTableComponent_groups: Array<DataTableGroup>= [
		{ title: 'Id', items: [
			{ values: [ { key: 'Id', value: '1', uiHint: 'Raw' }, { key: 'Name', value: 'Name #1', uiHint: 'Raw' }, { key: 'Something', value: 'asdasd1', uiHint: 'Raw' } ], expandedValues: [{ key: 'ExpandedId', value: 'ExpandedValue1', uiHint: 'Raw' }] },
			{ values: [ { key: 'Id', value: '2', uiHint: 'Raw' }, { key: 'Name', value: 'Name #2', uiHint: 'Raw' }, { key: 'Something', value: 'asdasd2', uiHint: 'Raw' } ], expandedValues: [{ key: 'ExpandedId', value: 'ExpandedValue2', uiHint: 'Raw' }] }
		] },
	];
	DataTableComponent_headers: Array<string> = [
		"Id",
		"Name",
		"Something",
	];
    get FilterableListComponent_items(): Array<FilterableListItem>
    {
        return [1,2,3,4,5].map(x => {
            let d = {
                title: `Item #${x}`,
                subtitle: 'Some subtitle',
                data: {
					GroupName: `Group ${(x % 2)}`,
					Href: `/href-${x}`,
					Name: `Item #${x}`,
					Description: 'Some desc',
					Icon: (x % 2 == 0) ? 'cloud' : ''
				}
            };
            return d;
        });
    }
	FilterInputComponent_value: string = "Some string here 1";
	FlowDiagramComponent_title: string = "Some string here 2";
	showLoader: boolean = false;
	showFloatingSquares: boolean = false;
	FlowDiagramComponent_steps: Array<FlowDiagramStep<any>>= [
		{ title: 'Start', type: FlowDiagramStepType.Start, data: null, connections: [{ target: 'Step #2', label: 'Go!',  }] },
		{ title: 'Step #2', type: FlowDiagramStepType.If, data: null, connections: [{ target: 'Step #3a', label: 'Yes',  }, { target: 'Step #3b', label: 'No',  }] },
		{ title: 'Step #3a', type: FlowDiagramStepType.Element, data: null, connections: [] },
		{ title: 'Step #3b', type: FlowDiagramStepType.Element, data: null, connections: [{ target: 'End', label: 'Good!',  }] },
		{ title: 'End', type: FlowDiagramStepType.End, data: null, connections: [] }
	];
	ProgressBarComponent_max: number = 200;
	ProgressBarComponent_success: number = 150;
	ProgressBarComponent_error: number = 25;
	SimpleDateTimeComponent_value: Date | null = new Date();
	SequenceDiagramComponent_sandboxScript: string = `
Frontend --> Web: User sends form
Web -> Web: Validate input
opt Invoice only
Web -> External Service: Data is sent to 3rd party | remark: Some remark here
Web -> Database: Backup of data is stored in database
end
Web -> Frontend: Confirmation is delivered
`;
	AlertComponent_value: boolean = true;
	CalendarComponent_value: Array<any> = [];
	CarouselComponent_items: Array<CarouselItem> = [
		{ url: "https://via.placeholder.com/2500x300", detailsHtml: "Item A <a href=\"asd\">some link</a>" },
		{ url: "https://via.placeholder.com/1000x2500", detailsHtml: "Item X <a href=\"asd\">some link</a>" },
		{ url: "https://via.placeholder.com/500x2000", detailsHtml: "Item B" },
		{ url: "https://via.placeholder.com/1234", detailsHtml: "Item C" },
		{ url: "https://via.placeholder.com/55", detailsHtml: "Item D" }
	];
	CheckboxComponent_value: boolean = true;
	DatePickerComponent_value: Date = new Date();
    DatePickerComponent_allowDate = (date: Date) => {
		return date.toString().includes('2');
    }
	DialogComponent_value: boolean = false;
	ExpansionPanelComponent_value: boolean = false;
	FetchStatusProgressComponent_status_loading: FetchStatusWithProgress | null = { inProgress: true, progress: 50, failed: false, errorMessage: null };
	FetchStatusProgressComponent_status_error: FetchStatusWithProgress | null = { inProgress: false, progress: 0, failed: true, errorMessage: 'Oh no! Something failed!' };
	InputComponent_value: string = "Some string here 4";
	PaginationComponent_value: number = 2;
	PagingComponent_value: number = 2;
	PagingComponent_count: number = 88;
	SelectComponent_value: string = "Simple B";
	SelectComponent_items: Array<string> = ['Simple A', 'Simple B', 'Simple C', 'Another X', 'Another Y', 'Another Z'];
	SelectComponent_itemsEmpty: Array<string> = [];
	SelectComponent_valueMultiple: Array<string> = ['s88', 'a67', 's2'];
	SelectComponent_itemsMultiple: Array<any> = [];
	SnackbarComponent_value: boolean = false;
	SwitchComponent_value: boolean = true;
	TextareaComponent_value: string = "Some string here 5";
	TextFieldComponent_value: string = "Some string here 6";
	TimespanInputComponent_value: string = "33:22:11";
	BarChartComponent_sets: Array<BarChartSet> = [
		{ label: 'Set 1', group: 'group1', color: '#4cff50' },
		{ label: 'Set 2', group: 'group2', color: '#FF0000' },
		{ label: 'Set 3', group: 'group3', color: '#0000FF' }
	];
	BarChartComponent_bars: Array<any>= [
        { label: 'Bar A', values: [100, 200, 300] },
        { label: 'Bar B', values: [50, 25, 12] },
        { label: 'Bar C', values: [200, 400, 600] }
	];
	DataOverTimeChartComponent_sets: Array<any>= [
        { label: 'Allowed', group: 'allowed', color: '#4cff50' },
        { label: 'Blocked', group: 'blocked', color: '#FF0000' }
    ];
	DataOverTimeChartComponent_entries: Array<any>= [
		{ date: new Date(1649435434932), group: 'blocked' },
		{ date: new Date(1649435534932), group: 'allowed' },
		{ date: new Date(1649435534932), group: 'blocked' },
		{ date: new Date(1649435634932), group: 'allowed' },
		{ date: new Date(1649435734932), group: 'blocked' },
		{ date: new Date(1649435834932), group: 'allowed' }
	];
	BackendInputComponent_config: HCBackendInputConfig = this.createBackendInputConfig('Int32');
	BackendInputComponent_value: string = "1234";
	UnknownBackendInputComponent_value: string = "Some string here 8";
	UnknownBackendInputComponent_type: string = "SomeType";
	ParameterInputAnyJsonComponent_name: string = "Any JSON name";
	ParameterInputAnyJsonComponent_value: string = "{ \"something\": true }";
	ParameterInputAnyJsonComponent_type: string = "Some string here 9";
	ParameterInputAnyJsonComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputPickReferenceComponent_value: string = "Some string here 10";
	ParameterInputPickReferenceComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeBooleanComponent_value: boolean = true;
	ParameterInputTypeBooleanComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeDateTimeComponent_value: string = "2021-08-14T22:22:00";
	ParameterInputTypeDateTimeComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeDateTimeOffsetComponent_value: string = "2021-08-14T18:18:18";
	ParameterInputTypeDateTimeOffsetComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeDecimalComponent_value: string = "12.34";
	ParameterInputTypeDecimalComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeDoubleComponent_value: string = "23.45";
	ParameterInputTypeDoubleComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeEnumComponent_value: string = "Some string here 15";
	ParameterInputTypeEnumComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeFlaggedEnumComponent_value: string = "Some string here 16";
	ParameterInputTypeFlaggedEnumComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeGenericListComponent_name: string = "Some string here 17";
	ParameterInputTypeGenericListComponent_value: string = "Some string here 18";
	ParameterInputTypeGenericListComponent_type: string = "Some string here 19";
	ParameterInputTypeGenericListComponent_listType: string = "Int32";
	ParameterInputTypeGenericListComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeGuidComponent_value: string = "Some string here 21";
	ParameterInputTypeGuidComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeHttpPostedFileBaseComponent_value: string = "Some string here 22";
	ParameterInputTypeHttpPostedFileBaseComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeInt32Component_value: string = "3456";
	ParameterInputTypeInt32Component_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeInt64Component_value: string = "45678901231234";
	ParameterInputTypeInt64Component_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeSingleComponent_value: string = "56.78";
	ParameterInputTypeSingleComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeStringComponent_value: string = "Some string here 26";
	ParameterInputTypeStringComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	ParameterInputTypeTimeSpanComponent_value: string = "Some string here 27";
	ParameterInputTypeTimeSpanComponent_config: HCBackendInputConfig = this.createBackendInputConfig();
	progressValue: number = 66;
	progressValueCustom: number = 75;
    hackyTimer: number = 0;
    Stepper_Steps: Array<StepperComponentStepViewModel> = [
			{
				Index: 0,
				Title: 'First step',
				Description: 'Some description',
				Error: null,
				Links: [],
				Icon: 'check',
				Timestamp: new Date(),
				HideTimeInTimestamp: true,
				IsCompleted: true
			},
			{
				Index: 1,
				Title: 'Second step',
				Description: 'Some description',
				Error: null,
				Links: [],
				Icon: '',
				Timestamp: new Date(),
				HideTimeInTimestamp: true,
				IsCompleted: false
			}
		];
    

	mounted(): void {
        setInterval(() => {
			this.hackyTimer++;
			this.progressValue += Math.random() * 25;
			if (this.progressValue > 100) this.progressValue = 0;
		}, 1000);

		let counter = 0;
		for (let i=0;i<1000;i++)
		{
			this.SelectComponent_itemsMultiple.push({ value: `s${counter}`, text: `Something #${counter}` });
			counter++;
			this.SelectComponent_itemsMultiple.push({ value: `a${counter}`, text: `Another #${counter}` });
			counter++;
		}
	}

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

    convertStringToSteps(text: string): Array<SequenceDiagramStep<any>>
    {
        var d = this.$refs.sandboxDiagram as SequenceDiagramComponent;
		if (d == null || this.hackyTimer == 0) return [];
        return d.convertStringToSteps<any>(text);
    }

    get SequenceDiagramComponent_steps(): Array<SequenceDiagramStep<any>>
    {
        return this.convertStringToSteps(this.SequenceDiagramComponent_sandboxScript);
    }

	notify(msg: string): void { alert(msg); }
}
</script>

<style scoped lang="scss">
.ok { color: rgb(54, 105, 54); }
.todo {
    color: rgb(122, 43, 43);
    font-weight: bold;
    &::before {
        content: '// ToDo: ';
        display: inline-block;
    }
}
.dev_border {
	border: 2px solid rgb(122, 155, 192);
	margin: 5px 0;
}
</style>
