<!-- src/components/modules/ReleaseNotes/ReleaseNotesSummaryComponent.vue -->
<template>
    <div>
        <shadow-root>
            <div @click="toggleVisibility" :style="styleToggleMetrics" :class="{ 'pulsating-circle': hasNewChanges }">{{ toggleButtonContent }}</div>

            <div :style="styleRoot" v-if="visible">
                <h3 v-if="config.Title" style="margin-top: 0;">{{ config.Title }} ({{ config.Version }})</h3>
                <p v-if="config.Description">{{ config.Description }}</p>
                <p v-if="config.BuiltAt">Built on {{ formatBuiltDate(config.BuiltAt) }}</p>

                <h4 style="margin-bottom: 5px">{{ changesTitle }}</h4>
                <ul>
                    <li v-for="(item, itemIndex) in includedChanges"
                        :key="`rn-item-${itemIndex}`">
                        <a :href="item.MainLink">{{ item.Title }}</a><div v-if="item.Description">{{ item.Description }}</div>
                    </li>
                </ul>
            </div>
            <div ref="styleRoot"></div>
        </shadow-root>
    </div>
</template>

<script lang="ts">
import IdUtils from "util/IdUtils";
import { Vue, Component, Prop, } from "vue-property-decorator";
import DateUtils from "util/DateUtils";
import { HCReleaseNotesViewModel } from "generated/Models/Core/HCReleaseNotesViewModel";
import { HCReleaseNoteChangeViewModel } from "generated/Models/Core/HCReleaseNoteChangeViewModel";

@Component({
    components: {
    }
})
export default class ReleaseNotesSummaryComponent extends Vue {
    @Prop()
    config!: HCReleaseNotesViewModel;

    id: string = IdUtils.generateId();
    visible: boolean = false;
    hasNewChanges: boolean = false;
    lastViewedVersionStorageKey: string = '__HC_ReleaseNotes_LastViewedVersion';

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
        this.createStyle();

        this.hasNewChanges = !!this.config.Version && localStorage.getItem(this.lastViewedVersionStorageKey) !== this.config.Version;
    }

    getInnerBarStyle(): any
    {
      return {
        'background-color': 'red'
      };
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

    //////////////
    //  STYLE  //
    ////////////
    // Using style getters because of shadow-dom
    get styleToggleMetrics(): any {
        const base: any = {
            "height": "1cm",
            "width": "1cm",
            "border-radius": "35%",

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
            'color': '#333',
            'border': '2px solid #ddd',
            'box-shadow': '#d5d7d5 4px 4px 6px 0px'
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

            setTimeout(() => {
                window.scrollTo({
                    top: (window.pageYOffset || document.documentElement.scrollTop) 
                        + this.$el.getBoundingClientRect().top - 100,
                    behavior: 'smooth'
                });
            }, 10);
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
