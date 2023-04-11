import { TKJobHistoryStatus } from "@generated/Enums/Core/TKJobHistoryStatus";
import { TKJobHistoryEntryViewModel } from "@generated/Models/Core/TKJobHistoryEntryViewModel";
import { TKJobStatusViewModel } from "@generated/Models/Core/TKJobStatusViewModel";

export default class JobUtils
{
    public static jobIcon(status: TKJobStatusViewModel | null, latestHistory: TKJobHistoryEntryViewModel | null): string {
        if (status?.IsRunning === true) return 'fast_forward';
        else if (status?.Status === TKJobHistoryStatus.Error) return 'error';
        else if (status?.Status === TKJobHistoryStatus.Success) return 'check_circle';
        else if (status?.Status === TKJobHistoryStatus.Warning) return 'warning';
        else if (status?.Status === TKJobHistoryStatus.Cancelled) return 'cancel';
        else if (latestHistory?.Status === TKJobHistoryStatus.Error) return 'error';
        else if (latestHistory?.Status === TKJobHistoryStatus.Success) return 'check_circle';
        else if (latestHistory?.Status === TKJobHistoryStatus.Warning) return 'warning';
        else if (latestHistory?.Status === TKJobHistoryStatus.Cancelled) return 'cancel';
        else if (status?.IsEnabled === false) return 'block';
        else return 'pending';
    }

    public static jobIconColor(status: TKJobStatusViewModel | null, latestHistory: TKJobHistoryEntryViewModel | null): string {
        const icon = this.jobIcon(status, latestHistory);
        if (icon == 'error') return 'error';
        else if (icon == 'warning') return 'warning';
        else if (icon == 'cancel') return '#bfa890';
        else if (icon == 'check_circle') return 'success';
        else if (icon == 'fast_forward') return 'info';
        else return '--color--accent-darken2';
    }
}
