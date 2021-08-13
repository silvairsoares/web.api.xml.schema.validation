using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using web.api.xml.schema.validation.Services.InterfacesServicos;
using web.api.xml.schema.validation.Services.Servicos;

namespace web.api.xml.schema.validation
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configurações da injeção de dependência do serviço validador de documentos XML
            services.AddScoped(typeof(IXMLValidationService), typeof(XMLValidationService));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "web.api.xml.schema.validation", Version = "v1" });
            });

            //Inclusão do framework de serialização NewtonSoft
            services.AddControllers().AddNewtonsoftJson();

            //Ajuste para permitir receber conteudos do tipo XML nos controllers da API
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson
                    (options => { }).AddXmlSerializerFormatters()
              .AddXmlDataContractSerializerFormatters();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "web.api.xml.schema.validation v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
