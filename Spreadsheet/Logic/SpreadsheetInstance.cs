using EithonBot.Spreadsheet.Handler;
using EithonBot.Spreadsheet.Models;
using EithonBot.Spreadsheet.NewFolder;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace EithonBot.Spreadsheet.Logic
{
    public class SpreadsheetInstance
    {
        public static SpreadsheetInstance Instance { get; private set; }

        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Discord integration";

        public Models.Spreadsheet Spreadsheet { get; private set; }
        internal Dictionary<string, DatabaseColumn> DatabaseColumns { get; private set; }
        internal Dictionary<string, DatabaseRow> DatabaseRows { get; private set; }
        internal Dictionary<string, DatabaseColumn> PartiesColumns { get; private set; }

        public SupportMethods SupportMethods { get; private set; }
        public CommandsParser CommandsParser { get; private set; }
        public ReactionsParser ReactionsParser { get; private set; }

        public SheetsService Service { get; private set; }

        static SpreadsheetInstance()
        {
            Instance = new SpreadsheetInstance("1pLMcQ7Uxha4g3c_poI7YTzZXla7omFwhQRUiCg8IzKI", "Guild database", "A", 3, "NW Parties", 3);
        }

        private SpreadsheetInstance(string spreadsheetId, string databaseSheet, string databaseFamilyNamesColumn, int databaseHeadersRow, string partiesSheet, int partiesHeadersRow)
        {
            UserCredential credential;
            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            Service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            Spreadsheet = new Models.Spreadsheet(spreadsheetId)
            {
                BDOMembersSheet = new DatabaseSheet(databaseSheet),
                BDOPartiesSheet = new DatabaseSheet(partiesSheet)
            };

            SupportMethods = new SupportMethods(Service, Spreadsheet);
            PopulateSpreadsheetModel(databaseFamilyNamesColumn, databaseHeadersRow, partiesHeadersRow);

            CommandsParser = new CommandsParser(Service, Spreadsheet, SupportMethods);
            ReactionsParser = new ReactionsParser(Service, Spreadsheet, SupportMethods);
        }

        private void PopulateSpreadsheetModel(string databaseFamilyNamesColumn, int databaseHeadersRow, int partiesHeadersRow)
        {
            var databaseSheet = Spreadsheet.BDOMembersSheet;
            databaseSheet.ColumnIdentifiersRow = databaseHeadersRow;
            databaseSheet.RowIdentifiersColumn = databaseFamilyNamesColumn;
            databaseSheet.DatabaseRows = SupportMethods.FetchAllDatabaseRowsFromSpreadsheet(databaseFamilyNamesColumn);
            databaseSheet.DatabaseColumns = SupportMethods.GetColumns(databaseSheet, databaseHeadersRow);

            var partiesSheet = Spreadsheet.BDOPartiesSheet;
            partiesSheet.ColumnIdentifiersRow = partiesHeadersRow;
            partiesSheet.DatabaseColumns = SupportMethods.GetColumns(partiesSheet, databaseHeadersRow);
        }
    }
}
