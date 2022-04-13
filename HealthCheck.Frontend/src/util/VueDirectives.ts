import { Directive } from "vue";

export interface CustomDirective {
    name: string;
    directive: Partial<Directive<any, any>>;
}
export default function getCustomDirectives(): Array<CustomDirective>
{
    return [
        // {
        //     name: 'set-max-height-from-child',
        //     directive: {
        //         mounted: (el: HTMLElement) => {
        //             el.style.maxHeight = `${el.children[0].clientHeight}px`;
        //         }
        //     }
        // },
        (() => {
            let selfElement: HTMLElement;
            let targetElements: Array<HTMLElement>;
            let observer: ResizeObserver;
            let setHeight = () => {
                const height = targetElements.reduce((total: number, e: HTMLElement) => total + e.clientHeight, 0);
                selfElement.style.maxHeight = `${height}px`;
            }
            return {
                name: 'set-max-height',
                directive: {
                    mounted: (el: HTMLElement, binding, vnode) => {
                        selfElement = el;
                        targetElements = [el];
                        const targetArg: string | number | null = (binding.arg == null || binding.arg.length == 0)
                            ? null
                            : (binding.arg == 'c' ? 'c' : Number(binding.arg));
                        if (targetArg == 'c') {
                            targetElements = [];
                            Array.from(el.children).forEach(e => targetElements.push(e as HTMLElement));
                        }
                        else if (targetArg != null) {
                            targetElements = [el.children[targetArg] as HTMLElement];
                        }
                        setHeight();
                        observer = new ResizeObserver(setHeight);
                        targetElements.forEach(e => observer.observe(e));
                    },
                    beforeUnmount: (el: HTMLElement) => {
                        observer.disconnect();
                    }
                }
            };
        })()
    ];
}