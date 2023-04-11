export default class Base64Util
{
    // https://developer.mozilla.org/en-US/docs/Glossary/Base64#solution_1_%E2%80%93_escaping_the_string_before_encoding_it

    public static base64Encode(input: string): string {
        const utf8Input = this.strToUTF8Arr(input);
        return this.base64EncArr(utf8Input);
    }

    private static base64EncArr(aBytes: Uint8Array): string {
        let nMod3: number = 2;
        let sB64Enc: string = "";
    
        const nLen = aBytes.length;
        let nUint24 = 0;
        for (let nIdx = 0; nIdx < nLen; nIdx++) {
        nMod3 = nIdx % 3;
        if (nIdx > 0 && ((nIdx * 4) / 3) % 76 === 0) {
            sB64Enc += "\r\n";
        }
    
        nUint24 |= aBytes[nIdx] << ((16 >>> nMod3) & 24);
        if (nMod3 === 2 || aBytes.length - nIdx === 1) {
            sB64Enc += String.fromCodePoint(
                Base64Util.uint6ToB64((nUint24 >>> 18) & 63),
                Base64Util.uint6ToB64((nUint24 >>> 12) & 63),
                Base64Util.uint6ToB64((nUint24 >>> 6) & 63),
                Base64Util.uint6ToB64(nUint24 & 63)
            );
            nUint24 = 0;
        }
        }
        return (
            // sB64Enc.substr(0, sB64Enc.length - 2 + nMod3) +
            sB64Enc.substring(0, sB64Enc.length - 2 + nMod3) +
            (nMod3 === 2 ? "" : nMod3 === 1 ? "=" : "==")
        );
    }
    
    /* Base64 string to array encoding */
    private static uint6ToB64(nUint6: number): number {
        return nUint6 < 26
        ? nUint6 + 65
        : nUint6 < 52
        ? nUint6 + 71
        : nUint6 < 62
        ? nUint6 - 4
        : nUint6 === 62
        ? 43
        : nUint6 === 63
        ? 47
        : 65;
    }
    
    private static strToUTF8Arr(inputStr: string): Uint8Array {
      let aBytes: Uint8Array;
      let nChr: number;
      const nStrLen = inputStr.length;
      let nArrLen = 0;
    
      /* mapping… */
      for (let nMapIdx = 0; nMapIdx < nStrLen; nMapIdx++) {
        nChr = inputStr.codePointAt(nMapIdx);
    
        if (nChr > 65536) {
          nMapIdx++;
        }
    
        nArrLen +=
          nChr < 0x80
            ? 1
            : nChr < 0x800
            ? 2
            : nChr < 0x10000
            ? 3
            : nChr < 0x200000
            ? 4
            : nChr < 0x4000000
            ? 5
            : 6;
      }
    
      aBytes = new Uint8Array(nArrLen);
    
      /* transcription… */
      let nIdx = 0;
      let nChrIdx = 0;
      while (nIdx < nArrLen) {
        nChr = inputStr.codePointAt(nChrIdx);
        if (nChr < 128) {
          /* one byte */
          aBytes[nIdx++] = nChr;
        } else if (nChr < 0x800) {
          /* two bytes */
          aBytes[nIdx++] = 192 + (nChr >>> 6);
          aBytes[nIdx++] = 128 + (nChr & 63);
        } else if (nChr < 0x10000) {
          /* three bytes */
          aBytes[nIdx++] = 224 + (nChr >>> 12);
          aBytes[nIdx++] = 128 + ((nChr >>> 6) & 63);
          aBytes[nIdx++] = 128 + (nChr & 63);
        } else if (nChr < 0x200000) {
          /* four bytes */
          aBytes[nIdx++] = 240 + (nChr >>> 18);
          aBytes[nIdx++] = 128 + ((nChr >>> 12) & 63);
          aBytes[nIdx++] = 128 + ((nChr >>> 6) & 63);
          aBytes[nIdx++] = 128 + (nChr & 63);
          nChrIdx++;
        } else if (nChr < 0x4000000) {
          /* five bytes */
          aBytes[nIdx++] = 248 + (nChr >>> 24);
          aBytes[nIdx++] = 128 + ((nChr >>> 18) & 63);
          aBytes[nIdx++] = 128 + ((nChr >>> 12) & 63);
          aBytes[nIdx++] = 128 + ((nChr >>> 6) & 63);
          aBytes[nIdx++] = 128 + (nChr & 63);
          nChrIdx++;
        } /* if (nChr <= 0x7fffffff) */ else {
          /* six bytes */
          aBytes[nIdx++] = 252 + (nChr >>> 30);
          aBytes[nIdx++] = 128 + ((nChr >>> 24) & 63);
          aBytes[nIdx++] = 128 + ((nChr >>> 18) & 63);
          aBytes[nIdx++] = 128 + ((nChr >>> 12) & 63);
          aBytes[nIdx++] = 128 + ((nChr >>> 6) & 63);
          aBytes[nIdx++] = 128 + (nChr & 63);
          nChrIdx++;
        }
        nChrIdx++;
      }
    
      return aBytes;
    }
}