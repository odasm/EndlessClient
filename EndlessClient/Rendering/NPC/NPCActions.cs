﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Chat;
using EOLib.Domain.Notifiers;

namespace EndlessClient.Rendering.NPC
{
    public class NPCActions : INPCActionNotifier
    {
        private readonly IHudControlProvider _hudControlProvider;
        private readonly INPCStateCache _npcStateCache;
        private readonly INPCRendererRepository _npcRendererRepository;
        private readonly IChatBubbleRepository _chatBubbleRepository;
        private readonly IChatBubbleTextureProvider _chatBubbleTextureProvider;

        public NPCActions(IHudControlProvider hudControlProvider,
                          INPCStateCache npcStateCache,
                          INPCRendererRepository npcRendererRepository,
                          IChatBubbleRepository chatBubbleRepository,
                          IChatBubbleTextureProvider chatBubbleTextureProvider)
        {
            _hudControlProvider = hudControlProvider;
            _npcStateCache = npcStateCache;
            _npcRendererRepository = npcRendererRepository;
            _chatBubbleRepository = chatBubbleRepository;
            _chatBubbleTextureProvider = chatBubbleTextureProvider;
        }

        public void StartNPCWalkAnimation(int npcIndex)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartWalkAnimation(npcIndex);
        }

        public void StartNPCAttackAnimation(int npcIndex)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartAttackAnimation(npcIndex);
        }

        public void RemoveNPCFromView(int npcIndex, bool showDeathAnimation)
        {
            //possible that the server might send a packet for the npc to be removed by the map switch is completed
            if (!_hudControlProvider.IsInGame || !_npcRendererRepository.NPCRenderers.ContainsKey(npcIndex))
                return;

            _npcStateCache.RemoveStateByIndex(npcIndex);

            if (!showDeathAnimation)
            {
                _npcRendererRepository.NPCRenderers[npcIndex].Dispose();
                _npcRendererRepository.NPCRenderers.Remove(npcIndex);
            }
            else
            {
                _npcRendererRepository.NPCRenderers[npcIndex].StartDying();
            }
        }

        public void ShowNPCSpeechBubble(int npcIndex, string message)
        {
            IChatBubble chatBubble;
            if (_chatBubbleRepository.NPCChatBubbles.TryGetValue(npcIndex, out chatBubble))
                chatBubble.SetMessage(isGroupChat: false, message: message);
            else
            {
                chatBubble = new ChatBubble(message,
                                            _npcRendererRepository.NPCRenderers[npcIndex],
                                            _chatBubbleTextureProvider);

                _chatBubbleRepository.NPCChatBubbles.Add(npcIndex, chatBubble);
            }
        }

        private INPCAnimator Animator => _hudControlProvider.GetComponent<INPCAnimator>(HudControlIdentifier.NPCAnimator);
    }
}
