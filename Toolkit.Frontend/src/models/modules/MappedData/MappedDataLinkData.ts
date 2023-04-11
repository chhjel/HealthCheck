export default interface MappedDataLinkData {
    type: 'ReferencedDefinition' | 'ClassDefinition';
    id: string;
    newWindow: boolean;
}
