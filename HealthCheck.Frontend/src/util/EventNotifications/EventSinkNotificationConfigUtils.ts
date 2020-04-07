import { EventSinkNotificationConfig, EventSinkNotificationConfigFilter, FilterMatchType, IEventNotifier } from "../../models/EventNotifications/EventNotificationModels";
import IdUtils from "../IdUtils";

export interface ConfigDescription
{
    description: string;
    conditions: Array<string>;
    actions: Array<string>;
}

export default class EventSinkNotificationConfigUtils
{
    static postProcessConfig(config: EventSinkNotificationConfig, notifiers: Array<IEventNotifier> | null): void {
        config.FromTime = (config.FromTime == null) ? null : new Date(config.FromTime);
        config.ToTime = (config.ToTime == null) ? null : new Date(config.ToTime);
        config.LastNotifiedAt = (config.LastNotifiedAt == null) ? null : new Date(config.LastNotifiedAt);
        config.LastChangedAt = new Date(config.LastChangedAt);
        
        config.NotifierConfigs.forEach(nconfig => {
            if (notifiers != null)
            {
                nconfig.Notifier = notifiers.filter(x => x.Id == nconfig.NotifierId)[0];
            }
        });

        this.postProcessPayloadFilter(config.EventIdFilter);
        config.PayloadFilters.forEach(x => this.postProcessPayloadFilter(x));
    }

    static postProcessPayloadFilter(filter: EventSinkNotificationConfigFilter): void {
        if (filter._frontendId == null)
        {
            filter._frontendId = IdUtils.generateId();
        }
    }

    static describeConfig(config: EventSinkNotificationConfig): ConfigDescription {
        
        let conditions = [
            `event id ${this.describeFilter(config.EventIdFilter)}`
        ];
        config.PayloadFilters
            .map(x => `payload ${this.describeFilter(x)}`)
            .forEach(x => conditions.push(x));

        let actions = config.NotifierConfigs
            .map(x => x.Notifier != null ? x.Notifier.Name : '')
            .filter(x => x != '');
        actions = Array.from(new Set(actions));
        let actionsDescription = actions.joinForSentence(', ', ' and ');
        if (actionsDescription != null && actionsDescription.length > 0)
        {
            actionsDescription = `then notify through ${actionsDescription}`;
        }
        else
        {
            actionsDescription = 'then <do nothing>';
        }

        let description = 
            `If ${conditions.joinForSentence(', ', ' and ')} ${actionsDescription}.`;

        return {
            description: description,
            conditions: conditions,
            actions: actions.map(x => `notify through ${x}`)
        };
    }

    static describeFilter(filter: EventSinkNotificationConfigFilter): string {
        let target = '';
        let propName = (filter.PropertyName == null || filter.PropertyName.trim().length == 0) 
            ? '<not set>'
            : `'${filter.PropertyName}'`;
        let filterValue = (filter.Filter == null || filter.Filter.trim().length == 0) 
            ? '<anything>'
            : `'${filter.Filter}'`;
        
        if (filter.PropertyName != null)
        {
            target = `property ${propName}`;
        }

        let matchType = 'matches';
        if (filter.MatchType == FilterMatchType.Contains)
        {
            matchType = 'contains';
        }
        else if (filter.MatchType == FilterMatchType.RegEx)
        {
            matchType = 'matches regex';
        }

        let caseSensitive = filter.CaseSensitive ? ' (case-sensitive)' : '';

        return `${target} ${matchType} ${filterValue}${caseSensitive}`;
    }

}
