using MassTransit;

namespace Coach.API.Consumers
{
    public class ConsumeConfigurator
    {
        private readonly Action<IBusRegistrationContext> _configurator;

        public ConsumeConfigurator(Action<IBusRegistrationContext> configurator)
        {
            _configurator = configurator;
        }

        public void ConfigureEndpoints(IBusRegistrationContext context)
        {
            _configurator(context);
        }
    }
}