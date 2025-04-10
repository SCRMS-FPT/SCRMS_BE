namespace BuildingBlocks.Messaging.Events
{
    public record SendMailEvent(string To,string Body,string Subject,bool isHtml) : IntegrationEvent;
}
