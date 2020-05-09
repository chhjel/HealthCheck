import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import DiagramsDataViewModel from "../models/Documentation/DiagramsDataViewModel";

export default class DocumentationService extends HCServiceBase
{
    public GetDiagramsData(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<DiagramsDataViewModel> | null = null
    ) : void
    {
        let url = this.options.DiagramsDataEndpoint;
        this.fetchExt<DiagramsDataViewModel>(url, 'GET', null, statusObject, callbacks);
    }
}
