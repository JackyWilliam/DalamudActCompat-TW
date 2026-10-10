# 共用好友與聊天客戶端接入

本階段提供 DACT / 後續寵物外掛共用的帳號、好友、線上狀態與聊天契約，以及 DACT 好友抽屜、聊天視窗和官方通知。寵物 AI 和支付不在本階段。現有“好友邀請”仍是註冊啟用碼邀請，與新增好友申請分開。

## 程式碼入口

- `Infrastructure/Cloud/CloudFriendsApi.cs`：HTTP 呼叫和 DTO。
- `Infrastructure/Cloud/CloudClientFriends.cs`：使用當前已驗證登入帳號的服務封裝；拒絕未完成驗證的儲存令牌，丟棄退出/切換帳號後的舊響應。
- `Infrastructure/Cloud/CloudFriendConnection.cs`：外掛連線心跳、相容退避和關閉清理。
- `CloudClientService`：驗證登入後啟動連線；退出、封禁、令牌失效、解除安裝時取消。
- `CloudChatPolicy.Notice`：當前使用者提示，顯示於設定與帳號頁，並由服務端快照返回。

業務介面優先呼叫 `CloudClientService.ListFriendsAsync`、`LookupFriendAsync`、`RequestFriendAsync`、`AcceptFriendAsync`、`DeclineFriendAsync`、`RemoveFriendAsync`、`SyncChatAsync`、`GetChatAsync`、`SendChatAsync`、`AcknowledgeChatAsync`。直接 HTTP 客戶端用於其他共享帳號客戶端或隔離測試；不要把登入令牌交給檢視層。

## 好友與線上

所有使用者介面複用帳號 Bearer session。基址沿用當前雲服務設定，路徑以 `api/v1/` 開頭：

| 操作 | HTTP 介面 |
|---|---|
| 列表 / 線上數 / 申請 | GET `friends` |
| 按完整帳號名查詢 | POST `friends/lookup`，`{username}` |
| 新增 | POST `friends/requests`，`{username}` |
| 接受 / 拒絕 | POST `friends/requests/{id}/accept` 或 `/decline`，`{}` |
| 解除 | DELETE `friends/{relationshipId}`，`{}` |
| 線上 / 斷開 | PUT / DELETE `friends/presence`，`{clientId}` |

關係中的 `direction` 是相對當前帳號的 `incoming/outgoing`；已接受關係包含 `conversationId`。雙向主動新增會接受已存在的申請；查詢不會自動接受。重複請求返回已有狀態。解除會刪除該好友會話及訊息；重新新增得到新關係與空會話，舊請求 ID 不能影響新關係。

線上來自真實客戶端心跳。每次連線用新 UUID，25 秒續約，90 秒未續約則離線。多客戶端/多會話取任一有效連線；有效令牌本身不代表線上。關閉只清理自己的連線，不會踢掉另一個外掛。每個心跳請求最多 5 秒，關閉清理最多 2 秒。服務端重啟後由下一次心跳重建線上狀態。

客戶端連線舊版服務時，好友 404 不影響登入和配置備份，線上探測每 5 分鐘重試。暫時網路失敗每 25 秒重試且不登出帳號。401 和帳號/機器封禁仍執行原有登入失效及封禁處理。客戶端未新增高頻日誌。

服務端初始資源上限為每帳號 200 好友、100 條待處理申請、16 個線上連線；查詢 20 次/分鐘、關係變更 60 次/分鐘、傳送 120 次/分鐘。429 暴露 `CloudApiException.RetryAfterSeconds`，由介面給出稍後重試提示。

## 好友備註雲同步

右鍵好友行選擇“設定備註…”或“修改備註…”，最多 40 個 Unicode 碼點，單行，儲存空文字或點選“清除備註”可恢復帳號名。列表、聊天標題、訊息作者和提醒顯示“備註（帳號名）”，帳號身份、管理員/贊助身份和訊息內容不變。

備註屬於當前登入帳號，直接儲存到好友服務，不依賴配置備份上傳。`PUT friends/{relationshipId}/remark` 接收 `{text,revision}` 並返回已儲存的 `{text,revision}`；好友列表中的 `remark` 只包含當前檢視者自己的值。同帳號其他裝置隨現有列表輪詢同步（通常約 5 秒）；對方和其他帳號看不到備註。解除關係會刪除雙方各自的備註，重新新增從空值開始。

編輯器保留開啟時的版本，其他裝置修改後返回 409，不靜默覆蓋。相同內容重試安全；請求結果未知時保留草稿並重新整理伺服器值。切換帳號丟棄舊快照和編輯狀態，舊請求不能使用新帳號憑據。舊服務未返回 `remark` 時禁用備註儲存，好友、聊天和配置備份繼續使用原有介面。本機聊天加密狀態格式保持不變。

## 訊息同步與數量限制

`SyncChatAsync` → 各會話摘要和給當前帳號的離線待收訊息；比較摘要 `Revision` 後用 `GetChatAsync` 獲取變化會話。摘要含 `Kind`、`Peer`、`HistoryCount`、`PendingCount`、`LatestMessageId`。完整快照含 `History`、`Pending`、`NextSendSequence`。訊息 ID 順序穩定，時間為 UTC。

- 每對好友雙方合計保留最近 **20 條歷史**。
- 每對好友雙方合計保留最新 **3 條離線待收**；超出實際刪除，剩餘沒有時間有效期。
- 對方線上時新訊息直接進入歷史；離線訊息由接收方消費快照後 ACK，併入歷史、再裁剪到 20 條。
- GET 和心跳不會消耗離線訊息。僅 FriendsUiManager 註冊的活躍消費者在收到有界模型後 ACK；關閉聊天窗保留該消費者，解除安裝時解除註冊。
- `DeliveredAt` 代表進入可用歷史的時間，不是已讀回執。

介面以新快照替換舊內容，同樣最多 20 歷史和 3 待收，不建立長期本機聊天正文存檔。ACK 最多包含 3 個發給自己的 ID；重複 ACK 安全。若訊息在讀取與確認之間已被裁剪，返回的 `RetiredIds` 表示它已不再保留，以返回快照校正本地狀態，不能重新插入。已讀標記是當前電腦當前帳號的 ID 水位，僅在對應聊天獲得焦點並滾動到底部時更新；不作為雙方已讀回執，不同步到其他電腦。

## 介面與本機可靠性

主視窗雲狀態右側顯示好友圖示與線上好友數，未讀訊息/申請單獨顯示紅點。抽屜支援完整帳號查詢、申請/接受/拒絕和解除關係；聊天窗獨立移動。官方身份來自服務端 kind，官方會話為只讀。來信只做短暫輕提示，不自動開啟聊天或請求焦點；點選入口/提醒後才展開。

FriendsChatController 的單獨工作執行緒處理網路和磁碟，幀執行緒僅入隊和讀取不可變快照。每次請求同時攜帶當前帳號代次，在 CloudClientService 捕獲憑據的鎖內校驗，切換帳號時舊請求不能使用新憑據，舊響應也不會顯示。

傳送先取伺服器序號，把原序號/UUID/正文寫入 DPAPI CurrentUser 加密發件箱，再進行 HTTP 請求。準備失敗保留編輯草稿；結果未知時保留原請求，重啟僅恢復待確認狀態、不自動重發。重試使用原請求；確認已在服務端歷史中時清除發件箱。使用者可明確放棄，但會告知原訊息可能已送達。序號被裁剪後不恢復正文、不自動另起傳送。共享配置目錄的多程序寫入由檔案鎖和增量合併保護，同一會話出現衝突時禁止覆蓋其他視窗待確認傳送。

本機 friends-state 只儲存最多 201 個會話的待確認傳送和已讀 ID，不儲存完整接收正文，且不在配置雲備份白名單內。每檔案上限 2 MiB，帳號按服務端 UUID 分隔；狀態損壞時報告讀取錯誤並停止消費，不靜默清空可能尚未確認的傳送。

## 傳送與重試

從會話讀取 `NextSendSequence`，生成 `Guid.NewGuid()`，建立不可變 `CloudChatSendRequest`。普通訊息僅填 `Text`；快捷訊息僅填 `QuickMessageId`：

```csharp
var view = await cloud.GetChatAsync(conversationId, cancellationToken);
var pending = new CloudChatSendRequest(
    view.NextSendSequence!.Value, Guid.NewGuid(),
    QuickMessageId: CloudChatPolicy.InviteNext);
// 保存 pending，直到收到明确结果；网络错误后重试仍使用这个对象。
var sent = await cloud.SendChatAsync(conversationId, pending, cancellationToken);
```

`invite_next` 傳送“下把邀我”，`when_finished` 傳送“你什麼時候結束”，它們是普通聊天文字，不會執行遊戲邀請。正文最多 2,000 個 Unicode 碼點，不能空白；服務端拒絕偽造傳送者欄位。

網路斷開、超時、5xx 或後續重試遇到 429 時，原請求可能已提交。必須重用**相同序號、操作 UUID 和內容**，不得自動換新序號重發。保留視窗內的重複請求返回同一個訊息和 `Duplicate=true`。

`CloudApiException.NextSendSequence` 保留服務端同步提示：`sequence_conflict` 表示另一個操作佔用該序號；`sequence_gap` 表示跳號；`sequence_retired` 表示該序號已處理且正文已經被裁剪。後者不能恢復原文，也不能靜默重新傳送。多個外掛共用一個帳號時，競爭同一序號僅有一個不同操作成功；衝突後同步並讓使用者明確決定下一次新傳送。

服務端僅保留每個傳送者的一條序號水位，不儲存無限的訊息回執或正文。冪等保證針對完整原請求，不針對更換序號後的舊 UUID。

## 官方訊息與使用者提示

官方會話 `Kind == "official"`，訊息 `Sender.IsOfficial == true`，名稱為“DACT 官方 / 管理員”，使用者 ID 為空。必須根據服務端身份欄位識別，不能透過暱稱判斷。普通客戶端可讀取與 ACK 自己的官方通知，不能冒充官方發信；這類會話的 `NextSendSequence` 為空。管理端直接發信不需要與使用者先成為好友，預設同樣使用 20/3 限制。

聊天訊息由服務端儲存以供管理員人工核查；配置備份仍由客戶端加密，兩者契約不同。本階段沒有自動違規檢測。當前提示原文：

> 請勿利用本功能從事洗錢、刷單、詐騙等違規違法活動。涉嫌違規的訊息將由管理員核查處理。

## 驗證與後續接入

`CloudFriendsSmokeTests` 已納入 PackageSmokeTests；`--friends-only` 可單獨執行序列化/錯誤處理、會話隔離、連線退出和舊服務/網路失敗相容測試。私有服務測試可透過隔離環境變數啟動 C# 真實 HTTP 聯調：覆蓋好友全流程、20/3、重試、ACK、快捷訊息、官方身份；另用升級前服務原始碼驗證登入、加密備份上傳/下載/解密和 404 退避。

另有真實排程競態迴歸：初始化期間，心跳先返回 401、備份請求後返回舊成功時，介面仍保持登入失效。`SetSignedIn` 同時核對當前令牌和封禁狀態，不能靠過期的重新整理結果恢復訪問。

釋出按先服務端後客戶端推進，反向升級也已做相容驗證；好友功能整合於 DACT 0.4.1.0。`--friends-ui-only` 驗證消費/未讀、原請求跨重啟重試、磁碟錯誤、帳號切換與共享配置保護；`--friends-native-only` 配合 DACT_TEST_CIMGUI 執行真實 cimgui 頂欄/抽屜/聊天繪製、視窗邊界、縮放及焦點檢查，明確屬於遊戲外驗證。RPets 與 DACT 兩客戶端的登入、退出、切號及啟動順序已透過隔離聯調；遊戲內視窗拖動、實際輸入、戰鬥焦點、實際提示音和不同客戶端體驗仍需實際驗收。
