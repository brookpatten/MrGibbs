using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Formatting;
using System.Web.Http;

using Owin;
using Ninject;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Models;

namespace MrGibbs.OnboardWebUI
{
    public class WebViewer:IViewer
    {
        private ILogger _logger;
        private IDisposable _webApp;
        private WebResolver _resolver;
        
		public WebViewer(ILogger logger, Action<Action<ISystemController, IRaceController>> queueCommand, IPlugin plugin,IKernel kernel)
        {
            //params to add:
            //wifi hostspot?
            //dhcp?
            //dns?

            //katana port
            //static content path


            _logger = logger;
            this.Plugin = plugin;

            //possibly do these as another plugin/dependency eg how bt or i2c works?
            //optionally configure wifi as hotspot
            //optionally configure dhcp
            //optionally configure dns

            //initialize owin/katana
			_webApp = WebApp.Start("http://*:9000",app=>
            {
                var webApiConfig = new System.Web.Http.HttpConfiguration();
                webApiConfig.MapHttpAttributeRoutes();
                webApiConfig.Formatters.Clear();
                webApiConfig.Formatters.Add(new JsonMediaTypeFormatter());
                webApiConfig.Formatters.JsonFormatter.SerializerSettings =
                                new JsonSerializerSettings
                                {
                                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                                };
                
				_resolver = new WebResolver (webApiConfig.DependencyResolver,logger, queueCommand, kernel);
				webApiConfig.DependencyResolver = _resolver;
                app.UseWebApi(webApiConfig);

				var fs = new PhysicalFileSystem("/home/brook/Desktop/MrGibbs/src/MrGibbs.OnboardWebUI/Web");
				app.UseFileServer(new FileServerOptions(){
					FileSystem = fs,
					RequestPath=new PathString(""),
					EnableDefaultFiles=true
				});


            });
            //initialize static content
            //initialize default page
            //initialize webapi

            //initialize websockets/signalr?

            //create a ninject kernel specifically for the webapp
            //add the logger and queue command, maybe state?
        }
        public void Update(State state)
        {
			_resolver.UpdateState (state);
            //somehow push updates via signal r?
            //or just an update to the web kernel state object?
        }
        public IPlugin Plugin { get; private set; }
        public void Dispose()
        {
            if(_webApp!=null)
            {
                _webApp.Dispose();
            }
			if(_resolver!=null)
            {
				_resolver.Dispose();
            }
            //shutdown owin/katana
            //shutdown dhcpdns/wifi?
        }
    }
}
