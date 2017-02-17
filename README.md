# Cofoundry.Plugins.Azure

[![Build status](https://ci.appveyor.com/api/projects/status/65bx24r2ugb1hoko?svg=true)](https://ci.appveyor.com/project/Cofoundry/cofoundry-plugins-azure)
[![NuGet](https://img.shields.io/nuget/v/Cofoundry.Plugins.Azure.svg)](https://www.nuget.org/packages/Cofoundry.Plugins.Azure/)
[![Gitter](https://img.shields.io/gitter/room/cofoundry-cms/cofoundry.svg)](https://gitter.im/cofoundry-cms/cofoundry)


This library is a plugin for [Cofoundry](https://www.cofoundry.org/). For more information on getting started with Cofoundry check out the [Cofoundry repository](https://github.com/cofoundry-cms/cofoundry).

## Overview

This library contains services, abstractions and helpers for running in an Azure environment. Principally this consists of:

- **AzureBlobFileService:** IFileStoreService for azure blog storage

## Settings

- **Cofoundry:Plugins:AzureBlobFileService:ConnectionString** The connection string to use when accessing files in blob storage
- **Cofoundry:Plugins:Azure:AutoRegisterServices:** Indicates whether to automatically register azure services. Defaults to true, but you may want to set this as false if developing locally 





