## Pre-requisites

The following software needs to be installed on your machine to build and run the solution.
Further steps provide installation examples are using `winget` utility from Microsoft for Windows 11+.

- [.NET 9](https://dotnet.microsoft.com/en-us/download/dotnet)

  Installation on Windows11+:

      winget install Microsoft.DotNet.SDK.9

- [NodeJs](https://nodejs.org/en/download)

  Installation of the LATEST NodeJs version on Windows11+:

      winget install OpenJS.NodeJS

  OR installation of LTS NodeJs version on Windows11+:

      winget install OpenJS.NodeJS.LTS

  OR use something like Node Version Manager (NVM) [nvm](https://github.com/coreybutler/nvm-windows)

      winget install CoreyButler.NVMforWindows
      nvm install latest
      nvm use latest

## Running the solution

- Open terminal in [./src/Blazor.Server](./src/Blazor.Server) folder
- Run `npm install` to install dependencies
- Run `dotnet dev-certs https` to install development certificates if required
- Run `dotnet resotre` to restore dependencies
- Run `dotnet watch` to start the solution in watch mode: both dotnet files and css files are watched  
  (current simplified implementation watches only [./src/Blazor.Server/app.css](./src/Blazor.Server/app.css) file)
- Open browser and navigate to `https://localhost:5000`
