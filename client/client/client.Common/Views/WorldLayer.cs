﻿using @base.control;
using @base.model;
using client.Common.Helper;
using client.Common.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CocosSharp;
using Microsoft.Xna.Framework;
using client.Common.Manager;




namespace client.Common.Views
{
    public class WorldLayer : CCLayerColor
    {
        public WorldLayer (RegionPosition regionPosition)
            : base ()
        {
            m_RegionView = new RegionView ();
            m_RegionManagerController = Controller.Instance.RegionStatesController.Curr as client.Common.Manager.RegionManagerController;
            m_EntityManagerController = Controller.Instance.DefinitionManagerController as client.Common.Manager.EntityManagerController;

            m_WorldTileMap = new CCTileMap (ClientConstants.TILEMAP_FILE);
            m_Geolocation = Geolocation.GetInstance;

            m_CurrentPositionNode = new DrawNode ();
            //m_PointedPositionNode = new DrawNode ();
            m_WorldTileMap.TileLayersContainer.AddChild (m_CurrentPositionNode);
            //m_WorldTileMap.TileLayersContainer.AddChild (m_PointedPositionNode);

            m_TerrainLayer = m_WorldTileMap.LayerNamed (ClientConstants.LAYER_TERRAIN);
            m_BuildingLayer = m_WorldTileMap.LayerNamed (ClientConstants.LAYER_BUILDING);
            m_UnitLayer = m_WorldTileMap.LayerNamed (ClientConstants.LAYER_UNIT);
            m_MenuLayer = m_WorldTileMap.LayerNamed (ClientConstants.LAYER_MENU);

            this.AddChild (m_WorldTileMap);

            this.Schedule (CheckGeolocation);

            m_Timer = new Stopwatch ();

            var TouchListener = new CCEventListenerTouchAllAtOnce ();
            TouchListener.OnTouchesMoved = onTouchesMoved;
            TouchListener.OnTouchesBegan = onTouchesBegan;
            TouchListener.OnTouchesEnded = onTouchesEnded;

            this.AddEventListener (TouchListener);
        }

        #region overide

        protected override void AddedToScene ()
        {
            base.AddedToScene ();

            SetMapAnchor (m_Geolocation.CurrentGamePosition);
            m_WorldTileMap.TileLayersContainer.PositionX = VisibleBoundsWorldspace.MidX;
            m_WorldTileMap.TileLayersContainer.PositionY = VisibleBoundsWorldspace.MidY;
            m_WorldTileMap.TileLayersContainer.Scale = m_Scale;

        }

        #endregion

        #region Listener

        void onTouchesMoved (List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count == 1) {
                var touch = touches [0];
                CCPoint diff = touch.Delta;
                m_WorldTileMap.TileLayersContainer.Position += diff;
            }
            if (touches.Count >= 2) {
                //do zoom stuff

                //get StartPositons
                /*
                CCPoint StartPoint0 = touches[0].StartLocation;
                CCPoint StartPoint1 = touches[1].StartLocation;
                float xDist = (StartPoint1.X - StartPoint0.X);
                float yDist = (StartPoint1.Y - StartPoint0.Y);
                float StartDistance = (xDist * xDist) + (yDist * yDist);


                CCPoint CurrentPoint0 = touches[0].Location;
                CCPoint CurrentPoint1 = touches[1].Location;

                //calculate distance 
                xDist = (CurrentPoint1.X - CurrentPoint0.X);
                yDist = (CurrentPoint1.Y - CurrentPoint0.Y);
                float CurrentDistance = (xDist * xDist) + (yDist * yDist);


                */


                CCPoint ScreenStart0 = touches [0].StartLocationOnScreen;
                CCPoint ScreenStart1 = touches [1].StartLocationOnScreen;

                float xDist = (ScreenStart1.X - ScreenStart0.X);
                float yDist = (ScreenStart1.Y - ScreenStart0.Y);
                float StartDistance = (xDist * xDist) + (yDist * yDist);

                //calculate Start distance



                //Get Current Position

                CCPoint CurrentPoint0 = touches [0].LocationOnScreen;
                CCPoint CurrentPoint1 = touches [1].LocationOnScreen;

                //calculate distance 
                xDist = (CurrentPoint1.X - CurrentPoint0.X);
                yDist = (CurrentPoint1.Y - CurrentPoint0.Y);
                float CurrentDistance = (xDist * xDist) + (yDist * yDist);

                xDist = VisibleBoundsWorldspace.MaxX;
                yDist = VisibleBoundsWorldspace.MaxY;

                float ScreenDistance = (xDist * xDist) + (yDist * yDist);

                float relation = (CurrentDistance - StartDistance) / ScreenDistance;

                var newScale = m_Scale + relation;
                if (0.3f < newScale && newScale < 2.0f) {
                    m_NewScale = newScale;
                    m_WorldTileMap.TileLayersContainer.Scale = m_NewScale;
                }

                /*
                if ( StartDistance > CurrentDistance && m_Scale > 0.3f)
                {
                    //zoom--
                    m_Scale += relation;
                    m_WorldTileMap.TileLayersContainer.Scale = m_Scale;

                }
                if (StartDistance < CurrentDistance && m_Scale < 2.0f)
                {
                    //Zoom++
                    m_Scale += relation;
                    m_WorldTileMap.TileLayersContainer.Scale = m_Scale;

                }
                */



            }


        }

        void onTouchesBegan (List<CCTouch> touches, CCEvent touchEvent)
        {
            m_Timer.Reset ();
            m_Timer.Start ();
        }

        void onTouchesEnded (List<CCTouch> touches, CCEvent touchEvent)
        {

            var touchEnd = touches [0];
            CheckCenterRegion (touchEnd.Location);

            m_Scale = m_NewScale;

            m_Timer.Stop ();
            if (m_Timer.ElapsedMilliseconds > 2000) {
                //get Touch
                var touch = touches [0];
                //get selected Tile
                //var location = m_TerrainLayer.WorldToParentspace (touch.Location);
                //var tileCoordinate = m_TerrainLayer.ClosestTileCoordAtNodePosition (location);
                 
                /*
                if(unit selected/unit move command){
                    //layer auf NULL checken
                    if( m_BuildingLayer == m_UnitLayer == NULL){
                        //move
                    }
                    
                    if( m_UnitLayer == TRUE ){
                        //Check if your own Unit
                        if(Unit.ownerid != userid){
                            //combat Menu
                        }else{
                            //move invalid
                        }
                    }
                    
                    if ( m_BuildingLayer == TRUE){
                        //check Building owner
                        if ( building.ownerid != userid ){
                            //Conquest Menu
                        }
                    }
                }
                */

                /*
                if (m_MenuLayer != NULL){

                    switch(MenuItems){
                    case unit_action1:
                        //do Stuff
                    break;
                    case building_action2:
                        //do Stuff
                    break;
                    }
                
                }
                */

                /*
                if (m_UnitLayer != NULL)
                {

                    //UnitMenu
                    DrawMenu();

                }
                */

                /*
                if (m_BuildingLayer != NULL){

                    //BuildingMenu
                
                }
                */
            }

        }


        #endregion

        #region Scheduling

        void SetEntitys (float frameTimesInSecond)
        {
            //TODO 
            throw new NotImplementedException ();
        }


        void CheckGeolocation (float frameTimesInSecond)
        {
            if (m_Geolocation.IsPositionChanged) {
                DrawRegionsAsync (m_Geolocation.CurrentGamePosition);
                DrawEntitiesAsync (m_Geolocation.CurrentGamePosition);
                m_Geolocation.IsPositionChanged = false;
            }

        }

        #endregion

        #region


        void SetCurrentPositionOnce (Position position)
        {
            var tileCoordinate = m_RegionView.GetCurrentTileInMap (position);
            m_CurrentPositionNode.DrawHexagonForIsoStagMap (ClientConstants.TILE_IMAGE_WIDTH, m_TerrainLayer,
                tileCoordinate, new CCColor4F (CCColor3B.Red), 255, 3.0f);
        }

        async Task DrawRegionsAsync (Position gamePosition)
        {
            GameAppDelegate.LoadingState = GameAppDelegate.Loading.RegionLoading;
            await m_RegionManagerController.LoadRegionsAsync ();
            GameAppDelegate.LoadingState = GameAppDelegate.Loading.RegionLoaded;
            m_RegionView.SetTilesInMap160 (m_TerrainLayer, m_RegionManagerController.GetRegionByGamePosition (gamePosition));
            SetCurrentPositionOnce (gamePosition);
            SetMapAnchor (gamePosition);
            m_CenterRegionPosition = new RegionPosition (gamePosition);

        }

        async Task DrawEntitiesAsync (Position gamePosition)
        {
            GameAppDelegate.LoadingState = GameAppDelegate.Loading.EntitiesLoading;
            await m_EntityManagerController.LoadEntitiesAsync (gamePosition, m_CenterRegionPosition);
            GameAppDelegate.LoadingState = GameAppDelegate.Loading.EntitiesLoaded;
            m_RegionView.SetTilesInMap160 (m_UnitLayer, m_RegionManagerController.GetRegion (m_CenterRegionPosition));
            m_RegionView.SetTilesInMap160 (m_BuildingLayer, m_RegionManagerController.GetRegion (m_CenterRegionPosition));
        }

        void CheckCenterRegion (CCPoint location)
        {            
            var mapCell = GetMapCell (m_WorldTileMap.LayerNamed ("Layer 0"), location);

//            if (m_RegionC.IsCellInOutsideRegion (mapCell)) {
//                var position = m_RegionC.GetCurrentGamePosition (mapCell, m_CenterRegionPosition);
//                DrawRegions (position);
//            }

        }


        void SetMapAnchor (Position anchorPosition)
        {
            var mapCellPosition = new MapCellPosition (m_RegionView.GetCurrentTileInMap (anchorPosition));
            var anchor = mapCellPosition.GetAnchor ();
            m_WorldTileMap.TileLayersContainer.AnchorPoint = anchor;
        }

        MapCellPosition GetMapCell (CCTileMapLayer layer, CCPoint location)
        {
            var point = layer.WorldToParentspace (location);
            var tileMapCooardinate = layer.ClosestTileCoordAtNodePosition (point);
            return new MapCellPosition (tileMapCooardinate);
        }


        #endregion

        #region Properties

        CCTileMap m_WorldTileMap;
        CCTileMapLayer m_TerrainLayer;
        CCTileMapLayer m_BuildingLayer;
        CCTileMapLayer m_UnitLayer;
        CCTileMapLayer m_MenuLayer;

        RegionView m_RegionView;
        RegionPosition m_CenterRegionPosition;
        client.Common.Manager.RegionManagerController m_RegionManagerController;
        client.Common.Manager.EntityManagerController m_EntityManagerController;

        DrawNode m_CurrentPositionNode;
        Geolocation m_Geolocation;

        float m_NewScale = 0.5f;
        float m_Scale = 0.5f;
        int counter = 0;

        Stopwatch m_Timer;

        #endregion
    }
}

