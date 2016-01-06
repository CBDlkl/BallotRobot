﻿using System;
using System.Globalization;
using System.Net;
using System.Threading;
using FSLib.Network.Http;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace LiuxiurongRobot
{
    public class CookieRobot : BaseRobot
    {
        public string UserId { set; get; }

        public CookieRobot(string userId)
        {
            UserId = userId;
        }

        public override string RunRobot(string ipProxy = null)
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1000 * new Random().Next(1, 2));
                    var client = new HttpClient();
                    if (!string.IsNullOrWhiteSpace(ipProxy))
                    {
                        //client.Setting.Proxy = new WebProxy(ipProxy);
                        client.CookieContainer.Add(new Cookie
                        {
                            Domain = "art-work.com.cn",
                            Name = "U2i3_2132_tom_wx_vote_vid16_userid",
                            Value = UserId
                        });

                        //"Accept:application/json, text/javascript, */*; q=0.01",
                        //   "Accept-Encoding:gzip, deflate, sdch",
                        //   "Accept-Language:zh-CN,zh;q=0.8",
                        //   "Connection:keep-alive",
                        //   "Host:art-work.com.cn",
                        //   "Referer:http://art-work.com.cn/plugin.php?id=tom_weixin_vote&mod=info&vid=16&tid=254&from=timeline&isappinstalled=1",
                        //   "User-Agent:Mozilla/5.0 (iPhone; CPU iPhone OS 7_0 like Mac OS X; en-us) AppleWebKit/537.51.1 (KHTML, like Gecko) Version/7.0 Mobile/11A465 Safari/9537.53",
                        //   "X-Requested-With:XMLHttpRequest"
                        client.Setting.Headers.Add("User-Agent",
                            "Mozilla/5.0 (iPhone; CPU iPhone OS 7_0 like Mac OS X; en-us) AppleWebKit/537.51.1 (KHTML, like Gecko) Version/7.0 Mobile/11A465 Safari/9537.53");

                    }
                    var demoa = client.Create<string>(HttpMethod.Get, "http://www.cnblogs.com/").Send();
                    Console.WriteLine("测试状态：" + demoa.ReadyState);
                    var hashformate = client.Create<string>(HttpMethod.Get, Settion.RootUrl, data: new
                    {
                        id = "tom_weixin_vote",
                        mod = "info",
                        vid = 16,
                        tid = 254
                    }).Send();
                    string formhash = "", tomhash = "";
                    if (!hashformate.IsValid())
                    {
                        Common.OutLog("获取From值异常");
                        continue;
                    }
                    var doc = new HtmlDocument();
                    doc.LoadHtml(hashformate.Result);
                    formhash = doc.DocumentNode.SelectSingleNode("//input[@name='formhash']").Attributes["value"].Value;
                    tomhash = doc.DocumentNode.SelectSingleNode("//input[@name='tomhash']").Attributes["value"].Value;
                    Common.OutLog("UserID：" + UserId);
                    Common.OutLog("提取FORMHASH值：" + formhash);
                    Common.OutLog("提取TOMHASH值：" + tomhash);

                    var context = client.Create<string>(HttpMethod.Get, Settion.RootUrl, data: new
                    {
                        id = "tom_weixin_vote",
                        mod = "save",
                        vid = 16,
                        formhash,
                        tomhash,
                        act = "tp",
                        tid = 254,
                        userid = UserId
                    });
                    context.Send();
                    if (context.IsValid())
                    {
                        Common.OutLog(context.Result);
                        var model = JsonConvert.DeserializeObject<dynamic>(context.Result);
                        if (model.status == "303")
                        {
                            Common.OutLog("IP被限制，自动切换IP代理！");
                            ipProxy = Common.GetIp();
                        }
                        else if (model.status == "302")
                        {
                            Console.WriteLine("该选手被所锁定！");
                            //ipProxy = GetIp();
                        }
                        else if (model.status == "100")
                        {
                            Console.WriteLine("伪造签证失效！");
                            return null;
                        }
                        else if (model.status == "200")
                        {
                            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " 成功投一票！" +
                                              DateTime.Now.ToString(CultureInfo.InvariantCulture));
                            return ipProxy;
                        }
                        else
                        {
                            Console.WriteLine("状态：" + model?.status);
                        }
                    }
                    else
                    {
                        Common.OutLog("错误！状态码：" + context.Response);
                        ipProxy = Common.GetIp();
                    }
                }

            }
            catch (Exception e)
            {
                Common.OutLog("出现一次错误，跳过本次" + e.Message);
            }
            Common.OutLog();
            return null;
        }
    }
}
