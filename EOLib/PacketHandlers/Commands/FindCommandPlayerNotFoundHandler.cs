﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Localization;
using EOLib.Net;

namespace EOLib.PacketHandlers.Commands
{
    public class FindCommandPlayerNotFoundHandler : FindCommandHandlerBase
    {
        public override PacketAction Action => PacketAction.Ping;

        protected override EOResourceID ResourceIDForResponse => EOResourceID.STATUS_LABEL_IS_ONLINE_NOT_FOUND;

        public FindCommandPlayerNotFoundHandler(IChatRepository chatRespository,
                                                ILocalizedStringFinder localizedStringFinder,
                                                IPlayerInfoProvider playerInfoProvider)
            : base(chatRespository, localizedStringFinder, playerInfoProvider)
        {
        }
    }
}