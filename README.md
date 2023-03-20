# ULTRAINTERFACE
A UI Library for ULTRAKILL

Currently still in development, come back later for a semi-working product :D

# Development Notes
These are just some things to take note of when developing ULTRAINTERFACE, and using a local install of the library:

- Local installs are always version `0.0.1`, so make sure to depend on that version when wanting to use a local install
- Building the AssetBundle isn't strictly necessary, as the git repo contains the bundle. This also has the benefit of causing a merge conflict if trying to use different bundles, forcing everyone to use the same bundle and keeping everyone on the same page
- [Script Engine](https://github.com/BepInEx/BepInEx.Debug#scriptengine) is ***HIGHLY*** recommended when both developing and using ULTRAINTERFACE, as it speeds up workflow tremendously. If you aren't using Script Engine when working on any BepInEx mod, you're missing out xD
- To test changes made to the Library, you should use the Example Mod, as this is both easy to do using the `build.ps1` script, and also lets other users see how to use your UI Elements from within this one repo. It also means that if you change something, it won't break the Example mod

# Building
Depending on what it is you want to build, this is either a very easy process or a bit more of an involved process.

Because ULTRAINTERFACE consists of both the actual library and an Asset Bundle, you'll be required to set up a Unity Project if you want to add new UI elements.

## Building Just The Library

### Requirements:
- [Powershell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.3) (If on Windows, the pre-install version will work fine)
- [NuGet](https://www.nuget.org/downloads)
- [DotNET SDK >= 4.7](https://dotnet.microsoft.com/en-us/download) (Version 6 is recommended, as it is required for building the Asset Bundle)

**NOTE:** Specifically `nuget.exe` needs to be downloaded, and its path needs to be specified in the `config.ps1` script. The built-in NuGet manager that comes with the DotNET SDK will not suffice.

### Building:
If you don't wish to add any new UI Elements, building just the library is quite straightforward:
- First, clone the repo
- Setup any vars needed in the `config.ps1` script located in the repo's root
- Run `build.ps1`

```
git clone https://github.com/BobbyShmurner/ULTRAINTERFACE/
cd ULTRAINTERFACE
./build.ps1
```

If you have correctly configured `config.ps1`, ULTRAINTERFACE should have been built and installed automatically. Hooray! :D

To build the example mod, run `build.ps1 -BuildExampleMod` to build and automatically install the Example Mod

Feel free to run `build.ps1 -Help` for a full list of flags