import { ParameterDetails } from "src/entry/index_store";

export default class TestsUtils
{
    static setParameterDetail<T>(store: any, parameterId: string, key: string, value: T): void {
        const details: ParameterDetails = {
            parameterId: parameterId,
            key: key,
            value: value
        };
        store.commit('setTestParameterDetails', details);
    }
    
    static getParameterDetail<T>(store: any, parameterId: string,key: string): T | null {
        const detailsForParameter = store.state.tests.parameterDetails[parameterId];
        if (!detailsForParameter)
        {
            return null;
        }
        return detailsForParameter[key];
    }
}
