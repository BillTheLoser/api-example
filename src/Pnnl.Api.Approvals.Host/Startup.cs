using GraphQL.Http;
using GraphQL.Server.Ui.Playground;
using GraphQL.Types;
using GraphQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Pnnl.Api.Approvals.Data.Http;
using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Api.Approvals.Data;
using Pnnl.Api.Approvals.Graphs.InputTypes;
using Pnnl.Api.Approvals.Graphs;
using Pnnl.Api.Approvals.Http.Extensions;
using Pnnl.Api.Approvals.Http.Middleware.GraphQL;
using Pnnl.Api.Approvals.Queries;
using Pnnl.AppCore.Data.Sql.DependencyInjection;
using Pnnl.AppCore.Data.Sql;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using GraphiQl;
using Microsoft.OpenApi.Models;
using System.Net.Http;
using System.Net;

namespace Pnnl.Api.Approvals.Host
{
    /// <summary>
    /// A class to define the request handling pipeling and configure services needed by the application.
    /// </summary>
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
              .AddControllers();

            services.AddMemoryCache();

                //Configure JSON settings
                services.AddMvc()
                    .AddNewtonsoftJson();

            // Add swagger services
            services.AddSwaggerGen(options =>
            {
                var section = Configuration.GetSection("Swagger");

                options.SwaggerDoc(
                    section["Version"],
                    new OpenApiInfo { Title = section["Title"], Version = section["Version"] }
                );

                var path = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, section["XmlComments"]);
                options.IncludeXmlComments(path);
            });

            // Add Windows Authentication 
            services.AddAuthentication(IISDefaults.AuthenticationScheme);

            services
                .AddSql()
                .ConfigureConnection("raa", Configuration.GetSection("data.sql").GetSection("raa"));

            // Add data providers
            services
                // Add MS SQL connections. Retrieves connection string from appsettings.json
                .AddSqlProcessStore(Configuration.GetSection("raa"))
                .AddSqlPersonIdentificationStore(Configuration.GetSection("raa"))
                .AddSqlActivityStore(Configuration.GetSection("raa"))
                .AddSqlSecurityStore(Configuration.GetSection("raa"))
                .AddSqlProcessNodeStore(Configuration.GetSection("raa"))
                // Adds Http clients configured in appsettings.
                //.AddHttpClients(Configuration.GetSection("Http"))
                .AddProcessFacade()
                .AddActivityFacade();

            // Add Http client
            services.AddHttpClient("RaaWebService", client =>
            {
                client.BaseAddress = new Uri(Configuration.GetSection("Http:RaaWebService:BaseAddress").Value);
            }).ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler
            {
                UseDefaultCredentials = true,
                Credentials = CredentialCache.DefaultNetworkCredentials
            });

            services.AddHttpClient("opwhse", client =>
            {
                client.BaseAddress = new Uri(Configuration.GetSection("Http:opwhse:BaseAddress").Value);
            }).ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler
            {
                UseDefaultCredentials = true,
                Credentials = CredentialCache.DefaultNetworkCredentials
            });

            // Add application specific stores
            services.Configure<HttpPersonStoreOptions>(Configuration.GetSection("OpwhseStore"));
            services.Configure<HttpRouteItemStoreOptions>(Configuration.GetSection("HttpRouteItemStore"));
            services.AddTransient<IApprovalsLegacyStore, HttpApprovalsLegacyStore>();
            services.AddTransient<IPersonStore, HttpPersonStore>();

            services.AddTransient<IContextPersonStore, ContextPersonStoreBase>();

            // Add GraphQL services
            ConfigureGraphQL(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            app.UseHttpsRedirection();

            app.UseRouting();

            app
                // Enable Swagger UI
                .UseSwagger()
                .UseSwaggerUI(options =>
                {
                    var section = Configuration.GetSection("Swagger");
                    options.SwaggerEndpoint(section["Endpoint"], section["Title"]);
                   // options.RoutePrefix = string.Empty;
                });

            // Enable CORS
            app.UseCors(policy =>
                       policy
                           .SetIsOriginAllowedToAllowWildcardSubdomains()
                           .WithOrigins(
                                   "https://*.pnl.gov",
                                   "https://*.pnnl.gov",
                                   "http://localhost:3000")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials()
                           .Build());

            app.UseAuthorization()
                .UseUserEnrichment()

            .UseMiddleware<GraphQLMiddleware>(new GraphQLSettings
            {
                BuildUserContext = ctx => new GraphQLUserContext
                {
                    User = ctx.GetUser()
                }
            })

            //.UseGraphiql("/ui/graphql", options => { options.GraphQlEndpoint = "/graphql"; })

            //.UseGraphQLPlayground(new GraphQLPlaygroundOptions()
            //{
            //    Path = "/ui/playground",
            //    GraphQLEndPoint = "/playground"
            //})

            .UseGraphQLPlayground(new GraphQLPlaygroundOptions()
            {
                Path = "/playground"
            })

            .UseGraphiQl();

              app.UseEndpoints(endpoints =>
              {
                  endpoints.MapControllers();
              });
        }

        private void ConfigureGraphQL(IServiceCollection services)
        {
            services.AddSingleton<IDependencyResolver>(p => new FuncDependencyResolver(p.GetRequiredService));
            services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
            services.AddSingleton<IDocumentWriter, DocumentWriter>();

            // Register all graphs, queries, inputs
            services
              // Queries
              .AddSingleton<ApprovalsQuery>()
              // Graphs
              .AddSingleton<ActivityGraph>()
              .AddSingleton<ActorGraph>()
              .AddSingleton<ProcessGraph>()
              .AddSingleton<UserGraph>()
              .AddSingleton<ProcessNodeGraph>()
              .AddSingleton<PagedProcessNodeResultGraph>()
              .AddSingleton<PageInfoGraph>()
              // Inputs
              .AddSingleton<ActivityStateInput>()
              .AddSingleton<ActorActionTakenEnumInput>()
              .AddSingleton<DateRangeInput>()
              .AddSingleton<ProcessFilterInput>()

              .AddSingleton<ISchema, ApprovalsSchema>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}



