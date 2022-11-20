import { Directive } from "vue";

export interface CustomDirective {
    name: string;
    directive: Partial<Directive<any, any>>;
}
export default function getCustomDirectives(): Array<CustomDirective>
{
    return [
        (() => {
            const getElementHeight = (e: HTMLElement) => {
                const margin = (parseFloat(e.style.marginTop) || 0) + (parseFloat(e.style.marginBottom) || 0);
                return e.offsetHeight + margin;
            }
            const setHeight = (el: HTMLElement) => {
                const targetElements: Array<HTMLElement> = (<any>el)._hc.targetElements;
                const height = targetElements.reduce((total: number, e: HTMLElement) => total + getElementHeight(e), 0);
                el.style.maxHeight = `${height}px`;
            }
            const onResize = (el: HTMLElement) => {
                setHeight(el);
            };
            return {
                name: 'set-max-height-from-children',
                directive: {
                    mounted: (el: HTMLElement, binding, vnode) => {
                        let targetElements: Array<HTMLElement> = [el];
                        Array.from(el.children).forEach(e => targetElements.push(e as HTMLElement));

                        const observer = new ResizeObserver(() => onResize(el));
                        targetElements.forEach(e => observer.observe(e));
                        
                        (<any>el)._hc = {};
                        (<any>el)._hc.targetElements = targetElements;
                        (<any>el)._hc.observer = observer
                        
                        setHeight(el);
                    },
                    beforeUnmount: (el: HTMLElement) => {
                        const observer: ResizeObserver = (<any>el)._hc.observer;
                        observer.disconnect();
                    }
                }
            };
        })(),
    ];
}