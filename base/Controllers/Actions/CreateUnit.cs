﻿namespace Core.Controllers.Actions
{
    using System;
    using System.Collections.Concurrent;

    using Core.Controllers.Actions;
    using Core.Models;
    using Core.Models.Definitions;

    /// <summary>
    /// Create unit.
    /// </summary>
    public class CreateUnit : Action
    {
        /// <summary>
        /// Position of the parent-entity (e.g. position of barracks, if it spawns an unit)
        /// </summary>
        public const string CREATE_POSITION = "CreatePosition";

        /// <summary>
        /// Unit Type which should be created
        /// </summary>
        public const string CREATION_TYPE = "CreateUnit";

        /// <summary>
        /// Initializes a new instance of the <see cref="Core.Controllers.Actions.CreateUnit"/> class.
        /// </summary>
        /// <param name="model">action model.</param>
        public CreateUnit(Core.Models.ModelEntity model)
            : base(model)
        {
            var action = (Core.Models.Action)Model;               
            var param = action.Parameters;

            if (param[CREATE_POSITION].GetType() != typeof(PositionI))
            {
                param[CREATE_POSITION] = new PositionI((Newtonsoft.Json.Linq.JContainer)param[CREATE_POSITION]);
            }
        }
            
        /// <summary>
        /// Returns a bag of all regions which could be affected by this action.
        /// </summary>
        /// <returns>The affected regions.</returns>
        public override ConcurrentBag<Core.Models.Region> GetAffectedRegions()
        {
            var regionManagerC = Controller.Instance.RegionManagerController;

            ConcurrentBag<Core.Models.Region> bag = new ConcurrentBag<Core.Models.Region>();
            var action = (Core.Models.Action)Model;
            var positionI = (PositionI)action.Parameters[CREATE_POSITION];
            var region = regionManagerC.GetRegion(positionI.RegionPosition);

            bag.Add(region);

            var adjacentRegions = GetAdjacentRegions(regionManagerC, region.RegionPosition, positionI);

            foreach (var adjRegions in adjacentRegions)
            {
                bag.Add(regionManagerC.GetRegion(adjRegions));
            }

            return bag;
        }

        /// <summary>
        /// Returns if the action is even possible.
        /// </summary>
        /// <returns> True if the Building is buildable at the current position, otherwise false.</returns>
        public override bool Possible()
        {   
            var regionManagerC = Controller.Instance.RegionManagerController;

            var action = (Core.Models.Action)Model;
            var positionI = (PositionI)action.Parameters[CREATE_POSITION];
            var type = (long)action.Parameters[CREATION_TYPE];
            var entity = Controller.Instance.RegionManagerController.GetRegion(positionI.RegionPosition).GetEntity(positionI.CellPosition);
           
            if (action.AccountID == entity.OwnerID)
            {
                RealCreatePosition = GetFreeField(positionI, regionManagerC);

                return RealCreatePosition != null;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Apply action-related changes to the world.
        /// </summary>
        /// <returns> Returns <see cref="System.Collections.Concurrent.ConcurrentBag"/> class with the affected region./></returns>
        public override ConcurrentBag<Core.Models.Region> Do()
        {   
            var regionManagerC = Controller.Instance.RegionManagerController;

            var action = (Core.Models.Action)Model;
            var positionI = (PositionI)action.Parameters[CREATE_POSITION];
            var type = (long)action.Parameters[CREATION_TYPE];

            positionI = RealCreatePosition;
            var region = regionManagerC.GetRegion(positionI.RegionPosition);
            var dt = Controller.Instance.DefinitionManagerController.DefinitionManager.GetDefinition((EntityType)type);

            // create the new entity and link to the correct account
            var entity = new Core.Models.Entity(
                IdGenerator.GetId(),
                dt,  
                action.Account,
                positionI);

            entity.Position = positionI;
            region.AddEntity(action.ActionTime, entity);

            if (action.Account != null)
            {
                action.Account.Units.AddLast(entity.Position);
            }

            return new ConcurrentBag<Core.Models.Region>() { region };
        }

        /// <summary>
        /// If an error occurred in do, this function will be called to set the data to the last known valid state.
        /// </summary>
        /// <returns>true if rewind was successfully</returns>
        public override bool Catch()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Override <see cref="base.model.RegionPosition"/> class and return the positionI of the region.
        /// </summary>
        /// <returns>The region position.</returns>
        public override Core.Models.RegionPosition GetRegionPosition()
        {
            var action = (Core.Models.Action)Model;
            var positionI = (PositionI)action.Parameters[CREATE_POSITION];
            return positionI.RegionPosition;
        }
     
        /// <summary>
        /// Check all possible spawn locations around a building.
        /// TODO: THOMAS FILL OUT THIS COMMENT
        /// </summary>
        /// <param name="position"></param>
        /// <param name="regionManagerC"></param>
        /// <returns> Position which is free or null if nothing is free </returns>   
        private PositionI GetFreeField(PositionI position, RegionManagerController regionManagerC)
        {     
            foreach (var surpos in LogicRules.GetSurroundedFields(position))
            {                   
                var td = regionManagerC.GetRegion(surpos.RegionPosition).GetTerrain(surpos.CellPosition);
                var ed = regionManagerC.GetRegion(surpos.RegionPosition).GetEntity(surpos.CellPosition);

                if (td.Walkable && ed == null)
                {
                    return surpos;
                }
            }
            return null;
        }

        /// <summary>
        /// TODO: THOMAS FILL OUT THIS COMMENT
        /// </summary>
        /// <param name="regionManagerC"></param>
        /// <param name="position"></param>
        /// <param name="buildpoint"></param>
        /// <returns></returns>
        private ConcurrentBag<RegionPosition> GetAdjacentRegions(RegionManagerController regionManagerC, RegionPosition position, PositionI buildpoint)
        {
            var list = new ConcurrentBag<RegionPosition>();
            var surlist = LogicRules.SurroundRegions;
            var regionSizeX = Constants.REGION_SIZE_X;
            var regionSizeY = Constants.REGION_SIZE_Y;

            if (buildpoint.CellPosition.CellX == 0)
            {
                var tempReg = position + surlist[LogicRules.SurroundRegions.Length];
                if (regionManagerC.GetRegion(tempReg).Exist)
                {
                    list.Add(tempReg);
                }

                for (int index = 0; index < 4; ++index)
                {
                    tempReg = position + surlist[index];
                    if (regionManagerC.GetRegion(tempReg).Exist)
                    {
                        list.Add(tempReg);
                    }
                }
            }
            else if (buildpoint.CellPosition.CellY == 0)
            {
                for (int index = 5; index <= LogicRules.SurroundRegions.Length; ++index)
                {
                    var tempReg = position + surlist[index];
                    if (regionManagerC.GetRegion(tempReg).Exist)
                    {
                        list.Add(tempReg);
                    }
                }

                var reg = position + surlist[0];
                if (regionManagerC.GetRegion(reg).Exist)
                {
                    list.Add(reg);
                }
                reg = position + surlist[1];
                if (regionManagerC.GetRegion(reg).Exist)
                {
                    list.Add(reg);
                }
            }
            else if (buildpoint.CellPosition.CellX == regionSizeX)
            {
                for (int index = 1; index < 6; ++index)
                {
                    var tempReg = position + surlist[index];
                    if (regionManagerC.GetRegion(tempReg).Exist)
                    {
                        list.Add(tempReg);
                    }
                }
            }
            else if (buildpoint.CellPosition.CellY == regionSizeY)
            {
                for (int index = 3; index <= LogicRules.SurroundRegions.Length; ++index)
                {
                    var tempReg = position + surlist[index];
                    if (regionManagerC.GetRegion(tempReg).Exist)
                    {
                        list.Add(tempReg);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Position where the unit should be created
        /// </summary>
        public PositionI RealCreatePosition;
    }
}