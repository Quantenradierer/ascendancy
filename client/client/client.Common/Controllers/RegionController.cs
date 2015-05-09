﻿using System;
using @base.model;
using @base.control;
using client.Common.helper;
using System.Threading.Tasks;
using client.Common.controller;
using CocosSharp;
using @base.model.definitions;
using client.Common.Helper;

namespace client.Common.Controllers
{
	public class RegionController : RegionManagerController
	{
		public RegionController ()
		{
			m_networkController = NetworkController.GetInstance;
			m_geolocation = Geolocation.GetInstance;
			m_terrainController = Controller.Instance.TerrainManagerController as TerrainController;

		}


		#region Region

		public Region GetRegionByGeolocator ()
		{
			var geolocationPosition = m_geolocation.CurrentGamePosition;
			return GetRegionByGamePosition (geolocationPosition);
		}

		public Region GetRegionByGamePosition (Position gameWorldPosition)
		{
			RegionPosition regionPosition	= new RegionPosition (gameWorldPosition);
			return GetRegion (regionPosition);
		}

		private async Task LoadRegionAsync (Region region)
		{
			string path = ReplacePath (ClientConstants.REGION_SERVER_PATH, region.RegionPosition);
			TerrainDefinition[,] terrain = null;

			await m_networkController.LoadTerrainsAsync (path);
			if (m_terrainController.TerrainDefinitionCount > 12)
				terrain = JsonToTerrain (m_networkController.JsonTerrainsString);

			if (terrain != null)
				region.AddTerrain (terrain);
			RegionManager.AddRegion (region);
		}


		public void SetTilesInMap (CCTileMapLayer mapLayer, CCTileMapCoordinates mapUpperLeftCoordinate, Region region)
		{
			for (int x = 0; x < Constants.REGION_SIZE_X; x++) {
				for (int y = 0; y < Constants.REGION_SIZE_Y; y++) {
					SetTileInMap (mapLayer, new CellPosition (region.RegionPosition.RegionX + x, region.RegionPosition.RegionY + y)
						, Modify.MapCellPosToTilePos ((mapUpperLeftCoordinate.Column + x), (mapUpperLeftCoordinate.Row + y)), region);
				}
			}
		}

		public void SetTileInMap (CCTileMapLayer mapLayer, CellPosition cellPosition, CCTileMapCoordinates mapCoordinat, Region region)
		{
			var gid = m_terrainController.TerrainDefToTileGid (region.GetTerrain (cellPosition));
			mapLayer.SetTileGID (gid, mapCoordinat);
		}

		public void SetEntitysInMap (CCTileMapLayer mapLayer, CCTileMapCoordinates mapUpperLeftCoordinate, Region region)
		{
			for (int x = 0; x < Constants.REGION_SIZE_X; x++) {
				for (int y = 0; y < Constants.REGION_SIZE_Y; y++) {
					SetEntityInMap (mapLayer, new CellPosition (region.RegionPosition.RegionX + x, region.RegionPosition.RegionY + y)
						, Modify.MapCellPosToTilePos ((mapUpperLeftCoordinate.Column + x), (mapUpperLeftCoordinate.Row + y)), region);
				}
			}
		}

		public void SetEntityInMap (CCTileMapLayer mapLayer, CellPosition cellPosition, CCTileMapCoordinates mapCoordinat, Region region)
		{
			// TODO build EntityController and EntityDefToEntityGid
			//var gid = m_terrainController.TerrainDefToTileGid (region.GetEntity (cellPosition));
			//mapLayer.SetTileGID (gid, mapCoordinat);

			throw new NotImplementedException ();
		}


		public override Region GetRegion (RegionPosition regionPosition)
		{
			var region = RegionManager.GetRegion (regionPosition);
			if (!region.Exist) {
				LoadRegionAsync (region);
			}

			return region;
		}

		#endregion

		#region private Fields

		private NetworkController m_networkController;
		private Geolocation m_geolocation;
		private TerrainController m_terrainController;

		#endregion
	}
}

