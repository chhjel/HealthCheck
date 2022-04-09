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
            let targetElement: HTMLElement;
            let observer: ResizeObserver;
            let setHeight = () => {
                // selfElement.style.maxHeight = null;
                selfElement.style.maxHeight = `${targetElement.clientHeight}px`;
                setTimeout(() => {
                }, 1);
            }
            return {
                name: 'set-max-height',
                directive: {
                    mounted: (el: HTMLElement, binding, vnode) => {
                        selfElement = el;
                        targetElement = el;
                        const childIndex: number | null = (binding.arg == null || binding.arg.length == 0) ? null : Number(binding.arg);
                        if (childIndex != null) {
                            targetElement = targetElement.children[childIndex] as HTMLElement;
                        }
                        setHeight();
                        observer = new ResizeObserver(setHeight);
                        observer.observe(targetElement);
                    },
                    beforeUnmount: (el: HTMLElement) => {
                        observer.disconnect();
                    }
                }
            };
        })()
    ];
}