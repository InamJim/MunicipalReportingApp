# Municipal Reporting App

A C# Windows Forms application for a municipal citizen portal. The project now includes:

- Report Issues – submit an issue with its location, category, description and optional attachments.
- Local Events and Announcements – browse upcoming community events, search by keyword/category/date, view priority notices and receive recommendations based on search activity.
- Service Request Status – still shown as a clearly labelled future Task 3 feature.

## Requirements and running the project

- Windows 10 or later
- .NET 8 SDK
- Visual Studio 2022 with the **.NET desktop development** workload (optional)

Open MunicipalReportingApp.csproj in Visual Studio, or run the following from the project folder on Windows:

```powershell
dotnet restore
dotnet build
dotnet run
```

This is a net8.0-windows WinForms application and must run on Windows (or a Windows virtual machine).

## Project structure

```text
MunicipalReportingApp/
├── MunicipalReportingApp.csproj
├── Program.cs
├── Models/
│   ├── Issue.cs
│   ├── LocalEvent.cs
│   └── EventSearchRecord.cs
├── Data/
│   ├── IssueRepository.cs
│   ├── EventRepository.cs
│   └── EventRecommendationEngine.cs
└── Forms/
    ├── MainMenuForm.cs
    ├── ReportIssuesForm.cs
    └── LocalEventsForm.cs
```

## Main Menu

The main menu offers the three requested options. "Report Issues" continues to open the existing Part 1 form, "Local Events and Announcements" now opens the new events form, and "Service Request Status" remains disabled because that functionality belongs to Task 3.

## Local Events and Announcements

The events form provides a searchable, chronological event list presented in cards. Each card displays the title, category, date/time, location, description, and whether the item is an announcement. The right-hand panels display recommendations, highest-priority announcements, and recent searches. Double-click an event or announcement to see its full details.

To search, enter an optional keyword, choose a category or "All categories", and optionally enable "Use range" to filter between two dates. The keyword is matched against the title, category, location and description. Results only include upcoming items and are ordered by date. Use "Clear" to restore the full upcoming list.

The sample catalogue is deliberately illustrative. It contains 12 demonstration events and notices relative to the current date; it is **not an official or live feed** from any municipality. The two maintenance/collection notices are explicitly marked as demonstration notices in their descriptions. Replace or connect the in-memory seed data to an approved municipal data source before real-world deployment.


## Existing issue reporting

The existing Part 1 **Report Issues** form is retained. It validates the required location/category/description fields, supports optional file attachments, shows progress feedback and stores submissions in an in-memory List<Issue> for the lifetime of the application. Issue and event information is currently session-only and is cleared when the application closes; persistence is a possible next enhancement.

## Suggested manual checks

1. Launch the app and verify that Report Issues and Local Events are enabled while Service Request Status is still labelled for Task 3.
2. Open Local Events, inspect the chronological cards, and double-click a card to display its details.
3. Search for a word such as "market"; change the category to Environment; and try a date range that includes no seeded events.
4. Perform several searches and confirm that recent searches are shown and the recommendations respond to categories/keywords you searched.
5. Confirm priority announcements are listed ahead of lower-priority items, then clear the search to restore the upcoming list.

## Limitations

- Event and issue data are stored in memory only.
- Events are sample entries rather than official municipal information.
- Service Request Status is reserved for Task 3.
- The app has not been connected to a remote events API or an authentication/administration system.
