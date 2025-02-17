using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Signature_Algorithm_Demo
{

    public class Program
    {

        // 需要填写的内容
        const string AccessKey = ""; // access_key
        const string AccessKeySecret = ""; // access_key_secret    
        const string AccessToken = ""; // Access_Token  
        const string RequestType = "";//对应接口的请求类型，可选值为get或者post
        const string OpenPlatformHttpHost = ""; // 开放平台对应的接口Host，例如接口地址为https://member.bilibili.com/arcopen/fn/user/account/info，则此处填写"https://member.bilibili.com"
        const string OpenPlatformHtppInterface = "";//开放平台对应的接口地址，例如接口地址为https://member.bilibili.com/arcopen/fn/user/account/info，则此处填写"/arcopen/fn/user/account/info"
        const string reqJson = "";//请求参数的body内容json字符串,如为get请求或者求参数为空则填写空字符串


        // 常量定义
        const string AcceptHeader = "Accept";
        const string AuthorizationHeader = "Authorization";
        const string JsonType = "application/json";
        const string BiliVersion = "2.0";
        const string HmacSha256 = "HMAC-SHA256";
        const string BiliTimestampHeader = "x-bili-timestamp";
        const string BiliSignatureMethodHeader = "x-bili-signature-method";
        const string BiliSignatureNonceHeader = "x-bili-signature-nonce";
        const string BiliAccessKeyIdHeader = "x-bili-accesskeyid";
        const string BiliSignVersionHeader = "x-bili-signature-version";
        const string BiliContentMD5Header = "x-bili-content-md5";
        const string AccessTokenHeader = "access-token";

        // 公共头部类
        public class CommonHeader
        {
            public string Accept { get; set; }
            public string Timestamp { get; set; }
            public string SignatureMethod { get; set; }
            public string SignatureVersion { get; set; }
            public string Authorization { get; set; }
            public string Nonce { get; set; }
            public string AccessKeyId { get; set; }
            public string ContentMD5 { get; set; }
            public string AccessToken { set; get; }

            // 将所有字段转换为 map
            public Dictionary<string, string> ToMap()
            {
                return new Dictionary<string, string>
                {
                    {BiliTimestampHeader, Timestamp},
                    {BiliSignatureMethodHeader, SignatureMethod},
                    {BiliSignatureNonceHeader, Nonce},
                    {BiliAccessKeyIdHeader, AccessKeyId},
                    {BiliSignVersionHeader, SignatureVersion},
                    {BiliContentMD5Header, ContentMD5},
                    {AuthorizationHeader, Authorization},
                    {AcceptHeader, Accept},
                    {AccessTokenHeader, AccessToken}
                };
            }

            // 生成需要加密的文本
            public string ToSortedString()
            {
                var sortedMap = ToMap().Where(kvp => kvp.Key.StartsWith("x-bili-")).OrderBy(kvp => kvp.Key).ToList();
                StringBuilder sb = new StringBuilder();
                foreach (var kvp in sortedMap)
                {
                    sb.Append($"{kvp.Key}:{kvp.Value}\n");
                }
                return sb.ToString().TrimEnd('\n');
            }
        }

        // 基础响应类
        public class BaseResp
        {
            public long Code { get; set; }
            public string Message { get; set; }
            public string RequestId { get; set; }
            public string Data { get; set; }
        }

        /// <summary>
        /// 测试签名
        /// </summary>
        /// <param name="Client_ID">用于计算签名的应用ClientID</param>
        /// <param name="App_Secret">用于计算签名的应用Secret</param>
        /// <param name="Nonce">用于计算签名的全网唯一字符串</param>
        /// <param name="TimeStamp">用于计算签名的秒级时间戳</param>
        /// <param name="ReqJson">用于计算签名的应用body内容或者已计算好的md5值</param>
        /// <returns></returns>
        public static string SignatureTest(string Client_ID, string App_Secret, string Nonce, string TimeStamp, string ReqJson)
        {
            var header = new CommonHeader
            {
                Timestamp = string.IsNullOrEmpty(TimeStamp) ? DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() : TimeStamp,
                SignatureMethod = HmacSha256,
                Nonce = string.IsNullOrEmpty(Nonce) ? Guid.NewGuid().ToString() : Nonce,
                AccessKeyId = Client_ID,
                SignatureVersion = BiliVersion,
                ContentMD5 = Regex.IsMatch(ReqJson, @"^[a-fA-F0-9]{32}$") ? ReqJson : Md5(ReqJson)
            };

            header.Authorization = CreateSignature(header, App_Secret);
            return header.Authorization;
        }

        // 主函数
        public static async Task Main(string[] args)
        {
           
            var response = await ApiRequest(reqJson, OpenPlatformHtppInterface, RequestType);
            if (response == null)
            {
                Console.WriteLine("请求失败");
            }
            else
            {
                if (!string.IsNullOrEmpty(response))
                {
                    try
                    {

                        Console.WriteLine($"\nResponse内容:\n");
                        // 解析 JSON 字符串到 JsonDocument
                        using (JsonDocument doc = JsonDocument.Parse(response))
                        {
                            // 创建 JsonWriterOptions，设置为格式化输出
                            JsonWriterOptions options = new JsonWriterOptions
                            {
                                Indented = true
                            };

                            // 创建一个内存流
                            using (var stream = new System.IO.MemoryStream())
                            {
                                // 使用 Utf8JsonWriter 来写入内存流
                                using (var writer = new Utf8JsonWriter(stream, options))
                                {
                                    doc.WriteTo(writer);
                                }

                                // 将内存流转换为字符串
                                string formattedJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());

                                // 输出格式化后的 JSON 到终端
                                Console.WriteLine(formattedJson);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"\n返回的内容可能不为Json格式，原始内容:\n{response}");
                    }
                }
                else
                {

                    Console.WriteLine($"\nResponse内容为空\n");
                }

            }
            while (true)
            {
                Console.ReadKey();
            }
        }

        // HTTP 请求示例方法
        public static async Task<string> ApiRequest(string reqJson, string requestUrl, string Htpp_Type)
        {
            Console.WriteLine($"请求地址：{OpenPlatformHttpHost}{requestUrl}");
            Console.WriteLine($"请求类型：{Htpp_Type}");
            Console.WriteLine($"请求参数：");
            if (!string.IsNullOrEmpty(reqJson))
                Console.WriteLine($"body:\n{reqJson}");
            else
                Console.WriteLine($"body为空\n");

            var header = new CommonHeader
            {
                Accept = JsonType,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                SignatureMethod = HmacSha256,
                SignatureVersion = BiliVersion,
                Authorization = "",
                Nonce = Guid.NewGuid().ToString(),
                AccessKeyId = AccessKey,
                ContentMD5 = Md5(reqJson),
                AccessToken = AccessToken
            };

            header.Authorization = CreateSignature(header, AccessKeySecret);


            Console.WriteLine($"计算生成的签名：{header.Authorization}\n");
            Console.WriteLine($"Header信息：\n");
            var sortedMap = header.ToMap().OrderBy(kvp => kvp.Key).ToList();
            foreach (var item in sortedMap)
            {
                Console.WriteLine($"{item.Key}:{item.Value}");
            }
            Console.WriteLine();
            using (var client = new HttpClient())
            {
                if (Htpp_Type.ToLower() == "post")
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{OpenPlatformHttpHost}{requestUrl}");
                    foreach (var kvp in header.ToMap())
                    {
                        requestMessage.Headers.Add(kvp.Key, kvp.Value);
                    }
                    requestMessage.Content = new StringContent(reqJson, Encoding.UTF8, JsonType);

                    var response = await client.SendAsync(requestMessage);
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return jsonResponse;
                }
                else if (Htpp_Type.ToLower() == "get")
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{OpenPlatformHttpHost}{requestUrl}");
                    requestMessage.Content = new StringContent("", Encoding.UTF8, "application/json");
                    foreach (var kvp in header.ToMap())
                    {
                        requestMessage.Headers.Add(kvp.Key, kvp.Value);
                    }

                    var response = await client.SendAsync(requestMessage);
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return jsonResponse;
                }
                else
                {
                    return null;
                }
            }
        }

        public static void VerifySignature()
        {
            Console.WriteLine("请输入计算签名的Client_ID：");
            string Client_ID = Console.ReadLine();
            Console.WriteLine("请输入计算签名的App_Secret：");
            string App_Secret = Console.ReadLine();
            Console.WriteLine("请输入计算签名的Nonce：");
            string Nonce = Console.ReadLine();
            Console.WriteLine("请输入用于计算签名的body ReqJson或者已计算好的md5内容：");
            string ReqJson = Console.ReadLine();
            Console.WriteLine("请输入计算签名的TimeStamp：");
            string TimeStamp = Console.ReadLine();
            Console.WriteLine("计算签名结果：\n" + SignatureTest(Client_ID, App_Secret, Nonce, TimeStamp, ReqJson));
            Console.WriteLine("\n计算完成，随时可关闭，需要再次计算请重新打开工具");
            while(true)
            {
                Console.ReadKey();
            }
        }

        // 生成Authorization加密串
        public static string CreateSignature(CommonHeader header, string accessKeySecret)
        {
            var sStr = header.ToSortedString();
            Console.WriteLine($"\n用于计算签名的字符串：\n------------------下面用于签名的字符包括换行符------------------\n{sStr}\n------------------上面用于签名的字符包括换行符------------------");
            return HmacSHA256(accessKeySecret, sStr);
        }

        // MD5加密
        public static string Md5(string str)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        // HMAC-SHA256算法
        public static string HmacSHA256(string key, string data)
        {
            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
