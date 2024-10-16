
# ShortsAI

ShortsAI is an AI-powered video creation platform designed to streamline video content creation with the help of artificial intelligence.

And it sucks

## Key Features
- **AI-Powered Script Generation**: Uses Chat GPT for auto-generating video scripts.
- **Text to Speech**: Integrates with Google Text To Speech API to convert generated scripts into natural-sounding voiceovers.
- **Image Generation**: Utilizes DALLE or DeepMind to create AI-generated images tailored to your content.
- **Seamless Video Assembly**: Leverages FFMPEG for assembling video components into high-quality outputs.

## Projects

### 1. Media Creator Functions
Backend functions that manage Azure blob storage and facilitate video generation. These functions are run on a local machine connected to the SQL database, ensuring fast and reliable processing.

### 2. Media Creator Site
An ASP.NET C# Server coupled with a React Client, designed with Google MUI for a modern and responsive user interface.

## Software Stack

### Server
- C#
- MVC Architecture for structured, maintainable code.

### Client
- React for dynamic, interactive UI components.
- Google MUI for material design-based styling.

### Video Generation
- **Script**: Chat GPT-powered script generation for engaging content.
- **Voice**: Google Text To Speech API for lifelike voiceover synthesis.
- **Images**: AI-generated visuals via DALLE or DeepMind integration.
- **Video Assembly**: FFMPEG for final video rendering and assembly.

### Database
- Custom Dapper Database Library for efficient data handling.
- Microsoft SQL Server as the primary database solution.

### Blob Storage
- Azure Blob Storage for scalable and secure media storage.

### YouTube Uploading
- Automates video uploads to YouTube using Selenium.
- **Important**: This feature is compatible only with a specific Chromium release (version X.X.X), due to certain limitations from Google.
- Ensure you are logged in as the YouTube user account for successful uploads.

## How to Run

### 1. Media Creator Functions
- Rename `temp.settings.json` to `local.settings.json`.
- Update `local.settings.json` with the required API keys and credentials.
- Add your `google-credentials.json`.
- Run the function app in Visual Studio or use the Azure Functions Core Tools by running `func start` in the project directory.

### 2. Media Creator Site
- Rename `appsettings.temp.json` to `appsettings.Development.json`.
- Update `appsettings.Development.json` with the necessary API keys and credentials.
- Run the site using Visual Studio or install the Azure Functions Core Tools and run `func start` in the project directory.

## Conclusion
ShortsAI combines cutting-edge AI technologies to simplify video creation, from scripting and voiceovers to image generation and final video assembly. Set it up and start generating engaging content with ease!
