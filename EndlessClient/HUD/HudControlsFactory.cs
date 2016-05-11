﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Sprites;
using EndlessClient.UIControls;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.HUD
{
	public class HudControlsFactory : IHudControlsFactory
	{
		private const int HUD_BASE_LAYER = 100;
		private const int HUD_CONTROL_LAYER = 130;

		private readonly INativeGraphicsManager _nativeGraphicsManager;
		private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
		private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
		private readonly IEndlessGameProvider _endlessGameProvider;
		private readonly ICharacterRepository _characterRepository;

		public HudControlsFactory(INativeGraphicsManager nativeGraphicsManager,
								  IGraphicsDeviceProvider graphicsDeviceProvider,
								  IClientWindowSizeProvider clientWindowSizeProvider,
								  IEndlessGameProvider endlessGameProvider,
								  ICharacterRepository characterRepository)
		{
			_nativeGraphicsManager = nativeGraphicsManager;
			_graphicsDeviceProvider = graphicsDeviceProvider;
			_clientWindowSizeProvider = clientWindowSizeProvider;
			_endlessGameProvider = endlessGameProvider;
			_characterRepository = characterRepository;
		}

		public IReadOnlyDictionary<HudControlIdentifier, IGameComponent> CreateHud()
		{
			var controls = new Dictionary<HudControlIdentifier, IGameComponent>
			{
				{HudControlIdentifier.HudBackground, CreateHudBackground()},

				{HudControlIdentifier.InventoryButton, CreateStateChangeButton(InGameStates.Inventory)},
				{HudControlIdentifier.ViewMapButton, CreateStateChangeButton(InGameStates.ViewMapToggle)},
				{HudControlIdentifier.ActiveSpellsButton, CreateStateChangeButton(InGameStates.ActiveSpells)},
				{HudControlIdentifier.PassiveSpellsButton, CreateStateChangeButton(InGameStates.PassiveSpells)},
				{HudControlIdentifier.ChatButton, CreateStateChangeButton(InGameStates.Chat)},
				{HudControlIdentifier.StatsButton, CreateStateChangeButton(InGameStates.Stats)},
				{HudControlIdentifier.OnlineListButton, CreateStateChangeButton(InGameStates.OnlineList)},
				{HudControlIdentifier.PartyButton, CreateStateChangeButton(InGameStates.Party)},
				{HudControlIdentifier.MacroButton, CreateStateChangeButton(InGameStates.Macro)},
				{HudControlIdentifier.SettingsButton, CreateStateChangeButton(InGameStates.Settings)},
				{HudControlIdentifier.HelpButton, CreateStateChangeButton(InGameStates.Help)},
				
				{HudControlIdentifier.ClockLabel, CreateClockLabel()},
				{HudControlIdentifier.UsageTracker, CreateUsageTracker()}
			};

			return controls;
		}

		private HudBackgroundFrame CreateHudBackground()
		{
			return new HudBackgroundFrame(_nativeGraphicsManager, _graphicsDeviceProvider)
			{
				DrawOrder = HUD_BASE_LAYER
			};
		}

		private XNAButton CreateStateChangeButton(InGameStates whichState)
		{
			if (whichState == InGameStates.News)
				throw new ArgumentOutOfRangeException("whichState", "News state does not have a button associated with it");
			var buttonIndex = (int) whichState;

			var mainButtonTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 25);
			var widthDelta = mainButtonTexture.Width/2;
			var heightDelta = mainButtonTexture.Height/11;

			var outSprite = new SpriteSheet(mainButtonTexture, new Rectangle(0, heightDelta*buttonIndex, widthDelta, heightDelta));
			var overSprite = new SpriteSheet(mainButtonTexture, new Rectangle(widthDelta, heightDelta * buttonIndex, widthDelta, heightDelta));
			var textures = new[] { outSprite.GetSourceTexture(), overSprite.GetSourceTexture() };

			var xPosition = buttonIndex < 6 ? 62 : 590;
			var yPosition = (buttonIndex < 6 ? 330 : 350) + (buttonIndex < 6 ? buttonIndex : buttonIndex - 6)*20;

			var retButton = new XNAButton(textures, new Vector2(xPosition, yPosition)) { DrawOrder = HUD_CONTROL_LAYER };
			//retButton.OnClick += //todo: game state controller, set in-game state?
			//retButton.OnMouseOver +=  //todo: set status label
										//DATCONST2.STATUS_LABEL_TYPE_BUTTON,
										//DATCONST2.STATUS_LABEL_HUD_BUTTON_HOVER_FIRST + buttonIndex
			return retButton;
		}

		private TimeLabel CreateClockLabel()
		{
			return new TimeLabel(_clientWindowSizeProvider)
			{
				DrawOrder = HUD_CONTROL_LAYER
			};
		}

		private UsageTrackerComponent CreateUsageTracker()
		{
			return new UsageTrackerComponent(_endlessGameProvider, _characterRepository);
		}
	}
}