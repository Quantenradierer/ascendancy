﻿
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Threading;

namespace server
{
	public class MvcApplication : System.Web.HttpApplication
	{
		public enum Phases
		{
			Started,
			Init,
			Running,
			Exit,
		}

		public static Phases Phase = Phases.Started; 


		public static void RegisterRoutes (RouteCollection routes)
		{
			routes.IgnoreRoute ("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.config/{*pathInfo}");
            routes.IgnoreRoute("{resource}.xml/{*pathInfo}");

			routes.MapRoute (
				"Login",                                           
				"Login",
				new { controller = "HTTP", action = "Login" }  
			);

			routes.MapRoute (
				"LoadRegions",                                           
				"LoadRegions",
				new { controller = "HTTP", action = "LoadRegions" }  
			);

			routes.MapRoute (
				"DoActions",                                           
				"DoActions",
				new { controller = "HTTP", action = "DoActions" }  
			);


			routes.MapRoute (
				"Default",
				"{controller}",
				new { controller = "HTTP", action = "Error" }
			);

		}
		
		public static void RegisterGlobalFilters (GlobalFilterCollection filters)
		{
			filters.Add (new HandleErrorAttribute ());
		}

		protected void Application_Start ()
		{
			Phase = Phases.Init;
			var world = @base.model.World.Instance;
			var controller = @base.control.Controller.Instance;

			var api = server.control.APIController.Instance;

			var regionManagerLastC = new control.RegionManagerController (null, world.RegionStates.Last);
			var regionManagerCurrC = new control.RegionManagerController (regionManagerLastC, world.RegionStates.Curr);
			var regionManagerNextC = new control.RegionManagerController (regionManagerCurrC, world.RegionStates.Next);

			controller.RegionStatesController = new @base.control.RegionStatesController (regionManagerLastC,
																						  regionManagerCurrC,
																						  regionManagerNextC);
			controller.DefinitionManagerController = new server.control.DefinitionManagerController ();
			controller.AccountManagerController = new server.control.AccountManagerController ();


			for (int Index = 0; Index < model.ServerConstants.ACTION_THREADS; ++Index)
			{
				ThreadPool.QueueUserWorkItem (new WaitCallback (server.control.APIController.Instance.Worker));
			}
				
			var cleanC = new @server.control.CleaningController ();
			ThreadPool.QueueUserWorkItem (new WaitCallback (cleanC.Run));

			Phase = Phases.Running;

			AreaRegistration.RegisterAllAreas ();
			RegisterGlobalFilters (GlobalFilters.Filters);
			RegisterRoutes (RouteTable.Routes);

		}
	}
}
