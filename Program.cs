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

namespace photo_api
{
    public class Program
    {
        public static void Main(string[] args)
        {       
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hc, l) =>
                { 
                    l.AddConsole();
                })
                .UseStartup<Startup>()
                .UseKestrel(options=>
                {
                    options.Listen(IPAddress.Any, 5000);
                    // options.Listen(IPAddress.Loopback, 5001, listenOptions => 
                    // {
                    //     listenOptions.UseHttps("localhost.pfx");
                    // });
                })
                .Build();
    }
}
