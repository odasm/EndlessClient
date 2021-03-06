﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.IO;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class WeaponRenderer : ICharacterPropertyRenderer
    {
        private readonly ICharacterRenderProperties _renderProperties;
        private readonly Texture2D _weaponTexture;
        private readonly IPubFile<EIFRecord> _itemFile;

        public bool CanRender => _weaponTexture != null && _renderProperties.WeaponGraphic != 0;

        public WeaponRenderer(ICharacterRenderProperties renderProperties,
                              Texture2D weaponTexture,
                              IPubFile<EIFRecord> itemFile)
        {
            _renderProperties = renderProperties;
            _weaponTexture = weaponTexture;
            _itemFile = itemFile;
        }

        public void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            if (_renderProperties.CurrentAction == CharacterActionState.Sitting ||
                _renderProperties.CurrentAction == CharacterActionState.SpellCast)
                return;

            var offsets = GetOffsets();
            var drawLoc = new Vector2(parentCharacterDrawArea.X + offsets.X, parentCharacterDrawArea.Y + offsets.Y);

            spriteBatch.Draw(_weaponTexture, drawLoc, null, Color.White, 0.0f, Vector2.Zero, 1.0f,
                             _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                             0.0f);
        }

        private bool IsWeaponAMeleeWeapon()
        {
            var weaponInfo = _itemFile.Data.SingleOrDefault(
                x => x.Type == ItemType.Weapon &&
                     x.DollGraphic == _renderProperties.WeaponGraphic);

            return weaponInfo == null || weaponInfo.SubType != ItemSubType.Ranged;
        }

        private Vector2 GetOffsets()
        {
            var weaponIsMelee = IsWeaponAMeleeWeapon();
            var weaponOffX = 0;

            if (weaponIsMelee && _renderProperties.AttackFrame == 2)
            {
                weaponOffX = _renderProperties.Gender == 0 ? 2 : 4;

                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Left))
                    weaponOffX *= -1;
            }

            var flippedOffset = _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? -10 : -28;
            return new Vector2(flippedOffset + weaponOffX, -7);
        }
    }
}
