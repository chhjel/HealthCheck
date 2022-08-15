import { Directive } from "vue";

export interface CustomDirective {
    name: string;
    directive: Partial<Directive<any, any>>;
}
export default function getCustomDirectives(): Array<CustomDirective>
{
    return [
        (() => {
            let selfElement: HTMLElement;
            let targetElements: Array<HTMLElement>;
            let observer: ResizeObserver;
            const getElementHeight = (e: HTMLElement) => {
                const margin = (parseFloat(e.style.marginTop) || 0) + (parseFloat(e.style.marginBottom) || 0);
                // const padding = (parseFloat(e.style.paddingTop) || 0) + (parseFloat(e.style.paddingBottom) || 0);
                return e.offsetHeight + margin;
            }
            const setHeight = () => {
                const height = targetElements.reduce((total: number, e: HTMLElement) => total + getElementHeight(e), 0);
                selfElement.style.maxHeight = `${height}px`;
            }
            const onResize = (entries: ResizeObserverEntry[], observer: ResizeObserver) => {
                setHeight();
            };
            return {
                name: 'set-max-height-from-children',
                directive: {
                    mounted: (el: HTMLElement, binding, vnode) => {
                        selfElement = el;
                        targetElements = [el];
                        Array.from(el.children).forEach(e => targetElements.push(e as HTMLElement));
                        // const targetArg: string | number | null = (binding.arg == null || binding.arg.length == 0)
                        //     ? null
                        //     : (binding.arg == 'c' ? 'c' : Number(binding.arg));
                        setHeight();
                        observer = new ResizeObserver(onResize);
                        targetElements.forEach(e => observer.observe(e));
                    },
                    beforeUnmount: (el: HTMLElement) => {
                        observer.disconnect();
                    }
                }
            };
        })(),
        // {
        //     name: 'set-max-height-from-child',
        //     directive: {
        //         mounted: (el: HTMLElement) => {
        //             el.style.maxHeight = `${el.children[0].clientHeight}px`;
        //         }
        //     }
        // },
    ];
}