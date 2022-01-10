using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using PettingZoo.Core.Settings;
using PettingZoo.Tapeti.AssemblyLoader;
using PettingZoo.Tapeti.NuGet;
using PettingZoo.WPF.ViewModel;

namespace PettingZoo.Tapeti.UI.PackageSelection
{
    public enum PackageSelectionSource
    {
        Assembly,
        NuGet
    }


    public class PackageSelectionViewModel : BaseViewModel
    {
        private readonly INuGetPackageManager nuGetPackageManager;

        private readonly DelegateCommand selectCommand;

        private PackageSelectionSource packageSelectionSource = PackageSelectionSource.Assembly;
        private string assemblyFilename = "";
        private readonly DelegateCommand assemblyBrowse;

        private string nuGetSearchTerm = "";
        private bool nuGetIncludePrerelease;
        private INuGetPackageSource? selectedNuGetSource;
        private INuGetPackage? selectedPackage;
        private INuGetPackageVersion? selectedVersion;

        private string packagesStatus = "";
        private Visibility packagesStatusVisibility = Visibility.Collapsed;


        public ICommand SelectCommand => selectCommand;


        public PackageSelectionSource PackageSelectionSource
        {
            get => packageSelectionSource;
            set => SetField(ref packageSelectionSource, value, otherPropertiesChanged: new[] { nameof(PackageSelectionSourceAssembly), nameof(PackageSelectionSourceNuGet) });
        }


        public bool PackageSelectionSourceAssembly
        {
            get => PackageSelectionSource == PackageSelectionSource.Assembly;
            set
            {
                if (value)
                    PackageSelectionSource = PackageSelectionSource.Assembly;
            }
        }

        public bool PackageSelectionSourceNuGet
        {
            get => PackageSelectionSource == PackageSelectionSource.NuGet;
            set
            {
                if (value)
                    PackageSelectionSource = PackageSelectionSource.NuGet;
            }
        }


        public string AssemblyFilename
        {
            get => assemblyFilename;
            set
            {
                if (!SetField(ref assemblyFilename, value))
                    return;

                if (!string.IsNullOrEmpty(value))
                    PackageSelectionSource = PackageSelectionSource.Assembly;
            }
        }

        public ICommand AssemblyBrowse => assemblyBrowse;


        public static string HintNuGetSources => string.Format(PackageSelectionStrings.HintNuGetSources, PettingZooPaths.InstallationRoot, PettingZooPaths.AppDataRoot);

        public string NuGetSearchTerm
        {
            get => nuGetSearchTerm;
            set
            {
                if (!SetField(ref nuGetSearchTerm, value, otherPropertiesChanged: new[] { nameof(NuGetSearchTermPlaceholderVisibility) }))
                    return;

                if (!string.IsNullOrEmpty(value))
                    PackageSelectionSource = PackageSelectionSource.NuGet;
            }
        }

        public bool NuGetIncludePrerelease
        {
            get => nuGetIncludePrerelease;
            set => SetField(ref nuGetIncludePrerelease, value);
        }

        public Visibility NuGetSearchTermPlaceholderVisibility => string.IsNullOrEmpty(NuGetSearchTerm) ? Visibility.Visible : Visibility.Hidden;
        public IReadOnlyList<INuGetPackageSource> NuGetSources => nuGetPackageManager.Sources;

        public INuGetPackageSource? SelectedNuGetSource
        {
            get => selectedNuGetSource;
            set
            {
                if (!SetField(ref selectedNuGetSource, value))
                    return;

                Packages.Clear();
                SelectedPackage = null;
            }
        }

        public ObservableCollectionEx<INuGetPackage> Packages { get; } = new();
        public ObservableCollectionEx<INuGetPackageVersion> Versions { get; } = new();


        public string PackagesStatus
        {
            get => packagesStatus;
            set => SetField(ref packagesStatus, value);
        }


        public Visibility PackagesStatusVisibility
        {
            get => packagesStatusVisibility;
            set => SetField(ref packagesStatusVisibility, value);
        }


        public INuGetPackage? SelectedPackage
        {
            get => selectedPackage;
            set
            {
                if (!SetField(ref selectedPackage, value))
                    return;

                Versions.Clear();
                SelectedVersion = null;
            }
        }


        public INuGetPackageVersion? SelectedVersion
        {
            get => selectedVersion;
            set => SetField(ref selectedVersion, value);
        }


        public class SelectPackageEventArgs
        {
            public IPackageAssemblies Assemblies { get; }


            public SelectPackageEventArgs(IPackageAssemblies assemblies)
            {
                Assemblies = assemblies;
            }
        }


        public event EventHandler<SelectPackageEventArgs>? Select;


        public PackageSelectionViewModel(INuGetPackageManager nuGetPackageManager)
        {
            this.nuGetPackageManager = nuGetPackageManager;

            selectCommand = new DelegateCommand(SelectExecute, SelectCanExecute);

            assemblyBrowse = new DelegateCommand(AssemblyBrowseExecute);


            // TODO remember source
            if (nuGetPackageManager.Sources.Count > 0)
                selectedNuGetSource = nuGetPackageManager.Sources[^1];

            var propertyChangedObservable = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => PropertyChanged += h,
                    h => PropertyChanged -= h);

            propertyChangedObservable
                .Where(e => e.EventArgs.PropertyName is nameof(NuGetSearchTerm) or nameof(NuGetIncludePrerelease) or nameof(SelectedNuGetSource))
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(SynchronizationContext.Current!)
                .Subscribe(_ => NuGetSearch());

            propertyChangedObservable
                .Where(e => e.EventArgs.PropertyName == nameof(SelectedPackage))
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOn(SynchronizationContext.Current!)
                .Subscribe(_ => NuGetGetVersions());


            propertyChangedObservable
                .Where(e => e.EventArgs.PropertyName is nameof(PackageSelectionSource) or nameof(AssemblyFilename) or nameof(SelectedVersion))
                .ObserveOn(SynchronizationContext.Current!)
                .Subscribe(_ => selectCommand.RaiseCanExecuteChanged());
        }


        private void SelectExecute()
        {
            IPackageAssemblies? assemblies = PackageSelectionSource switch
            {
                PackageSelectionSource.Assembly => !string.IsNullOrWhiteSpace(AssemblyFilename) ? new FilePackageAssemblies(AssemblyFilename) : null,
                PackageSelectionSource.NuGet => SelectedVersion != null ? new NuGetPackageAssemblies(SelectedVersion) : null,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            if (assemblies != null)
                Select?.Invoke(this, new SelectPackageEventArgs(assemblies));
        }


        private bool SelectCanExecute()
        {
            return PackageSelectionSource switch
            {
                PackageSelectionSource.Assembly => !string.IsNullOrWhiteSpace(AssemblyFilename),
                PackageSelectionSource.NuGet => SelectedVersion != null,
                _ => false
            };
        }


        private void AssemblyBrowseExecute()
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = PackageSelectionStrings.AssemblyFileFilter
            };

            if (!dialog.ShowDialog().GetValueOrDefault())
                return;

            AssemblyFilename = dialog.FileName;
        }


        private CancellationTokenSource? nuGetSearchCancellationTokenSource;

        private void NuGetSearch()
        {
            if (SelectedNuGetSource == null)
                return;

            nuGetSearchCancellationTokenSource?.Cancel();

            nuGetSearchCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = nuGetSearchCancellationTokenSource.Token;

            var source = SelectedNuGetSource;
            var searchTerm = NuGetSearchTerm;
            var includePrerelease = NuGetIncludePrerelease;

            SelectedPackage = null;
            Packages.Clear();

            PackagesStatus = PackageSelectionStrings.Loading;
            PackagesStatusVisibility = Visibility.Visible;

            Task.Run(async () =>
            {
                try
                {
                    var packages = await source.Search(searchTerm, includePrerelease, cancellationToken);

                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Packages.ReplaceAll(packages);
                        SelectedPackage = null;

                        PackagesStatus = "";
                        PackagesStatusVisibility = Visibility.Collapsed;

                        Versions.Clear();
                        SelectedVersion = null;
                    });
                }
                catch (OperationCanceledException)
                {
                    // By design...
                }
                catch (Exception e)
                {
                    PackagesStatus = e.Message;
                    PackagesStatusVisibility = Visibility.Visible;
                }
            }, CancellationToken.None);
        }


        private CancellationTokenSource? nuGetGetVersionsCancellationTokenSource;

        private void NuGetGetVersions()
        {
            if (SelectedPackage == null)
                return;

            nuGetGetVersionsCancellationTokenSource?.Cancel();

            nuGetGetVersionsCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = nuGetGetVersionsCancellationTokenSource.Token;

            var package = SelectedPackage;

            Task.Run(async () =>
            {
                try
                {
                    var versions = await package.GetVersions(cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        Versions.ReplaceAll(versions);
                        SelectedVersion = versions.Count > 0 ? versions[0] : null;
                    });
                }
                catch (OperationCanceledException)
                {
                    // By design...
                }
            }, CancellationToken.None);
        }
    }


    public class DesignTimePackageSelectionViewModel : PackageSelectionViewModel
    {
        public DesignTimePackageSelectionViewModel() : base(new DesignTimeNuGetPackageManager())
        {
            Packages.ReplaceAll(new []
            {
                new DesignTimeNuGetPackage("Tapeti", "Shameless plug", "M. van Renswoude", "2.8"),
                new DesignTimeNuGetPackage("Messaging.Example", "Some messaging package with a very long description to test the text trimming. It should be very very very very very very very very very very very long indeed.", "Anonymoose", "0.9")
            });

            PackagesStatus = @"This is a very long status message, which is not unreasonable since exceptions can occur while fetching NuGet packages and they will be displayed here.";
            PackagesStatusVisibility = Visibility.Visible;
        }


        private class DesignTimeNuGetPackageManager : INuGetPackageManager
        {
            public IReadOnlyList<INuGetPackageSource> Sources { get; } = new[]
            {
                new DesignTimeNuGetPackageSource("nuget.org")
            };
        }


        private class DesignTimeNuGetPackageSource : INuGetPackageSource
        {
            public string Name { get; }


            public DesignTimeNuGetPackageSource(string name)
            {
                Name = name;
            }


            public Task<IReadOnlyList<INuGetPackage>> Search(string searchTerm, bool includePrerelease, CancellationToken cancellationToken)
            {
                return Task.FromResult(Array.Empty<INuGetPackage>() as IReadOnlyList<INuGetPackage>);
            }
        }


        private class DesignTimeNuGetPackage : INuGetPackage
        {
            public string Title { get; }
            public string Description { get; }
            public string Authors { get; }
            public string Version { get; }


            public DesignTimeNuGetPackage(string title, string description, string authors, string version)
            {
                Title = title;
                Description = description;
                Authors = authors;
                Version = version;
            }


            public Task<IReadOnlyList<INuGetPackageVersion>> GetVersions(CancellationToken cancellationToken)
            {
                return Task.FromResult(Array.Empty<INuGetPackageVersion>() as IReadOnlyList<INuGetPackageVersion>);
            }
        }
    }
}
