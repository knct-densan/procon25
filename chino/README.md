# chino
とってもキュートな本番実装

## 必要なプログラム
### ImageMagick
http://www.imagemagick.org/script/binary-releases.php#windows からポータブル版を
ダウンロードして, chino.exe と同じディレクトリ(例えば bin\Debug\)に
展開して、フォルダ名を"imagemagick"に変更すればOK.

### RyuJIT
http://aka.ms/RyuJIT (ダウンロード注意) でダウンロード可能. 
インストール後, バッチファイルに
```bat
@echo off
set COMPLUS_AltJit=*
start chino.exe
```
を記述し動かすと感覚的にもかなり高速に動作する. 
(GitHubのReleaseにアップロードしているZIPファイルにはバッチファイルが含まれている).
CTP4(Community Technical Preview)なので, 念入りに動作確認・テストが必要.
