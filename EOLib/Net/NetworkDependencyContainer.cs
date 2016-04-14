﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net.Communication;
using EOLib.Net.Connection;
using EOLib.Net.PacketProcessing;
using Microsoft.Practices.Unity;

namespace EOLib.Net
{
	public class NetworkDependencyContainer : IInitializableContainer
	{
		public void RegisterDependencies(IUnityContainer container)
		{
			container.RegisterType<IPacketEncoderService, PacketEncoderService>();
			container.RegisterType<IPacketSequenceService, PacketSequenceService>();
			container.RegisterType<IInitDataGeneratorService, InitDataGeneratorService>();
			container.RegisterType<IHashService, HashService>();
			//the repository is a "disposer" of the NetworkClient (so it gets cleaned up later)
			container.RegisterType<INetworkClientDisposer, NetworkClientRepository>(new ContainerControlledLifetimeManager());

			container.RegisterType<INetworkClientFactory, NetworkClientFactory>();

			container.RegisterType<INetworkClientRepository, NetworkClientRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<INetworkClientProvider, NetworkClientRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IPacketQueueRepository, PacketQueueRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IPacketQueueProvider, PacketQueueRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IPacketEncoderRepository, PacketEncoderRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ISequenceRepository, SequenceRepository>(new ContainerControlledLifetimeManager());

			container.RegisterType<IPacketProcessorActions, PacketProcessActions>();
			container.RegisterType<INetworkConnectionActions, NetworkConnectionActions>();
			
			//must be a singleton: tracks a thread and has internal state.
			container.RegisterType<IBackgroundReceiveActions, BackgroundReceiveActions>(new ContainerControlledLifetimeManager());
		}

		public void InitializeDependencies(IUnityContainer container)
		{
			//create the packet queue object (can be recreated later)
			var packetQueueRepository = container.Resolve<IPacketQueueRepository>();
			packetQueueRepository.PacketQueue = new PacketQueue();

			//create the client object (can be recreated later)
			var clientRepo = container.Resolve<INetworkClientRepository>();
			var clientFactory = container.Resolve<INetworkClientFactory>();
			clientRepo.NetworkClient = clientFactory.CreateNetworkClient();
		}
	}
}
