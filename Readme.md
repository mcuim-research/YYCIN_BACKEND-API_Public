# BACKEND-API

YOYO Check It Now 的後端 API，使用 ASP.NET Core、Dapper、SQL Server、Argon2、Google Cloud Vision 與 OpenAI。

## 本機啟動

專案目標框架為 .NET 6，程式使用 C# 12；請使用支援 C# 12 的 .NET SDK，並安裝 .NET 6 runtime。SQL Server 結構及資料需自行準備，本倉庫不包含正式資料庫、個資或內部文件。

1. 將 `test3/appsettings.Local.example.json` 複製為 `test3/appsettings.Local.json`。
2. 填入自己的 SQL 連線、SMTP 帳密與 `App:PublicBaseUrl`。本機設定檔已加入 `.gitignore`。
3. 需要處方辨識功能時，於執行環境設定 `OPENAPI_API_KEY`，並以 `GOOGLE_APPLICATION_CREDENTIALS` 指向倉庫外的 Google Cloud 服務帳號檔案，或使用 Application Default Credentials。
4. 執行：

```sh
dotnet restore test3/test3.csproj
dotnet run --project test3/test3.csproj
```

預設本機網址為 `http://localhost:5292/`，API 文件位於 `/swagger`。未提供 SQL、SMTP 或雲端憑證時，對應功能無法使用。

## 設定方式

| 設定 | 用途 | 環境變數名稱 |
|---|---|---|
| `ConnectionStrings:DevConnection` | 自行提供的 SQL Server 連線字串 | `ConnectionStrings__DevConnection` |
| `App:PublicBaseUrl` | 驗證信連結及郵件圖片的網站根網址，可包含代理前綴 | `App__PublicBaseUrl` |
| `Smtp:Host`、`Smtp:Port`、`Smtp:UseSsl` | SMTP 伺服器設定 | `Smtp__Host`、`Smtp__Port`、`Smtp__UseSsl` |
| `Smtp:SenderEmail`、`Smtp:SenderName` | 寄件者資訊 | `Smtp__SenderEmail`、`Smtp__SenderName` |
| `Smtp:Username`、`Smtp:Password` | SMTP 登入帳密 | `Smtp__Username`、`Smtp__Password` |
| `OPENAPI_API_KEY` | 處方辨識使用的 OpenAI API Key，沿用原程式的變數名稱 | `OPENAPI_API_KEY` |
| `GOOGLE_APPLICATION_CREDENTIALS` | Google Cloud 憑證檔案路徑 | `GOOGLE_APPLICATION_CREDENTIALS` |

載入順序為 `appsettings.json`、環境對應設定、本機 `appsettings.Local.json`，最後由環境變數覆蓋。

前端目前只有編譯後的靜態檔。可在 `test3/wwwroot/app-config.js` 設定 `YOYO_APP_CONFIG.apiBaseUrl`，預設 `/` 會使用目前站台。前端路由及資產以 `test3/wwwroot/index.html` 的 `<base href="/">` 為根路徑。

透過 `/yoyo/` 等反向代理前綴部署前端時，請將 `<base href>` 改為 `/yoyo/`，並調整 `App:PublicBaseUrl`；若 API 也位於相同前綴，將 `apiBaseUrl` 改為 `/yoyo/`。這些設定不需要寫入正式主機名稱。

## 公開檔案範圍

倉庫保留程式碼及空白／示範設定。憑證、本機設定、處方照片、上傳檔、`bin/`、`obj/`、`temp/` 與作業系統暫存檔不應提交。示範資料請使用合成內容，不要加入真實處方或患者資訊。
