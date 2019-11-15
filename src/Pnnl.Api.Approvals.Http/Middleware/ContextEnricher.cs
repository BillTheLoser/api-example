using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Api.Approvals.Http.Extensions;

namespace Pnnl.Api.Approvals.Http.Middleware
{
    /// <summary>
    /// Represents a middleware component responsible for enriching an HTTP request with user information.
    /// </summary>
    public class ContextEnricher
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IPersonStore _store;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextEnricher"/> class.
        /// </summary>
        /// <param name="next">The HTTP request delegate.</param>
        /// <param name="loggerFactory">The factory used to create loggers.</param>
        /// <param name="store">The repository used to retrieve aggregate profiles.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="next"/> request delegate is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="loggerFactory"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">The profile <paramref name="store"/> is <see langword="null"/>.</exception>
        public ContextEnricher(RequestDelegate next, ILoggerFactory loggerFactory, IPersonStore store)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _next = next ?? throw new ArgumentNullException(nameof(next));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _logger = loggerFactory.CreateLogger<ContextEnricher>();
        }

        /// <summary>
        /// Asynchronously invokes this middleware using the specified HTTP <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The HTTP-specific information about the current HTTP request.</param>
        /// <returns>A <see cref="Task"/> that represents the pending operation.</returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (context.User.Identity.IsAuthenticated)
                {
                    // Resolve network identity from claims identity
                    string domain = context.User.Identity.Name.Contains("\\")
                        ? context.User.Identity.Name.Split("\\".ToCharArray()).First()
                        : "PNL";

                    string username = context.User.Identity.Name.Contains("\\")
                        ? context.User.Identity.Name.Split("\\".ToCharArray()).Last()
                        : context.User.Identity.Name;

                    // Retrieve user using network identity
                    var user = await _store.GetByNetworkIdAsync(domain, username, context.RequestAborted);

                    // Hackity Hack Hack Hack
                    if(user == null)
                    {
                        if(username.StartsWith("msa"))
                        {
                            user = new Operations.Person();
                            user.Network = new Operations.NetworkIdentifier();
                            user.Network.Domain = domain;
                            user.Network.Username = username;
                        }

                    }

                    if (user == null)
                    {
                        using (var ctx = new PrincipalContext(ContextType.Domain, domain))
                        {
                            Principal person = null;
                            try
                            {
                                person = Principal.FindByIdentity(ctx, username);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error retrieving identity {context.User.Identity.Name}. Reason: {ex}");
                            }

                            if (person != null)
                            {
                                try
                                {
                                    if (person.GetUnderlyingObject() is DirectoryEntry personObject &&
                                        personObject.Properties.Contains("extensionAttribute2"))
                                    {
                                        if (personObject.Properties["extensionAttribute2"].Value is string hanfordId)
                                        {
                                            try
                                            {
                                                user = await _store.GetByIdAsync(hanfordId, context.RequestAborted);
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(
                                                    "Error retrieving user with Hanford Id {HanfordId}. Reason: {@Exception}",
                                                    hanfordId, ex);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(
                                        "Error retrieving Hanford Id for {WindowsIdentityName}. Reason: {@Exception}",
                                        context.User.Identity.Name, ex);
                                }                                
                            }
                        }
                    }

                    if (user == null)
                    {
                        _logger.LogError(
                            "<{MiddlewareType}> Unable to find user for identity {NetworkDomain}\\{NetworkUserName}",
                            GetType(), domain, username);

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }

                    // Add user profile to scope of request
                    context.Items[HttpContextItemKeys.ProcessApi.User] = user;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"{GetType()} Middleware failure. Reason: {exception}");
            }

            await _next.Invoke(context);
        }
    }
}
