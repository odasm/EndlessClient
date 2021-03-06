﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EndlessClient.HUD
{
    public interface IStatusLabelTextRepository
    {
        string StatusText { get; set; }

        DateTime SetTime { get; set; }
    }

    public interface IStatusLabelTextProvider
    {
        string StatusText { get; }

        DateTime SetTime { get; }
    }

    public class StatusLabelTextRepository : IStatusLabelTextRepository, IStatusLabelTextProvider
    {
        public string StatusText { get; set; }

        public DateTime SetTime { get; set; }

        public StatusLabelTextRepository()
        {
            StatusText = "";
            SetTime = DateTime.Now;
        }
    }
}
