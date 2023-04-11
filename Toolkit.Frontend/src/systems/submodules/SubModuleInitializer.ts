import GetDefinedModules from "./SubModuleDefinitions";

export default function InitializeSubModules() {
    initializeNewModules(document.body);

    const targetNode: HTMLElement = document.body;
    const config: MutationObserverInit = { childList: true, subtree: true };

    const callback: MutationCallback = (mutationsList) => {
        for (const mutation of mutationsList) {
            if (mutation.type === "childList") {
                initializeNewModules(document.body);
            }
        }
    };

    const observer = new MutationObserver(callback);
    observer.observe(targetNode, config);
}

interface InitializedModule {
    element: HTMLElement;
}
let initializedModules = Array<InitializedModule>();

function initializeNewModules(container: HTMLElement) {
    const elementsToInitialize: Array<HTMLElement> = [];
    const allElementsWithModuleAttribute = container.querySelectorAll<HTMLElement>("[data-submodule]");
    allElementsWithModuleAttribute.forEach((element) => {
        if (!initializedModules.some((x) => x.element == element)) {
            elementsToInitialize.push(element);
        }
    });

    elementsToInitialize.forEach((element) => {
        initializeModule(element);
    });
}

function initializeModule(element: HTMLElement) {
    const moduleName = element.dataset.submodule;
    if (moduleName == null) {
        return;
    }

    const moduleOptionsJson = element.dataset.submoduleoptions;
    let moduleOptions: any = {};
    if (moduleOptionsJson != null) {
        //console.log("ModuleOption", moduleOptionsJson)
        try {
            moduleOptions = JSON.parse(moduleOptionsJson);
        } catch (e) {
            console.warn(`Module '${moduleName}' has invalid options object: `, moduleOptionsJson);
            moduleOptions = {};
        }
    }

    const moduleClass = GetDefinedModules()[moduleName];
    if (moduleClass == null) {
        console.warn(`Could not find a registered module with the classname '${moduleName}'. Did you add it to ModuleDefinitions.ts?`);
        return;
    }

    const moduleInstance = new moduleClass();
    moduleInstance.preInitializeModule(element, moduleOptions);
    moduleInstance.init();

    initializedModules.push({
        element: element
    });
}
