# HSP Decompiler (deHSP)

HSP（Hot Soup Processor）2/3でコンパイルされたファイル（`.ax`、`.exe`、`.dpm`）をソースファイル（`.hsp`、`.as`）に戻すツールです。

## 機能

- HSP3（`.ax` → `.hsp`）およびHSP2（`.ax` → `.as`）のデコンパイル
- DPMアーカイブからのファイル抽出
- HSP実行ファイル（`.exe`）からのDPM抽出
- DPMアーカイブ内の暗号化ファイルの復号（XOR+ADDブルートフォース）
- クロスプラットフォーム対応（Windows、macOS、Linux）
- CLIツールおよびGUIアプリケーション

## 動作環境

- .NET 8.0 SDK 以降

## ビルド

```bash
dotnet build HspDecompiler.sln
```

## 使い方

### CLI

```bash
# .axファイルをデコンパイル
dotnet run --project src/HspDecompiler.Cli -- input.ax

# 出力ディレクトリを指定してデコンパイル
dotnet run --project src/HspDecompiler.Cli -- input.ax -o ./output

# DPMアーカイブを展開
dotnet run --project src/HspDecompiler.Cli -- archive.dpm -o ./extracted

# HSP実行ファイルから展開
dotnet run --project src/HspDecompiler.Cli -- game.exe -o ./extracted
```

オプション:

```text
dehsp <入力ファイル> [オプション]
  -o, --output <dir>       出力ディレクトリ（デフォルト: 入力ファイルと同じディレクトリ）
  -d, --dictionary <path>  Dictionary.csvのパス（デフォルト: 実行ファイルと同じディレクトリ）
  --no-decrypt             暗号化ファイルの復号をスキップ
  --skip-encrypted         DPMから非暗号化ファイルのみ抽出
  -v, --verbose            詳細ログ出力
  --version                バージョン表示
  -h, --help               ヘルプ表示
```

### GUI

```bash
dotnet run --project src/HspDecompiler.Gui
```

またはファイルをアプリケーションウィンドウにドラッグ＆ドロップしてください。

## プロジェクト構成

| プロジェクト         | 説明                               |
| -------------------- | ---------------------------------- |
| `HspDecompiler.Core` | コアライブラリ（全アルゴリズム）   |
| `HspDecompiler.Cli`  | コマンドラインインターフェース     |
| `HspDecompiler.Gui`  | Avalonia クロスプラットフォームGUI |

## Dictionary.csv

`Dictionary.csv`は実行ファイルと同じディレクトリに置く必要があります。デコンパイル時に使用するHSP3の組み込みコマンド、関数、パラメータ型を定義するファイルです。

## 歴史

### 作者

| ソフト名                                                                 | 作者                                                                                                  | 説明                   |
| ------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------- | ---------------------- |
| [HSP逆コンパイラ](http://www.vector.co.jp/soft/win95/prog/se390297.html) | [きつつき](http://www.vector.co.jp/vpack/browse/person/an043697.html)                                 | オリジナルのソフト     |
| [HSPdeco](https://osdn.jp/projects/hspdeco/)                             | [minorshift](https://osdn.jp/users/minorshift/)                                                       | オリジナルの改良版     |
| [HSPdecom](http://stpr18.blogspot.jp/2015/10/hspdecohspelona.html)       | [したぷる](https://www.blogger.com/profile/00794326060600750840)、[YSRKEN](https://github.com/YSRKEN) | HSPdecoの改良版        |
| [HSPdecoのパッチ](http://vivibit.net/hspdeco/)                           | [xx2zz](http://vivibit.net/about/)                                                                    | 復号失敗時の対策パッチ |

### バージョン履歴

| 日付       | ソフト名        | バージョン | 説明                                                       |
| ---------- | --------------- | ---------- | ---------------------------------------------------------- |
| 2006/01/28 | HSP逆コンパイラ | 1.0        | 当初シェアウェアとして公開                                 |
| 2007/09/10 | HSP逆コンパイラ | 1.1        | HSP3に対応                                                 |
| 2010/09/12 | HSP逆コンパイラ | 1.2        | PDS・OSSになった他、バグ修正                               |
| 2012/01/13 | HSPdeco         | 1.0        | axファイルのデコード機能を開放、バグ修正                   |
| 2015/12/15 | HSPdecom        | 1.0        | 変数名復元をサポート、辞書データ追加                       |
| 2016/08/16 | HSPdecom        | 1.1        | パッチを全て付加、GitHubに上げ直し                         |
| 2026       | deHSP           | 2.0        | .NET 8.0 + Avalonia、CLIツール、クロスプラットフォーム対応 |

## ライセンス

HSPdecom部分はPDSライセンス、HSPdeco部分はzlib/libpngライセンスです。
詳しくはLICENSEファイルを参照してください。
