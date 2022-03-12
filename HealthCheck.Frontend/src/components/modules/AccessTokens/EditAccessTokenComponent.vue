<!-- src/components/modules/AccessTokens/EditAccessTokenComponent.vue -->
<template>
    <div>
        <v-text-field type="text"
            label="Name"
            v-model="data.name"
            :disabled="readonly"
        ></v-text-field>

        <simple-date-time-component
            v-model="data.ExpiresAt"
            :readonly="readonly"
            name="Expiration date"
            />

        <input-header-component name="Allow killswitch" description="If killswitch is allowed, the user of the token can choose to delete it at any time." />
        <v-checkbox 
            class="mt-2"
            :label="(data.AllowKillswitch ? 'Allowed' : 'Not allowed')"
            v-model="data.AllowKillswitch"
            :disabled="readonly" />

        <h3>Roles</h3>
        <p class="mb-0">Give token access to the following roles:</p>
        <v-layout row wrap>
            <div class="mr-2"
                v-for="(role, rindex) in accessData.Roles"
                :key="`access-role-${rindex}`">
                <v-checkbox 
                    class="mt-2"
                    :label="role.Name"
                    :input-value="roleIsEnabled(role.Id)"
                    :disabled="readonly"
                    @change="(v) => onRoleToggled(role.Id, v)" />
            </div>
        </v-layout>

        <h3>Modules</h3>
        <p class="mb-0">Give token access to the following modules:</p>

        <v-chip
            color="primary"
            v-for="(filterChoice, fcIndex) in moduleFilters"
            :key="`filter-choice-${fcIndex}`"
            :outline="!filterChoice.selected"
            :disabled="readonly"
            class="filter-choice"
            :class="{ 'selected': filterChoice.selected }"
            @click="onModuleAccessToggled(filterChoice.moduleId, !filterChoice.selected)">
                {{ filterChoice.label }}
                <v-icon right v-if="!filterChoice.selected">add</v-icon>
                <v-icon right v-if="filterChoice.selected">close</v-icon>
            </v-chip>

        <div class="access-grid ml-4 mt-2">
            <div class="access-grid--row"
                v-for="(module, mindex) in filteredModuleOptions"
                :key="`access-module-${mindex}`">
                
                <div class="access-grid--row--header">
                    {{ module.ModuleName }}
                </div>

                <div v-if="module.AccessOptions.length == 0 && module.AccessCategories.length == 0"
                    style="margin-left: 20px;">No specific access options available for this module.</div>
                
                <div class="access-grid--row--options"
                    v-if="hasAccessToModule(module.ModuleId)">
                    <div v-if="module.AccessOptions.length > 0"
                        class="access-grid--row--options--header">Access options:</div>
                    <div class="access-grid--row--options--item"
                        v-for="(option, moindex) in module.AccessOptions"
                        :key="`access-module-${mindex}-option-${moindex}`">
                        <v-checkbox hide-details
                            :label="option.Name"
                            :disabled="readonly"
                            :input-value="moduleOptionIsEnabled(module.ModuleId, option.Id)"
                            @change="(v) => onModuleAccessOptionToggled(module.ModuleId, option.Id, v)" />
                    </div>
                </div>

                <div class="access-grid--row--options"
                    v-if="hasAccessToModule(module.ModuleId)">
                    <div v-if="module.AccessCategories.length > 0"
                        class="access-grid--row--options--header">Limit to categories:</div>
                    <div class="access-grid--row--cat--item"
                        v-for="(cat, moindex) in module.AccessCategories"
                        :key="`access-module-${mindex}-cat-${moindex}`">
                        <v-checkbox hide-details
                            :label="cat.Name"
                            :disabled="readonly"
                            :input-value="moduleCategoryIsEnabled(module.ModuleId, cat.Id)"
                            @change="(v) => onModuleAccessCategoryToggled(module.ModuleId, cat.Id, v)" />
                    </div>
                </div>

                <div class="access-grid--row--options"
                    v-if="hasAccessToModule(module.ModuleId) && module.AccessIds.length > 0">
                    <div class="access-grid--row--options--header">Limit to:</div>
                    <v-autocomplete
                        :items="module.AccessIds"
                        item-value="Id"
                        item-text="Name"
                        multiple
                        chips
                        clearable
                        class="filter-input"
                        :readonly="readonly"
                        :input="getModuleAccessIds(module.ModuleId)"
                        @change="(v) => onModuleAccessIdChanged(module.ModuleId, v)"
                        ></v-autocomplete>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { AccessData, CreatedAccessData, ModuleAccessData } from '@services/AccessTokensService';
import SimpleDateTimeComponent from '@components/Common/SimpleDateTimeComponent.vue';
import InputHeaderComponent from '@components/Common/Basic/InputHeaderComponent.vue';

@Options({
    components: {
        SimpleDateTimeComponent,
        InputHeaderComponent
    }
})
export default class EditAccessTokenComponent extends Vue {
    @Prop({ required: true })
    accessData!: AccessData;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    @Prop({ required: false, default: {
        roles: [],
        modules: []
    }})
    value!: CreatedAccessData;

    data: CreatedAccessData = {
        Name: 'New Token',
        Roles: [],
        Modules: [],
        ExpiresAt: null,
        AllowKillswitch: true
    };

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get filteredModuleOptions(): Array<ModuleAccessData> {
        return this.accessData.ModuleOptions.filter(x => this.hasAccessToModule(x.ModuleId));
    }

    get moduleFilters(): Array<any> {
        return this.accessData.ModuleOptions.map(x => {
            return {
                moduleId: x.ModuleId,
                selected: this.hasAccessToModule(x.ModuleId),
                label: x.ModuleName
            };
        });
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    hasAccessToModule(moduleId: string): boolean {
        return this.data.Modules.some(x => x.ModuleId == moduleId);
    }
    
    moduleOptionIsEnabled(moduleId: string, option: string): boolean {
        const module = this.data.Modules.filter(x => x.ModuleId == moduleId)[0];
        if (module == null) return false;
        return module.Options.some(x => x == option);
    }
    
    moduleCategoryIsEnabled(moduleId: string, category: string): boolean {
        const module = this.data.Modules.filter(x => x.ModuleId == moduleId)[0];
        if (module == null) return false;
        return module.Categories.some(x => x == category);
    }

    getModuleAccessIds(moduleId: string): Array<string> {
        const module = this.data.Modules.filter(x => x.ModuleId == moduleId)[0];
        if (module == null) return [];
        return module.Ids;
    }

    roleIsEnabled(roleId: string): boolean {
        return this.data.Roles.some(x => x == roleId);
    }

    notifyChange(): void {
        this.$emit('input', this.data);
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch('value')
    onValueChanged(): void {
        this.data = this.value;
    }

    onRoleToggled(roleId: string, enabled: boolean): void {
        if (enabled && !this.roleIsEnabled(roleId))
        {
            this.data.Roles.push(roleId);
        }
        else if (!enabled && this.roleIsEnabled(roleId))
        {
            const index = this.data.Roles.findIndex(x => x == roleId);
            Vue.delete(this.data.Roles, index);
        }
        this.notifyChange();
    }

    onModuleAccessToggled(moduleId: string, enabled: boolean): void {
        if (enabled && !this.hasAccessToModule(moduleId))
        {
            this.data.Modules.push({
                ModuleId: moduleId,
                Options: [],
                Categories: [],
                Ids: []
            });
        }
        else if (!enabled && this.hasAccessToModule(moduleId))
        {
            const index = this.data.Modules.findIndex(x => x.ModuleId == moduleId);
            Vue.delete(this.data.Modules, index);
        }
        this.notifyChange();
    }

    onModuleAccessOptionToggled(moduleId: string, option: string, enabled: boolean): void {
        if (enabled && !this.hasAccessToModule(moduleId))
        {
            this.onModuleAccessToggled(moduleId, true);
        }

        const module = this.data.Modules.filter(x => x.ModuleId == moduleId)[0];
        if (enabled && !this.moduleOptionIsEnabled(moduleId, option))
        {
            module.Options.push(option);
        }
        else if (!enabled && this.moduleOptionIsEnabled(moduleId, option))
        {
            const index = module.Options.findIndex(x => x == option);
            Vue.delete(module.Options, index);
        }
        this.notifyChange();
    }

    onModuleAccessCategoryToggled(moduleId: string, category: string, enabled: boolean): void {
        if (enabled && !this.hasAccessToModule(moduleId))
        {
            this.onModuleAccessToggled(moduleId, true);
        }

        const module = this.data.Modules.filter(x => x.ModuleId == moduleId)[0];
        if (enabled && !this.moduleCategoryIsEnabled(moduleId, category))
        {
            module.Categories.push(category);
        }
        else if (!enabled && this.moduleCategoryIsEnabled(moduleId, category))
        {
            const index = module.Categories.findIndex(x => x == category);
            Vue.delete(module.Categories, index);
        }
        this.notifyChange();
    }

    onModuleAccessIdChanged(moduleId: string, ids: Array<string>): void {
        const module = this.data.Modules.filter(x => x.ModuleId == moduleId)[0];
        if (module == null) return;
        module.Ids = ids;
    }
}
</script>

<style scoped lang="scss">
.access-grid {
    .access-grid--row {
        margin-bottom: 10px;
        
        .access-grid--row--header {
            font-weight: 600;
            font-size: 15px;
        }
        .access-grid--row--options--header {
            align-self: center;
            margin-left: 20px;
            margin-top: 5px;
        }

        .access-grid--row--options {
            display: flex;
            flex-wrap: wrap;

            .access-grid--row--options--item,
            .access-grid--row--cat--item {
                margin-left: 20px;
            }
        }
    }
}
.filter-choice {
    &.selected {
        color: #fff;
        font-weight: 600;
    }
}
</style>

<style lang="scss">
.access-grid {
    .access-grid--row {
        .access-grid--row--options {
            .access-grid--row--options--item,
            .access-grid--row--cat--item {
                .v-input {
                    margin-top: 4px;
                    margin-bottom: 4px;
                }
            }
        }
    }
}
</style>

