using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using SharedLibrary.StaticFiles;

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
                .UseKestrel(options =>
                {
                    // With debug configuration the usage of HTTPS protocol depends on the settings
                    // of Constants.UseHttps variable, but with release configuration HTTPS
                    // protocol is disabled
                    #if DEBUG
                        if (Constants.UseHttps)
                        {
                            // HTTPS on standard HTTPS port 443
                            options.Listen(IPAddress.Any, 443, listenOptions => 
                            {
                                listenOptions.NoDelay = false;
                                // Certificate
                                listenOptions.UseHttps(Constants.HttpsCertificatePath, Constants.HttpsCertificatePassword);
                            });
                        }
                        else
                        {
                            // HTTP on standard HTTP port 80
                            options.Listen(IPAddress.Any, 80);
                        }
                    #else
                        // HTTP on standard HTTP port 80
                        options.Listen(IPAddress.Any, 80);
                    #endif

                })
                .UseStartup<Startup>()
                .Build();
    }
}
