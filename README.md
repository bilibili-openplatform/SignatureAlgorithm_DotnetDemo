## 说明 
本项目为哔哩哔哩开放平台文档中各项接口的签名实现案例。
用于验证签名以及基础请求示例。

## 使用范围
本签名示例的覆盖范围为[哔哩哔哩开放平台文档中心](https://open.bilibili.com/doc)中相关接口的签名实现，不包含[直播创作者服务中心](https://open-live.bilibili.com/document/bdb1a8e5-a675-5bfe-41a9-7a7163f75dbf#h1-u5E73u53F0u4ECBu7ECD)中的相关接口，请注意。

## 使用方法
在`Program.cs`头部需要填写的内容中填写注释说明中对应的内容。
```C#
        // 需要填写的内容
        const string AccessKey = ""; // access_key
        const string AccessKeySecret = ""; // access_key_secret    
        const string AccessToken = ""; // Access_Token  
        const string RequestType = "";//对应接口的请求类型，可选值为get或者post
        const string OpenPlatformHttpHost = ""; // 开放平台对应的接口Host，例如接口地址为https://member.bilibili.com/arcopen/fn/user/account/info，则此处填写"https://member.bilibili.com"
        const string OpenPlatformHtppInterface = "";//开放平台对应的接口地址，例如接口地址为https://member.bilibili.com/arcopen/fn/user/account/info，则此处填写"/arcopen/fn/user/account/info"
        const string reqJson = "";//请求参数的body内容json字符串,如为get请求或者求参数为空则填写空字符串
```

- 例如
```C#
        //本示例内容为虚构内容，请根据从开放平台获取的到的实际内容进行填写
        const string AccessKey = "34c0f583f0414123";
        const string AccessKeySecret = "abc7736bb78947d5a4a90690c861c456"; 
        const string AccessToken = "0594436e79c607569b8d387e5f29311";
        const string RequestType = "get";
        const string OpenPlatformHttpHost = "https://member.bilibili.com"; 
        const string OpenPlatformHtppInterface = "/arcopen/fn/user/account/info";
        const string reqJson = "";
```

然后运行即可得到示例结果
- 例如
![image](https://github.com/user-attachments/assets/ce8d803f-c0b5-438e-ab20-c2812c6023bd)


