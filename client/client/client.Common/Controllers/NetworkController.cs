﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ModernHttpClient;
using XLabs.Platform.Services.Geolocation;

using Core.Models.Definitions;
using Core.Models;
using client.Common.Helper;
using System.Collections.Generic;



namespace client.Common.Controllers
{
    public sealed class NetworkController
    {
        #region Singleton

        /// <summary>
        /// The lazy singleton.
        /// </summary>
        private static readonly Lazy<NetworkController> lazy =
            new Lazy<NetworkController>(() => new NetworkController());

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static NetworkController Instance { get { return lazy.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="client.Common.Controllers.NetworkController"/> class.
        /// </summary>
        private NetworkController()
        {
            ExceptionMessage = "";
            JsonTerrainsString = "";
            m_client = new HttpClient(new NativeMessageHandler());
            m_sessionID = Guid.Empty;
        }

        #endregion

        #region Networking

        /// <summary>
        /// Gets the exception message.
        /// </summary>
        /// <value>The exception message.</value>
        public string ExceptionMessage
        {
            get;
            private set; 
        }

        /// <summary>
        /// Gets the json terrains string.
        /// </summary>
        /// <value>The json terrains string.</value>
        public string JsonTerrainsString
        {
            get; 
            private set;
        }

        /// <summary>
        /// Gets the json terrain type string.
        /// </summary>
        /// <value>The json terrain type string.</value>
        public string JsonTerrainTypeString
        {
            get; 
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is logedin.
        /// </summary>
        /// <value><c>true</c> if this instance is logedin; otherwise, <c>false</c>.</value>
        public bool IsLoggedIn
        {
            get
            {
                if (m_sessionID != Guid.Empty)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Loads the terrains async and write it in property JsonTerrainString.
        /// </summary>
        /// <param name="jsonRegionServerPath">Json region server path.</param>
        public async Task LoadTerrainsAsync(string jsonRegionServerPath)
        {
            try
            {
                HttpResponseMessage response = await m_client.GetAsync(new Uri(jsonRegionServerPath));
                if (response != null)
                {
                    response.EnsureSuccessStatusCode();
                    JsonTerrainsString = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
                throw ex;
            }
        }

        /// <summary>
        /// Loads the terrain types async and write it in property JsonTerrainTypString
        /// </summary>
        /// <param name="jsonTerrainTypeServerPath">Json terrain type server path.</param>
        public async Task LoadTerrainTypesAsync(string jsonTerrainTypeServerPath)
        {
            try
            {
                HttpResponseMessage response = await m_client.GetAsync(new Uri(jsonTerrainTypeServerPath));
                if (response != null)
                {
                    response.EnsureSuccessStatusCode();
                    JsonTerrainTypeString = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
                throw ex;
            }

        }

        /// <summary>
        /// Login async to the server and save the sessionID.
        /// </summary>
        /// <returns>The AccoountId.</returns>
        /// <param name="currentGamePosition">Current game position.</param>
        /// <param name="user">User.</param>
        /// <param name="password">Password.</param>
        public async Task<Account> LoginAsync(Core.Models.Position currentGamePosition, string user, string password)
        {
            try
            {
                Account account = null;
                var request = new @base.connection.LoginRequest(currentGamePosition, user, password);
                var json = JsonConvert.SerializeObject(request);

                var path = ClientConstants.LOGIN_PATH;
                path = path.Replace(ClientConstants.LOGIC_SERVER_JSON, json);

                HttpResponseMessage response = await m_client.GetAsync(new Uri(ClientConstants.LOGIC_SERVER + path));
                if (response != null)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonFromServer = await response.Content.ReadAsStringAsync();

                    var loginResponse = JsonConvert.DeserializeObject<@base.connection.LoginResponse>(jsonFromServer);
                    if (loginResponse.Status == @base.connection.LoginResponse.ReponseStatus.OK)
                    {
                        m_sessionID = loginResponse.SessionID;
                        account = new Account(loginResponse.AccountId, user);
                    }
                    else
                    {
                        ExceptionMessage = "Login failure";
                    }

                }
                return account;
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
                throw ex;
            }
        }

        /// <summary>
        /// Loads the entities async.
        /// </summary>
        /// <returns>The entities async.</returns>
        /// <param name="currentGamePosition">Current game position.</param>
        /// <param name="regionPositions">Region positions.</param>
        public async Task<@base.connection.Response> LoadEntitiesAsync(Core.Models.Position currentGamePosition, RegionPosition[] regionPositions)
        {
            try
            {
                var request = new @base.connection.LoadRegionsRequest(m_sessionID, currentGamePosition, regionPositions);
                var json = JsonConvert.SerializeObject(request);

                var path = ClientConstants.LOAD_REGIONS_PATH;
                path = path.Replace(ClientConstants.LOGIC_SERVER_JSON, json);

                HttpResponseMessage response = await m_client.GetAsync(new Uri(ClientConstants.LOGIC_SERVER + path));
                if (response != null)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonFromServer = await response.Content.ReadAsStringAsync();

                    var entitiesResponse = JsonConvert.DeserializeObject<@base.connection.Response>(jsonFromServer);

                    if (entitiesResponse.Status == @base.connection.Response.ReponseStatus.OK)
                    {
                        return entitiesResponse;
                    }

                }
                return new @base.connection.Response();

            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
                throw ex;
            }
        }

        public async Task<bool> DoActionsAsync(Core.Models.Position currentGamePosition, Core.Models.Action[] actions)
        {
            try
            {
                var request = new @base.connection.DoActionsRequest(m_sessionID, currentGamePosition, actions);
                var json = JsonConvert.SerializeObject(request);

                var path = ClientConstants.DO_ACTIONS_PATH;
                path = path.Replace(ClientConstants.LOGIC_SERVER_JSON, json);

                HttpResponseMessage response = await m_client.GetAsync(new Uri(ClientConstants.LOGIC_SERVER + path));
                if (response != null)
                {
                    response.EnsureSuccessStatusCode();

                    var jsonFromServer = await response.Content.ReadAsStringAsync();

                    var entitiesResponse = JsonConvert.DeserializeObject<@base.connection.Response>(jsonFromServer);
                    if (entitiesResponse.Status == @base.connection.Response.ReponseStatus.OK)
                    {
                        return true;
                    }

                }
                return false;

            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
                throw ex;
            }
        }




        #endregion

        #region private Fields

        private HttpClient m_client;
        private Guid m_sessionID;

        #endregion
    }
}

