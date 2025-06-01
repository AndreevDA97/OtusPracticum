using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace OtusPracticum.KafkaQueue
{
    public class KafkaClientHandle
    {
        private readonly IProducer<byte[], byte[]> kafkaProducer;

        public KafkaClientHandle(IOptions<KafkaSettings> config)
        {
            var conf = new ProducerConfig
            {
                ClientId = "dotnet producer",
                BootstrapServers = config.Value.Host
            };
            kafkaProducer = new ProducerBuilder<byte[], byte[]>(conf).Build();
        }

        public Handle Handle { get => kafkaProducer.Handle; }

        public void Dispose()
        {
            kafkaProducer.Flush();
            kafkaProducer.Dispose();
        }
    }
}
