﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        public delegate void LockerItemsChangedEvent(short id, int amount, byte weight, byte maxWeight, bool addToExistingAmount, List<InventoryItem> lockerItems);
        public delegate void LockerUpgradeEvent(int goldRemaining, byte lockerUpgrades);

        public event Action<byte, byte, List<InventoryItem>> OnLockerOpen;
        public event LockerItemsChangedEvent OnLockerItemChange;
        public event LockerUpgradeEvent OnLockerUpgrade;

        private void _createLockerMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Locker, PacketAction.Open), _handleLockerOpen, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Locker, PacketAction.Get), _handleLockerGet, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Locker, PacketAction.Reply), _handleLockerReply, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Locker, PacketAction.Buy), _handleLockerBuy, true);
        }

        /// <summary>
        /// Opens a locker at X/Y coordinates
        /// </summary>
        public bool OpenLocker(byte x, byte y)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Locker, PacketAction.Open);
            pkt.AddChar(x);
            pkt.AddChar(y);

            return m_client.SendPacket(pkt);
        }

        /// <summary>
        /// Deposit an item in your private locker
        /// </summary>
        public bool LockerAddItem(byte x, byte y, short id, int amount)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Locker, PacketAction.Add);
            pkt.AddChar(x);
            pkt.AddChar(y);
            pkt.AddShort(id);
            pkt.AddThree(amount);

            return m_client.SendPacket(pkt);
        }

        /// <summary>
        /// Withdraw an item from your private locker
        /// </summary>
        public bool LockerTakeItem(byte x, byte y, short id)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.Locker, PacketAction.Take);
            pkt.AddChar(x);
            pkt.AddChar(y);
            pkt.AddShort(id);

            return m_client.SendPacket(pkt);
        }

        /// <summary>
        /// Handles LOCKER_OPEN from server for opening a locker
        /// </summary>
        private void _handleLockerOpen(OldPacket pkt)
        {
            if (OnLockerOpen == null) return;

            byte x = pkt.GetChar();
            byte y = pkt.GetChar();

            List<InventoryItem> items = new List<InventoryItem>();
            while (pkt.ReadPos != pkt.Length)
            {
                items.Add(new InventoryItem(pkt.GetShort(), pkt.GetThree()));
            }

            OnLockerOpen(x, y, items);
        }

        /// <summary>
        /// Handles LOCKER_REPLY from server for adding an item to locker
        /// </summary>
        private void _handleLockerReply(OldPacket pkt)
        {
            if (OnLockerItemChange == null) return;
            //inventory info for amount remaining for character
            short itemID = pkt.GetShort();
            int amount = pkt.GetInt();
            byte weight = pkt.GetChar();
            byte maxWeight = pkt.GetChar();

            //items in the locker
            List<InventoryItem> items = new List<InventoryItem>();
            while (pkt.ReadPos != pkt.Length)
            {
                items.Add(new InventoryItem(pkt.GetShort(), pkt.GetThree()));
            }

            OnLockerItemChange(itemID, amount, weight, maxWeight, false, items);
        }

        /// <summary>
        /// Handles LOCKER_GET from server for taking an item from locker
        /// </summary>
        private void _handleLockerGet(OldPacket pkt)
        {
            if (OnLockerItemChange == null) return;

            short itemID = pkt.GetShort();
            int amount = pkt.GetThree();
            byte weight = pkt.GetChar();
            byte maxWeight = pkt.GetChar();

            List<InventoryItem> items = new List<InventoryItem>();
            while (pkt.ReadPos != pkt.Length)
            {
                items.Add(new InventoryItem(pkt.GetShort(), pkt.GetThree()));
            }

            OnLockerItemChange(itemID, amount, weight, maxWeight, true, items);
        }

        /// <summary>
        /// Handles LOCKER_BUY from server when buying a locker unit upgrade
        /// </summary>
        /// <param name="pkt"></param>
        private void _handleLockerBuy(OldPacket pkt)
        {
            if (OnLockerUpgrade != null)
                OnLockerUpgrade(pkt.GetInt(), pkt.GetChar()); //gold remaining, num upgrades
        }
    }
}
