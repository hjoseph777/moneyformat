# CurrencyTray

Small Windows tray app to view and switch currency locale format.

## Technical

- Language: C#
- Runtime: .NET 9 (`net9.0-windows`)
- App type: C# application with `Program.Main` entry point (tray app)
- Startup on Windows: supported by adding a shortcut to the app in the user Startup folder.

## Author

- Owner
- Harry Joseph
- June 08 2026

## Screenshots

### 1) Current format (French)

Basic info: after switching to French locale (`fr-CA`), format changes to `12 345,67 $` with comma decimal, space thousands separator, and suffix symbol.

![Current Format - French](.vscode/Screenshot%202026-06-08%20142155.png)

### 2) Change format menu (switch target)

Basic info: this shows the menu while French is currently selected (`fr-CA`); pick `en-CA` to switch to English style.

![Change Format Menu](.vscode/Screenshot%202026-06-08%20142347.png)

### 3) Hover tooltip (current/existing format)

Basic info: hovering over the tray icon shows the live current value and locale.

![Hover Tooltip - Current Format](.vscode/Screenshot%202026-06-09%20081136.png)

### 4) After switch (English)

Basic info: after switching to English locale (`en-CA`), format changes to `12,345.67` with dot decimal and comma thousands separator.

![After Switch - English](.vscode/Screenshot%202026-06-09%20081510.png)

## English

### Run
1. Open a terminal in this folder.
2. Run:

```bash
dotnet run --project CurrencyTray.csproj
```

### Use
1. Find the `$` icon in the system tray (or hidden icons).
2. Right-click the icon.
3. The top of the menu shows current format details.
4. Use **Change format** to switch locale:
   - English Canada (en-CA)
   - French Canada (fr-CA)
   - English US (en-US)
   - French France (fr-FR)
5. App refreshes every 2 seconds.

### Notes
- Only one instance can run at a time.
- Locale changes are applied for current user.
- Running apps may need refresh/reopen to pick new format.

## Francais

### Lancer
1. Ouvrir un terminal dans ce dossier.
2. Executer:

```bash
dotnet run --project CurrencyTray.csproj
```

### Utilisation
1. Trouver l'icone `$` dans la barre systeme (ou icones cachees).
2. Clic droit sur l'icone.
3. Le haut du menu montre le format actuel.
4. Utiliser **Change format** pour changer la langue:
   - English Canada (en-CA)
   - French Canada (fr-CA)
   - English US (en-US)
   - French France (fr-FR)
5. Mise a jour automatique toutes les 2 secondes.

### Notes
- Une seule instance peut etre active.
- Le changement est applique pour l'utilisateur courant.
- Certaines apps deja ouvertes peuvent demander un refresh ou une reouverture.
