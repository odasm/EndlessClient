﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PELoaderLib;

namespace EOLib.Graphics.Test
{
    [TestClass, ExcludeFromCodeCoverage]
    public class GraphicsDependencyContainerTest
    {
        private readonly Dictionary<GFXTypes, IPEFile> _gfxFiles = new Dictionary<GFXTypes, IPEFile>();

        [TestMethod]
        public void RegistersDependencies_DoesRegistrations()
        {
            var unityContainer = new UnityContainer();
            var container = new GraphicsDependencyContainer();

            container.RegisterDependencies(unityContainer);

            Assert.AreNotEqual(0, unityContainer.Registrations.Count());
        }

        [TestMethod, ExpectedException(typeof(LibraryLoadException))]
        public void InitializeDependencies_PEFileError_ExpectIOExceptionIsThrownAsLibraryLoadException()
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<IPEFileCollection>(new InjectionFactory(c => CreatePEFileCollection()));
            var container = new GraphicsDependencyContainer();

            var file1Mock = new Mock<IPEFile>();
            file1Mock.Setup(x => x.Initialize()).Throws<IOException>();
            _gfxFiles.Add(GFXTypes.PreLoginUI, file1Mock.Object);

            container.InitializeDependencies(unityContainer);
        }

        [TestMethod, ExpectedException(typeof(LibraryLoadException))]
        public void InitializeDependencies_PEFileInitializeIsFalse_ExpectLibraryLoadException()
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<IPEFileCollection>(new InjectionFactory(c => CreatePEFileCollection()));
            var container = new GraphicsDependencyContainer();

            var file1Mock = new Mock<IPEFile>();
            file1Mock.Setup(x => x.Initialized).Returns(false);
            _gfxFiles.Add(GFXTypes.PreLoginUI, file1Mock.Object);

            container.InitializeDependencies(unityContainer);
        }

        [TestMethod]
        public void InitializeDependencies_InitializesGFXFiles()
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<IPEFileCollection>(new ContainerControlledLifetimeManager(),
                                                           new InjectionFactory(c => CreatePEFileCollection()));
            var container = new GraphicsDependencyContainer();

            var file1Mock = new Mock<IPEFile>();
            file1Mock.Setup(x => x.Initialized).Returns(true);
            var file2Mock = new Mock<IPEFile>();
            file2Mock.Setup(x => x.Initialized).Returns(true);
            var file3Mock = new Mock<IPEFile>();
            file3Mock.Setup(x => x.Initialized).Returns(true);

            _gfxFiles.Add(GFXTypes.PreLoginUI, file1Mock.Object);
            _gfxFiles.Add(GFXTypes.PostLoginUI, file2Mock.Object);
            _gfxFiles.Add(GFXTypes.MapTiles, file3Mock.Object);

            container.InitializeDependencies(unityContainer);

            Mock.Get(unityContainer.Resolve<IPEFileCollection>())
                .Verify(x => x.PopulateCollectionWithStandardGFX(), Times.Once);
            file1Mock.Verify(x => x.Initialize(), Times.Once);
            file2Mock.Verify(x => x.Initialize(), Times.Once);
            file3Mock.Verify(x => x.Initialize(), Times.Once);
        }

        private IPEFileCollection CreatePEFileCollection()
        {
            var collection = new Mock<IPEFileCollection>();
            collection.Setup(x => x.GetEnumerator()).Returns(_gfxFiles.GetEnumerator());
            return collection.Object;
        }
    }
}
