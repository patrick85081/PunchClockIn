# 打卡幫手
## 開發緣由
三級警戒爆發令許多人措手不及，隨著WFH帶來線上打卡的需求，臨時就使用Google Sheet來做工具，
但每天打開表單寻找空位填資料，到了月底就要開始花時間在所有人的記錄裡面檢查有沒有自己忘打卡，變成很不方便的一件事，
從原本只想做個統計找出哪一天忘了打卡，到後來變成打卡小幫手，
從電腦使用者登入自動打上班卡，下班前提醒彈性下班時間，可以在軟件內檢查有沒有忘打卡，手動打卡與填寫日報

## 功能介紹
1. 打卡記錄瀏覽
    * 沒打到卡的 標示紅色提醒
   
      ![Non Punch](./snapshot/NonPunch.jpg)  
     
    * 開關 下班前 30, 5 分鐘提醒
   
      ![30分鐘前通知](./snapshot/30MinuteNotify.jpg)  
   
      ![5分鐘前通知](./snapshot/5MinuteNotify.jpg)  

2. 日報瀏覽

3. 程式內進行上班、下班打卡，和日報填寫

   ![日報填寫](./snapshot/PunchAndDaily.png)

4. 下班忘打卡時，自動補打
    > 下班時間超過一定時間，自動打下班卡

5. 日報、打卡網址連結

6. 配合另一隻Windows服務，偵測到使用者登入，打API給打卡程式，自動化上班打卡

7. 功能設定
   * 使用者設定
   * 開關下班通知功能
   * 開關下班自動打卡功能
   
   ![Setting](./snapshot/Setting.png)  

## 運用技術
   * 使用`Google Sheet API`讀寫表格
   * 使用`LiteDb`輕量級NoSQL資料庫記錄資料
   * 使用`MahApps.Metro`UI控制項
   * 使用`Quartz.Net`做更新與通知排程
   * 使用`ini-parser-netstandard`來取代WinApi讀寫Ini檔案功能
   * 使用`Hardcodet.NotifyIcon.Wpf`來做系統欄通知
   * 使用`Dapplo.Microsoft.Extensions.Hosting.Wpf`來統一`.Net Core泛型主機`程式風格
   * 使用`Autofac`做依賴注入功能
   * 使用`ReactiveUI`做響應式MVVM框架
   * 使用`NLog`做日誌記錄

## 程式用途
   * PunchClockIn - 打卡助手
   * LogonWorkOnService - 使用者登入偵測 (選配，需註冊成Service)

## 設定檔
設定檔位於`Control.ini`中，必備參數如下
``` ini
[App]
# 申請Google API 的授權
ClientSecretFilePath=<Google Sheet Client Secret>
# 打卡的Sheet Id
PunchSpreadsheetId=<Punch Sheet Id>
# 日報填寫的Sheet Id
DailySpreadsheetId=<Daily Sheet Id>
```
