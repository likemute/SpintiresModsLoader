## Mod Manager for Spintires game.

Описание на русском языке [здесь](https://github.com/likemute/SpintiresModsLoader/blob/master/Readme.md)

#### Does not use the media folder, but uses the config file without clogging the game, when there are many mods

![Версия 0.0.0.1](https://img.shields.io/badge/Version-0.0.0.1-green.svg)
![Статус - Alpha](https://img.shields.io/badge/Status-Alpha-yellow.svg)

---

![screenshot](https://raw.githubusercontent.com/likemute/SpintiresModsLoader/master/images/screenshot_en.png)

Features
* Enable / Disable game mods
* Change the priority of mods. (Mods sometimes have the same files. Upper the mod in list - higher the priority)
* Recognition of downloaded mods in the archives of 7z, zip, rar
* Ability to add title, authorship, version to the mod when adding (if the mod archive does not contain a file with this information)
* For automatic recognition of mod title, authorship, version, use the xml sample file:
```xml
<?xml version="1.0" encoding="utf-8"?>
<modInfo>
  <name>Имя мода</name>
  <version>Версия</version> <!-- Версия вида: 1.1, 1.1.1, 1.1.1.1 -->
  <author>Автор</author>
  <versionDate>2017-06-17T19:02:17Z</versionDate>
</modInfo>
```

  * Repacked mod files are located in AppData\Roaming\SpintiresModsLoader\Mods
  * This project includes third-party libraries: 
    * sevenZipSharp
    * 7z
    * System.Windows.Interactivity
    * Application icon from Resources\wheel.ico was submitted on: http://www.flaticon.com (designed by Freepik from Flaticon)


Theoretically, it would be possible to add remote repositores of mods, updating etc., but it is hardly necessary because of the size of the community and my personal time :)

---

First of all i wrote the program for myself, it is delivered - as is. License: GPLv3

---
Support the developer please :)

[<img src="https://img.shields.io/badge/donate-Paypal-blue.svg">](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=QFPDXQMZGMHKA)
[<img src="https://img.shields.io/badge/donate-Yandex-orange.svg">](http://yasobe.ru/na/likemute)