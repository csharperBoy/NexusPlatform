
using MediatR;

namespace StreamBus.Application.Notifications
{
    public record StreamMessageNotification<TMessage>(TMessage Message) : INotification
        where TMessage : class;
}
