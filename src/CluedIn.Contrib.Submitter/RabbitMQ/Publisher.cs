using Castle.Windsor;
using CluedIn.Core;
using CluedIn.Core.Agent;
using CluedIn.Core.Bus;
using CluedIn.Core.Data;
using CluedIn.Core.Messages.Processing;
using CluedIn.Core.Serialization;
using EasyNetQ;
using EasyNetQ.DI;

namespace CluedIn.Contrib.Submitter.RabbitMQ;

public static class Publisher
{
    private static readonly ApplicationContext s_applicationContext = new(new WindsorContainer());

    private static IBus CreateBus()
    {
        return RabbitHutch.CreateBus(
            Environment.GetEnvironmentVariable("RABBITMQ_CONNECTION_STRING"),
            register => { register.Register<ISerializer, EasyNetQMessageSerializer>(); });
    }

    public static async Task Publish(this Clue clue, Context context)
    {
        context.Bus ??= CreateBus();

        var command = new ProcessPrioritizedClueCommand(
            JobRunId.Empty,
            CompressedClue.Compress(clue, s_applicationContext));
        await context.Bus.PubSub.PublishAsync(command, command.GetType())
            .ConfigureAwait(false);
    }
}
