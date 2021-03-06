﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public interface IMapEntityRendererProvider : IDisposable
    {
        IReadOnlyList<IMapEntityRenderer> MapBaseRenderers { get; }

        IReadOnlyList<IMapEntityRenderer> MapEntityRenderers { get; }
    }
}
