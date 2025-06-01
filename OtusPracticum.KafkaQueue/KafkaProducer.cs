using Confluent.Kafka;

namespace OtusPracticum.KafkaQueue
{
    public class KafkaProducer<K, V>(KafkaClientHandle handle)
    {
        private readonly IProducer<K, V> kafkaHandle = new DependentProducerBuilder<K, V>(handle.Handle).Build();

        public Task ProduceAsync(string topic, Message<K, V> message)
        {
            return kafkaHandle.ProduceAsync(topic, message);
        }

        public void Produce(string topic, Message<K, V> message, Action<DeliveryReport<K, V>>? deliveryHandler = null)
        {
            kafkaHandle.Produce(topic, message, deliveryHandler);
        }

        public void Flush(TimeSpan timeout)
        {
            kafkaHandle.Flush(timeout);
        }
    }
}
