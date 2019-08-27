# Asp.net Core(2.2) MVC using Entity Framework

Project showcases external login mechanism for using options like Google(or can be Facebook, Microsoft) to log in user using OAuth protocol

<br/>

# Steps to get the project running

Pre-requisites:

>1. [.Net Core 2.2 SDK](https://www.microsoft.com/net/download/dotnet-core/2.2)
>2. [Visual Studio Code](https://code.visualstudio.com/) or Recommended - [Visual Studio Community editon version 15.9.1](https://visualstudio.microsoft.com/vs/community/) or later editor

<br/>

Clone the current repository locally as
 `git clone https://github.com/NileshSP/aspnetcoreoauth.git`

<br/>

Steps: using Visual Studio community edition editor
>1. Open the solution file (aspnetcoreoauth.sln) available in the root folder of the downloaded repository
>2. Await until the project is ready as per the status shown in taskbar which loads required packages in the background
>3. Hit -> F5 or select 'Debug -> Start Debugging' option to run the project

<br/>

Steps: using Visual Studio code editor
>1. Open the root folder of the downloaded repository 
>2. Await until the project is ready as per the status shown in taskbar which loads required packages in the background
>3. Open Terminal - 'Terminal -> New Terminal' and execute commands as `cd aspnetcoreoauth` & `dotnet build` & `dotnet run` sequentially
OR
>4. Hit -> F5 or select 'Debug -> Start Debugging' option to run the project

<br/>

Once the project is build and run, a browser page would be presented with navigation options on right wherein 'Websites data' option contains functionality related to data access from in-memory/sql database


![alt text](https://github.com/NileshSP/aspnetcoreoauth/blob/master/screenshot.gif "Working example..")
<br/>

# Root folder contents: 
>1. aspnetcoreoauth folder: contains frontend UI built using React.js(in ClientApp folder) and .Net Core Web Api endpoints
>2. aspnetcoreoauthTests folder: unit tests for Web Api Endpoints
>3. aspnetcoreoauth.sln solution file
>4. Readme.md file for project information
>5. aspnetcore.yml for CI/CD using github actions
