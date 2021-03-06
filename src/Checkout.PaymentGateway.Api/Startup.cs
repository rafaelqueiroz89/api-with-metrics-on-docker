using System.Reflection;

using Checkout.PaymentGateway.CQRS.Commands;
using Checkout.PaymentGateway.DataModel;
using Checkout.PaymentGateway.Domain.Interfaces;
using Checkout.PaymentGateway.Infrastructure;
using Checkout.PaymentGateway.Infrastructure.Database;

using FluentValidation.AspNetCore;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using Prometheus;

namespace Checkout.PaymentGateway.Api
{
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddFluentValidation(opt =>
            {
                opt.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            }).AddNewtonsoftJson();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c => c.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "Payment Gateway",
                    Version = "v1",
                    Contact = new OpenApiContact()
                    {
                        Name = "Rafael Queiroz"
                    }
                }));

            services.AddMediatR(typeof(RequestPaymentCommand).Assembly);
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TimerPipeline<,>));
            services.AddScoped<IPaymentGatewayRepository, PaymentGatewayRepository>();
            services.AddScoped<IBankRepository, BankRepository>();
            services.AddTransient(typeof(PaymentGatewayContext));
            services.AddScoped<IPaymentGatewayDbContextUnitOfWork, PaymentGatewayDbContextUnitOfWork>();

            services.AddDbContext<PaymentGatewayContext>(options =>
                                                         options.UseInMemoryDatabase(databaseName: "PaymentGatewaySvc"));
        }

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        public void Configure(IApplicationBuilder app)
        {
            // Custom Metrics to count requests for each endpoint and the method
            var counter = Metrics.CreateCounter("gatewayapi_path_counter", "Counts requests", new CounterConfiguration
            {
                LabelNames = new[] { "method", "endpoint" }
            }); app.Use((context, next) =>
            {
                counter.WithLabels(context.Request.Method, context.Request.Path).Inc();
                return next();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Gateway");
            });

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseHttpMetrics();
            app.UseMetricServer();
        }
    }
}