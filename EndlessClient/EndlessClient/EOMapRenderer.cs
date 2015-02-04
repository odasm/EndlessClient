﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using EndlessClient.Handlers;
using EOLib;
using EOLib.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace EndlessClient
{
	public enum WarpAnimation
	{
		None,
		Scroll,
		Admin,
		Invalid = 255
	}

	//returned from CheckCoordinates
	//convenience wrapper
	public struct TileInfo
	{
		public enum ReturnType //this struct is used sort of like a union - different data returned in different cases
		{
			IsTileSpec, //indicates that a normal tile spec is returned
			IsWarpSpec, //indicates that a normal warp spec is returned
			IsOtherPlayer, //other player is in the way, spec/warp are invalid
			IsOtherNPC //other npc is in the way, spec/warp are invalid
		}

		public ReturnType ReturnValue;

		public TileSpec Spec;
		public EOLib.Warp Warp;
	}

	public class EOChatBubble : DrawableGameComponent
	{
		private XNALabel m_label;
		private readonly DrawableGameComponent m_ref;
		private readonly bool isChar; //true if character, false if npc

		private SpriteBatch sb;

		private Vector2 drawLoc;

		private const int TL = 0, TM = 1, TR = 2;
		private const int ML = 3, MM = 4, MR = 5;
		private const int RL = 6, RM = 7, RR = 8, NUB = 9;
		private static bool textsLoaded;
		private static Texture2D[] texts;

		private DateTime? m_startTime;
		
		public EOChatBubble(EOCharacterRenderer following)
			: base(EOGame.Instance)
		{
			m_ref = following;
			isChar = true;
			DrawOrder = following.DrawOrder + 1;
			_initLabel();
			Visible = false;
			EOGame.Instance.Components.Add(this);
		}

		public EOChatBubble(NPC following)
			: base(EOGame.Instance)
		{
			m_ref = following;
			isChar = false;
			DrawOrder = following.DrawOrder + 1;
			_initLabel();
			Visible = false;
			EOGame.Instance.Components.Add(this);
		}

		public void SetMessage(string message)
		{
			m_label.Text = message;
			m_label.Visible = true;
			if(!Game.Components.Contains(m_label))
				Game.Components.Add(m_label);

			Visible = true;
			if(!Game.Components.Contains(this))
				Game.Components.Add(this);

			m_startTime = DateTime.Now;
		}

		private void _initLabel()
		{
			m_label = new XNALabel(new Rectangle(1, 1, 1, 1), "Microsoft Sans Serif", 8.5f)
			{
				Visible = true,
				DrawOrder = DrawOrder + 1,
				TextWidth = 165,
				TextAlign = ContentAlignment.MiddleCenter,
				ForeColor = System.Drawing.Color.Black,
				AutoSize = true,
				Text = ""
			};

			_setLabelDrawLoc();
		}

		private void _setLabelDrawLoc()
		{
			Rectangle refArea = isChar ? ((EOCharacterRenderer) m_ref).DrawAreaWithOffset : ((NPC) m_ref).DrawArea;
			int extra = textsLoaded ? texts[ML].Width : 0;
			m_label.DrawLocation = new Vector2(refArea.X + (refArea.Width / 2.0f) - (m_label.ActualWidth / 2.0f) + extra, refArea.Y - m_label.Texture.Height);
		}

		public new void LoadContent()
		{
			if(sb == null)
				sb = new SpriteBatch(GraphicsDevice);

			if (!textsLoaded)
			{
				texts = new Texture2D[10];
				texts[TL] = Game.Content.Load<Texture2D>("ChatBubble\\TL");
				texts[TM] = Game.Content.Load<Texture2D>("ChatBubble\\TM");
				texts[TR] = Game.Content.Load<Texture2D>("ChatBubble\\TR");
				texts[ML] = Game.Content.Load<Texture2D>("ChatBubble\\ML");
				texts[MM] = Game.Content.Load<Texture2D>("ChatBubble\\MM");
				texts[MR] = Game.Content.Load<Texture2D>("ChatBubble\\MR");
				//typed an R instead of a B. I'm tired; somehow bot=R made more sense than bot=B
				texts[RL] = Game.Content.Load<Texture2D>("ChatBubble\\RL");
				texts[RM] = Game.Content.Load<Texture2D>("ChatBubble\\RM");
				texts[RR] = Game.Content.Load<Texture2D>("ChatBubble\\RR");
				texts[NUB] = Game.Content.Load<Texture2D>("ChatBubble\\NUB");
				textsLoaded = true;
			}

			base.LoadContent();
		}

		public override void Update(GameTime gameTime)
		{
			if (!Visible)
				return;

			if (!(m_ref is EOCharacterRenderer || m_ref is NPC))
				Dispose(); //"It's over, Anakin, I have the high ground!" "Don't try it!"

			_setLabelDrawLoc();
			try
			{
				drawLoc = m_label.DrawLocation - new Vector2(texts[TL].Width, texts[TL].Height);
			}
			catch (NullReferenceException)
			{
				return; //nullreference here means that the textures haven't been loaded yet...try it on the next pass
			}

			//This replaces the goAway timer.
			if (m_startTime.HasValue && (DateTime.Now - m_startTime.Value).TotalMilliseconds > Constants.ChatBubbleTimeout)
			{
				Visible = false;
				m_label.Visible = false;
				m_startTime = null;
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!textsLoaded || !Visible) return;
			int xCov = texts[TL].Width;
			int yCov = texts[TL].Height;
			if (sb == null) return;

			sb.Begin();

			//top row
			sb.Draw(texts[TL], drawLoc, Color.White);
			int xCur;
			for (xCur = xCov; xCur < m_label.ActualWidth; xCur += texts[TM].Width)
			{
				sb.Draw(texts[TM], drawLoc + new Vector2(xCur, 0), Color.White);
			}
			sb.Draw(texts[TR], drawLoc + new Vector2(xCur, 0), Color.White);

			//middle area
			int y;
			for (y = yCov; y < m_label.Texture.Height - (m_label.Texture.Height%texts[ML].Height); y += texts[ML].Height)
			{
				sb.Draw(texts[ML], drawLoc + new Vector2(0, y), Color.White);
				int x;
				for (x = xCov; x < xCur; x += texts[MM].Width)
				{
					sb.Draw(texts[MM], drawLoc + new Vector2(x, y), Color.White);
				}
				sb.Draw(texts[MR], drawLoc + new Vector2(xCur, y), Color.White);
			}

			//bottom row
			sb.Draw(texts[RL], drawLoc + new Vector2(0, y), Color.White);
			int x2;
			for (x2 = xCov; x2 < xCur; x2 += texts[RM].Width)
			{
				sb.Draw(texts[RM], drawLoc + new Vector2(x2, y), Color.White);
			}
			sb.Draw(texts[RR], drawLoc + new Vector2(x2, y), Color.White);
			y += texts[RM].Height;
			sb.Draw(texts[NUB], drawLoc + new Vector2((x2 + texts[RR].Width - texts[NUB].Width)/2f, y - 1), Color.White);

			try
			{
				sb.End();
			}
			catch (ObjectDisposedException) { }
			base.Draw(gameTime);
		}

		public new void Dispose()
		{
			Dispose(true);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (sb != null)
				{
					sb.Dispose();
					sb = null;
				}
				if (m_label != null)
					m_label.Close();
			}

			base.Dispose(disposing);
		}
	}

	public class EOMapRenderer : DrawableGameComponent
	{
		/// <summary>
		/// Indices of the mini map gfx in their single texture (for source rectangle offset)
		/// </summary>
		private enum MiniMapGfx
		{
			//for drawing the lines
			UpLine,
			LeftLine,
			Corner,
			Solid, //indicates wall or obstacle
			Green, //other player
			Red, //attackable npc
			Orange, //you!
			Blue, //tile that you can interact with
			Purple //npc
		}

		//collections
		public List<MapItem> MapItems { get; private set; }
		private readonly List<Character> otherPlayers = new List<Character>();
		private readonly List<EOCharacterRenderer> otherRenderers = new List<EOCharacterRenderer>();
		private readonly List<NPC> npcList = new List<NPC>();
		private static readonly object npcListLock = new object();

		public MapFile MapRef { get; private set; }
		
		//cursor members
		private Vector2 cursorPos;
		private int gridX, gridY;
		private readonly Texture2D mouseCursor;
		private Rectangle _cursorSourceRect;
		private readonly XNALabel _mouseoverName;
		private MouseState _prevState;
		private bool _hideCursor;
		//public cursor members
		public bool MouseOver
		{
			get
			{
				MouseState ms = Mouse.GetState();
				return ms.X > 0 && ms.Y > 0 && ms.X < 640 && ms.Y < 320;
			}
		}
		public Point GridCoords
		{
			get { return new Point(gridX, gridY); }
		}

		//rendering members
		private RenderTarget2D _rtMapObjAbovePlayer, _rtMapObjBelowPlayer;
		private BlendState _playerBlend;
		private SpriteBatch sb;
		private bool m_showShadows;
		private bool m_showMiniMap;

		private DateTime? m_mapLoadTime;
		private int m_transitionMetric;

		//animated tile/wall members
		private Vector2 _tileSrc;
		private Vector2 _wallSrc;
		private TimeSpan? lastAnimUpdate;

		//door members
		private readonly Timer _doorTimer;
		private EOLib.Warp _door;
		private byte _doorY; //since y-coord not stored in Warp object...

		private ManualResetEventSlim m_eventChangeMap;

		public EOMapRenderer(Game g, MapFile mapObj)
			: base(g)
		{
			if(g == null)
				throw new NullReferenceException("The game must not be null");
			if(!(g is EOGame))
				throw new ArgumentException("The game must be an EOGame instance");

			MapItems = new List<MapItem>();

			sb = new SpriteBatch(Game.GraphicsDevice);

			mouseCursor = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 24, true);
			_cursorSourceRect = new Rectangle(0, 0, mouseCursor.Width / 5, mouseCursor.Height);
			_mouseoverName = new XNALabel(new Rectangle(1, 1, 1, 1), "Microsoft Sans Serif", 8.75f)
			{
				Visible = true,
				Text = "",
				ForeColor = System.Drawing.Color.White,
				DrawOrder = (int)ControlDrawLayer.BaseLayer + 3
			};

			Visible = true;

			//shadows on by default
			if (!World.Instance.Configuration.GetValue(ConfigStrings.Settings, ConfigStrings.ShowShadows, out m_showShadows))
				m_showShadows = true;

			_doorTimer = new Timer(_doorTimerCallback);
			SetActiveMap(mapObj);
		}

		/* PUBLIC INTERFACE -- CHAT + MAP RELATED */
		public void RenderChatMessage(TalkType messageType, int playerID, string message, ChatType chatType = ChatType.None)
		{
			//convert the messageType into a valid ChatTab to pass everything on to
			ChatTabs tab;
			switch (messageType)
			{
				case TalkType.NPC:
				case TalkType.Local: tab = ChatTabs.Local; break;
				case TalkType.Party: tab = ChatTabs.Group; break;
				default: throw new NotImplementedException();
			}

			DrawableGameComponent dgc;
			string playerName = null;
			if (messageType == TalkType.NPC)
			{
				lock(npcListLock)
					dgc = npcList.Find(_npc => _npc.Index == playerID);
				if (dgc != null)
					playerName = ((NPC) dgc).Data.Name;
			}
			else
			{
				dgc = otherRenderers.Find(_rend => _rend.Character.ID == playerID);
				if (dgc != null)
					playerName = ((EOCharacterRenderer)dgc).Character.Name;
			}

			if (playerName == null) return;

			if(playerName.Length > 1)
				playerName = char.ToUpper(playerName[0]) + playerName.Substring(1);

			if (EOGame.Instance.Hud == null)
				return;
			EOGame.Instance.Hud.AddChat(tab, playerName, message, chatType);

			MakeSpeechBubble(dgc, message);
		}

		public void MakeSpeechBubble(DrawableGameComponent follow, string message)
		{
			if (follow == null)
				follow = World.Instance.ActiveCharacterRenderer; /* Calling with null assumes Active Character */

			//show just the speech bubble, since this should be called from the HUD and rendered there already

// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
			if (follow is EOCharacterRenderer)
				((EOCharacterRenderer)follow).SetChatBubbleText(message);
			else if (follow is NPC)
				((NPC)follow).SetChatBubbleText(message);
// ReSharper restore CanBeReplacedWithTryCastAndCheckForNull
		}

		public void SetActiveMap(MapFile newActiveMap)
		{
			if (m_eventChangeMap == null)
			{
				m_eventChangeMap = new ManualResetEventSlim(true);
			}
			else
			{
				m_eventChangeMap.Wait();
			}

			MapRef = newActiveMap;
			MapItems.Clear();
			otherRenderers.ForEach(_rend => _rend.Dispose());
			otherRenderers.Clear();
			otherPlayers.Clear();
			lock (npcListLock)
			{
				npcList.ForEach(_npc => _npc.Dispose());
				npcList.Clear();
			}

			//need to reset door-related members when changing maps.
			if (_door != null)
			{
				_door.doorOpened = false;
				_door.backOff = false;
				_door = null;
				_doorY = 0;
				_doorTimer.Change(Timeout.Infinite, Timeout.Infinite);
			}

			m_mapLoadTime = DateTime.Now;
			m_transitionMetric = 1;
			if (MapRef.MapAvailable == 0)
				m_showMiniMap = false;

			m_eventChangeMap.Set();
		}

		public TileInfo CheckCoordinates(byte destX, byte destY)
		{
			lock (npcListLock)
			{
				if (npcList.Any(npc => npc.X == destX && npc.Y == destY))
				{
					return new TileInfo {ReturnValue = TileInfo.ReturnType.IsOtherNPC};
				}
			}

			if (otherPlayers.Any(player => player.X == destX && player.Y == destY))
			{
				return new TileInfo { ReturnValue = TileInfo.ReturnType.IsOtherPlayer };
			}

			EOLib.Warp warp = MapRef.WarpLookup[destY, destX];
			if (warp != null)
			{
				return new TileInfo { ReturnValue = TileInfo.ReturnType.IsWarpSpec, Warp = warp };
			}

			Tile tile = MapRef.TileLookup[destY, destX];
			if (tile != null)
			{
				return new TileInfo { ReturnValue = TileInfo.ReturnType.IsTileSpec, Spec = tile.spec };
			}

			return destX <= MapRef.Width && destY <= MapRef.Height //don't need to check zero bounds: because byte type is always positive (unsigned)
				? new TileInfo { ReturnValue = TileInfo.ReturnType.IsTileSpec, Spec = TileSpec.None }
				: new TileInfo { ReturnValue = TileInfo.ReturnType.IsTileSpec, Spec = TileSpec.MapEdge };
		}

		public void ToggleMapView()
		{
			if(MapRef.MapAvailable != 0)
				m_showMiniMap = !m_showMiniMap;
		}

		/* PUBLIC INTERFACE -- OTHER PLAYERS */
		public void AddOtherPlayer(Character c, WarpAnimation anim = WarpAnimation.None)
		{
			Character other;
			if ((other = otherPlayers.Find(x => x.Name == c.Name && x.ID == c.ID)) == null)
			{
				otherPlayers.Add(c);
				otherRenderers.Add(new EOCharacterRenderer(c));
				otherRenderers[otherRenderers.Count - 1].Visible = true;
				otherRenderers[otherRenderers.Count - 1].Initialize();
			}
			else
			{
				other.ApplyData(c);
			}

			//TODO: Add whatever magic is necessary to make the player appear all pretty (with animation)
		}

		public void RemoveOtherPlayer(short id, WarpAnimation anim = WarpAnimation.None)
		{
			Character c;
			if ((c = otherPlayers.Find(cc => cc.ID == id)) != null)
			{
				otherPlayers.Remove(c);
				otherRenderers.RemoveAll(rend => rend.Character == c);
			}

			//TODO: Add warp animation when valid
		}

		public void ClearOtherPlayers()
		{
			otherRenderers.Clear();
			otherPlayers.Clear();
		}

		public void OtherPlayerFace(short ID, EODirection direction)
		{
			Character c;
			if((c = otherPlayers.Find(cc => cc.ID == ID)) != null)
			{
				c.RenderData.SetDirection(direction);
			}
		}

		public void OtherPlayerWalk(short ID, EODirection direction, byte x, byte y)
		{
			Character c;
			if ((c = otherPlayers.Find(cc => cc.ID == ID)) != null)
			{
				c.Walk(direction, x, y, false);
				List<EOCharacterRenderer> rends = otherRenderers.Where(rend => rend.Character == c).ToList();
				EOCharacterRenderer renderer;
				if (rends.Count > 0 && (renderer = rends[0]) != null)
				{
					renderer.PlayerWalk();//do the actual drawing of the other player walking
				}
			}
		}

		public void OtherPlayerAttack(short ID, EODirection direction)
		{
			Character c;
			if ((c = otherPlayers.Find(cc => cc.ID == ID)) != null)
			{
				c.Attack(direction);
				List<EOCharacterRenderer> rends = otherRenderers.Where(rend => rend.Character == c).ToList();
				EOCharacterRenderer renderer;
				if (rends.Count > 0 && (renderer = rends[0]) != null)
				{
					renderer.PlayerAttack();//do the actual drawing of the other player walking
				}
			}
		}

		public void OtherPlayerHide(short ID, bool hidden)
		{
			Character c = otherPlayers.Find(_char => _char.ID == ID);
			if (c != null)
			{
				c.RenderData.SetHidden(hidden);
			}
		}

		public void UpdateOtherPlayers()
		{
			//when mainplayer walks, tell other players to update!
			otherPlayers.ForEach(x => x.RenderData.SetUpdate(true));
		}

		public void UpdateOtherPlayer(short playerId, bool sound, CharRenderData newRenderData)
		{
			Character c =  playerId == World.Instance.MainPlayer.ActiveCharacter.ID ? World.Instance.MainPlayer.ActiveCharacter : otherPlayers.Find(cc => cc.ID == playerId);
			if (c != null)
			{
				c.EquipItem(ItemType.Boots, 0, newRenderData.boots, true);
				c.EquipItem(ItemType.Armor, 0, newRenderData.armor, true);
				c.EquipItem(ItemType.Hat, 0, newRenderData.hat, true);
				c.EquipItem(ItemType.Shield, 0, newRenderData.shield, true);
				c.EquipItem(ItemType.Weapon, 0, newRenderData.weapon, true);
				//todo: play sound?
			}
		}

		public void UpdateOtherPlayer(short playerId, byte hairColor, byte hairStyle = 255)
		{
			Character c = playerId == World.Instance.MainPlayer.ActiveCharacter.ID ? World.Instance.MainPlayer.ActiveCharacter : otherPlayers.Find(cc => cc.ID == playerId);
			if (c != null)
			{
				c.RenderData.SetHairColor(hairColor);
				if (hairStyle != 255) c.RenderData.SetHairStyle(hairStyle);
			}
		}

		public Character GetOtherPlayer(short playerId)
		{
			return otherPlayers.Find(_c => _c.ID == playerId);
		}

		/* PUBLIC INTERFACE -- OTHER NPCS */
		public void AddOtherNPC(NPC newGuy)
		{
			lock (npcListLock)
			{
				NPC exists;
				if ((exists = npcList.Find(_npc => _npc.Index == newGuy.Index)) == null)
				{
					newGuy.Initialize();
					newGuy.Visible = true;
					npcList.Add(newGuy);
				}
				else
				{
					exists.Dispose();
					npcList.Remove(exists);
					newGuy.Initialize();
					newGuy.Visible = true;
					npcList.Add(newGuy);
				}
			}
		}

		public void RemoveOtherNPC(byte index, bool fadeOut)
		{
			lock (npcListLock)
			{
				NPC npc = npcList.Find(_npc => _npc.Index == index);
				if (npc != null)
				{
					if (fadeOut)
						npc.FadeAway();
					else
					{
						npc.Dispose();
						npcList.Remove(npc);
					}
				}
			}
		}

		public void ClearOtherNPCs()
		{
			lock(npcListLock)
				npcList.Clear();
		}

		public void NPCWalk(byte index, byte x, byte y, EODirection dir)
		{
			NPC toWalk;
			lock(npcListLock)
				toWalk = npcList.Find(_npc => _npc.Index == index);
			if (toWalk != null && !toWalk.Walking)
			{
				toWalk.Walk(x, y, dir);
			}
		}

		/* PUBLIC INTERFACE -- DOORS */
		public void OpenDoor(byte x, short y)
		{
			if (_door != null && _door.doorOpened)
			{
				_door.doorOpened = false;
				_door.backOff = false;
				_doorY = 0;
			}

			WarpRow row;
			if ((row = MapRef.WarpRows.Find(wr => wr.y == y)).tiles.Count > 0)
			{
				if ((_door = row.tiles.Find(w => w.x == x)) != null)
				{
					_door.doorOpened = true;
					_doorY = (byte)y;
					_doorTimer.Change(3000, 0);
				}
			}
		}

		private void _doorTimerCallback(object state)
		{
			_door.doorOpened = false;
			_door.backOff = false; //back-off from sending a door packet.
			_doorY = 0;
			_doorTimer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		/* GAME COMPONENT DERIVED METHODS */
		public override void Initialize()
		{
			_rtMapObjAbovePlayer = new RenderTarget2D(Game.GraphicsDevice, 
				Game.GraphicsDevice.PresentationParameters.BackBufferWidth, 
				Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
				false,
				SurfaceFormat.Color,
				DepthFormat.None);

			_rtMapObjBelowPlayer = new RenderTarget2D(Game.GraphicsDevice,
				Game.GraphicsDevice.PresentationParameters.BackBufferWidth, 
				Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
				false,
				SurfaceFormat.Color,
				DepthFormat.None);

			_playerBlend = new BlendState
			{
				BlendFactor = new Color(255, 255, 255, 64),

				AlphaSourceBlend = Blend.One,
				AlphaDestinationBlend = Blend.One,
				AlphaBlendFunction = BlendFunction.Add,

				ColorSourceBlend = Blend.BlendFactor,
				ColorDestinationBlend = Blend.One
			};

			base.Initialize();
		}

		public override void Update(GameTime gameTime)
		{
			World.Instance.ActiveCharacterRenderer.Update(gameTime);
			IEnumerable<EOCharacterRenderer> toAdd = otherRenderers.Where(rend => !Game.Components.Contains(rend));
			foreach (EOCharacterRenderer rend in toAdd)
				rend.Update(gameTime); //do update logic here: other renderers will NOT be added to Game's components

			lock(npcListLock)
				npcList.Where(_npc => !Game.Components.Contains(_npc)).ToList().ForEach(_n => _n.Update(gameTime));

			//lazy init
			if (lastAnimUpdate == null) lastAnimUpdate = gameTime.TotalGameTime;

			if ((gameTime.TotalGameTime - lastAnimUpdate.Value).TotalMilliseconds > 500)
			{
				_wallSrc = new Vector2(32 + _wallSrc.X, 0);
				if(_wallSrc.X > 96)
					_wallSrc = new Vector2(0, 0);

				_tileSrc = new Vector2(64 + _tileSrc.X, 0);
				if (_tileSrc.X > 192)
					_tileSrc = Vector2.Zero;


				lastAnimUpdate = gameTime.TotalGameTime;
			}

			MouseState ms = Mouse.GetState();
			//need to solve this system of equations to get x, y on the grid
			//edit: why didn't i take a linear algebra class....
			//(x * 32) - (y * 32) + 288 - c.OffsetX, => pixX = 32x - 32y + 288 - c.OffsetX
			//(y * 16) + (x * 16) + 144 - c.OffsetY  => 2pixY = 32y + 32x + 288 - 2c.OffsetY
			//										 => 2pixY + pixX = 64x + 576 - c.OffsetX - 2c.OffsetY
			//										 => 2pixY + pixX - 576 + c.OffsetX + 2c.OffsetY = 64x
			//										 => gridX = (pixX + 2pixY - 576 + c.OffsetX + 2c.OffsetY) / 64; <=
			//pixY = (gridX * 16) + (gridY * 16) + 144 - c.OffsetY =>
			//(pixY - (gridX * 16) - 144 + c.OffsetY) / 16 = gridY

			if (MouseOver) //checks bounds for map rendering area
			{
				Character c = World.Instance.MainPlayer.ActiveCharacter;
				//center the cursor on the mouse pointer
				int msX = ms.X - _cursorSourceRect.Width / 2;
				int msY = ms.Y - _cursorSourceRect.Height / 2;
				/*align cursor to grid based on mouse position*/
				gridX = (int)Math.Round((msX + 2 * msY - 576 + c.OffsetX + 2 * c.OffsetY) / 64.0);
				gridY = (int)Math.Round((msY - gridX * 16 - 144 + c.OffsetY) / 16.0);
				cursorPos = _getDrawCoordinates(gridX, gridY, c);
				if (gridX >= 0 && gridX <= MapRef.Width && gridY >= 0 && gridY <= MapRef.Height)
				{
					TileInfo ti = CheckCoordinates((byte)gridX, (byte)gridY);
					switch (ti.ReturnValue)
					{
						case TileInfo.ReturnType.IsOtherNPC:
							_cursorSourceRect.Location = new Point(mouseCursor.Width / 5, 0);
							NPC npc;
							lock (npcListLock)
								if ((npc = npcList.Find(_npc => _npc.X == gridX && _npc.Y == gridY)) == null)
									break;
							_mouseoverName.Visible = true;
							_mouseoverName.Text = npc.Data.Name;
							_mouseoverName.ResizeBasedOnText();
							_mouseoverName.ForeColor = System.Drawing.Color.White;
							_mouseoverName.DrawLocation = new Vector2(cursorPos.X + 16, cursorPos.Y - 32/* - _mouseoverName.Texture.Height*/);
							break;
						case TileInfo.ReturnType.IsOtherPlayer:
							_cursorSourceRect.Location = new Point(mouseCursor.Width / 5, 0);
							EOCharacterRenderer _rend;
							_mouseoverName.Visible = true;
							_mouseoverName.Text = (_rend = otherRenderers.Find(_p => _p.Character.X == gridX && _p.Character.Y == gridY) ?? World.Instance.ActiveCharacterRenderer).Character.Name;
							_mouseoverName.ResizeBasedOnText();
							_mouseoverName.ForeColor = System.Drawing.Color.White;
							_mouseoverName.DrawLocation = new Vector2(cursorPos.X + 16 - _rend.DrawArea.Width / 2f,
								cursorPos.Y - _rend.DrawArea.Height - _mouseoverName.Texture.Height);
							break;
						default:
							if (gridX == c.X && gridY == c.Y) goto case TileInfo.ReturnType.IsOtherPlayer; //same logic if it's the active character

							_hideCursor = false;
							if (ti.ReturnValue == TileInfo.ReturnType.IsTileSpec)
							{
								switch (ti.Spec)
								{
									case TileSpec.Wall:
									case TileSpec.JammedDoor:
									case TileSpec.MapEdge:
									case TileSpec.FakeWall:
										//hide cursor
										_hideCursor = true;
										break;
									case TileSpec.ChairDown:
									case TileSpec.ChairLeft:
									case TileSpec.ChairRight:
									case TileSpec.ChairUp:
									case TileSpec.ChairDownRight:
									case TileSpec.ChairUpLeft:
									case TileSpec.ChairAll:
									case TileSpec.Chest:
									case TileSpec.BankVault:
									case TileSpec.Board1:
									case TileSpec.Board2:
									case TileSpec.Board3:
									case TileSpec.Board4:
									case TileSpec.Board5:
									case TileSpec.Board6:
									case TileSpec.Board7:
									case TileSpec.Board8:
									case TileSpec.Jukebox:
										//highlight cursor
										_cursorSourceRect.Location = new Point(mouseCursor.Width / 5, 0);
										break;
									case TileSpec.Jump:
									case TileSpec.Water:
									case TileSpec.Arena:
									case TileSpec.AmbientSource:
									case TileSpec.Spikes:
									case TileSpec.SpikesTrap:
									case TileSpec.SpikesTimed:
									case TileSpec.None:
										//normal cursor
										_cursorSourceRect.Location = new Point(0, 0);
										break;
								}
							}
							else
								_cursorSourceRect.Location = new Point(0, 0);
							_mouseoverName.Text = "";
							break;
					}

					MapItem mi; //value type...dumb comparisons needed since can't check for non-null
					if ((mi = MapItems.Find(_mi => _mi.x == gridX && _mi.y == gridY)).x == gridX && mi.y == gridY && mi.x > 0 && mi.y > 0)
					{
						_cursorSourceRect.Location = new Point(2 * (mouseCursor.Width / 5), 0);
						_mouseoverName.Visible = true;
						_mouseoverName.Text = EOInventoryItem.GetNameString(mi.id, mi.amount);
						_mouseoverName.ResizeBasedOnText();
						_mouseoverName.ForeColor = EOInventoryItem.GetItemTextColor(mi.id);
						_mouseoverName.DrawLocation = new Vector2(cursorPos.X, cursorPos.Y - 16 - _mouseoverName.Texture.Height);

						if (_prevState.LeftButton == ButtonState.Pressed && ms.LeftButton == ButtonState.Released)
						{
							//todo: need to check if it is protected item (drop protection)
							//		the server won't let you pick it up anyway, but need to set the status label somehow
							if (!Item.GetItem(mi.uid))
								EOGame.Instance.LostConnectionDialog();
						}
					}

					if (_mouseoverName.Text.Length > 0 && !Game.Components.Contains(_mouseoverName))
						Game.Components.Add(_mouseoverName);
				}
			}
			_drawMapObjectsAndActors(); //if any player has been updated redraw the render target

			_prevState = ms;
			base.Update(gameTime);
		}
		
		public override void Draw(GameTime gameTime)
		{
			if (MapRef != null)
			{
				m_eventChangeMap.Wait();

				_drawGroundLayer();
				if(MapItems.Count > 0)
					_drawMapItems();

				if (m_mapLoadTime != null && (DateTime.Now - m_mapLoadTime.Value).TotalMilliseconds > 2000)
					m_mapLoadTime = null;

				sb.Begin();
				/*_drawCursor()*/
				if (!_hideCursor && gridX >= 0 && gridY >= 0 && gridX <= MapRef.Width && gridY <= MapRef.Height)
					sb.Draw(mouseCursor, cursorPos, _cursorSourceRect, Color.White);
				
				/*_drawPlayersNPCsAndMapObjects()*/
				sb.Draw(_rtMapObjAbovePlayer, Vector2.Zero, Color.White);
				sb.Draw(_rtMapObjBelowPlayer, Vector2.Zero, Color.White);
#if DEBUG
				sb.DrawString(World.DBG, string.Format("FPS: {0}", World.FPS), new Vector2(30, 30), Color.White);
#endif
				sb.End();

				if(m_showMiniMap)
					_drawMiniMap();

				m_eventChangeMap.Set();
			}

			base.Draw(gameTime);
		}

		/* DRAWING-RELATED HELPER METHODS */
		// Special Thanks: HotDog's client. Used heavily as a reference for numeric offsets/techniques, with some adjustments here and there.
		private void _drawGroundLayer()
		{
			Character c = World.Instance.MainPlayer.ActiveCharacter;
			const int localViewLength = 10;
			int xMin = c.X - localViewLength < 0 ? 0 : c.X - localViewLength,
				xMax = c.X + localViewLength > MapRef.Width ? MapRef.Width : c.X + localViewLength;
			int yMin = c.Y - localViewLength < 0 ? 0 : c.Y - localViewLength,
				yMax = c.Y + localViewLength > MapRef.Height ? MapRef.Height : c.Y + localViewLength;

			Func<GFX, bool> xGFXQuery = gfx => gfx.x >= xMin && gfx.x <= xMax && gfx.x <= MapRef.Width;
			Func<GFXRow, bool> yGFXQuery = row => row.y >= yMin && row.y <= yMax && row.y <= MapRef.Height;

			sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			//render fill tile first
			if (MapRef.FillTile > 0)
			{
				//only do the cache lookup once!
				Texture2D fillTileRef = GFXLoader.TextureFromResource(GFXTypes.MapTiles, MapRef.FillTile, true);
				for (int i = yMin; i <= yMax; ++i)
				{
					for (int j = xMin; j <= xMax; ++j)
					{
						//only render fill layer when the ground layer is not present!
						if (MapRef.GFXRows[0, i].column[j].tile >= 0)
							continue;

						Vector2 pos = _getDrawCoordinates(j, i, c);
						sb.Draw(fillTileRef, new Vector2(pos.X - 1, pos.Y - 2), Color.FromNonPremultiplied(255,255,255,_getAlpha(j, i, c)));
					}
				}
			}

			//ground layer next
			IEnumerable<GFXRow> ground = MapRef.GfxRows[0].Where(yGFXQuery);
			foreach (GFXRow row in ground)
			{
				IEnumerable<GFX> tiles = row.tiles.Where(xGFXQuery);
				foreach (GFX tile in tiles)
				{
					if (tile.tile == 0)
						continue;
					
					//render tile.tile at tile.x, row.y
					Texture2D nextTile = GFXLoader.TextureFromResource(GFXTypes.MapTiles, tile.tile, true);
					Vector2 pos = _getDrawCoordinates(tile.x, row.y, c);
					Rectangle? src = nextTile.Width > 64 ? new Rectangle?(new Rectangle((int)_tileSrc.X, (int)_tileSrc.Y, nextTile.Width / 4, nextTile.Height)) : null;
					if (nextTile.Width > 64)
						sb.Draw(nextTile, new Vector2(pos.X - 1, pos.Y - 2), src, Color.FromNonPremultiplied(255, 255, 255, _getAlpha(tile.x, row.y, c)));
					else
						sb.Draw(nextTile, new Vector2(pos.X - 1, pos.Y - 2), Color.FromNonPremultiplied(255, 255, 255, _getAlpha(tile.x, row.y, c)));
				}
			}
			sb.End();
		}

		private void _drawMapItems()
		{
			Character c = World.Instance.MainPlayer.ActiveCharacter;
			
			// Queries (func) for the gfx items within range of the character's X coordinate
			Func<GFX, bool> xGFXQuery = gfx => gfx.x >= c.X - Constants.ViewLength && gfx.x <= c.X + Constants.ViewLength && gfx.x <= MapRef.Width;
			// Queries (func) for the gfxrow items within range of the character's Y coordinate
			Func<GFXRow, bool> yGFXQuery = row => row.y >= c.Y - Constants.ViewLength && row.y <= c.Y + Constants.ViewLength && row.y <= MapRef.Height;

			//items next!
			IEnumerable<MapItem> items = MapItems.Where(item => xGFXQuery(new GFX { x = item.x }) && yGFXQuery(new GFXRow { y = item.y }));

			sb.Begin();
			foreach (MapItem item in items)
			{
				ItemRecord itemData = (ItemRecord)World.Instance.EIF.Data.Find(i => i is ItemRecord && (i as ItemRecord).ID == item.id);
				Vector2 itemPos = _getDrawCoordinates(item.x + 1, item.y, c);
				if (itemData.Type == ItemType.Money)
				{
					int gfx = item.amount >= 100000 ? 4 : (
						item.amount >= 10000 ? 3 : (
						item.amount >= 100 ? 2 : (
						item.amount >= 2 ? 1 : 0)));

					Texture2D moneyMoneyMan = GFXLoader.TextureFromResource(GFXTypes.Items, 269 + 2 * gfx, true);
					sb.Draw(moneyMoneyMan, 
						new Vector2(itemPos.X - (int)Math.Round(moneyMoneyMan.Width / 2.0), itemPos.Y - (int)Math.Round(moneyMoneyMan.Height / 2.0)), 
						Color.White);
				}
				else
				{
					Texture2D itemTexture = GFXLoader.TextureFromResource(GFXTypes.Items, 2*itemData.Graphic - 1, true);
					sb.Draw(itemTexture, new Vector2(itemPos.X - (int)Math.Round(itemTexture.Width / 2.0), itemPos.Y - (int)Math.Round(itemTexture.Height / 2.0)), Color.White);
				}
			}
			sb.End();
		}

		private void _drawMapObjectsAndActors()
		{
			//also, certain spikes only appear when a player is over them...yikes.

			if (MapRef == null) return;
			Character c = World.Instance.MainPlayer.ActiveCharacter;
			List<EOCharacterRenderer> otherChars = new List<EOCharacterRenderer>(otherRenderers); //copy of list (can remove items)
			List<NPC> otherNpcs;
			lock(npcListLock) //when drawing a frame, don't want to consider NPCs that are added/removed mid-draw - they will be taken care of on next update
				otherNpcs = new List<NPC>(npcList);

			// Queries (func) for the items within range of the character's X coordinate, passed as expressions to list linq extensions
			Func<GFX, int, bool> xGFXDistanceQuery = (gfx, dist) => gfx.x >= c.X - dist && gfx.x <= c.X + dist && gfx.x <= MapRef.Width;
			Func<GFXRow, int, bool> yGFXDistanceQuery = (row, dist) => row.y >= c.Y - dist && row.y <= c.Y + dist && row.y <= MapRef.Height;

			GraphicsDevice.SetRenderTarget(_rtMapObjAbovePlayer);
			GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);
			sb.Begin();

			//Get all the row lists in-range of player up front. Retrieved here in order to be rendered
			List<GFXRow> overlayObjRows = MapRef.GfxRows[2].Where(_row => yGFXDistanceQuery(_row, 10)).ToList();
			List<GFXRow> wallRowsRight = MapRef.GfxRows[4].Where(_row => yGFXDistanceQuery(_row, 20)).ToList();
			List<GFXRow> wallRowsDown = MapRef.GfxRows[3].Where(_row => yGFXDistanceQuery(_row, 20)).ToList();
			List<GFXRow> shadowRows = MapRef.GfxRows[7].Where(_row => yGFXDistanceQuery(_row, 10)).ToList();
			List<GFXRow> mapObjRows = MapRef.GfxRows[1].Where(_row => yGFXDistanceQuery(_row, 22)).ToList();
			List<GFXRow> roofRows = MapRef.GfxRows[5].Where(_row => yGFXDistanceQuery(_row, 10)).ToList();
			List<GFXRow> overlayTileRows = MapRef.GfxRows[6].Where(_row=>yGFXDistanceQuery(_row, 10)).ToList();

			bool targetChanged = false;
			//no need to iterate over the entire map rows if they won't be included in the render.
			for (int rowIndex = Math.Max(c.Y - 22, 0); rowIndex <= Math.Min(c.Y + 22, MapRef.Height); ++rowIndex)
			{
				//any objects, NPCs and players with a Y coordinate <= player Y coordinate (ie above player) render to one target
				//all others render 'below' player on separate target so blending works for only the objects below the player
				if (!targetChanged && ((!c.Walking && rowIndex >= c.Y) || (c.Walking && rowIndex >= c.DestY)))
				{
					sb.End();
					GraphicsDevice.SetRenderTarget(_rtMapObjBelowPlayer);
					GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);
					sb.Begin();
					targetChanged = true;
				}

				GFXRow row; //reused for each layer

				//overlay/mask  objects
				if ((row = overlayObjRows.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> overlayObj = row.tiles.Where(_gfx => xGFXDistanceQuery(_gfx, 10)).ToList();
					foreach (GFX obj in overlayObj)
					{
						Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapOverlay, obj.tile, true);
						Vector2 pos = _getDrawCoordinates(obj.x, row.y, c);
						pos = new Vector2(pos.X + 16, pos.Y - 11);
						sb.Draw(gfx, pos, Color.FromNonPremultiplied(255, 255, 255, _getAlpha(obj.x, row.y, c)));
					}
				}

				//shadows
				if (m_showShadows && (row = shadowRows.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> shadows = row.tiles.Where(_gfx => xGFXDistanceQuery(_gfx, 10)).ToList();
					foreach (GFX shadow in shadows)
					{
						Vector2 loc = _getDrawCoordinates(shadow.x, row.y, c);
						sb.Draw(GFXLoader.TextureFromResource(GFXTypes.Shadows, shadow.tile, true), new Vector2(loc.X - 24, loc.Y - 12), Color.FromNonPremultiplied(255, 255, 255, 60));
					}
				}

				//walls - two layers: facing different directions
				//this layer faces to the right
				if ((row = wallRowsRight.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> walls = row.tiles.Where(_gfx => xGFXDistanceQuery(_gfx, 20)).ToList();
					foreach (GFX obj in walls)
					{
						int gfxNum = obj.tile;
						if (_door != null && _door.x == obj.x && _doorY == row.y && _door.doorOpened)
							gfxNum++;

						Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapWalls, gfxNum, true);
						Vector2 loc = _getDrawCoordinates(obj.x, row.y, c);
						Rectangle? src = gfx.Width > 32 ? new Rectangle?(new Rectangle((int)_wallSrc.X, (int)_wallSrc.Y, gfx.Width / 4, gfx.Height)) : null;
						loc = new Vector2(loc.X - (int)Math.Round((gfx.Width > 32 ? gfx.Width / 4.0 : gfx.Width) / 2.0) + 47, loc.Y - (gfx.Height - 29));
						sb.Draw(gfx, loc, src, Color.FromNonPremultiplied(255, 255, 255, _getAlpha(obj.x, row.y, c)));
					}
				}

				//UPDATE: NPCs and characters are not drawn after map objects, rather inbetween the wall directions.

				// ReSharper disable AccessToModifiedClosure
				IEnumerable<NPC> thisRowNpcs = otherNpcs.Where(
					_npc => (_npc.Walking ? _npc.DestY == rowIndex : _npc.Y == rowIndex) &&
							_npc.X >= c.X - Constants.ViewLength &&
							_npc.X <= c.X + Constants.ViewLength);
				foreach (NPC npc in thisRowNpcs) npc.DrawToSpriteBatch(sb, true);
				// ReSharper restore AccessToModifiedClosure

				// ReSharper disable AccessToModifiedClosure
				IEnumerable<EOCharacterRenderer> thisRowChars = otherChars.Where(
					_char => (_char.Character.Walking ? _char.Character.DestY == rowIndex : _char.Character.Y == rowIndex) &&
							 _char.Character.X >= c.X - Constants.ViewLength &&
							 _char.Character.X <= c.X + Constants.ViewLength);
				foreach (EOCharacterRenderer _char in thisRowChars) _char.Draw(sb, true);
				// ReSharper restore AccessToModifiedClosure
				
				//this layer faces to the down
				if ((row = wallRowsDown.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> walls = row.tiles.Where(_gfx => xGFXDistanceQuery(_gfx, 20)).ToList();
					foreach (GFX obj in walls)
					{
						int gfxNum = obj.tile;
						if (_door != null && _door.x == obj.x && _doorY == row.y && _door.doorOpened)
							gfxNum++;

						Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapWalls, gfxNum, true);
						Vector2 loc = _getDrawCoordinates(obj.x, row.y, c);
						Rectangle? src = gfx.Width > 32 ? new Rectangle?(new Rectangle((int)_wallSrc.X, (int)_wallSrc.Y, gfx.Width / 4, gfx.Height)): null;
						loc = new Vector2(loc.X - (int)Math.Round((gfx.Width > 32 ? gfx.Width / 4.0 : gfx.Width) / 2.0) + 15, loc.Y - (gfx.Height - 29));
						sb.Draw(gfx, loc, src, Color.FromNonPremultiplied(255, 255, 255, _getAlpha(obj.x, row.y, c)));
					}
				}

				//map objects
				if ((row = mapObjRows.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> objs = row.tiles.Where(_gfx => xGFXDistanceQuery(_gfx, 13)).ToList();
					foreach (GFX obj in objs)
					{
						Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapObjects, obj.tile, true);
						Vector2 loc = _getDrawCoordinates(obj.x, row.y, c);
						loc = new Vector2(loc.X - (int)Math.Round(gfx.Width / 2.0) + 29, loc.Y - (gfx.Height - 28));
						sb.Draw(gfx, loc, Color.FromNonPremultiplied(255, 255, 255, _getAlpha(obj.x, row.y, c)));
					}
				}

				//roofs (after objects - for outdoor maps, which actually have roofs, this makes more sense)
				if ((row = roofRows.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> roofs = row.tiles.Where(_gfx => xGFXDistanceQuery(_gfx, 10)).ToList();
					foreach (GFX roof in roofs)
					{
						Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapWallTop, roof.tile, true);
						Vector2 loc = _getDrawCoordinates(roof.x, row.y, c);
						loc = new Vector2(loc.X - 2, loc.Y - 63);
						sb.Draw(gfx, loc, Color.FromNonPremultiplied(255, 255, 255, _getAlpha(roof.x, row.y, c)));
					}
				}
				
				//overlay tiles (counters, etc)
				if ((row = overlayTileRows.Find(_row => _row.y == rowIndex)).y == rowIndex && row.tiles != null)
				{
					List<GFX> tiles = row.tiles.Where(_gfx => xGFXDistanceQuery(_gfx, 10)).ToList();
					foreach (GFX tile in tiles)
					{
						Texture2D gfx = GFXLoader.TextureFromResource(GFXTypes.MapTiles, tile.tile, true);
						Vector2 loc = _getDrawCoordinates(tile.x, row.y, c);
						loc = new Vector2(loc.X - 2, loc.Y - 31);
						sb.Draw(gfx, loc, Color.White);
					}
				}

			}

			try
			{
				sb.End();
			}
			catch(InvalidOperationException)
			{
				sb.Dispose();
				sb = new SpriteBatch(Game.GraphicsDevice);
			}

			sb.Begin(SpriteSortMode.Deferred, World.Instance.MainPlayer.ActiveCharacter.RenderData.hidden ? BlendState.NonPremultiplied : _playerBlend);
			World.Instance.ActiveCharacterRenderer.Draw(sb, true);
			sb.End();

			GraphicsDevice.SetRenderTarget(null);
		}

		private void _drawMiniMap()
		{
			Texture2D miniMapText = GFXLoader.TextureFromResource(GFXTypes.PostLoginUI, 45, true);
			Character c = World.Instance.MainPlayer.ActiveCharacter;

			sb.Begin();
			for (int row = Math.Max(c.Y - 30, 0); row <= Math.Min(c.Y + 30, MapRef.Height); ++row)
			{
				for (int col = Math.Max(c.X - 30, 0); col <= Math.Min(c.Y + 30, MapRef.Width); ++col)
				{
					Rectangle miniMapRect = new Rectangle(0, 0, miniMapText.Width / 9, miniMapText.Height);
					bool isEdge = false;
					Vector2 loc = _getMiniMapDrawCoordinates(col, row, c);
					if (c.X == col && c.Y == row)
					{
						//draw orange thing
						miniMapRect.Offset((int) MiniMapGfx.Orange*miniMapRect.Width, 0);
					}
					else
					{
						TileInfo info = CheckCoordinates((byte) col, (byte) row);
						switch (info.ReturnValue)
						{
							case TileInfo.ReturnType.IsTileSpec:
								switch (info.Spec)
								{
									case TileSpec.Wall:
										miniMapRect.Offset((int)MiniMapGfx.Solid * miniMapRect.Width, 0);
										//draw block
										break;
									case TileSpec.BankVault:
									case TileSpec.ChairAll:
									case TileSpec.ChairDown:
									case TileSpec.ChairLeft:
									case TileSpec.ChairRight:
									case TileSpec.ChairUp:
									case TileSpec.ChairDownRight:
									case TileSpec.ChairUpLeft:
									case TileSpec.Chest:
										//draw exclamation
										miniMapRect.Offset((int)MiniMapGfx.Blue * miniMapRect.Width, 0);
										break;
									case TileSpec.MapEdge:
										isEdge = true;
										break;
								}
								break;
							case TileInfo.ReturnType.IsOtherNPC:
								//draw NPC - red or purple depending on type
								NPC npc;
								if ((npc = npcList.Find(_n => _n.X == col && _n.Y == row)) != null)
								{
									if (npc.Data.Type == NPCType.Aggressive || npc.Data.Type == NPCType.Passive)
									{
										miniMapRect.Offset((int)MiniMapGfx.Red * miniMapRect.Width, 0);
									}
									else
									{
										miniMapRect.Offset((int)MiniMapGfx.Purple * miniMapRect.Width, 0);
									}
								}
								break;
							case TileInfo.ReturnType.IsOtherPlayer:
								miniMapRect.Offset((int)MiniMapGfx.Green * miniMapRect.Width, 0);
								//draw Green
								break;
							case TileInfo.ReturnType.IsWarpSpec:
								if(info.Warp.door != 0)
									miniMapRect.Offset((int)MiniMapGfx.Blue * miniMapRect.Width, 0);
								break;
						}
					}

					if (!isEdge)
					{
						sb.Draw(miniMapText, loc,
							new Rectangle((int) MiniMapGfx.UpLine*miniMapRect.Width, 0, miniMapRect.Width, miniMapRect.Height),
							Color.FromNonPremultiplied(255, 255, 255, 128));
						sb.Draw(miniMapText, loc,
							new Rectangle((int)MiniMapGfx.LeftLine * miniMapRect.Width, 0, miniMapRect.Width, miniMapRect.Height),
							Color.FromNonPremultiplied(255, 255, 255, 128));
					}
					sb.Draw(miniMapText, loc, miniMapRect, Color.FromNonPremultiplied(255, 255, 255, 128));
				}
			}
			sb.End();
		}

		/// <summary>
		/// does the offset for tiles/items
		/// <para>(x * 32 - y * 32 + 288 - c.OffsetX), (y * 16 + x * 16 + 144 - c.OffsetY)</para>
		/// <para>Additional offsets for some gfx will need to be made - this Vector2 is a starting point with calculations required for ALL gfx</para>
		/// </summary>
		private Vector2 _getDrawCoordinates(int x, int y, Character c)
		{
			return new Vector2((x * 32) - (y * 32) + 288 - c.OffsetX, (y * 16) + (x * 16) + 144 - c.OffsetY);
		}

		private Vector2 _getMiniMapDrawCoordinates(int x, int y, Character c)
		{
			return new Vector2((x * 13) - (y * 13) + 288 - (c.X * 13 - c.Y * 13), (y * 7) + (x * 7) + 144 - (c.Y * 7 + c.X * 7));
		}
		
		private int _getAlpha(int objX, int objY, Character c)
		{
			bool enableTransition;
			//[TRANSITION]
			//Enabled=false #disable the fancy transition when changing maps
			if (World.Instance.Configuration.GetValue(ConfigStrings.Settings, ConfigStrings.ShowTransition, out enableTransition) && !enableTransition)
				return 255;

			//get greater of deltas between the map object and the character
			int metric = Math.Max(Math.Abs(objX - c.X), Math.Abs(objY - c.Y));
			const double TRANSITION_TIME_MS = 125.0; //1/8 second for transition on each tile metric

			int alpha;
			if (m_mapLoadTime == null || metric < m_transitionMetric || metric == 0)
				alpha = 255;
			else if (metric == m_transitionMetric)
			{
				double ms = (DateTime.Now - m_mapLoadTime.Value).TotalMilliseconds;
				alpha = (int)Math.Round((ms / TRANSITION_TIME_MS) * 255);
				if (ms / TRANSITION_TIME_MS >= 1)
				{
					m_mapLoadTime = DateTime.Now;
					m_transitionMetric++;
				}
			}
			else
				alpha = 0;

			return alpha;
		}

		/* DISPOSABLE INTERFACE OVERRIDES AND STUFF */
		public new void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing)
			{
				base.Dispose(false);return;
			}

			m_eventChangeMap.Dispose();
			m_eventChangeMap = null;

			foreach (EOCharacterRenderer cr in otherRenderers)
				cr.Dispose();

			lock (npcListLock)
			{
				foreach (NPC npc in npcList)
					npc.Dispose();
			}

			_mouseoverName.Dispose();
			_rtMapObjAbovePlayer.Dispose();
			_rtMapObjBelowPlayer.Dispose();
			_playerBlend.Dispose();
			sb.Dispose();
			_doorTimer.Dispose();

			base.Dispose(true);
		}
	}
}
