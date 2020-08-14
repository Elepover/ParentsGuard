# 配置指南

## 安装服务

1. 从 [Releases 页面](https://github.com/Elepover/ParentsGuard/releases)下载编译好的二进制文件
2. 将解压的内容放置在 Program Files 等文件夹内
3. 使用 .NET 框架自带的 `InstallUtil.exe` 安装服务<br>ℹ 通常，文件路径为<br>`%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe`<br>则调用 `InstallUtil.exe <ParentsGuard.exe 路径>` 即可自动安装服务（需要管理员权限）
4. 前往 `services.msc` 启动 Parents' Guard 服务，生成配置文件<br>ℹ 服务第一次启动，生成完配置文件后将立即退出，故 Windows 服务管理器将提示错误，可忽略

## 配置服务

<details>
<summary>默认配置文件内容</summary>
```json
{
  "defaultAction": "delete",
  "fileFilter": "*.exe",
  "ignoredLocations": [
    "C:\Windows"
  ]
  "timeout": 300,
  "subscriptionUpdateTimeout": 10,
  "subscriptionIgnoreHttpCode": false,
  "allowSubscriptionUrlChange": false,
  "subscriptions": [
    {
      "updateInterval": 3600,
      "retryCount": 3,
      "enabled": true,
      "url": "https://example.com/subs.json?key=1234567890"
    }
  ],
  "ruleSets": [
    {
      "enabled": true,
      "subscriptionUrl": "https://example.com/subs.json?key=1234567890",
      "fileNameRules": [
        {
          "fileName": "",
          "useRegex": false,
          "caseSensitive": false,
          "containsExtension": true
        }
      ],
      "hashRules": [
        {
          "hashType": "sha256",
          "hash": ""
        }
      ],
      "signatureRules": [
        {
          "dataType": "hash",
          "data": ""
        }
      ],
      "complexRules": [
        {
          "fileNameRules": [
            {
              "fileName": "",
              "useRegex": false,
              "caseSensitive": false,
              "containsExtension": true
            }
          ],
          "hashRules": [
            {
              "hashType": "sha256",
              "hash": ""
            }
          ],
          "signatureRules": [
            {
              "dataType": "hash",
              "data": ""
            }
          ]
        }
      ]
    }
  ]
}
```
<!--GitHub Markdown 能正确解析，Visual Studio Code 不太行-->
</details>

### `defaultAction`

检测到目标文件时采取的动作。

类型: `string`

可选值: `delete`, `block`

`delete`: 删除该文件  
`block`: 使用 Windows 权限系统阻止文件执行

### `fileFilter`

需要匹配屏蔽哪些类型的文件。

类型: `string`

如: `*.exe`, `*.exe|*.dll`, `*`

### `ignoredLocations`

忽略的路径。

类型: `List<string>`

默认包含 `%WINDIR%` 即 `C:\Windows`

### `timeout`

当文件被锁定时的等待时间，单位为秒。

类型: `int`

### `subscriptionUpdateTimeout`

订阅更新的网络请求超时，单位为秒。

类型: `int`

### `subscriptionIgnoreHttpCode`

是否忽略订阅更新时返回的 4xx/5xx HTTP 状态码。

类型: `bool`

### `allowSubscriptionUrlChange`

如果订阅下发新的订阅 URL，是否跟随改变订阅 URL.

类型: `bool`

### `subscriptions`

所有订阅项目。

类型: `List<Subscription>`

#### `updateInterval`

每隔多长时间更新一次订阅，单位为秒。

类型: `int`

#### `retryCount`

订阅更新出错的最大重试次数。

类型: `int`

#### `enabled`

是否启用该订阅及其附属规则。

类型: `bool`

#### `url`

订阅 URL。

类型: `string`

### `ruleSets`

所有屏蔽规则。

类型: `List<RuleSet>`

#### `enabled`

是否启用该规则集。

类型: `bool`

#### `subscriptionUrl`

该规则集对应的订阅 URL.

类型: `string`

#### `fileNameRules`

基于文件名的屏蔽规则。

类型: `FileNameBlockRule`

##### `fileName`

要匹配的文件名。

类型: `string`

##### `useRegex`

是否使用正则表达式。

类型: `bool`

##### `caseSensitive`

文件名是否大小写敏感。

类型: `bool`

##### `containsExtension`

文件名是否包含扩展名。

类型: `bool`

#### `hashRules`

基于文件哈希值的屏蔽规则。

⚠ 此规则命中前将等待文件锁定解除（如：下载完成），对性能也存在一定影响。

类型: `List<HashBlockRule>`

##### `hashType`

哈希类型。

类型: `string`

可选值: `sha1`, `sha256`, `sha384`, `sha512`

##### `hash`

十六进制，不带 `0x` 前导的哈希值，不区分大小写。

类型: `string`

#### `signatureRules`

基于数字签名的屏蔽规则。

⚠ 此规则命中前将等待文件锁定解除。

类型: `List<SignatureBlockRule>`

##### `dataType`

数据类型。

类型: `string`

可选值: `cn`, `regex`, `hash`, `full`

`cn`: 匹配 **C**ommon **N**ame 字段  
`regex`: 使用正则表达式匹配证书 `Subject` 字段的 Distinguished Name  
`hash`: 证书哈希，不区分大小写  
`full`: `base64` 编码的证书数据

#### `complexRules`

复合文件名、哈希、证书规则的复合规则。

类型: `List<ComplexBlockRule>`
