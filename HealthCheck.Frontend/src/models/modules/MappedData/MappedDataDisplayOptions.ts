type PropertyNameType = 'actual' | 'serialized';
export default interface MappedDataDisplayOptions {
    showPropertyNames: PropertyNameType;
    showPropertyRemarks: boolean;
    showMappedToPropertyNames: PropertyNameType;
    showMappedToTypes: boolean;
    showMappedToDeclaringTypes: boolean;
    showExampleValues: boolean;
}
