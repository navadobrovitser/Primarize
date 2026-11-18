# Primerize | מערכת חכמה לניהול בחירות מקדימות

## אודות הפרויקט

**Primerize** הוא פרויקט Full Stack לניהול בחירות מקדימות, המשלב פלטפורמה שיתופית, מערכת הצבעה מדורגת ובינה מלאכותית.

המערכת מאפשרת לחברי מפלגה מאושרים להכיר את המועמדים, לצפות בקורות החיים ובפוסטים שלהם, להגיב, לסמן לייקים ולהשתתף בהצבעות.

בנוסף, המערכת כוללת ממשק ניהול, מנגנון התאמה חכמה של מועמדים וצ'אט תקנון המבוסס על RAG.

## קישורים למערכת

🌐 **אתר המערכת:**  
http://primerize.runasp.net/

⚙️ **שרת API ו־Swagger:**  
http://primerizeserver.runasp.net/

**GitHub:**
- [Backend – C#](https://github.com/navadobrovitser/Primarize)
- [Frontend – React](https://github.com/navadobrovitser/PrimerizeReact)

## טכנולוגיות

| צד שרת | צד לקוח |
|---|---|
| C# / ASP.NET Core Web API (.NET 10) | React + TypeScript |
| Entity Framework Core | Redux Toolkit |
| SQL Server | Vite + CSS |
| JWT + AutoMapper | Fetch API |
| Gemini AI + RAG | |

השרת בנוי בארכיטקטורת שכבות: **API, BLL, DAL ו־DTO**, תוך שימוש ב־Dependency Injection.

## יכולות מרכזיות

- **ניהול משתמשים:** התחברות, אימות באמצעות קוד במייל והרשאות מנהל.
- **מועמדים:** פרופיל אישי, קורות חיים, תמונות וסרטונים.
- **פלטפורמה שיתופית:** פרסום פוסטים, תגובות ולייקים.
- **הצבעות:** דירוג מועמדים, מניעת הצבעה כפולה ופרסום תוצאות.
- **התאמה חכמה:** דירוג מועמדים לפי דרישות המשתמש באמצעות Gemini.
- **צ'אט תקנון:** מענה לשאלות בעזרת RAG וחיפוש סמנטי.
- **ממשק מנהל:** ניהול משתמשים ופתיחה וסגירה של בחירות.

## הוראות הפעלה מקומית

**דרישות:** .NET SDK 10, SQL Server, Node.js, npm ו־Git.

### Backend

```bash
git clone https://github.com/navadobrovitser/Primarize.git
cd Primarize
dotnet restore
dotnet build
dotnet run --project API --launch-profile https
```

כתובת ה־API המקומית: `https://localhost:7227`

### Frontend

```bash
git clone https://github.com/navadobrovitser/PrimerizeReact.git
cd PrimerizeReact
npm install
npm run dev
```

כתובת הפיתוח: `http://localhost:5173`

יש להפעיל את שרת ה־API לפני השימוש באתר ולוודא שכתובת השרת מוגדרת נכון בקובץ `src/config/api.ts`.

## הגדרות ומשתני סביבה

לפני ההפעלה יש להגדיר:

| משתנה | תפקיד |
|---|---|
| `ConnectionStrings__DefaultConnection` | חיבור ל־SQL Server |
| `Jwt__Key` | מפתח לחתימת JWT |
| `Gemini__ApiKey` | מפתח Gemini API |
| `Email__SenderEmail` | כתובת מייל שולחת |
| `Email__Username` | שם משתמש SMTP |
| `Email__Password` | סיסמת SMTP |
| `Email__SmtpServer` | שרת SMTP |
| `Email__Port` | פורט SMTP |
| `Frontend__BaseUrl` | כתובת האתר לקישורי מייל |

הגדרות נוספות כגון `Jwt__Issuer`, `Jwt__Audience` ו־`Jwt__ExpiresMinutes` נמצאות ב־`appsettings.json` וניתנות לדריסה באמצעות משתני סביבה.

**חשוב:** אין להעלות סיסמאות, מחרוזות חיבור רגישות או מפתחות API ל־GitHub.

## פריסה לשרת

המערכת פרוסה ב־**MonsterASP.NET**.

פריסת השרת מתבצעת באמצעות GitHub Actions, הכולל Build, Publish והעלאה אוטומטית באמצעות Web Deploy.

פרטי הגישה לפריסה נשמרים ב־GitHub Actions Secrets ולא בקוד המקור.

---

**Primerize – פרויקט גמר Full Stack | React, C#, SQL Server & AI**
