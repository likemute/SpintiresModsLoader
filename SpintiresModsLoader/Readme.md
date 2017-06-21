## Менеджер модов для игры Spintires. 
#### Не использует папку media, а использует файл конфига, не засоряя игру, когда стоит много модов

![Версия 0.0.0.1](https://img.shields.io/badge/%D0%92%D0%B5%D1%80%D1%81%D0%B8%D1%8F-0.0.0.1-green.svg)
![Статус - Alpha](https://img.shields.io/badge/%D0%A1%D1%82%D0%B0%D1%82%D1%83%D1%81-Alpha-yellow.svg)

---

Возможности
* Включение/Отключение модов игры
* Изменение приоритета модов. (Моды иногда имеют одинаковые файлы, чем мод выше в списке, тем его приоритет выше )
* Распознавание скачанных модов в архивах 7z, zip, rar
* Возможность дать название мода при добавлении, авторства, версии (если файл мода не содержит файл с этой информацией)
* Для автоматического определения мода, авторства, версии используется xml файл образца:
```xml
<?xml version="1.0" encoding="utf-8"?>
<modInfo>
  <name>Имя мода</name>
  <version>Версия</version> <!-- Версия вида: 1.1, 1.1.1, 1.1.1.1 -->
  <author>Автор</author>
  <versionDate>2017-06-17T19:02:17Z</versionDate>
</modInfo>
```
  * Перепакованные файлы модов лежат в AppData\Roaming\SpintiresModsLoader\Mods
  * В проект включены сторонние библиотеки: 
    * sevenZipSharp
    * 7z
    * System.Windows.Interactivity
    * Иконка для приложения из Resources\wheel.ico была представлена на ресурсе: http://www.flaticon.com (designed by Freepik from Flaticon)



Теоретически можно было бы добавить ф-ционал репозитариев модов и т.д., но вряд ли это нужно из-за размера коммьюнити и моего личного времени :)

---

Программа быля написана для себя, поставляется - как есть под лицензией : GPLv3

Вкратце: можно использовать код у себя, расширять функционал программы и т.д. , но выпускать код основанный на коде программы тоже под GPL с открытием исходных кодов

---
Поддержать разработчика

[<img src="https://img.shields.io/badge/donate-Paypal-blue.svg">](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=QFPDXQMZGMHKA)
[<img src="https://img.shields.io/badge/donate-Yandex-orange.svg">](http://yasobe.ru/na/likemute)