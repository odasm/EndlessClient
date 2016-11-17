﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using Microsoft.Xna.Framework;

namespace EndlessClient.Input
{
    public class UserInputHandler : GameComponent, IUserInputHandler
    {
        private readonly List<InputHandlerBase> _handlers;

        public UserInputHandler(IEndlessGameProvider endlessGameProvider,
                                IKeyStateProvider keyStateProvider,
                                IUserInputTimeRepository userInputTimeRepository,
                                IArrowKeyController arrowKeyController,
                                IControlKeyController controlKeyController)
            : base((Game)endlessGameProvider.Game)
        {
            _handlers = new List<InputHandlerBase>
            {
                new ArrowKeyHandler(endlessGameProvider,
                    keyStateProvider,
                    userInputTimeRepository,
                    arrowKeyController),
                new ControlKeyHandler(endlessGameProvider,
                    keyStateProvider,
                    userInputTimeRepository,
                    controlKeyController)
            };
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var handler in _handlers)
                handler.Update(gameTime);

            base.Update(gameTime);
        }
    }

    public interface IUserInputHandler : IGameComponent
    {
    }
}