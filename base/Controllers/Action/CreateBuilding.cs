﻿using System;
using System.Collections.Concurrent;

using @base.control;
using @base.model.definitions;
using @base.model;
using @base.Models.Definition;

namespace @base.control.action
{
    public class CreatBuilding : Action
    {
        public const string CREATE_POSITION = "CreatePosition";
        public const string CREATION_TYPE = "CreateBuilding";

        /// <summary>
        /// Constructor of the class CreateHeadquarter.
        /// </summary>
        /// <param name="model"></param>
        public CreatBuilding(model.ModelEntity model)
            : base(model)
        {
            var action = (model.Action)Model;
            var param = action.Parameters;

            if (param[CREATE_POSITION].GetType() != typeof(PositionI))
            {
                param[CREATE_POSITION] = new model.PositionI((Newtonsoft.Json.Linq.JContainer)param[CREATE_POSITION]);
            }
        }

        /// <summary>
        /// Identify the affected region by this action.
        /// </summary>
        /// <param name="regionManagerC"> Access to maybe changed Regions.</param>
        /// <returns> Returns <see cref="System.Collections.Concurrent.ConcurrentBag<t>"/> with the affected region. </returns>
        override public ConcurrentBag<model.Region> GetAffectedRegions(RegionManagerController regionManagerC)
        {
            var action = (model.Action)Model;
            var positionI = (PositionI)action.Parameters[CREATE_POSITION];

            return new ConcurrentBag<model.Region>() { regionManagerC.GetRegion(positionI.RegionPosition) };
        }

        /// <summary>
        /// Returns if the action is even possible.
        /// </summary>
        /// <param name="regionManagerC"></param>
        /// <returns> True if the Headquarte is buildable at the current position, otherwise false.</returns>
        public override bool Possible(RegionManagerController regionManagerC)
        {
            var action = (model.Action)Model;
            var positionI = (PositionI)action.Parameters[CREATE_POSITION];

            var td = (TerrainDefinition)regionManagerC.GetRegion(positionI.RegionPosition).GetTerrain(positionI.CellPosition);

            // entity and terrain check 
            return td.Buildable;         
        }

        /// <summary>
        /// Apply action-related changes to the world.
        /// </summary>
        /// <param name="regionManagerC"></param>
        /// <returns> Returns <see cref="System.Collections.Concurrent.ConcurrentBag<t>"/> class with the affected region./></returns>
        public override ConcurrentBag<model.Region> Do(RegionManagerController regionManagerC)
        {
            var action = (model.Action)Model;
            var positionI = (PositionI)action.Parameters[CREATE_POSITION];
            var type = (long)action.Parameters[CREATION_TYPE];

            //positionI = RealCreatePosition;
            var region = regionManagerC.GetRegion(positionI.RegionPosition);
            var dt = Controller.Instance.DefinitionManagerController.DefinitionManager.GetDefinition((int)type);

            // create the new entity and link to the correct account
            var entity = new @base.model.Entity(IdGenerator.GetId(),               
                dt,  
                action.Account,
                positionI);

            entity.Position = positionI;
            region.AddEntity(action.ActionTime, entity);

            if (action.Account != null)
            {
                action.Account.Buildings.AddLast(entity.Position);
            }
            return new ConcurrentBag<model.Region>() { region };
        }

        /// <summary>
        /// In case of errors, revert the world data to a valid state.
        /// </summary>
        /// <param name="regionManagerC"></param>
        /// <returns></returns>
        public override bool Catch(RegionManagerController regionManagerC)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check all possible spawn locations around a building.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="regionManagerC"></param>
        /// <returns> Position which is free or null if nothing is free </returns>   
        private PositionI GetFreeField(PositionI position, RegionManagerController regionManagerC)
        {
            var temp = LogicRules.SurroundTiles;

            for (int index = 0; index < LogicRules.SurroundTiles.Length; ++index)
            {
                var surpos = temp[index] + position;

                var td = regionManagerC.GetRegion(surpos.RegionPosition).GetTerrain(surpos.CellPosition);
                var ed = regionManagerC.GetRegion(surpos.RegionPosition).GetEntity(surpos.CellPosition);

                if (td.Buildable && ed == null)
                {
                    return surpos;
                }
            }
            return null;
        }

        /// <summary>
        /// Ovveride <see cref="base.model.RegionPosition"/> class and return the positionI of the region.
        /// </summary>
        /// <returns></returns>
        override public @base.model.RegionPosition GetRegionPosition()
        {
            var action = (model.Action)Model;
            var positionI = (PositionI)action.Parameters[CREATE_POSITION];
            return positionI.RegionPosition;
        }

    }
}
