using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace api.rpi.gpio
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
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddJsonOptions(
                    options =>
                    {
                        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                    });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IApplicationLifetime lifetime)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseMvc();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });


            // Ja Warning!
            RunRequestsToSample(lifetime.ApplicationStopping);

            //GetLocalIPAddress();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }

        private static async Task RunRequestsToSample(CancellationToken token)
        {
            // Stuur elke minuut je ip adres op, voor als deze veranderd
            while (!token.IsCancellationRequested)
            {
                var ip = GetLocalIPAddress();

                using (HttpClient client = new HttpClient())
                {
                    var url = $"http://opvolger.net/online.php?ip={ip}";
                    await client.GetAsync(url, token);
                    Console.WriteLine($"{url}");
                }
                await Task.Delay(60000, token);
            }
        }

        public static string GetLocalIPAddress()
        {
            var firstUpInterfaces = NetworkInterface.GetAllNetworkInterfaces().Where(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up);
            foreach (var firstUpInterface in firstUpInterfaces)
            {
                    var props = firstUpInterface.GetIPProperties();
                    // get first IPV4 address assigned to this interface
                    var firstIpV4Address = props.UnicastAddresses
                        .Where(c => c.Address.AddressFamily == AddressFamily.InterNetwork)
                        .Select(c => c.Address)
                        .FirstOrDefault();
                    var ip = firstIpV4Address;
                if (ip != null && !ip.ToString().StartsWith("10") && !ip.ToString().StartsWith("127") && !ip.ToString().EndsWith(".1"))
                {
                    Console.WriteLine($"ip: {ip?.ToString()}");
                    return ip?.ToString();
                }
                Console.WriteLine($"ip niet goed: {ip?.ToString()}");
            }


            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                Console.WriteLine($"AddressFamily: {ip.AddressFamily}");
                Console.WriteLine($"ip: {ip.ToString()}");
                if (ip.AddressFamily == AddressFamily.InterNetwork && !ip.ToString().StartsWith("10"))
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
