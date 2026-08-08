using Grpc.Core;

namespace StreamBus.Application.Options
{

    public class StreamConsumerOptions<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        public string ServerAddress { get; set; } = string.Empty;
        public Method<TRequest, TResponse>? GrpcMethod { get; set; }
        public TRequest? InitialRequest { get; set; }
    }
}
