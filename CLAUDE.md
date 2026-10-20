# ge_template_server_csharp

ge_template のサーバ（C#）。

全体ルール・仕様書・設計書は `../ge_template_spec` にある。作業前に `../ge_template_spec/CLAUDE.md` と対象機能の `design.md` を読むこと。
**設計書に書かれていないものは実装しない。**

## 環境

- .NET 10 SDK（10.0.113 で確認）。`dotnet --list-sdks` で 10.0 が出ること。
  - 出ない場合は、PATH で `C:\Program Files\dotnet\` が `C:\Program Files (x86)\dotnet\` より前にあるか確認する。
- ASP.NET Core Minimal API（D001）、SQLite + EF Core 10.0.13（D002）。
- マイグレーションの作成には dotnet-ef を使う: `dotnet tool install --global dotnet-ef`
- DB ファイルは `src/GeTemplate.Server/App_Data/ge_template.db`（Git に含めない）。起動時にマイグレーションが適用され、無ければ作られる。消すとデータは初期化される。

## フォルダ構成

```
GeTemplate.Server.slnx
src/GeTemplate.Server/          サーバ本体
  Common/                       エラー形式・認証など機能をまたぐもの
  Data/                         DbContext・エンティティ・マイグレーション（Data/Migrations/）
  Features/<機能名>/            機能ごとのエンドポイントとサービス
tests/GeTemplate.Server.Tests/  テスト（xUnit・WebApplicationFactory）
```

## 起動・ビルド・テストの実行

```powershell
dotnet run --project src/GeTemplate.Server   # http://localhost:5080 で起動
dotnet build GeTemplate.Server.slnx
dotnet test GeTemplate.Server.slnx
```

テストは一時フォルダに DB ファイルを作るので、開発用の DB には影響しない。

## マイグレーション

エンティティを変えたら、`src/GeTemplate.Server` で次を実行してマイグレーションを追加する。

```powershell
dotnet ef migrations add <名前> -o Data/Migrations
```

## コーディング規約

- 名前空間は `GeTemplate.Server.<機能名>`。
- 命名は C# の標準（クラス・メソッド・プロパティは PascalCase、private フィールドは `_camelCase`）。
- nullable 参照型を有効にする。
- API の名前・リクエスト・レスポンスは設計書の通りにする。変える必要があれば先に設計書を直す。
- クラス名・用語は設計書と `../ge_template_spec/common/glossary.md` に合わせる。
- 名前空間 `GeTemplate.Server.User` とエンティティ `GeTemplate.Server.Data.User` の名前が重なるため、`using GeTemplate.Server.Data;` は `namespace` 宣言の後に書く（前に書くと `User` が名前空間として解決されてしまう）。
- 疑似サーバ（クライアントの `MockServerLogic`）と同じ規則にする。規則を変えるときは、設計書とクライアントも直す。

## Git

- `main` に直接コミットしない。`feature/<機能ID>_<名前>` ブランチで作業する。
- コミット・push・PR はユーザーに指示されたときだけ行う。
