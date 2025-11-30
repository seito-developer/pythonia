# テーブル定義書

## 0. 前提

- タイムスタンプ型は `timestamptz`、既定は `now()`。
- 多言語は `_ja` / `_en` の列で管理。
- 文字列長や制約は必要に応じて追記。
- イベントの詳細は `jsonb` に格納。
- RLS は別紙で定義（要：本人のみ参照・更新、公開読み取りなど）。

---

## ENUM

### puzzle_difficulty（設計難易度）

- `beginner` / `intermediate` / `advanced`

### user_difficulty_vote（感じた難易度）

- `easy`（楽勝） / `normal`（まあまあ） / `hard`（むずかしい）

---

## 1. profiles（ユーザープロフィール）

| カラム名           | 型          | PK  | FK             | NULL | 既定  | 説明                     |
| ------------------ | ----------- | --- | -------------- | ---- | ----- | ------------------------ |
| user_id            | uuid        | ◯   | auth.users(id) | ×    |       | Supabase 認証ユーザー ID |
| display_name       | text        |     |                | ◯    |       | 表示名                   |
| locale             | text        |     |                | ◯    | 'ja'  | 既定言語                 |
| premium_life_bonus | int         |     |                | ◯    | 0     | 課金による常時ライフ加算 |
| created_at         | timestamptz |     |                | ×    | now() | 登録日時                 |

---

## 2. user_settings（ユーザー設定）

| カラム名      | 型          | PK  | FK                | NULL | 既定  | 説明                      |
| ------------- | ----------- | --- | ----------------- | ---- | ----- | ------------------------- |
| user_id       | uuid        | ◯   | profiles(user_id) | ×    |       | ユーザー ID               |
| show_tooltips | boolean     |     |                   | ×    | true  | 長押しツールチップ ON/OFF |
| lang          | text        |     |                   | ◯    | 'ja'  | UI 言語                   |
| updated_at    | timestamptz |     |                   | ×    | now() | 最終更新                  |

---

## 3. devices（デバイス／匿名識別）

| カラム名    | 型          | PK  | FK                | NULL | 既定  | 説明               |
| ----------- | ----------- | --- | ----------------- | ---- | ----- | ------------------ |
| device_id   | uuid        | ◯   |                   | ×    |       | 端末 ID            |
| platform    | text        |     |                   | ◯    |       | iOS / Android 等   |
| model       | text        |     |                   | ◯    |       | 端末モデル         |
| app_version | text        |     |                   | ◯    |       | アプリ版           |
| user_id     | uuid        |     | profiles(user_id) | ◯    |       | ログイン後に紐付け |
| created_at  | timestamptz |     |                   | ×    | now() | 初回起動           |
| last_seen   | timestamptz |     |                   | ◯    |       | 最終起動           |

---

## 4. puzzles（パズル・マスタ）

| カラム名   | 型                | PK  | FK  | NULL | 既定       | 説明           |
| ---------- | ----------------- | --- | --- | ---- | ---------- | -------------- |
| id         | text              | ◯   |     | ×    |            | 例：`pz_0001`  |
| title_ja   | text              |     |     | ×    |            | タイトル（JA） |
| title_en   | text              |     |     | ×    |            | タイトル（EN） |
| difficulty | puzzle_difficulty |     |     | ×    | 'beginner' | 設計難易度     |
| life_base  | int               |     |     | ◯    | 3          | ベースライフ   |
| active     | boolean           |     |     | ×    | true       | 公開フラグ     |

---

## 5. puzzle_pieces（パズルの手札ピース）

> 手札（使えるピース）の定義。固定ピースは正解位置に初期配置＆ドラッグ不可。

| カラム名   | 型          | PK  | FK          | NULL | 既定  | 説明                                 |
| ---------- | ----------- | --- | ----------- | ---- | ----- | ------------------------------------ |
| id         | bigserial   | ◯   |             | ×    |       | ピース ID                            |
| puzzle_id  | text        |     | puzzles(id) | ×    |       | 所属パズル                           |
| text       | text        |     |             | ×    |       | コードテキスト（UI 表示用）          |
| is_dummy   | boolean     |     |             | ×    | false | ダミーピースか                       |
| is_fixed   | boolean     |     |             | ×    | false | 固定ピース（初期配置・ドラッグ不可） |
| help_ja    | text        |     |             | ◯    |       | 長押し説明（JA）                     |
| help_en    | text        |     |             | ◯    |       | 長押し説明（EN）                     |
| group_id   | int         |     |             | ◯    |       | ランダム化のグループ（任意）         |
| created_at | timestamptz |     |             | ×    | now() | 登録日時                             |

**並び順について**

- 手札の表示順はクライアント側でシャッフル（または `ORDER BY random()`）。
- `group_id` が同じピースを塊として扱う運用も可能。

---

## 6. puzzle_solutions（正解配置）

> 行番号とインデントを保持。複数解は `solution_group` でグルーピング。

| カラム名       | 型          | PK  | FK                | NULL | 既定  | 説明               |
| -------------- | ----------- | --- | ----------------- | ---- | ----- | ------------------ |
| id             | bigserial   | ◯   |                   | ×    |       | 行 ID              |
| puzzle_id      | text        |     | puzzles(id)       | ×    |       | 該当パズル         |
| piece_id       | bigint      |     | puzzle_pieces(id) | ×    |       | 使用ピース         |
| line_index     | int         |     |                   | ×    |       | 行（0,1,2…）       |
| indent_level   | int         |     |                   | ×    |       | インデント（0〜6） |
| solution_group | int         |     |                   | ◯    | 0     | 正解パターン番号   |
| created_at     | timestamptz |     |                   | ×    | now() | 登録日時           |

**判定**

- 盤面の `{piece_id, line_index, indent_level}` 配列が、いずれかの `solution_group` の完全一致であれば正解。
- `is_fixed=true` のピースはここに定義された位置へ初期配置し、ドラッグ不可。

---

## 7. events（イベントログ）

| カラム名  | 型          | PK  | FK                 | NULL | 既定  | 説明                                                                          |
| --------- | ----------- | --- | ------------------ | ---- | ----- | ----------------------------------------------------------------------------- |
| id        | bigserial   | ◯   |                    | ×    |       | イベント ID                                                                   |
| device_id | uuid        |     | devices(device_id) | ×    |       | 発生端末                                                                      |
| user_id   | uuid        |     | profiles(user_id)  | ◯    |       | ユーザー（匿名は null）                                                       |
| puzzle_id | text        |     | puzzles(id)        | ◯    |       | パズル ID                                                                     |
| type      | text        |     |                    | ×    |       | `puzzle_start`/`submit`/`puzzle_finish`/`hint_open`/`reset_board`/`nav_click` |
| payload   | jsonb       |     |                    | ×    | '{}'  | 詳細（経過秒、grid ハッシュ等）                                               |
| app_build | text        |     |                    | ◯    |       | ビルド番号                                                                    |
| ts        | timestamptz |     |                    | ×    | now() | 発生時刻                                                                      |

**推奨インデックス**：`(ts)`, `(puzzle_id)`, `(user_id)`

---

## 8. user_puzzle_stats（ユーザー × パズルの集計）

| カラム名         | 型          | PK  | FK                | NULL | 既定 | 説明             |
| ---------------- | ----------- | --- | ----------------- | ---- | ---- | ---------------- |
| user_id          | uuid        | ◯   | profiles(user_id) | ×    |      | ユーザー         |
| puzzle_id        | text        | ◯   | puzzles(id)       | ×    |      | パズル           |
| attempts         | int         |     |                   | ×    | 0    | 試行回数         |
| clears           | int         |     |                   | ×    | 0    | クリア回数       |
| best_time_sec    | float8      |     |                   | ◯    | -1   | ベストタイム     |
| total_time_sec   | float8      |     |                   | ◯    | 0    | 累計プレイ時間   |
| total_hints_used | int         |     |                   | ◯    | 0    | 累計ヒント使用   |
| last_stars       | int         |     |                   | ◯    |      | 最後の星（1〜3） |
| last_played_at   | timestamptz |     |                   | ◯    |      | 最終プレイ       |

---

## 9. puzzle_daily_stats（日次集計・運営向け）

| カラム名     | 型     | PK  | FK          | NULL | 既定 | 説明           |
| ------------ | ------ | --- | ----------- | ---- | ---- | -------------- |
| day          | date   | ◯   |             | ×    |      | 集計日         |
| puzzle_id    | text   | ◯   | puzzles(id) | ×    |      | パズル         |
| plays        | int    |     |             | ×    | 0    | プレイ数       |
| clears       | int    |     |             | ×    | 0    | クリア数       |
| avg_time_sec | float8 |     |             | ◯    |      | 平均クリア時間 |
| avg_hints    | float8 |     |             | ◯    |      | 平均ヒント数   |

---

## 10. user_puzzle_feedback（感じた難易度の履歴）

| カラム名  | 型                   | PK  | FK                | NULL | 既定  | 説明                   |
| --------- | -------------------- | --- | ----------------- | ---- | ----- | ---------------------- |
| id        | bigserial            | ◯   |                   | ×    |       | 行 ID                  |
| user_id   | uuid                 |     | profiles(user_id) | ×    |       | ユーザー               |
| puzzle_id | text                 |     | puzzles(id)       | ×    |       | パズル                 |
| vote      | user_difficulty_vote |     |                   | ×    |       | `easy`/`normal`/`hard` |
| played_at | timestamptz          |     |                   | ×    | now() | 投票日時               |
| note      | text                 |     |                   | ◯    |       | 任意メモ               |

---

## 11. user_puzzle_labels（感じた難易度の最新ラベル：再チャレンジ用）

| カラム名      | 型                   | PK  | FK                | NULL | 既定  | 説明               |
| ------------- | -------------------- | --- | ----------------- | ---- | ----- | ------------------ |
| user_id       | uuid                 | ◯   | profiles(user_id) | ×    |       | ユーザー           |
| puzzle_id     | text                 | ◯   | puzzles(id)       | ×    |       | パズル             |
| last_vote     | user_difficulty_vote |     |                   | ×    |       | 最新の感じた難易度 |
| last_voted_at | timestamptz          |     |                   | ×    | now() | 最新更新日時       |

**用途**

- 「楽勝/まあまあ/むずかしい」別の再チャレンジ一覧を高速に表示。
- 投票時：`user_puzzle_feedback` に INSERT、同時に本テーブルを UPSERT。

---

## 12. puzzle_sets（パズルセット・マスタ）

| カラム名         | 型                | PK  | FK  | NULL | 既定  | 説明                     |
| ---------------- | ----------------- | --- | --- | ---- | ----- | ------------------------ |
| id               | text              | ◯   |     | ×    |       | `set_beginner_01` 等     |
| title_ja         | text              |     |     | ×    |       | セット名（JA）           |
| title_en         | text              |     |     | ×    |       | セット名（EN）           |
| description_ja   | text              |     |     | ◯    |       | 説明（JA）               |
| description_en   | text              |     |     | ◯    |       | 説明（EN）               |
| difficulty       | puzzle_difficulty |     |     | ◯    |       | セット代表難易度（任意） |
| is_free          | boolean           |     |     | ×    | false | 無料セットなら true      |
| free_trial_count | int               |     |     | ×    | 0     | 体験できる先頭 N 問      |
| sort_order       | int               |     |     | ◯    |       | 一覧並び順               |
| icon_url         | text              |     |     | ◯    |       | アイコン                 |
| banner_url       | text              |     |     | ◯    |       | バナー                   |
| active           | boolean           |     |     | ×    | true  | 公開フラグ               |
| created_at       | timestamptz       |     |     | ×    | now() | 登録日時                 |

---

## 13. puzzle_set_puzzles（セット内パズルの並び）

| カラム名     | 型          | PK  | FK              | NULL | 既定  | 説明                  |
| ------------ | ----------- | --- | --------------- | ---- | ----- | --------------------- |
| set_id       | text        | ◯   | puzzle_sets(id) | ×    |       | 所属セット            |
| puzzle_id    | text        | ◯   | puzzles(id)     | ×    |       | 収録パズル            |
| index_in_set | int         |     |                 | ×    |       | セット内表示順（0〜） |
| is_featured  | boolean     |     |                 | ×    | false | セット紹介で強調表示  |
| created_at   | timestamptz |     |                 | ×    | now() | 登録日時              |

**解放判定（クライアント）**

- `is_free = true` → 全問解放
- 未購入の有料セット → `index_in_set < free_trial_count` のみ解放

---

## 14. products（販売商品マスタ）

| カラム名         | 型          | PK  | FK  | NULL | 既定              | 説明                                       |
| ---------------- | ----------- | --- | --- | ---- | ----------------- | ------------------------------------------ |
| id               | uuid        | ◯   |     | ×    | gen_random_uuid() | 商品 ID                                    |
| code             | text        |     |     | ×    |                   | 内部コード（例：`prod_set_ext_01`）        |
| store            | text        |     |     | ×    |                   | `appstore` / `play`                        |
| store_product_id | text        |     |     | ×    |                   | ストア SKU（例：`com.pythonia.set.ext01`） |
| title_ja         | text        |     |     | ◯    |                   | 商品名（JA）                               |
| title_en         | text        |     |     | ◯    |                   | 商品名（EN）                               |
| price_display    | text        |     |     | ◯    |                   | 表示用価格（※実価格はストア側管理）        |
| active           | boolean     |     |     | ×    | true              | 販売中                                     |
| created_at       | timestamptz |     |     | ×    | now()             | 登録日時                                   |

---

## 15. set_products（セットと商品を結び付け）

| カラム名   | 型          | PK  | FK              | NULL | 既定  | 説明       |
| ---------- | ----------- | --- | --------------- | ---- | ----- | ---------- |
| set_id     | text        | ◯   | puzzle_sets(id) | ×    |       | 対象セット |
| product_id | uuid        | ◯   | products(id)    | ×    |       | 対応商品   |
| created_at | timestamptz |     |                 | ×    | now() | 登録日時   |

---

## 16. entitlements（所有権：購入＝解放の実体）

| カラム名          | 型          | PK  | FK                | NULL | 既定              | 説明                             |
| ----------------- | ----------- | --- | ----------------- | ---- | ----------------- | -------------------------------- |
| id                | uuid        | ◯   |                   | ×    | gen_random_uuid() | 権利 ID                          |
| user_id           | uuid        |     | profiles(user_id) | ×    |                   | ユーザー                         |
| set_id            | text        |     | puzzle_sets(id)   | ×    |                   | 解放されるセット                 |
| source_product_id | uuid        |     | products(id)      | ◯    |                   | 由来商品（バウチャ等は null 可） |
| valid             | boolean     |     |                   | ×    | true              | 有効フラグ                       |
| granted_at        | timestamptz |     |                   | ×    | now()             | 付与日時                         |
| expires_at        | timestamptz |     |                   | ◯    |                   | 有効期限（買い切りは null）      |

**クライアント判定**

- `entitlements` に `user_id & set_id` の有効行があれば、そのセットの**全問解放**。

---

## 17. purchases（課金レシート）

| カラム名     | 型          | PK  | FK                | NULL | 既定              | 説明                 |
| ------------ | ----------- | --- | ----------------- | ---- | ----------------- | -------------------- |
| id           | uuid        | ◯   |                   | ×    | gen_random_uuid() | 購入 ID              |
| user_id      | uuid        |     | profiles(user_id) | ×    |                   | 購入ユーザー         |
| store        | text        |     |                   | ×    |                   | `appstore` / `play`  |
| product_id   | text        |     |                   | ×    |                   | ストア商品 ID（SKU） |
| valid        | boolean     |     |                   | ×    | false             | 検証済フラグ         |
| receipt      | jsonb       |     |                   | ◯    |                   | レシート             |
| purchased_at | timestamptz |     |                   | ×    | now()             | 購入日時             |
| validated_at | timestamptz |     |                   | ◯    |                   | 検証日時             |

---

# リレーションまとめ

| リレーション                                                                                                                                             | 内容              |
| -------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------- |
| `auth.users` → `profiles`                                                                                                                                | 1:1               |
| `profiles` → `user_settings` / `user_puzzle_stats` / `user_puzzle_feedback` / `user_puzzle_labels` / `entitlements` / `purchases`                        | 1:N               |
| `devices` → `events`                                                                                                                                     | 1:N               |
| `profiles` → `events`                                                                                                                                    | 1:N（ログイン後） |
| `puzzles` → `puzzle_pieces` / `puzzle_solutions` / `events` / `user_puzzle_stats` / `user_puzzle_feedback` / `user_puzzle_labels` / `puzzle_daily_stats` | 1:N               |
| `puzzle_sets` → `puzzle_set_puzzles` / `set_products` / `entitlements`                                                                                   | 1:N               |
| `products` → `set_products` / `entitlements(source_product_id)`                                                                                          | 1:N               |

---

# インデックス推奨（要件に応じて）

- `events(ts)`, `events(puzzle_id)`, `events(user_id)`
- `puzzle_set_puzzles(set_id, index_in_set)`
- `user_puzzle_stats(user_id, puzzle_id)`
- `user_puzzle_labels(user_id, last_vote, last_voted_at)`
- `entitlements(user_id, set_id, valid, expires_at)`
- `puzzle_pieces(puzzle_id)`
- `puzzle_solutions(puzzle_id, solution_group, line_index)`

---

# 運用メモ

- **固定ピース**：`puzzle_pieces.is_fixed = true` は `puzzle_solutions` の位置へ初期配置し、UI ではドラッグ不可に。
- **複数正解**：`solution_group` 単位で完全一致を判定。
- **手札ランダム**：クライアント側でシャッフル（または SQL `ORDER BY random()`）。
- **感じた難易度**：クリア後に投票 → 履歴テーブルに INSERT、最新ラベルは UPSERT。
- **解放判定**：`puzzle_sets.is_free` / `free_trial_count` / `entitlements` を組み合わせて実装。
