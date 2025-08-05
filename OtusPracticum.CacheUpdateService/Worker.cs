using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OtusPracticum.Entities;
using OtusPracticum.KafkaQueue;
using OtusPracticum.KafkaQueue.Models;
using OtusPracticum.Services;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace OtusPracticum.CacheUpdateService
{
    public class Worker(
        IOptions<KafkaSettings> options,
        NpgsqlService npgsql,
        IDistributedCache cache,
        PostRepository postRepository,
        IConfiguration configuration,
        UserAuthService userAuthService) : BackgroundService
    {
        private readonly IOptions<KafkaSettings> options = options;
        private readonly IDistributedCache cache = cache;
        private readonly UserAuthService userAuthService = userAuthService;
        private readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);
        private readonly string signalrHost = configuration["LiveFeedService:URL"]!;

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            // добавление пустой записи для принудительного создания темы в Kafka
            //using var producer = new ProducerBuilder<string, string>(GetConsumerConfig()).Build();
            //producer.Produce("feed-posts", new Message<string, string> { Key = "", Value = "" });

             await using var connection = new HubConnectionBuilder()
                .WithUrl(signalrHost, x => x.AccessTokenProvider = async () =>
                    await userAuthService.GetTokenAsync())
                .Build();
            await connection.StartAsync(ct);

            using var consumer = new ConsumerBuilder<string, string>(GetConsumerConfig()).Build();
            consumer.Subscribe("feed-posts");
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var consumerResult = consumer.Consume(ct);
                    if (consumerResult.IsPartitionEOF)
                    {
                        await Task.Delay(2000, ct);
                        continue;
                    }
                    Console.WriteLine($"Обработка сообщения {consumerResult.Message.Value}; Ключ {consumerResult.Message.Key}");
                    if (string.IsNullOrEmpty(consumerResult.Message.Value))
                    {
                        consumer.StoreOffset(consumerResult);
                        continue;
                    }

                    var message = JsonSerializer.Deserialize<FeedUpdateMessage>(consumerResult.Message.Value, jsonOptions)!;
                    if (message.ActionType == ActionTypeEnum.FullReload)
                    {
                        Console.WriteLine($"Инициирована перезагрузка кеша");
                        await ReloadFeedAsync(message.Author_id, $"feed-{consumerResult.Message.Key}", ct);
                        consumer.StoreOffset(consumerResult);
                        continue;
                    }
                    else
                    {
                        await connection.InvokeAsync("Send", consumerResult.Message.Value, consumerResult.Message.Key, ct);
                    }

                    var user_id = Guid.Parse(consumerResult.Message.Key);
                    if (message.Post_id is null)
                        throw new Exception($"Invalid message with actiontype = {message.ActionType} and post_id = null");
                    Console.WriteLine($"Обработка пользователя {user_id}");
                    var key = $"feed-{user_id}";
                    var cachedFeedJson = await cache.GetStringAsync(key, ct);
                    if (cachedFeedJson is null)
                    {
                        await ReloadFeedAsync(user_id, key, ct);
                        continue;
                    }
                    var cachedFeed = JsonSerializer.Deserialize<List<Post>>(cachedFeedJson, jsonOptions)!;
                    if (message.ActionType == ActionTypeEnum.Delete)
                    {
                        Console.WriteLine($"Удаление поста {message.Post_id}");
                        var postForDelete = cachedFeed.FirstOrDefault(p => p.Post_id == message.Post_id);
                        if (postForDelete != null)
                        {
                            cachedFeed.Remove(postForDelete);
                            await cache.SetStringAsync(key, JsonSerializer.Serialize(cachedFeed, jsonOptions), ct);
                        }
                        continue;
                    }

                    var post = await postRepository.GetPostAsync(message.Post_id.Value)
                       ?? throw new Exception($"post {message.Post_id.Value} not found in db");

                    if (!cachedFeed.Any(f => f.Post_id == message.Post_id.Value) && message.ActionType == ActionTypeEnum.Create)
                    {
                        Console.WriteLine($"Добавление поста {message.Post_id}");
                        cachedFeed.Add(post);
                        if (cachedFeed.Count > 1000)
                        {
                            var oldestPost = cachedFeed.MinBy(f => f.Creation_datetime)!;
                            cachedFeed.Remove(oldestPost);
                        }
                        await cache.SetStringAsync(key, JsonSerializer.Serialize(cachedFeed, jsonOptions), ct);
                    }
                    else if (cachedFeed.Any(f => f.Post_id == message.Post_id.Value) && message.ActionType == ActionTypeEnum.Update)
                    {
                        Console.WriteLine($"Обновление поста {message.Post_id}");
                        var cachedPost = cachedFeed.First(p => p.Post_id == post.Post_id);
                        cachedPost = post;
                        await cache.SetStringAsync(key, JsonSerializer.Serialize(cachedFeed, jsonOptions), ct);
                    }
                    consumer.StoreOffset(consumerResult);
                }
                catch (Exception e)
                {
                    if (e is TaskCanceledException || e is OperationCanceledException)
                    {
                        Console.WriteLine("Shutdown");
                        break;
                    }
                    Console.WriteLine(e.ToString());
                }
            }
            consumer.Close();
        }

        private async Task ReloadFeedAsync(Guid user_id, string key, CancellationToken ct)
        {
            var feedFromDb = await postRepository.GetFeedAsync(user_id, 0, 1000);
            await cache.SetStringAsync(key, JsonSerializer.Serialize(feedFromDb, jsonOptions), ct);
        }

        private ConsumerConfig GetConsumerConfig()
        {
            return new ConsumerConfig
            {
                GroupId = "CaseUpdateService",
                EnableAutoOffsetStore = false,
                EnableAutoCommit = true,
                EnablePartitionEof = true,
                AllowAutoCreateTopics = true,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                BootstrapServers = options.Value.Host
            };
        }
    }
}
