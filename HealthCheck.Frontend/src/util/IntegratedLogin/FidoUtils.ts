export default class FidoUtils
{
    private static _algs: { [key: string]: number } = {
        'RS1': -65535,
        'RS512': -259,
        'RS384': -258,
        'RS256': -257,
        'PS512': -39,
        'PS384': -38,
        'PS256': -37,
        'ES512': -36,
        'ES384': -35,
        'EdDSA': -8,
        'ES256': -7
    }
    static getAlgId(alg: string) : number | undefined{
        return FidoUtils._algs[alg];
    }
}
