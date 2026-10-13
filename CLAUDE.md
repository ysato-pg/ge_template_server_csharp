# ge_template_server_csharp

ge_template のサーバ（C#）。

全体ルール・仕様書・設計書は `../ge_template_spec` にある。作業前に `../ge_template_spec/CLAUDE.md` と対象機能の `design.md` を読むこと。
**設計書に書かれていないものは実装しない。**

## 環境

- フレームワーク・通信方式・DB は未決定。`../ge_template_spec/common/decisions.md` で決まったら、この章にバージョンとセットアップ手順を書く。
- .NET SDK はまだインストールされていない。

## ビルド・テストの実行

<!-- プロジェクト作成後に書く。例:
```powershell
dotnet build
dotnet test
```
-->

## コーディング規約

- 名前空間は `GeTemplate.Server.<機能名>`。
- 命名は C# の標準（クラス・メソッド・プロパティは PascalCase、private フィールドは `_camelCase`）。
- nullable 参照型を有効にする。
- API の名前・リクエスト・レスポンスは設計書の通りにする。変える必要があれば先に設計書を直す。
- クラス名・用語は設計書と `../ge_template_spec/common/glossary.md` に合わせる。

## Git

- `main` に直接コミットしない。`feature/<機能ID>_<名前>` ブランチで作業する。
- コミット・push・PR はユーザーに指示されたときだけ行う。
