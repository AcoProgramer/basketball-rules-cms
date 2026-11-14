# Basketball Rules CMS

A local WPF content management application for basketball rules. Created as an academic project and maintained in this portfolio by **Aleksandar Petrović**.

Administrators manage rules, images and rich-text descriptions; visitors browse the published content. The interface is primarily Serbian.

## Features

- Login with separate **Admin** and **Visitor** roles.
- Browse rules in a table and open their details.
- Create, edit, validate and delete basketball-rule entries.
- Edit descriptions with a RichTextBox: bold, italic, underline, font, size and color.
- Count words and display validation feedback for individual fields.
- Store structured content in XML, descriptions in individual RTF files and images in local folders.
- Generate five demonstration entries, their images and RTF descriptions on first launch.

## Technology

C#, **.NET 9 for Windows**, WPF, XML serialization and RTF documents. NuGet dependencies are declared in the project: FontAwesome5 and Notification.Wpf.

## Build and run

Requirements: Windows, the .NET 9 SDK and Visual Studio with the **.NET desktop development** workload, or the .NET CLI.

```powershell
dotnet restore BasketballRulesCms.sln
dotnet build BasketballRulesCms.sln -c Release
dotnet run --project BasketballRulesCms.App/BasketballRulesCms.App.csproj -c Release
```

Alternatively, open `BasketballRulesCms.sln` in Visual Studio, restore packages, build and run the application.

## Demonstration accounts

These are public, built-in accounts for the local coursework demonstration:

- **Admin:** username `admin`, password `admin123`.
- **Visitor:** username `visitor`, password `visitor123`.

Accounts are stored in a local XML file using a simplified coursework authentication model. This implementation is intended for a local demonstration, not deployment with real user credentials.

## Data and structure

```text
BasketballRulesCms.sln
BasketballRulesCms.App/
  Models/       Rule and user models
  Services/     XML repositories, RTF/images and initial data
  Views/        Login, list, details and add/edit screens
```

The application creates `Data/users.xml`, `Data/rules.xml`, images and RTF files beside its executable. Runtime data and build outputs are excluded from version control; a clean clone generates its demonstration content automatically.

## Academic context

This project was created as an academic WPF application for managing basketball-rule content. Its design separates domain models, storage services and application views.

The basketball descriptions are demonstration content for exercising the CMS, not an authoritative or maintained rules reference. Third-party packages retain their own licenses; this repository does not assign a new license to external material.
