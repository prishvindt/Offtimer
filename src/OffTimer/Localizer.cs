using System.Globalization;

namespace OffTimer;

internal sealed class Localizer
{
    private readonly AppSettings _settings;

    private static readonly Dictionary<string, Dictionary<string, string>> Strings = new()
    {
        ["en"] = new Dictionary<string, string>
        {
            ["app_title"] = "OffTimer",
            ["minutes"] = "Minutes",
            ["ok"] = "OK",
            ["cancel_timer"] = "Cancel timer",
            ["timer_off"] = "Timer is off",
            ["timer_active"] = "Shutdown in {0}",
            ["overlay_on"] = "Overlay: on",
            ["overlay_off"] = "Overlay: off",
            ["settings"] = "Settings",
            ["show"] = "Show",
            ["exit"] = "Exit",
            ["invalid_minutes"] = "Enter minutes from 1 to 1440.",
            ["shutdown_error"] = "Failed to schedule shutdown: {0}",
            ["cancel_error"] = "Failed to cancel shutdown: {0}",
            ["language"] = "Language",
            ["language_auto"] = "Auto",
            ["language_ru"] = "Russian",
            ["language_en"] = "English",
            ["overlay_enabled"] = "Show overlay during timer",
            ["overlay_size"] = "Overlay size",
            ["overlay_color"] = "Overlay color",
            ["overlay_corner"] = "Overlay corner",
            ["corner_topleft"] = "Top left",
            ["corner_topright"] = "Top right",
            ["corner_bottomleft"] = "Bottom left",
            ["corner_bottomright"] = "Bottom right",
            ["minimize_on_start"] = "Minimize to tray when timer starts",
            ["save"] = "Save",
            ["cancel"] = "Cancel",
            ["settings_saved"] = "Settings saved.",
            ["settings_path"] = "Settings file",
            ["running_in_tray"] = "OffTimer is running in tray."
        },
        ["ru"] = new Dictionary<string, string>
        {
            ["app_title"] = "OffTimer",
            ["minutes"] = "Минуты",
            ["ok"] = "OK",
            ["cancel_timer"] = "Отключить таймер",
            ["timer_off"] = "Таймер выключен",
            ["timer_active"] = "Выключение через {0}",
            ["overlay_on"] = "Оверлей: вкл",
            ["overlay_off"] = "Оверлей: выкл",
            ["settings"] = "Настройки",
            ["show"] = "Показать",
            ["exit"] = "Выход",
            ["invalid_minutes"] = "Введите минуты от 1 до 1440.",
            ["shutdown_error"] = "Не удалось запланировать выключение: {0}",
            ["cancel_error"] = "Не удалось отменить выключение: {0}",
            ["language"] = "Язык",
            ["language_auto"] = "Авто",
            ["language_ru"] = "Русский",
            ["language_en"] = "English",
            ["overlay_enabled"] = "Показывать оверлей во время таймера",
            ["overlay_size"] = "Размер оверлея",
            ["overlay_color"] = "Цвет оверлея",
            ["overlay_corner"] = "Угол оверлея",
            ["corner_topleft"] = "Левый верхний",
            ["corner_topright"] = "Правый верхний",
            ["corner_bottomleft"] = "Левый нижний",
            ["corner_bottomright"] = "Правый нижний",
            ["minimize_on_start"] = "Сворачивать в трей после запуска таймера",
            ["save"] = "Сохранить",
            ["cancel"] = "Отмена",
            ["settings_saved"] = "Настройки сохранены.",
            ["settings_path"] = "Файл настроек",
            ["running_in_tray"] = "OffTimer работает в трее."
        }
    };

    public Localizer(AppSettings settings)
    {
        _settings = settings;
    }

    public string CurrentLanguage
    {
        get
        {
            if (_settings.Language is "ru" or "en")
            {
                return _settings.Language;
            }

            return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ru", StringComparison.OrdinalIgnoreCase)
                ? "ru"
                : "en";
        }
    }

    public string T(string key)
    {
        var lang = CurrentLanguage;
        if (Strings.TryGetValue(lang, out var set) && set.TryGetValue(key, out var value))
        {
            return value;
        }

        return Strings["en"].TryGetValue(key, out var fallback) ? fallback : key;
    }

    public string F(string key, params object[] args) => string.Format(T(key), args);
}
