﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace server
{
	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterRoutes (RouteCollection routes)
		{
//			routes.IgnoreRoute ("{resource}.axd/{*pathInfo}");
//            routes.IgnoreRoute("{resource}.config/{*pathInfo}");

			routes.MapRoute (
				"Default",
				"{controller}/{action}",
				new { controller = "Login", action = "LoginID"}
			);

		}

		public static void RegisterGlobalFilters (GlobalFilterCollection filters)
		{
			filters.Add (new HandleErrorAttribute ());
		}

		protected void Application_Start ()
		{
            //Database.SetInitializer(new ASDInitializer());

			AreaRegistration.RegisterAllAreas ();
			RegisterGlobalFilters (GlobalFilters.Filters);
			RegisterRoutes (RouteTable.Routes);
		}
	}
}
