﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Character;

namespace EOLib.Domain.Chat.Commands
{
    public class UsageCommand : IPlayerCommand
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly IChatRepository _chatRepository;

        public string CommandText => "usage";

        public UsageCommand(ICharacterProvider characterProvider,
                            IChatRepository chatRepository)
        {
            _characterProvider = characterProvider;
            _chatRepository = chatRepository;
        }

        public bool Execute(string parameter)
        {
            var usage = _characterProvider.MainCharacter.Stats[CharacterStat.Usage];
            var message = $"[x] usage: {usage/60}hrs. {usage%60}min.";

            var chatData = new ChatData("System", message, ChatIcon.LookingDude);
            _chatRepository.AllChat[ChatTab.Local].Add(chatData);

            return true;
        }
    }
}
