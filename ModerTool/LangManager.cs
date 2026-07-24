using System;
using System.Collections.Generic;

namespace ModerTool
{
    public static class LangManager
    {
        public static bool IsRussian { get; private set; } = true;
        public static event Action? LanguageChanged;

        private static readonly Dictionary<string, string> RuStrings = new()
        {
            { "NavHome", "Home" }, { "NavProgramm", "Programm" }, { "NavAccounts", "Accounts" },
            { "NavLogs", "Logs" }, { "NavGameFolder", "GameFolder" }, { "NavLinks", "Links" }, { "NavExit", "Exit" },

            { "HeaderHome", "HOME" }, { "HeaderProgramm", "PROGRAMM" }, { "HeaderAccounts", "ACCOUNTS" },
            { "HeaderLogs", "LOGS" }, { "HeaderGameFolder", "GAMEFOLDER" }, { "HeaderLinks", "LINKS" },

            { "HomeTitle", "Утилита удаленного контроля" },
            { "HomeDesc", "Запустите AnyDesk для подключения к рабочему столу игрока." },
            { "HomeBtn", "Запустить AnyDesk" },

            { "ProgSysFolders", "Системные папки" }, { "ProgUtils", "Утилиты" },
            { "ProgStore", "Store" }, { "ProgAppSwitched", "AppSwitched" },
            { "ProgHistory", "History" }, { "ProgPrefetch", "Prefetch" }, { "ProgTemp", "Temp" },
            { "ProgRecent", "Recent" }, { "ProgRecentAD", "RecentAD" },
            { "ProgLastActivity", "LastActivityView" }, { "ProgShellBag", "ShellBag" },
            { "ProgEverything", "Everything" }, { "ProgUSBDeview", "USBDeview" }, { "ProgProcessHacker", "ProcessHacker" },

            { "AccScan", "Сканировать Steam" }, { "AccExport", "Выгрузить SteamID" },
            { "AccSteamID", "Steam64ID" }, { "AccName", "Name" }, { "AccLevel", "Level" }, { "AccVAC", "VAC" },

            { "AccStatusLoading", "Загрузка..." }, { "AccStatusUnknown", "Неизвестно" },
            { "AccStatusHidden", "Скрыт" }, { "AccStatusClean", "Чисто" }, { "AccStatusError", "Ошибка" },

            { "LogsTitle", "Logs" },

            // Game Folder Section
            { "GameScanBtn", "Сканировать игры" },
            { "GameOpenBtn", "Открыть" },

            { "LogStarted", "SD Checker запущен. Готов к работе." },
            { "LogLangChanged", "Язык интерфейса изменен." },
            { "LogConfigCreated", "Создан файл settings.json. Настройте его и перезапустите программу." },
            { "LogConfigCreateError", "Ошибка создания конфига: " },
            { "LogConfigNoKey", "ВНИМАНИЕ: Steam API Ключ не указан в settings.json!" },
            { "LogConfigLoaded", "Конфигурация успешно загружена." },
            { "LogConfigReadError", "Ошибка чтения settings.json: " },
            { "LogAnyDeskStarted", "AnyDesk успешно запущен." },
            { "LogAnyDeskNotFound", "AnyDesk.exe не найден. Поместите файл в папку Tools." },
            { "LogAnyDeskError", "Ошибка запуска AnyDesk: " },
            { "LogFolderOpened", "Открыта папка: " },
            { "LogFolderNotFound", "Папка не найдена: " },
            { "LogFolderAccessError", "Ошибка доступа к папке: " },
            { "LogRegOpened", "Открыт реестр: " },
            { "LogRegError", "Ошибка открытия реестра: " },
            { "LogToolStarted", "Утилита запущена: " },
            { "LogToolNotFound", "Утилита не найдена: " },
            { "LogToolError", "Ошибка запуска утилиты: " },
            { "LogSteamNoKey", "ОШИБКА: Укажите Steam Web API Key в файле settings.json!" },
            { "LogSteamNotFound", "Steam не установлен." },
            { "LogVdfNotFound", "Файл loginusers.vdf не найден." },
            { "LogScanFound", "Найдено аккаунтов: " },
            { "LogScanGetData", ". Получаем данные..." },
            { "LogAccChecked", "Аккаунт проверен: " },
            { "LogAccCheckError", "Ошибка проверки " },
            { "LogScanComplete", "Проверка аккаунтов завершена." },
            { "LogNoExportData", "Нет данных для выгрузки." },
            { "LogExportSaved", "Аккаунты сохранены в " },
            { "LogExportError", "Ошибка сохранения: " },
            { "LogSteamPathNotFound", "Steam не найден." },
            
            // Game Folder Logs
            { "LogGameScanStarted", "Сканирование игр Steam запущено..." },
            { "LogLibraryFoldersNotFound", "Файл libraryfolders.vdf не найден." },
            { "LogLibraryFoldersError", "Ошибка чтения библиотек Steam: " },
            { "LogManifestReadError", "Ошибка чтения манифеста игры: " },
            { "LogGameScanComplete", "Найдено игр: " },

            { "LogLinkError", "Не удалось открыть ссылку: " }
        };

        private static readonly Dictionary<string, string> EnStrings = new()
        {
            { "NavHome", "Home" }, { "NavProgramm", "Programm" }, { "NavAccounts", "Accounts" },
            { "NavLogs", "Logs" }, { "NavGameFolder", "GameFolder" }, { "NavLinks", "Links" }, { "NavExit", "Exit" },

            { "HeaderHome", "HOME" }, { "HeaderProgramm", "PROGRAMM" }, { "HeaderAccounts", "ACCOUNTS" },
            { "HeaderLogs", "LOGS" }, { "HeaderGameFolder", "GAMEFOLDER" }, { "HeaderLinks", "LINKS" },

            { "HomeTitle", "Remote Control Utility" },
            { "HomeDesc", "Launch AnyDesk to connect to the player's desktop." },
            { "HomeBtn", "Launch AnyDesk" },

            { "ProgSysFolders", "System Folders" }, { "ProgUtils", "Utilities" },
            { "ProgStore", "Store" }, { "ProgAppSwitched", "AppSwitched" },
            { "ProgHistory", "History" }, { "ProgPrefetch", "Prefetch" }, { "ProgTemp", "Temp" },
            { "ProgRecent", "Recent" }, { "ProgRecentAD", "RecentAD" },
            { "ProgLastActivity", "LastActivityView" }, { "ProgShellBag", "ShellBag" },
            { "ProgEverything", "Everything" }, { "ProgUSBDeview", "USBDeview" }, { "ProgProcessHacker", "ProcessHacker" },

            { "AccScan", "Scan Steam" }, { "AccExport", "Export SteamID" },
            { "AccSteamID", "Steam64ID" }, { "AccName", "Name" }, { "AccLevel", "Level" }, { "AccVAC", "VAC" },

            { "AccStatusLoading", "Loading..." }, { "AccStatusUnknown", "Unknown" },
            { "AccStatusHidden", "Hidden" }, { "AccStatusClean", "Clean" }, { "AccStatusError", "Error" },

            { "LogsTitle", "Logs" },

            // Game Folder Section
            { "GameScanBtn", "Scan Games" },
            { "GameOpenBtn", "Open" },

            { "LogStarted", "SD Checker started. Ready to work." },
            { "LogLangChanged", "Interface language changed." },
            { "LogConfigCreated", "Created settings.json. Configure it and restart the program." },
            { "LogConfigCreateError", "Config creation error: " },
            { "LogConfigNoKey", "WARNING: Steam API Key not specified in settings.json!" },
            { "LogConfigLoaded", "Configuration loaded successfully." },
            { "LogConfigReadError", "Error reading settings.json: " },
            { "LogAnyDeskStarted", "AnyDesk launched successfully." },
            { "LogAnyDeskNotFound", "AnyDesk.exe not found. Place the file in the Tools folder." },
            { "LogAnyDeskError", "AnyDesk launch error: " },
            { "LogFolderOpened", "Folder opened: " },
            { "LogFolderNotFound", "Folder not found: " },
            { "LogFolderAccessError", "Folder access error: " },
            { "LogRegOpened", "Registry opened: " },
            { "LogRegError", "Registry open error: " },
            { "LogToolStarted", "Utility launched: " },
            { "LogToolNotFound", "Utility not found: " },
            { "LogToolError", "Utility launch error: " },
            { "LogSteamNoKey", "ERROR: Specify Steam Web API Key in settings.json!" },
            { "LogSteamNotFound", "Steam is not installed." },
            { "LogVdfNotFound", "loginusers.vdf file not found." },
            { "LogScanFound", "Accounts found: " },
            { "LogScanGetData", ". Fetching data..." },
            { "LogAccChecked", "Account checked: " },
            { "LogAccCheckError", "Check error for " },
            { "LogScanComplete", "Account verification completed." },
            { "LogNoExportData", "No data to export." },
            { "LogExportSaved", "Accounts saved to " },
            { "LogExportError", "Save error: " },
            { "LogSteamPathNotFound", "Steam path not found." },

            // Game Folder Logs
            { "LogGameScanStarted", "Steam game scanning started..." },
            { "LogLibraryFoldersNotFound", "libraryfolders.vdf not found." },
            { "LogLibraryFoldersError", "Steam library read error: " },
            { "LogManifestReadError", "Game manifest read error: " },
            { "LogGameScanComplete", "Games found: " },

            { "LogLinkError", "Failed to open link: " }
        };

        public static void ToggleLanguage()
        {
            IsRussian = !IsRussian;
            LanguageChanged?.Invoke();
        }

        public static string Get(string key)
        {
            var dict = IsRussian ? RuStrings : EnStrings;
            return dict.TryGetValue(key, out var value) ? value : key;
        }
    }
}