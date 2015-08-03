﻿using Core.Models;
using Core.Models.Definitions;
using Client.Common.Helper;
using Client.Common.Controllers;
using Client.Common.Models;
using System.Threading.Tasks;


namespace Client.Common.Manager
{
    /// <summary>
    /// The Region manager controller to control the regions.
    /// </summary>
    public class RegionManagerController : Core.Controllers.RegionManagerController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Client.Common.Manager.RegionManagerController"/> class.
        /// </summary>
        public RegionManagerController()
        {
        }

        #region Regions

        /// <summary>
        /// Gets the region by current geolocation.
        /// </summary>
        /// <returns>The region by current geolocation.</returns>
        public Region GetRegionByGeolocator()
        {
            var geolocationPosition = Geolocation.Instance.CurrentGamePosition;
            return GetRegionByGamePosition(geolocationPosition);
        }

        /// <summary>
        /// Gets the region by game position.
        /// </summary>
        /// <returns>The region by game position.</returns>
        /// <param name="gameWorldPosition">Game world position.</param>
        public Region GetRegionByGamePosition(Position gameWorldPosition)
        {
            RegionPosition regionPosition = new RegionPosition(gameWorldPosition);
            return GetRegion(regionPosition);
        }

        /// <summary>
        /// Gets the region.If needed loads the region.
        /// </summary>
        /// <returns>The region.</returns>
        /// <param name="regionPosition">Region position.</param>
        public override Region GetRegion(RegionPosition regionPosition)
        {
            var region = World.Instance.RegionManager.GetRegion(regionPosition);

            if (!region.Exist)
            {
                LoadRegionAsync(region);
            }
				
            return region;
        }

        /// <summary>
        /// Loads the terrain async over the network controller and adds the region to the world.
        /// </summary>
        /// <returns>The task async.</returns>
        /// <param name="region">Region.</param>
        private async Task LoadRegionAsync(Region region)
        {
            TerrainDefinition[,] terrain = await NetworkController.Instance.LoadTerrainsAsync(region.RegionPosition);

            if (terrain != null)
                region.AddTerrain(terrain);

            try
            {
                World.Instance.RegionManager.AddRegion(region);
            }
            catch
            {
                if (null != null)
                {
                }
            }
        }

        /// <summary>
        /// Loads the regions async around the current region position by the geolocator.
        /// </summary>
        /// <returns>The task async.</returns>
        public async Task LoadRegionsAsync()
        {
            await LoadRegionsAsync(Geolocation.Instance.CurrentRegionPosition);
        }

        /// <summary>
        /// Loads the regions async around the surrender region position.
        /// </summary>
        /// <returns>The task async.</returns>
        /// <param name="regionPosition">Region position.</param>
        public async Task LoadRegionsAsync(RegionPosition regionPosition)
        {
            var WorldRegions = GetWorldNearRegionPositions(regionPosition);

            foreach (var RegionPosition in WorldRegions)
            {
                var region = World.Instance.RegionManager.GetRegion(RegionPosition);

                if (!region.Exist)
                {
                    await LoadRegionAsync(region);
                }
            }
        }

        /// <summary>
        /// Dos the action async.
        /// Send the actions to the server and load the response.
        /// </summary>
        /// <returns>True once the action is done.</returns>
        /// <param name="currentGamePosition">Current game position.</param>
        /// <param name="actions">Actions.</param>
        public async Task<bool> DoActionAsync(Core.Models.Position currentGamePosition, Core.Models.Action[] actions)
        {
            await NetworkController.Instance.DoActionsAsync(currentGamePosition, actions);
            await EntityManagerController.Instance.LoadEntitiesAsync(currentGamePosition, currentGamePosition.RegionPosition);
            return true;
        }


        #endregion

        #region RegionPositions

        /// <summary>
        /// Gets the world near the region position. Around 5x5 regions at the region position.
        /// </summary>
        /// <returns>The regions around the positions.</returns>
        /// <param name="regionPosition">Region position.</param>
        public RegionPosition[,] GetWorldNearRegionPositions(RegionPosition regionPosition)
        {
            int halfX = ClientConstants.DRAW_REGIONS_X / 2;
            int halfY = ClientConstants.DRAW_REGIONS_X / 2;

            RegionPosition[,] worldRegion = new RegionPosition[5, 5];
            for (int x = -halfX; x <= halfX; x++)
            {
                for (int y = -halfY; y <= halfY; y++)
                {
                    worldRegion[x + halfX, y + halfY] = new RegionPosition(regionPosition.RegionX + x,
                        regionPosition.RegionY + y);
                }
            }

            return worldRegion;
        }

        #endregion

        #region private Fields


        #endregion
    }
}

