﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Globalization;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Chat;
using EOLib.Localization;

namespace EndlessClient.HUD.Chat
{
    public class ChatNotificationActions : IChatEventNotifier
    {
        private readonly IChatRepository _chatRepository;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ILocalizedStringService _localizedStringService;
        private readonly IStatusLabelSetter _statusLabelSetter;

        public ChatNotificationActions(IChatRepository chatRepository,
                                       IHudControlProvider hudControlProvider,
                                       ILocalizedStringService localizedStringService,
                                       IStatusLabelSetter statusLabelSetter)
        {
            _chatRepository = chatRepository;
            _hudControlProvider = hudControlProvider;
            _localizedStringService = localizedStringService;
            _statusLabelSetter = statusLabelSetter;
        }

        public void NotifyPrivateMessageRecipientNotFound(string recipientName)
        {
            var whichTab = _chatRepository.PMTarget1.ToLower() == recipientName.ToLower()
                ? new Optional<ChatTab>(ChatTab.Private1)
                : _chatRepository.PMTarget2.ToLower() == recipientName.ToLower()
                    ? new Optional<ChatTab>(ChatTab.Private2)
                    : Optional<ChatTab>.Empty;

            if (whichTab.HasValue)
            {
                if (whichTab == ChatTab.Private1)
                    _chatRepository.PMTarget1 = string.Empty;
                else if (whichTab == ChatTab.Private2)
                    _chatRepository.PMTarget2 = string.Empty;

                _chatRepository.AllChat[whichTab].Clear();

                var chatPanel = _hudControlProvider.GetComponent<ChatPanel>(HudControlIdentifier.ChatPanel);
                chatPanel.ClosePMTab(whichTab);
            }
        }

        public void NotifyPlayerMutedByAdmin(string adminName)
        {
            var chatTextBox = _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox);
            var chatMode = _hudControlProvider.GetComponent<ChatModePictureBox>(HudControlIdentifier.ChatModePictureBox);

            var endMuteTime = DateTime.Now.AddMinutes(Constants.MuteDefaultTimeMinutes);
            chatTextBox.SetMuted(endMuteTime);
            chatMode.SetMuted(endMuteTime);

            chatTextBox.Text = string.Empty;
            _chatRepository.LocalTypedText = string.Empty;

            var chatData = new ChatData(_localizedStringService.GetString(EOResourceID.STRING_SERVER),
                _localizedStringService.GetString(EOResourceID.CHAT_MESSAGE_MUTED_BY) + " " + adminName,
                ChatIcon.Exclamation,
                ChatColor.Server);
            _chatRepository.AllChat[ChatTab.Local].Add(chatData);

            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                Constants.MuteDefaultTimeMinutes.ToString(CultureInfo.InvariantCulture),
                EOResourceID.STATUS_LABEL_MINUTES_MUTED);
        }
    }
}