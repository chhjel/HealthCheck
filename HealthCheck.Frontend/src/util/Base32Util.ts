export default class Base32Util
{
    private static charTable = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    private static byteTable = [
        0xff, 0xff, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
        0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e,
        0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
        0x17, 0x18, 0x19, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
        0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e,
        0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
        0x17, 0x18, 0x19, 0xff, 0xff, 0xff, 0xff, 0xff
    ];

    static quintetCount(buff: any): any {
        var quintets = Math.floor(buff.length / 5);
        return buff.length % 5 === 0 ? quintets : quintets + 1;
    }

    static encode(plain: string): any {
        var i = 0;
        var j = 0;
        var shiftIndex = 0;
        var digit = 0;
        var encoded: string = '';
        /* byte by byte isn't as pretty as quintet by quintet but tests a bit
            faster. will have to revisit. */
        while (i < plain.length) {
            var current = plain.charCodeAt(i);
            if (shiftIndex > 3) {
                digit = current & (0xff >> shiftIndex);
                shiftIndex = (shiftIndex + 5) % 8;
                digit = (digit << shiftIndex) | ((i + 1 < plain.length)
                    ? plain.charCodeAt(i + 1)
                    : 0) >> (8 - shiftIndex);
                i++;
            }
            else {
                digit = (current >> (8 - (shiftIndex + 5))) & 0x1f;
                shiftIndex = (shiftIndex + 5) % 8;
                if (shiftIndex === 0)
                    i++;
            }
            encoded += this.charTable.charAt(digit);
            j++;
        }
        for (i = j; i < encoded.length; i++) {
            encoded += '=';
        }
        return encoded;
    }

    static decode(encoded: string): string {
        var shiftIndex = 0;
        var plainDigit = 0;
        var plainChar: any;
        var decoded: string = '';
        /* byte by byte isn't as pretty as octet by octet but tests a bit
            faster. will have to revisit. */
        for (var i = 0; i < encoded.length; i++) {
            if (encoded[i] === '=') {
                break;
            }
            var encodedByte = encoded[i].charCodeAt(0) - 0x30;
            if (encodedByte < this.byteTable.length) {
                plainDigit = this.byteTable[encodedByte];
                if (shiftIndex <= 3) {
                    shiftIndex = (shiftIndex + 5) % 8;
                    if (shiftIndex === 0) {
                        plainChar |= plainDigit;
                        decoded += String.fromCharCode(plainChar);
                        plainChar = 0;
                    }
                    else {
                        plainChar |= 0xff & (plainDigit << (8 - shiftIndex));
                    }
                }
                else {
                    shiftIndex = (shiftIndex + 5) % 8;
                    plainChar |= 0xff & (plainDigit >>> shiftIndex);
                    decoded += String.fromCharCode(plainChar);
                    plainChar = 0xff & (plainDigit << (8 - shiftIndex));
                }
            }
            else {
                throw new Error('Invalid input - it is not base32 encoded string');
            }
        }
        return decoded;
    }
}
