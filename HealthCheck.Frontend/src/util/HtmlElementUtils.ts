export default class HtmlElementUtils
{
    static IsInViewport(elem: Element) {
        const bounding = elem.getBoundingClientRect();
        return (
            bounding.top >= 0 &&
            bounding.left >= 0 &&
            bounding.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
            bounding.right <= (window.innerWidth || document.documentElement.clientWidth)
        );
    };

    static CreateQuerySelector(elem: Element): string
    {
        var element = elem;
        var str = "";

        function loop(element:  Element) {
            if (element == null) {
                return str;
            }

            // stop here = element has ID
            if (element.getAttribute("id")) {
                str = str.replace(/^/, " #" + element.getAttribute("id"));
                str = str.replace(/\s/, "");
                str = str.replace(/\s/g, " > ");
                return str;
            }

            // stop here = element is body
            if (document.body === element) {
                str = str.replace(/^/, " body");
                str = str.replace(/\s/, "");
                str = str.replace(/\s/g, " > ");
                return str;
            }

            // concat all classes in "queryselector" style
            if (element.getAttribute("class")) {
                var elemClasses = ".";
                elemClasses += element.getAttribute("class");
                elemClasses = elemClasses.replace(/\s/g, ".");
                elemClasses = elemClasses.replace(/^/g, " ");
                
                let classSelector = elemClasses.replace(/\.+$/g, '');
                let classResults = document.querySelectorAll(classSelector);
                if (classResults.length == 1 && classResults[0] === elem) {
                    str = classSelector;
                    return str;
                }

                classSelector = `${element.tagName} ${classSelector}`;
                classResults = document.querySelectorAll(classSelector);
                if (classResults.length == 1 && classResults[0] === elem) {
                    str = classSelector;
                    return str;
                }

                var classNth = "";

                // check if element class is the unique child
                var childrens = (element.parentNode == null) ? [] : element.parentNode.children;

                if (childrens.length < 2) {
                    return;
                }

                var similarClasses = [];

                for (var i = 0; i < childrens.length; i++) {
                    if (element.getAttribute("class") == childrens[i].getAttribute("class")) {
                        similarClasses.push(childrens[i]);
                    }
                }

                if (similarClasses.length > 1) {
                    for (var j = 0; j < similarClasses.length; j++) {
                        if (element === similarClasses[j]) {
                            j++;
                            classNth = ":nth-of-type(" + j + ")";
                            break;
                        }
                    }
                }

                str = str.replace(/^/, elemClasses + classNth);
            } else {
                // // get nodeType
                // var name = element.nodeName;
                // name = name.toLowerCase();
                // var nodeNth = "";

                // var childrens = (element.parentNode == null) ? [] : element.parentNode.children;

                // if (childrens.length > 2) {
                //     var similarNodes = [];

                //     for (var i = 0; i < childrens.length; i++) {
                //     if (element.nodeName == childrens[i].nodeName) {
                //         similarNodes.push(childrens[i]);
                //     }
                //     }

                //     if (similarNodes.length > 1) {
                //     for (var j = 0; j < similarNodes.length; j++) {
                //         if (element === similarNodes[j]) {
                //         j++;
                //         nodeNth = ":nth-of-type(" + j + ")";
                //         break;
                //         }
                //     }
                //     }
                // }

                // str = str.replace(/^/, " " + name + nodeNth);
            }

            if (element.parentNode) {
                loop(<any>element.parentNode);
            } else {
                str = str.replace(/\s/g, " > ");
                str = str.replace(/\s/, "");
                return str;
            }
        }

        loop(element);

        return str;
    }
}
