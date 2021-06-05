export default class WebAuthnUtil
{
    static coerceToArrayBuffer(thing: string | Array<any> | Uint8Array | ArrayBufferLike): ArrayBuffer {
        let converted = thing;
        if (typeof converted === "string") {
            // base64url to base64
            converted = converted.replace(/-/g, "+").replace(/_/g, "/");

            // base64 to Uint8Array
            const str = window.atob(converted);
            const bytes = new Uint8Array(str.length);
            for (let i = 0; i < str.length; i++) {
                bytes[i] = str.charCodeAt(i);
            }
            converted = bytes;
        }

        // Array to Uint8Array
        if (Array.isArray(converted)) {
            converted = new Uint8Array(converted);
        }

        // Uint8Array to ArrayBuffer
        if (converted instanceof Uint8Array) {
            converted = converted.buffer;
        }

        // error if none of the above worked
        if (!(converted instanceof ArrayBuffer)) {
            throw new TypeError("could not coerce to ArrayBuffer");
        }

        if (converted.byteLength <= 0)
        {
            throw new TypeError("coerced  length is zero");
        }

        return converted;
    };

    static coerceToBase64Url(thing: any): any {
        // Array or ArrayBuffer to Uint8Array
        if (Array.isArray(thing)) {
            thing = Uint8Array.from(thing);
        }

        if (thing instanceof ArrayBuffer) {
            thing = new Uint8Array(thing);
        }

        // Uint8Array to base64
        if (thing instanceof Uint8Array) {
            var str = "";
            var len = thing.byteLength;

            for (var i = 0; i < len; i++) {
                str += String.fromCharCode(thing[i]);
            }
            thing = window.btoa(str);
        }

        if (typeof thing !== "string") {
            throw new Error("could not coerce to string");
        }

        // base64 to base64url
        // NOTE: "=" at the end of challenge is optional, strip it off here
        thing = thing.replace(/\+/g, "-").replace(/\//g, "_").replace(/=*$/g, "");

        return thing;
    };
}
