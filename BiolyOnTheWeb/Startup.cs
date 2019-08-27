using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System;

namespace BiolyOnTheWeb
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<WebUpdater>(new Func<IServiceProvider, WebUpdater>(x => new WebUpdater((IJSRuntime)x.GetService(typeof(IJSRuntime)))));
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
