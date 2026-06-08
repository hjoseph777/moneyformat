# CurrencyTray

Small Windows tray app to view and switch currency locale format.

## Technical

- Language: C#
- Runtime: .NET 9 (`net9.0-windows`)
- App type: C# application with `Program.Main` entry point (tray app)
- Startup on Windows: supported by adding a shortcut to the app in the user Startup folder.

## Author

- Owner

## Screenshots

### Tray Menu

![Tray Menu](.vscode/Screenshot%202026-06-08%20142155.png)

### Format Details

![Format Details](.vscode/Screenshot%202026-06-08%20142347.png)

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
