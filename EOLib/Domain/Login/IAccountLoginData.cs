﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.BLL;

namespace EOLib.Domain.Login
{
	public interface IAccountLoginData
	{
		LoginReply Response { get; }

		IReadOnlyList<ICharacter> Characters { get; }
	}
}