﻿using Core.Controllers.Actions;
using Core.Models;
using Client.Common.Helper;
using Client.Common.Models;
using Client.Common.Manager;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using CocosSharp;
using Microsoft.Xna.Framework;


namespace Client.Common.Views
{
    /// <summary>
    /// The World layer.
    /// </summary>
    public class WorldLayer : CCLayerColor
    {
        /// <summary>
        /// Load phases.
        /// </summary>
        public enum Phases
        {
            Start,
            LoadTerrain,
            LoadEntities,
            Idle,
            Exit
        }

        /// <summary>
        /// Gets the load phase.
        /// </summary>
        /// <value>The phase.</value>
        public Phases Phase
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the region view.
        /// </summary>
        /// <value>The region view.</value>
        public RegionView RegionView
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the center position.
        /// </summary>
        /// <value>The center position.</value>
        public Position CenterPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client.Common.Views.WorldLayer"/> class.
        /// </summary>
        /// <param name="gameScene">Game scene.</param>
        public WorldLayer(GameScene gameScene)
            : base()
        {
            m_gameScene = gameScene;

            RegionView = new RegionView();
            m_regionManagerController = Core.Controllers.Controller.Instance.RegionManagerController as Client.Common.Manager.RegionManagerController;

            WorldTileMap = new CCTileMap(ClientConstants.TILEMAP_FILE);
            CenterPosition = Geolocation.Instance.CurrentGamePosition;

            m_currentPositionNode = new DrawNode();
            WorldTileMap.TileLayersContainer.AddChild(m_currentPositionNode);

            TerrainLayer = WorldTileMap.LayerNamed(ClientConstants.LAYER_TERRAIN);
            BuildingLayer = WorldTileMap.LayerNamed(ClientConstants.LAYER_BUILDING);
            UnitLayer = WorldTileMap.LayerNamed(ClientConstants.LAYER_UNIT);
            MenuLayer = WorldTileMap.LayerNamed(ClientConstants.LAYER_MENU);

            ClearLayers();

            RegionView.TerrainLayer = TerrainLayer;
            RegionView.BuildingLayer = BuildingLayer;
            RegionView.UnitLayer = UnitLayer;
            RegionView.MenuLayer = MenuLayer;


           
            this.AddChild(WorldTileMap);


            m_worker = new Views.Worker(this);
            EntityManagerController.Worker = m_worker;

            Schedule(m_worker.Schedule);
            Schedule(CheckGeolocation);


        }

        /// <summary>
        /// Clears the Layers for initialization.
        /// </summary>
        void ClearLayers()
        {
            var coordHelper = new CCTileMapCoordinates(0, 0);
            BuildingLayer.RemoveTile(coordHelper);
            UnitLayer.RemoveTile(coordHelper);
            MenuLayer.RemoveTile(coordHelper);
        }


        #region overide

        /// <summary>
        /// Add the tilemap to scene.
        /// </summary>
        protected override void AddedToScene()
        {
            base.AddedToScene();

            SetMapAnchor(Geolocation.Instance.CurrentGamePosition);
            WorldTileMap.TileLayersContainer.PositionX = VisibleBoundsWorldspace.MidX;
            WorldTileMap.TileLayersContainer.PositionY = VisibleBoundsWorldspace.MidY;
            ScaleWorld(ClientConstants.TILEMAP_NORM_SCALE);

        }

        #endregion


        /// <summary>
        /// Move the tilelayer position a little bit.
        /// </summary>
        public void UglyDraw()
        {
            //TODO: find better solution
            WorldTileMap.TileLayersContainer.Position += new CCPoint(0.0001f, 0.0001f);
        }


        #region Scheduling

        /// <summary>
        /// Checks the geolocation. If The geolocation is changed draw the regions new.
        /// </summary>
        /// <param name="frameTimesInSecond">Frame times in second.</param>
        void CheckGeolocation(float frameTimesInSecond)
        {
            if (Geolocation.Instance.IsPositionChanged)
            {
                DrawRegionsAsync(Geolocation.Instance.CurrentGamePosition);
                Geolocation.Instance.IsPositionChanged = false;
            }

        }

        #endregion

        #region

        /// <summary>
        /// Sets the current position once in the tilemap by draw a red hexagon on the current position.
        /// </summary>
        /// <param name="position">Position.</param>
        void SetCurrentPositionOnce(Position position)
        {
            var tileCoordinate = Helper.PositionHelper.PositionToTileMapCoordinates(CenterPosition, new PositionI(position));
            m_currentPositionNode.DrawHexagonForIsoStagMap(ClientConstants.TILE_IMAGE_WIDTH, TerrainLayer,
                tileCoordinate, new CCColor4F(CCColor3B.Red), 255, 3.0f);
//            var tileCoordinate = m_regionView.GetCurrentTileInMap (m_geolocation.CurrentGamePosition);

            bool isInWorld = false;
            m_currentPositionNode.Visible = false;

            if (CenterPosition.RegionPosition.Equals(Geolocation.Instance.CurrentRegionPosition))
                isInWorld = true;

            if (tileCoordinate.Column > -1 && isInWorld)
            {
                m_currentPositionNode.Visible = true;
                m_currentPositionNode.DrawHexagonForIsoStagMap(ClientConstants.TILE_IMAGE_WIDTH, TerrainLayer,
                    tileCoordinate, new CCColor4F(CCColor3B.Red), 255, 3.0f);
            } 
        }

        /// <summary>
        /// Draws the regions async.
        /// </summary>
        /// <returns>The task async.</returns>
        /// <param name="gamePosition">Game position.</param>
        async Task DrawRegionsAsync(Position gamePosition)
        {
            CenterPosition = gamePosition;
            Phase = Phases.LoadTerrain;
            await m_regionManagerController.LoadRegionsAsync(new RegionPosition(gamePosition));
            Phase = Phases.LoadEntities;
            await EntityManagerController.Instance.LoadEntitiesAsync(gamePosition, CenterPosition.RegionPosition);
            Phase = Phases.Idle;

            // set the loaded regions in a 160x160 Map
            RegionView.SetTilesInMap160(m_regionManagerController.GetRegionByGamePosition(gamePosition));
            SetCurrentPositionOnce(gamePosition);
            SetMapAnchor(gamePosition);

            CenterPosition = gamePosition;
            //SetMapAnchor (gamePosition);
            UglyDraw();
        }

        /// <summary>
        /// Set the point to parentspace.
        /// </summary>
        /// <returns>The point in parentspace.</returns>
        /// <param name="point">Point.</param>
        public CCPoint LayerWorldToParentspace(CCPoint point)
        {
            return TerrainLayer.WorldToParentspace(point);
        }

        /// <summary>
        /// Closests the tile coordinate at node position.
        /// </summary>
        /// <returns>The tile coordinate at node position.</returns>
        /// <param name="point">Point.</param>
        public CCTileMapCoordinates ClosestTileCoordAtNodePosition(CCPoint point)
        {
            return TerrainLayer.ClosestTileCoordAtNodePosition(point);
        }

        /// <summary>
        /// Dos the action.
        /// </summary>
        /// <param name="action">Action.</param>
        public void DoAction(Core.Models.Action action)
        {
            var actions = new List<Core.Models.Action>();
            actions.Add(action);
            m_regionManagerController.DoActionAsync(Geolocation.Instance.CurrentGamePosition, actions.ToArray());
            //var mapCell = GetMapCell(m_terrainLayer, new CCPoint(VisibleBoundsWorldspace.MidX, VisibleBoundsWorldspace.MidY));
            //var position = RegionView.GetCurrentGamePosition(mapCell, CenterPosition.RegionPosition);
            //DrawRegionsAsync (position);
        }


        /// <summary>
        /// Moves the world.
        /// </summary>
        /// <param name="diff">Diff.</param>
        public void MoveWorld(CCPoint diff)
        {
            var anchor = WorldTileMap.TileLayersContainer.AnchorPoint;
            diff.X = diff.X / WorldTileMap.TileLayersContainer.ScaledContentSize.Width;
            diff.Y = diff.Y / WorldTileMap.TileLayersContainer.ScaledContentSize.Height;
            anchor.X -= diff.X;
            anchor.Y -= diff.Y;
            WorldTileMap.TileLayersContainer.AnchorPoint = anchor;
        }

        /// <summary>
        /// Gets the scalefactor.
        /// </summary>
        /// <returns>The scalefactor.</returns>
        public float GetScale()
        {
            return m_scale;
        }

        /// <summary>
        /// Scales the world.
        /// </summary>
        /// <param name="newScale">New scale.</param>
        public void ScaleWorld(float newScale)
        {
            if (ClientConstants.TILEMAP_MIN_SCALE < newScale &&
                newScale < ClientConstants.TILEMAP_MAX_SCALE)
            {
                m_scale = newScale;
                WorldTileMap.TileLayersContainer.Scale = m_scale;
            }
        }


        /// <summary>
        /// Sets the map anchor.
        /// </summary>
        /// <param name="anchorPosition">Anchor position.</param>
        void SetMapAnchor(Position anchorPosition)
        {
            var mapCellPosition = PositionHelper.PositionToMapCellPosition(CenterPosition, new PositionI(anchorPosition));//new MapCellPosition(RegionView.GetCurrentTileInMap(anchorPosition));
            var anchor = mapCellPosition.GetAnchor();
            WorldTileMap.TileLayersContainer.AnchorPoint = anchor;
        }

        /// <summary>
        /// Gets the map cell at the location on the layer.
        /// </summary>
        /// <returns>The map cell.</returns>
        /// <param name="layer">Layer.</param>
        /// <param name="location">Location.</param>
        MapCellPosition GetMapCell(CCTileMapLayer layer, CCPoint location)
        {
            var point = layer.WorldToParentspace(location);
            var tileMapCooardinate = layer.ClosestTileCoordAtNodePosition(point);
            return new MapCellPosition(tileMapCooardinate);
        }

        /// <summary>
        /// Checks the center region to draw new regions at the view if the map is moved.
        /// </summary>
        public void CheckCenterRegion()
        {  
            var mapCell = GetMapCell(TerrainLayer, new CCPoint(VisibleBoundsWorldspace.MidX, VisibleBoundsWorldspace.MidY));

            if (RegionView.IsCellInOutsideRegion(mapCell))
            {
                CenterPosition = RegionView.GetCurrentGamePosition(mapCell, CenterPosition.RegionPosition);
                DrawRegionsAsync(CenterPosition);
            }

        }



        #endregion

        #region Properties

        /// <summary>
        /// Gets the world tile map.
        /// </summary>
        /// <value>The world tile map.</value>
        public CCTileMap WorldTileMap
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the terrain layer.
        /// </summary>
        /// <value>The terrain layer.</value>
        public CCTileMapLayer TerrainLayer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the building layer.
        /// </summary>
        /// <value>The building layer.</value>
        public CCTileMapLayer BuildingLayer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the unit layer.
        /// </summary>
        /// <value>The unit layer.</value>
        public CCTileMapLayer UnitLayer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the menu layer.
        /// </summary>
        /// <value>The menu layer.</value>
        public CCTileMapLayer MenuLayer
        {
            get;
            private set;
        }

        /// <summary>
        /// The m region manager controller.
        /// </summary>
        Client.Common.Manager.RegionManagerController m_regionManagerController;
        /// <summary>
        /// The m current position node.
        /// </summary>
        DrawNode m_currentPositionNode;
        /// <summary>
        /// The m worker.
        /// </summary>
        Worker m_worker;
        /// <summary>
        /// The m scale.
        /// </summary>
        float m_scale;
        /// <summary>
        /// The m_game scene.
        /// </summary>
        GameScene m_gameScene;

        #endregion
    }
}

