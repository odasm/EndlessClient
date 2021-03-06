// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

#pragma once

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include <msclr\marshal.h>

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Drawing;
using namespace EOLib::Graphics;

namespace EOCLI
{
	public ref class GFXLoaderCLI : INativeGraphicsLoader
	{
	public:
		GFXLoaderCLI();
		~GFXLoaderCLI();
		!GFXLoaderCLI();

		virtual Bitmap^ LoadGFX(GFXTypes file, int resourceVal);

	private:
		HANDLE LoadLibraryModule(GFXTypes file);
		HANDLE LoadLibraryImage(GFXTypes file, HANDLE library, int resourceVal);

		Dictionary<GFXTypes, int>^ m_libraries;
		bool m_isDisposed;
	};
}
