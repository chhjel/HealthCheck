import { EventSinkNotificationConfig, EventSinkNotificationConfigFilter, FilterMatchType, IEventNotifier } from "../../models/modules/EventNotifications/EventNotificationModels";
import IdUtils from "../IdUtils";

export interface ConfigDescription
{
    description: string;
    conditions: Array<ConfigFilterDescription>;
    actions: Array<ConfigActionDescription>;
}
export interface ConfigFilterDescription
{
    id: string;
    description: string;
}
export interface ConfigActionDescription
{
    id: string;
    description: string;
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
        
        let conditions: Array<ConfigFilterDescription> = [
            {
                id: config.EventIdFilter._frontendId,
                description: `event id ${this.describeFilter(config.EventIdFilter)}`
            }
        ];
        config.PayloadFilters
            .forEach(x => conditions.push({
                id: x._frontendId,
                description: `payload ${this.describeFilter(x)}`
            }));

        let actions = config.NotifierConfigs
            .filter(x => x.Notifier != null)
            .map(x => {
                return {
                    id: x.NotifierId,
                    description: x.Notifier != null ? x.Notifier.Name : ''
                };
            });
        
        // counts
        actions = actions.map(x => {
            const count = actions.filter(y => y.description == x.description).length;
            if (count > 1) {
                x.description = `${x.description} (x${count})`;
            }
            return x;
        });

        // distinct
        actions = actions.filter((x, i) => actions.findIndex(y => y.id == x.id) == i);

        let actionsDescription = actions.map(x => x.description).joinForSentence(', ', ' and ');
        if (actionsDescription != null && actionsDescription.length > 0)
        {
            actionsDescription = `then notify through ${actionsDescription}`;
        }
        else
        {
            actionsDescription = 'then <do nothing>';
        }

        let description = 
            `If ${conditions.map(x => x.description).joinForSentence(', ', ' and ')} ${actionsDescription}.`;

        return {
            description: description,
            conditions: conditions,
            actions: actions.map(x => {
                // x.description = `notify through ${x.description}`;
                return x;
            })
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
        else if (filter.MatchType == FilterMatchType.StartsWith)
        {
            matchType = 'starts with';
        }
        else if (filter.MatchType == FilterMatchType.EndsWith)
        {
            matchType = 'ends with';
        }

        let caseSensitive = filter.CaseSensitive ? ' (case-sensitive)' : '';

        return `${target} ${matchType} ${filterValue}${caseSensitive}`;
    }

}
