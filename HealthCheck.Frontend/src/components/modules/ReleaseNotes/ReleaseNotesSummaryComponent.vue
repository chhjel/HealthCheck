<!-- src/components/modules/ReleaseNotes/ReleaseNotesSummaryComponent.vue -->
<template>
    <div v-if="isVisible">
        <shadow-root>
            <div @click="toggleVisibility" :style="styleToggleMetrics" :class="{ 'pulsating-circle': hasNewChanges }"
              title="Latest changes">{{ toggleButtonContent }}</div>

            <div :style="styleRoot" v-if="visible">
                <div style="display: flex; flex-wrap: wrap; align-items: baseline;">
                  <h3 v-if="config.Title" style="margin-top: 0; flex: 1; white-space: nowrap">{{ config.Title }} ({{ config.Version }})</h3>
                  <div style="font-size: small; font-family: monospace;">
                    When to show release notes? 
                    <div style="display:inline-block">
                      <span v-for="(choice, index) in visiblityChoices"
                        :key="`${id}-${index}-vischoice`"
                        :style="styleVisibilityConfigChoice(choice.visibility)"
                        @click.stop.prevent="setVisibilityConfig(choice.visibility)">[{{ choice.label }}]</span>
                    </div>
                  </div>
                </div>
                <p v-if="config.Description">{{ config.Description }}</p>
                <p v-if="config.BuiltAt">Built on {{ formatBuiltDate(config.BuiltAt) }}</p>

                <h4 style="margin-bottom: 5px">{{ changesTitle }}</h4>
                <ul>
                    <li v-for="(item, itemIndex) in includedChanges"
                        :key="`rn-item-${itemIndex}`">
                        <a :href="item.MainLink" target="_blank">{{ item.Title }}</a><div v-if="item.Description">{{ item.Description }}</div>
                    </li>
                </ul>
            </div>
            <div ref="styleRoot"></div>
        </shadow-root>
    </div>
</template>

<script lang="ts">
import IdUtils from "@util/IdUtils";
import { Vue, Prop, } from "vue-property-decorator";
import { Options } from "vue-class-component";
import DateUtils from "@util/DateUtils";
import { HCReleaseNotesViewModel } from "@generated/Models/Core/HCReleaseNotesViewModel";
import { HCReleaseNoteChangeViewModel } from "@generated/Models/Core/HCReleaseNoteChangeViewModel";

interface VisibilityConfigOption {
  label: string;
  visibility: Visibility;
}
type Visibility = 'never' | 'onNewVersion' | 'always';
@Options({
    components: {
    }
})
export default class ReleaseNotesSummaryComponent extends Vue {
    @Prop()
    config!: HCReleaseNotesViewModel;

    id: string = IdUtils.generateId();
    visible: boolean = false;
    hasNewChanges: boolean = false;
    hadNewChangesOnMounted: boolean = false;
    lastViewedVersionStorageKey: string = '__HC_ReleaseNotes_LastViewedVersion';
    visibilityStorageKey: string = '__HC_ReleaseNotes_Visibility';
    visibilityConfig: Visibility = 'always';
    visiblityChoices: Array<VisibilityConfigOption> = [
      { label: 'Always', visibility: "always" },
      { label: 'New versions only', visibility: "onNewVersion" },
      { label: 'Never', visibility: "never" }
    ];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
        this.createStyle();

        this.hasNewChanges = !!this.config.Version && localStorage.getItem(this.lastViewedVersionStorageKey) !== this.config.Version;
        this.hadNewChangesOnMounted = this.hasNewChanges;
        this.loadVisibilityConfig();
    }

    getInnerBarStyle(): any
    {
      return {
        'background-color': 'red'
      };
    }

    loadVisibilityConfig(): void {
        const storedVisibilityConfig = localStorage.getItem(this.visibilityStorageKey);
        if (storedVisibilityConfig == 'never') this.visibilityConfig = 'never';
        else if (storedVisibilityConfig == 'onNewVersion') this.visibilityConfig = 'onNewVersion';
    }

    setVisibilityConfig(vis: Visibility): void {
      this.visibilityConfig = vis;
      localStorage.setItem(this.visibilityStorageKey, vis);
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get includedChanges(): Array<HCReleaseNoteChangeViewModel> {
        return this.config.Changes;
    }

    get toggleButtonContent(): string {
        return '📝';
    }

    get isVisible(): boolean {
      return this.visibilityConfig == 'always'
        || (this.visibilityConfig == "onNewVersion" && this.hadNewChangesOnMounted);
    }

    //////////////
    //  STYLE  //
    ////////////
    // Using style getters because of shadow-dom
    styleVisibilityConfigChoice(vis: Visibility): any {
      let style: any = {
        'margin-right': '5px',
        'color': '#4d6d89'
      };
      if (this.visibilityConfig == vis) {
        style['font-weight'] = '600';
        style['color'] = '#111';
      } else {
        style['cursor'] = 'pointer';
      }
      return style;
    }

    get styleToggleMetrics(): any {
        const base: any = {
            "height": "1cm",
            "width": "1cm",

            "position": "fixed",
            "left": "10px",
            "bottom": "calc(10px + 1.5cm)",
            
            "cursor": "pointer",
            "opacity": "0.8",
            "font-size": "0.6cm",

            "display": "flex",
            "justify-content": "center",
            "align-items": "center",
            "user-select": "none"
        };

        const colors: any = {
            "background-color": "#fff",
            "border": "2px solid #929fbd",
        };

        return { ...base, ...colors };
    }

    get styleRoot(): any {
        return {
            'display': 'inline-block',
            'margin': '20px',
            'margin-left': '1.5cm',
            'padding': '20px',
            'font-family': 'sans-serif',
            "background-color": "rgb(255, 255, 255)",
            'color': '#333',
            "border": "8px solid rgb(225 225 225)",
            "position": "fixed",
            "bottom": "0",
            "left": "0",
            "max-height": "75%",
            "max-width": "65%",
            "overflow-x": "auto",
            "box-shadow": "0 0 20px 1px #3a3a3a75"
        };
    }

    get changesTitle(): string {
        const count = this.includedChanges.length;
        const word = count === 1 ? "change" : "changes";
        return `${count} ${word}:`;
    }

    ////////////////
    //  METHODS  //
    //////////////
    toggleVisibility(): void {
        this.visible = !this.visible;

        if (this.visible)
        {
            localStorage.setItem(this.lastViewedVersionStorageKey, this.config.Version);
            this.hasNewChanges = false;
        }
    }

    formatBuiltDate(date: Date): string {
        return DateUtils.FormatDate(date, "dddd dd. MMMM HH:mm");
    }

    formatChangeDate(date: Date): string {
        return DateUtils.FormatDate(date, "dd. MMMM HH:mm");
    }
    
    createStyle(): void {
        const createdStyleTag = document.createElement("style");
        createdStyleTag.textContent = `

.pulsating-circle:before {
  content: "";
  position: absolute;
  display: block;
  width: 200%;
  height: 200%;
  box-sizing: border-box;
  border-radius: 50%;
  background-color: #01a4e9;
  -webkit-animation: pulse-ring 2.25s cubic-bezier(0.215, 0.61, 0.355, 1) infinite;
          animation: pulse-ring 2.25s cubic-bezier(0.215, 0.61, 0.355, 1) infinite;
}
.pulsating-circle:after {
  content: "";
  position: absolute;
  left: 0;
  top: 0;
  display: block;
  width: 100%;
  height: 100%;
  border-radius: 50%;
  box-shadow: 0 0 8px rgba(0, 0, 0, 0.3);
  -webkit-animation: pulse-dot 2.25s cubic-bezier(0.455, 0.03, 0.515, 0.955) -0.4s infinite;
          animation: pulse-dot 2.25s cubic-bezier(0.455, 0.03, 0.515, 0.955) -0.4s infinite;
}

@-webkit-keyframes pulse-ring {
  0% {
    transform: scale(0.33);
  }
  80%, 100% {
    opacity: 0;
  }
}

@keyframes pulse-ring {
  0% {
    transform: scale(0.33);
  }
  80%, 100% {
    opacity: 0;
  }
}
@-webkit-keyframes pulse-dot {
  0% {
    transform: scale(0.8);
  }
  50% {
    transform: scale(1);
  }
  100% {
    transform: scale(0.8);
  }
}
@keyframes pulse-dot {
  0% {
    transform: scale(0.8);
  }
  50% {
    transform: scale(1);
  }
  100% {
    transform: scale(0.8);
  }
}
`;
        const parent = <HTMLElement>this.$refs.styleRoot;
        parent.appendChild(createdStyleTag);
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
</style>
