﻿using System;
using System.Collections.Concurrent;

namespace client.Common.Controllers
{
    public class Worker
    {
        public Worker(Views.WorldLayer worldLayer)
        {
            WorldLayer = worldLayer;
            Queue = new ConcurrentQueue<@base.model.Action> ();

            var param = new ConcurrentDictionary<string, object> ();
            param [@base.control.action.CreateUnit.CREATE_POSITION] = new @base.model.PositionI ((int) (WorldLayer.CenterPosition.X + 5), (int) (WorldLayer.CenterPosition.Y - 36));
            param [@base.control.action.CreateUnit.CREATION_TYPE] = (long)60;

            var action = new @base.model.Action (GameAppDelegate.Account, @base.model.Action.ActionType.CreateUnit, param);

            Queue.Enqueue (action);

            var param2 = new ConcurrentDictionary<string, object> ();
            param2 [@base.control.action.MoveUnit.START_POSITION] = new @base.model.PositionI ((int) (WorldLayer.CenterPosition.X + 5), (int) (WorldLayer.CenterPosition.Y - 35));
            param2 [@base.control.action.MoveUnit.END_POSITION] = new @base.model.PositionI ((int) (WorldLayer.CenterPosition.X + 5), (int) (WorldLayer.CenterPosition.Y + 35));
            var action2 = new @base.model.Action (GameAppDelegate.Account, @base.model.Action.ActionType.MoveUnit, param2);

            Queue.Enqueue (action2);

        
        }

        public void Schedule(float frameTimesInSecond)
        {
            if (Action != null)
            {
                var actionV = (Views.Action.Action)Action.View;
                if (actionV.Schedule (frameTimesInSecond))
                {
                    // action was successfully executed, let the next be executed
                    Action = null;
                }
            }
            else if (Queue.TryDequeue (out Action))
            {
                var regionC = @base.control.Controller.Instance.RegionManagerController;
                var actionC = (@base.control.action.Action) Action.Control;
                var actionV = CreateActionView(Action);
                var affectedRegions = actionC.GetAffectedRegions (regionC);
                actionC.Possible (regionC);
                actionV.BeforeDo ();
                actionC.Do (regionC);
            }

        }

        static Views.Action.Action CreateActionView(@base.model.Action action)
        {
            switch(action.Type)
            {
            case(@base.model.Action.ActionType.CreateUnit):
                return new Views.Action.CreateUnit (action, WorldLayer);

            case(@base.model.Action.ActionType.MoveUnit):
                return new Views.Action.MoveUnit (action, WorldLayer);

            case(@base.model.Action.ActionType.CreateHeadquarter):
                throw new NotImplementedException ();
            }

            return new Views.Action.Action(action);
        }

        static public ConcurrentQueue<@base.model.Action> Queue;
        static public @base.model.Action Action = null;
        static public Views.WorldLayer WorldLayer;

    }
}
