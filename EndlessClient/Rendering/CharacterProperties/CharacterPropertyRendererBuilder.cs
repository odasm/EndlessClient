﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EOLib;
using EOLib.Data.BLL;
using EOLib.IO;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
	public class CharacterPropertyRendererBuilder
	{
		private readonly SpriteBatch _spriteBatch;
		private readonly ICharacterRenderProperties _renderProperties;
		private readonly ICharacterTextures _textures;
		private readonly IDataFile<ItemRecord> _itemDataFile;

		public CharacterPropertyRendererBuilder(SpriteBatch spriteBatch,
												ICharacterRenderProperties renderProperties,
												ICharacterTextures textures,
												IDataFile<ItemRecord> itemDataFile)
		{
			_spriteBatch = spriteBatch;
			_renderProperties = renderProperties;
			_textures = textures;
			_itemDataFile = itemDataFile;
		}

		public IEnumerable<ICharacterPropertyRenderer> BuildList(bool isShieldOnBack, bool isBowEquipped)
		{
			var rendererList = new List<ICharacterPropertyRenderer>();

			if (ShieldEquipped() && IsShieldBehindCharacter(isBowEquipped, isShieldOnBack))
				rendererList.Add(new ShieldRenderer(_spriteBatch, _renderProperties, _textures.Shield));

			if (WeaponEquipped() && IsWeaponBehindCharacter())
				rendererList.Add(new WeaponRenderer(_spriteBatch, _renderProperties, _textures.Weapon));

			rendererList.Add(new SkinRenderer(_spriteBatch, _renderProperties, _textures.Skin));
			if (IsCharacterDoingEmote())
			{
				rendererList.Add(new FaceRenderer(_spriteBatch, _renderProperties, _textures.Face));
				rendererList.Add(new EmoteRenderer(_spriteBatch, _renderProperties, _textures.Emote));
			}

			if (BootsEquipped())
				rendererList.Add(new BootsRenderer(_spriteBatch, _renderProperties, _textures.Boots, _itemDataFile));

			if (ArmorEquipped())
				rendererList.Add(new ArmorRenderer(_spriteBatch, _renderProperties, _textures.Armor, _itemDataFile));

			if (WeaponEquipped() && !rendererList.OfType<WeaponRenderer>().Any())
				rendererList.Add(new WeaponRenderer(_spriteBatch, _renderProperties, _textures.Weapon));

			var hairOnTopOfHat = new List<ICharacterPropertyRenderer>();
			if (HatEquipped())
				hairOnTopOfHat.Add(new HatRenderer(_spriteBatch, _renderProperties, _textures.Hat, _itemDataFile));
			if (!IsBald())
				hairOnTopOfHat.Add(new HairRenderer(_spriteBatch, _renderProperties, _textures.Hair));
			if (hairOnTopOfHat.Any())
				rendererList.AddRange(IsHairOnTopOfHat() ? hairOnTopOfHat : hairOnTopOfHat.ToArray().Reverse());

			if (ShieldEquipped() && !rendererList.OfType<ShieldRenderer>().Any())
				rendererList.Add(new ShieldRenderer(_spriteBatch, _renderProperties, _textures.Hair));

			return rendererList;
		}

		private bool IsShieldBehindCharacter(bool isBowEquipped, bool isShieldOnBack)
		{
			//todo: this may or may not work because boolean logic is confusing

			bool facingDownOrRight = _renderProperties.Direction == EODirection.Down ||
									 _renderProperties.Direction == EODirection.Right;

			return (facingDownOrRight && isBowEquipped) || !isShieldOnBack;
		}

		private bool IsWeaponBehindCharacter()
		{
			//todo: this may or may not work because boolean logic is confusing

			var weaponInfo = _itemDataFile.Data.SingleOrDefault(
				x => x.Type == ItemType.Weapon &&
					 x.DollGraphic == _renderProperties.WeaponGraphic);

			// weapon will be drawn later if:
			//  - attack frame is 2 (or data is NULL in which case ignore)
			//      - When attack frame is 0, want it behind character's hand
			//  - Direction is right or down
			//  - Weapon subtype is not Ranged (or weapon info is NULL in which case ignore)
			var pass1 = _renderProperties.AttackFrame < 2;
			var pass2 = _renderProperties.Direction != EODirection.Down && _renderProperties.Direction != EODirection.Right;
			var pass3 = weaponInfo != null && weaponInfo.SubType == ItemSubType.Ranged;

			return pass1 && pass2 && pass3;
		}

		private bool IsCharacterDoingEmote()
		{
			return _renderProperties.CurrentAction == CharacterActionState.Emote &&
				   _renderProperties.EmoteFrame > 0;
		}

		private bool IsHairOnTopOfHat()
		{
			//todo: i might have this backwards...

			var hatInfo = _itemDataFile.Data.SingleOrDefault(
				x => x.Type == ItemType.Hat &&
					 x.DollGraphic == _renderProperties.HatGraphic);

			return hatInfo != null && hatInfo.SubType == ItemSubType.FaceMask;
		}

		private bool BootsEquipped()
		{
			return _renderProperties.BootsGraphic != 0 && _textures.Boots != null;
		}

		private bool ArmorEquipped()
		{
			return _renderProperties.ArmorGraphic != 0 && _textures.Armor != null;
		}

		private bool HatEquipped()
		{
			return _renderProperties.HatGraphic != 0 && _textures.Hat != null;
		}

		private bool ShieldEquipped()
		{

			return _renderProperties.ShieldGraphic != 0 && _textures.Shield != null;
		}

		private bool WeaponEquipped()
		{
			return _renderProperties.WeaponGraphic != 0 && _textures.Weapon != null;
		}

		private bool IsBald()
		{
			return _renderProperties.HairStyle != 0 && _textures.Hair != null;
		}
	}
}
