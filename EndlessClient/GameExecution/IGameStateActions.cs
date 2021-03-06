﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.GameExecution
{
    public interface IGameStateActions
    {
        void ChangeToState(GameStates newState);

        void RefreshCurrentState();

        void ExitGame();
    }
}
