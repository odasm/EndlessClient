﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace EndlessClient.Game
{
	public interface IEndlessGame : IDisposable
	{
		ContentManager Content { get; }
		GameWindow Window { get; }

		void Run();
	}
}