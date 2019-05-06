using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                // .UseKestrel(options =>
                // {
                //     // Easy mode (http only)
                //     options.Listen(IPAddress.Any, 80); // HTTP port

                //     // Verbose
                //     options.Listen(IPAddress.Any, 443, listenOptions => // HTTPS port
                //     {
                //         listenOptions.NoDelay = false;
                //         // Enable https
                //         listenOptions.UseHttps("/Users/sapoi/Desktop/tmp_https_certificate/certificate.pfx", "PEMpass");
                //     });
                // })
                .UseStartup<Startup>()
                .Build();
    }
}
