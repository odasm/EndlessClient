﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class UnknownLayerRenderer : BaseMapEntityRenderer
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICurrentMapProvider _currentMapProvider;

        public override MapRenderLayer RenderLayer => MapRenderLayer.Unknown;

        protected override int RenderDistance => 12;

        public UnknownLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                    ICurrentMapProvider currentMapProvider,
                                    ICharacterProvider characterProvider,
                                    IRenderOffsetCalculator renderOffsetCalculator)
            : base(characterProvider, renderOffsetCalculator)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _currentMapProvider = currentMapProvider;
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return CurrentMap.GFX[MapLayer.Unknown][row, col] > 0;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            int gfxNum = CurrentMap.GFX[MapLayer.Unknown][row, col];
            var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapWallTop, gfxNum, true);

            var pos = GetDrawCoordinatesFromGridUnits(col, row);
            pos = new Vector2(pos.X, pos.Y - 65);

            spriteBatch.Draw(gfx, pos, Color.FromNonPremultiplied(255, 255, 255, alpha));
        }

        private IMapFile CurrentMap => _currentMapProvider.CurrentMap;
    }
}
