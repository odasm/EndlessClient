﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using System.Linq;
using System.Windows.Forms;
using EOLib.Config;
using EOLib.DependencyInjection;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Practices.Unity;

namespace EndlessClient.GameExecution
{
    public abstract class GameRunnerBase : IGameRunner
    {
        private readonly IUnityContainer _unityContainer;

        protected GameRunnerBase(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public virtual bool SetupDependencies()
        {
            var registrar = new DependencyRegistrar(_unityContainer);

            registrar.RegisterDependencies(DependencyContainerProvider.DependencyContainers);

            try
            {
                registrar.InitializeDependencies(
                    DependencyContainerProvider.DependencyContainers
                        .OfType<IInitializableContainer>()
                        .ToArray());
            }
            catch (ConfigLoadException cle)
            {
                ShowErrorMessage(cle.Message, "Error loading config file!");
                return false;
            }
            catch (DataFileLoadException dfle)
            {
                ShowErrorMessage(dfle.Message, "Error loading data files!");
                return false;
            }
            catch (DirectoryNotFoundException dnfe)
            {
                ShowErrorMessage(dnfe.Message, "Missing required directory");
                return false;
            }
            catch (FileNotFoundException fnfe)
            {
                ShowErrorMessage(fnfe.Message, "Missing required file");
                return false;
            }
            catch (LibraryLoadException lle)
            {
                var message =
                    $"There was an error loading GFX{(int) lle.WhichGFX:000}.EGF : {lle.WhichGFX}. Place all .GFX files in .\\gfx\\. The error message is:\n\n\"{lle.Message}\"";
                ShowErrorMessage(message, "GFX Load Error");
                return false;
            }

            return true;
        }

        private void ShowErrorMessage(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public virtual void RunGame()
        {
            var game = _unityContainer.Resolve<IEndlessGame>();
            game.Run();
        }
    }
}
