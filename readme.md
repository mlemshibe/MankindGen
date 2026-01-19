# MankindGen - PS1 Style Low-Poly Human Generator

Unity向けPS1世代風ローポリ人間モデルをプロシージャルに生成するツールです。

## 特徴

- **PS1スタイルのローポリモデル**: 約200-300ポリゴンのシンプルでブロッキーな人間モデル
- **プロシージャル生成**: シード値によるランダム生成で無限のバリエーション
- **カスタマイズ可能なパラメータ**: 体型、顔、髪型、衣服など細かく調整可能
- **PS1風テクスチャ**: 低解像度（32x32〜64x64）のピクセレートテクスチャ

## 構成

### モデル構造

- **頭部**: 8角形ベースの輪郭 + プロシージャル生成の鼻
- **頭髪**: 6種類のスタイル（Bald, Short, Medium, Long, Spiky, Slicked）
- **胴体**: 上半身・下半身を分離、シンプルな円柱ベース
- **四肢**: テーパー付き円柱、手足は単純化されたブロック形状

### テクスチャ

- **肌**: 均質な単一色テクスチャ
- **顔**: 64x64の一枚テクスチャ（目・鼻・口の位置は固定、顔形状に合わせてUV変形）
- **髪**: 微細なストライプパターン付き単一色
- **衣服**: 上半身・下半身それぞれ64x64の一枚テクスチャ

## 使い方

### 基本的な使用法

1. Unityでプロジェクトを開く
2. メニューから `GameObject > MankindGen > Create Human Generator` を選択
3. インスペクタで「Generate」または「Random Generate」ボタンをクリック

### スクリプトからの使用

```csharp
using MankindGen;

// ランダム生成
HumanGenerator generator = gameObject.AddComponent<HumanGenerator>();
generator.GenerateRandom();

// 特定のシードで生成
generator.GenerateWithSeed(12345);

// パラメータを指定して生成
generator.parameters = HumanParameters.CreateRandom();
generator.parameters.hairStyle = HairStyle.Spiky;
generator.parameters.skinColor = new Color(0.87f, 0.72f, 0.60f);
generator.Generate();
```

### バッチ生成

1. メニューから `Window > MankindGen > Batch Generate Humans` を選択
2. 生成数、シード、配置間隔を設定
3. 「Generate Batch」をクリック

## パラメータ

### Body Proportions
- `height`: 全体の高さスケール (0.8〜1.2)
- `shoulderWidth`: 肩幅 (0.7〜1.3)
- `hipWidth`: 腰幅 (0.7〜1.3)
- `armLength`: 腕の長さ (0.8〜1.2)
- `legLength`: 脚の長さ (0.8〜1.2)
- `torsoLength`: 胴体の長さ (0.8〜1.2)

### Head Parameters
- `headWidth/Height/Depth`: 頭部の各寸法 (0.8〜1.2)
- `jawWidth`: 顎の幅 (0.0〜1.0)
- `chinPointiness`: 顎の尖り具合 (0.0〜1.0)

### Face Parameters
- `noseLength/Width`: 鼻のサイズ (0.5〜1.5)
- `noseHeight`: 鼻の高さ位置 (0.0〜1.0)
- `faceProfileAngle`: 顔の横顔角度 (-0.3〜0.3)

### Hair Parameters
- `hairStyle`: Bald, Short, Medium, Long, Spiky, Slicked
- `hairVolume`: ボリューム (0.8〜1.5)
- `hairLength`: 長さ (0.0〜1.0)
- `hairColor`: 髪色

### Clothing Parameters
- `upperClothing`: TShirt, LongSleeve, Jacket
- `lowerClothing`: Pants, Shorts
- `upperClothingColor/lowerClothingColor`: 衣服の色

## ライセンス

MIT License

## 技術仕様

- Unity 2021.3+ 推奨
- ランタイムメッシュ生成
- Unlit/Texture シェーダー使用（PS1風フラットシェーディング）
