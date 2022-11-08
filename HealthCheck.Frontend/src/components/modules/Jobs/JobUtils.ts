import { HCJobHistoryStatus } from "@generated/Enums/Core/HCJobHistoryStatus";
import { HCJobHistoryEntryViewModel } from "@generated/Models/Core/HCJobHistoryEntryViewModel";
import { HCJobStatusViewModel } from "@generated/Models/Core/HCJobStatusViewModel";

export default class JobUtils
{
    public static jobIcon(status: HCJobStatusViewModel | null, latestHistory: HCJobHistoryEntryViewModel | null): string {
        if (status?.IsRunning === true) return 'fast_forward';
        else if (latestHistory?.Status === HCJobHistoryStatus.Warning) return 'warning';
        else if (latestHistory?.Status === HCJobHistoryStatus.Error) return 'error';
        else if (status?.LastRunWasSuccessful === true) return 'check_circle';
        else if (latestHistory?.Status === HCJobHistoryStatus.Success) return 'check_circle';
        else if (status?.LastRunWasSuccessful === false) return 'error';
        else if (status?.IsEnabled === false) return 'block';
        else return 'pending';
    }

    public static jobIconColor(status: HCJobStatusViewModel | null, latestHistory: HCJobHistoryEntryViewModel | null): string {
        const icon = this.jobIcon(status, latestHistory);
        if (icon == 'error') return 'error';
        else if (icon == 'warning') return 'warning';
        else if (icon == 'check_circle') return 'success';
        else if (icon == 'fast_forward') return 'info';
        else return '--color--accent-darken2';
    }
}
