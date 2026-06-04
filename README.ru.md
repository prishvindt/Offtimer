# OffTimer

OffTimer — маленькая портативная программа для автоматического выключения компьютера на Windows 10 и Windows 11.

Программа написана на C# WinForms и рассчитана на сборку в self-contained single-file `.exe`.

English readme: [README.md](README.md)

## возможности

- Портативная программа, установка не нужна.
- Ввод своего времени выключения в минутах.
- Быстрые кнопки таймера: `30`, `45`, `60`, `90`.
- Отмена активного таймера.
- Значок в трее.
- Закрытие основного окна не останавливает таймер, а прячет программу в трей.
- Запускается только один экземпляр программы. Повторный запуск открывает уже работающее окно.
- Сквозной оверлей поверх всех окон.
- Настройки оверлея:
  - включен / выключен;
  - размер шрифта;
  - цвет;
  - угол экрана.
- Оверлей показывает только цифры:
  - если осталось больше 60 секунд — минуты, например `43`, `13`, `09`;
  - если осталась последняя минута — секунды, например `60`, `59`, `58`.
- Если оверлей выключен, он всё равно принудительно появляется на последних 60 секундах.
- Интерфейс на русском и английском.
- Режимы языка: авто, русский, english.

## системные требования

- Windows 10 или Windows 11.
- Для опубликованной self-contained сборки установленный .NET Runtime не нужен.
- .NET 10 SDK нужен только для сборки из исходников.

## как работает выключение

OffTimer использует стандартную команду Windows:

```powershell
shutdown.exe /s /t <seconds>
```

Отмена таймера выполняется командой:

```powershell
shutdown.exe /a
```

Программа делает обычное выключение. Принудительное закрытие программ через `/f` не используется.

## портативные настройки

OffTimer хранит настройки здесь:

```text
offtimer/settings.json
```

Сначала программа пытается создать эту папку рядом с `OffTimer.exe`.

Если рядом с exe нельзя писать, используется запасной путь:

```text
%AppData%\OffTimer\settings.json
```

Сам exe автоматически никуда не переносится.

## сборка из исходников

Установи .NET 10 SDK, затем выполни:

```powershell
dotnet restore src/OffTimer/OffTimer.csproj
```

Сборка Windows x64:

```powershell
dotnet publish src/OffTimer/OffTimer.csproj -c Release -r win-x64 --self-contained true -o publish/win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true /p:DebugType=embedded
```

Сборка Windows x86:

```powershell
dotnet publish src/OffTimer/OffTimer.csproj -c Release -r win-x86 --self-contained true -o publish/win-x86 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true /p:DebugType=embedded
```

В папке результата будет `OffTimer.exe`.

## github actions

В проект уже добавлен workflow:

```text
.github/workflows/build.yml
```

Он собирает две версии:

- `win-x64`
- `win-x86`

Запустить сборку можно вручную во вкладке GitHub Actions.

## структура проекта

```text
OffTimer.sln
src/OffTimer/
  AppPaths.cs
  AppSettings.cs
  CountdownController.cs
  Localizer.cs
  MainForm.cs
  NativeMethods.cs
  OverlayForm.cs
  Program.cs
  SettingsForm.cs
  SettingsService.cs
  ShutdownService.cs
  SingleInstance.cs
  assets/offtimer.ico
.github/workflows/build.yml
README.md
README.ru.md
LICENSE
```

## что проверить перед обычным использованием

Это ранняя версия. Перед ежедневным использованием надо проверить:

- запуск таймера;
- отмену таймера;
- поведение трея;
- поведение оверлея;
- сборки x64 и x86;
- отмену выключения из программы и через `shutdown /a`.

## лицензия

MIT
