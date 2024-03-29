﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PettingZoo.Core.Generator;
using PettingZoo.Core.Settings;
using PettingZoo.Tapeti.AssemblyLoader;
using PettingZoo.Tapeti.NuGet;
using PettingZoo.Tapeti.UI.ClassSelection;
using PettingZoo.Tapeti.UI.PackageSelection;
using PettingZoo.WPF.ProgressWindow;
using Serilog;

namespace PettingZoo.Tapeti
{
    public class TapetiClassLibraryExampleGenerator : IExampleGenerator
    {
        private readonly ILogger logger;


        public TapetiClassLibraryExampleGenerator(ILogger logger)
        {
            this.logger = logger;
        }


        public void Select(object? ownerWindow, Action<IExample> onExampleSelected)
        {
            var packageManager = new NuGetPackageManager(logger)
                .WithSourcesFrom(Path.Combine(PettingZooPaths.InstallationRoot, @"nuget.config"))
                .WithSourcesFrom(Path.Combine(PettingZooPaths.AppDataRoot, @"nuget.config"));

            var viewModel = new PackageSelectionViewModel(packageManager);
            var selectionWindow = new PackageSelectionWindow(viewModel)
            {
                Owner = ownerWindow as Window
            };

            viewModel.Select += (_, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var windowBounds = selectionWindow.RestoreBounds;
                    selectionWindow.Close();

                    var progressWindow = new ProgressWindow(TapetiClassLibraryExampleGeneratorStrings.ProgressWindowTitle);
                    progressWindow.Left = windowBounds.Left + (windowBounds.Width - progressWindow.Width) / 2;
                    progressWindow.Top = windowBounds.Top + (windowBounds.Height - progressWindow.Height) / 2;
                    progressWindow.Show();

                    var cancellationToken = progressWindow.CancellationToken;

                    Task.Run(async () =>
                    {
                        try
                        {
                            var assemblies = await args.Assemblies.GetAssemblies(progressWindow, cancellationToken);

                            // var classes = 
                            var examples = LoadExamples(assemblies);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                progressWindow.Close();
                                progressWindow = null;

                                var classSelectionViewModel = new ClassSelectionViewModel(examples);
                                var classSelectionWindow = new ClassSelectionWindow(classSelectionViewModel)
                                {
                                    Top = windowBounds.Top,
                                    Left = windowBounds.Left,
                                    Width = windowBounds.Width,
                                    Height = windowBounds.Height
                                };

                                classSelectionViewModel.Select += (_, example) =>
                                {
                                    classSelectionWindow.Close();
                                    onExampleSelected(example);
                                };

                                classSelectionWindow.ShowDialog();
                            });
                        }
                        catch (Exception e)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                // ReSharper disable once ConstantConditionalAccessQualifier - if I remove it, there's a "Dereference of a possibly null reference" warning instead
                                progressWindow?.Close();

                                if (e is not OperationCanceledException)
                                    MessageBox.Show($"Error while loading assembly: {e.Message}", "Petting Zoo - Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                            });
                        }
                    }, CancellationToken.None);
                });
            };

            selectionWindow.ShowDialog();
        }


        private static IEnumerable<IClassTypeExample> LoadExamples(IEnumerable<IPackageAssembly> assemblies)
        {
            var assemblyParser = new AssemblyParser.AssemblyParser(
                PettingZooPaths.AppDataAssemblies,
                PettingZooPaths.InstallationAssemblies
                );

            return assemblies
                .SelectMany(a =>
                {
                    using var stream = a.GetStream();
                    return assemblyParser.GetExamples(stream).ToArray();
                })
                .ToArray();
        }
    }
}
