// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EOLib.Domain;
using EOLib.IO.Repositories;
using EOLib.Net.API;

namespace EOLib.IO.Actions
{
	public class FileRequestActions : IFileRequestActions
	{
		private readonly INumberEncoderService _numberEncoderService;
		private readonly IFileChecksumProvider _fileChecksumProvider;
		private readonly IPubFileRepository _pubFileRepository;
		private readonly IMapFileRepository _mapFileRepository;

		public FileRequestActions(INumberEncoderService numberEncoderService,
								  IFileChecksumProvider fileChecksumProvider,
								  IPubFileRepository pubFileRepository,
								  IMapFileRepository mapFileRepository)
		{
			_numberEncoderService = numberEncoderService;
			_fileChecksumProvider = fileChecksumProvider;
			_pubFileRepository = pubFileRepository;
			_mapFileRepository = mapFileRepository;
		}

		public bool NeedsFile(InitFileType fileType, short optionalID = 0)
		{
			if (fileType == InitFileType.Map)
				return NeedMap(optionalID);
			
			return NeedPub(fileType);
		}

		public async Task GetMapFromServer(short mapID)
		{
			await Task.FromResult(false);
		}

		public async Task GetItemFileFromServer()
		{
			await Task.FromResult(false);
		}

		public async Task GetNPCFileFromServer()
		{
			await Task.FromResult(false);
		}

		public async Task GetSpellFileFromServer()
		{
			await Task.FromResult(false);
		}

		public async Task GetClassFileFromServer()
		{
			await Task.FromResult(false);
		}

		private bool NeedMap(short mapID)
		{
			try
			{
				var expectedChecksum = _numberEncoderService.DecodeNumber(_fileChecksumProvider.MapChecksums[mapID]);
				var expectedLength = _fileChecksumProvider.MapLengths[mapID];

				var actualChecksum = _numberEncoderService.DecodeNumber(_mapFileRepository.MapFiles[mapID].Properties.Checksum);
				var actualLength = _mapFileRepository.MapFiles[mapID].Properties.FileSize;

				return expectedChecksum != actualChecksum || expectedLength != actualLength;
			}
			catch (KeyNotFoundException) { return true; } //ID not in a dictionary
		}

		private bool NeedPub(InitFileType fileType)
		{
			switch (fileType)
			{
				case InitFileType.Item:
					return _pubFileRepository.ItemFile == null ||
					       _fileChecksumProvider.EIFChecksum != _pubFileRepository.ItemFile.Rid ||
					       _fileChecksumProvider.EIFLength != _pubFileRepository.ItemFile.Len;
				case InitFileType.Npc:
					return _pubFileRepository.NPCFile == null ||
					       _fileChecksumProvider.ENFChecksum != _pubFileRepository.NPCFile.Rid ||
					       _fileChecksumProvider.ENFLength != _pubFileRepository.NPCFile.Len;
				case InitFileType.Spell:
					return _pubFileRepository.SpellFile == null ||
					       _fileChecksumProvider.ESFChecksum != _pubFileRepository.SpellFile.Rid ||
					       _fileChecksumProvider.ESFLength != _pubFileRepository.SpellFile.Len;
				case InitFileType.Class:
					return _pubFileRepository.ClassFile == null ||
					       _fileChecksumProvider.ECFChecksum != _pubFileRepository.ClassFile.Rid ||
					       _fileChecksumProvider.ECFLength != _pubFileRepository.ClassFile.Len;
				default:
					throw new ArgumentOutOfRangeException("fileType", fileType, null);
			}
		}
	}
}