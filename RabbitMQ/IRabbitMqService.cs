namespace FoodDelivery.RabbitMQ
{
    public interface IRabbitMqService
    {
        void PublishMessage(string message, string queueName);
        void SubscribeMessage(string queueName, Action<string> onMessageReceived);
    }
}
