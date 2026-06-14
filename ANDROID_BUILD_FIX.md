# Исправление сборки Android (Unity)

## Проблема 1: ClassNotFoundException com.google.prefab.cli.AppKt (кириллица в пути)

**Симптом:** `Could not find or load main class com.google.prefab.cli.AppKt` при задаче `configureCMakeRelWithDebInfo[arm64-v8a]`. В логе путь к `.gradle` отображается с кракозябрами (`└эфЁхщ` вместо `Андрей`).

**Причина:** Gradle по умолчанию использует `C:\Users\Андрей\.gradle`. Из-за кириллицы в пути Java/Gradle некорректно передают classpath, и prefab (библиотека games-frame-pacing) не загружается.

**Решение:** В проекте добавлен скрипт **Assets/Editor/AndroidGradleUserHome.cs**. Он при открытии проекта выставляет **Gradle User Home** в папку проекта (только ASCII): `E:\Project\Lemmings runner\Library\GradleUserHome`.

1. Открой проект в Unity — скрипт выполнится сам при загрузке.
2. Либо один раз выбери в меню: **Edit → Android → Set Gradle User Home to project (fix Cyrillic path)**.
3. Собери Android снова: **File → Build Settings → Build**.

При первой сборке Gradle скачает зависимости в новую папку. Старую папку `C:\Users\Андрей\.gradle` можно не трогать.

---

## Проблема 2: Remote host terminated the handshake (TLS / dl.google.com)

**Симптом:** `Could not GET 'https://dl.google.com/dl/android/maven2/...'` → `Remote host terminated the handshake`. Gradle не может скачать зависимости (например `com.android.tools.analytics-library:shared:31.10.0`).

**Причина:** Ошибка TLS при подключении к Google Maven (сеть, прокси, антивирус или настройки Java).

**Что уже сделано в проекте:** В **Assets/Plugins/Android/gradleTemplate.properties** добавлены параметры JVM: `-Dhttps.protocols=TLSv1.2,TLSv1.3`.

**Если ошибка остаётся:**

1. **Сеть:** Отключи VPN, попробуй другую сеть (например мобильный интернет).
2. **Прокси:** Если ты за корпоративным прокси, в `gradleTemplate.properties` добавь (подставь свои хост и порт):
   ```
   systemProp.https.proxyHost=proxy.company.com
   systemProp.https.proxyPort=443
   systemProp.https.proxyUser=user
   systemProp.https.proxyPassword=pass
   ```
3. **Антивирус/firewall:** Временно отключи или добавь исключение для Java/Gradle и папки проекта.
4. **Другой JDK:** В **Edit → Preferences → External Tools** укажи **JDK** от Android Studio (например `C:\Program Files\Android\Android Studio\jbr`) вместо встроенного Unity — у него могут быть другие настройки TLS.
5. **Повтор:** Иногда сбой временный — удали `Library\GradleUserHome\caches` (или только `Library\Bee\Android`), затем собери снова.

---

## Проблема 2b: Не удалось подключиться к dl.google.com

**Симптом:** `Could not GET 'https://dl.google.com/...'` → в сообщении кракозябры или «не удалось подключиться» (dl.google.com). Gradle не может скачать JAR (builder, bundletool и т.д.).

**Причина:** Сеть не доходит до серверов Google (блокировка, фаервол, регион, DNS).

**Что сделано в проекте:** В **settingsTemplate.gradle** добавлен явный репозиторий `https://maven.google.com` — иногда он доступен, когда dl.google.com нет.

**Если всё равно не качает:**

1. **VPN:** Если в твоём регионе ограничен доступ к Google — включи VPN и собери снова.
2. **Другая сеть:** Попробуй мобильный интернет (раздача с телефона) или другую Wi‑Fi сеть.
3. **Проверка в браузере:** Открой в браузере `https://dl.google.com/dl/android/maven2/` — если страница не открывается, проблема в сети/доступе, не в Unity.
4. **Корпоративная сеть:** Попроси IT дать зеркало Maven или настрой прокси (см. раздел про прокси выше).
5. **Очистка кэша:** Удали папку `E:\Project\Lemmings runner\Library\GradleUserHome\caches` и собери заново — иногда помогает после смены сети/VPN.

---

## Проблема 3: SDK is read-only
- `Exception while marshalling ... package.xml. Probably the SDK is read-only` — Gradle пытается писать в SDK Unity в `C:\Program Files\...`, куда нет прав записи.
- `configureCMakeRelWithDebInfo[arm64-v8a] FAILED` — сборка нативного модуля (IL2CPP) падает.

## Решение 1: Внешний Android SDK (рекомендуется)

1. **Установи Android Studio** (если ещё нет): https://developer.android.com/studio  
   Либо только **Command Line Tools**: https://developer.android.com/studio#command-tools

2. **Узнай путь к SDK** (обычно):
   - Windows: `C:\Users\<Имя>\AppData\Local\Android\Sdk`
   - Либо в Android Studio: **File → Settings → Appearance & Behavior → System Settings → Android SDK** — вверху указан "Android SDK Location".

3. **В Unity укажи внешний SDK:**
   - **Edit → Preferences** (или **Unity → Settings** на Mac)
   - Раздел **External Tools**
   - **Android SDK path:** укажи путь из шага 2 (например `C:\Users\Андрей\AppData\Local\Android\Sdk`)
   - **JDK:** оставь встроенный или укажи JDK от Android Studio (часто `C:\Program Files\Android\Android Studio\jbr`)

4. **Пересобери проект:** File → Build Settings → Build.

Так Gradle и CMake будут работать с папкой, в которую есть права записи.

---

## Решение 2: Очистка кэша и повторная сборка

Иногда помогает сброс кэша Android/IL2CPP:

1. Закрой Unity.
2. Удали папку:  
   `E:\Project\Lemmings runner\Library\Bee\Android`
3. Запусти Unity и собери снова: **File → Build Settings → Build**.

---

## Решение 3: Запуск Unity от имени администратора (временно)

Если срочно нужно собрать и не настраивать внешний SDK:
- Запусти Unity **от имени администратора** (правый клик по ярлыку → «Запуск от имени администратора»).
- Тогда запись в `C:\Program Files\Unity\...\AndroidPlayer\SDK\` может пройти.

Это не лучшая практика, лучше использовать Решение 1.

---

## Если CMake всё равно падает

После настройки внешнего SDK открой полный лог сборки:
- В Unity: **Edit → Preferences → External Tools** — включи **Custom Gradle Properties** при необходимости.
- Либо после сборки открой отчёт:  
  `E:\Project\Lemmings runner\Library\Bee\Android\Prj\IL2CPP\Gradle\build\reports\problems\problems-report.html`  
  (папка `Library` создаётся при сборке).

В логе ищи строки с **CMake Error** или **FAILED** — по ним можно понять (NDK не найден, несовместимая версия и т.д.). Для Unity 6000.3 обычно нужен NDK из списка поддерживаемых в **External Tools** (например NDK 25 или 26).
