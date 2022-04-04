using Haeahn.Performance.Transaction.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    internal class WebAPIClient
    {

        //public static readonly HttpClient _client = new HttpClient();
        public static readonly Uri BaseUri = new Uri("http://api.haeahn.com:80/");
        public static readonly string AuthKey = "hubhaeahnrest";


        // 사용자 윈도우 레지스트리에 등록된 PROC Uuid 읽어서 등록된 uuid를 통한 해안 sso 로그인 시도
        public static async Task<LoginResult> UuidLogin()
        {
            var stringContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("UUID", CurrentUser.Instance.Uuid)
            });

            using (var client = new HttpClient())
            {
                client.BaseAddress = BaseUri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP POST
                HttpResponseMessage response = await client.PostAsync("api/uuidlogin", stringContent);
                if (response.IsSuccessStatusCode)
                {
                    // CurrentUser.Instance에 반환된 값들 set
                    var content = await response.Content.ReadAsStringAsync();
                    //var currentUser = JsonConvert.DeserializeObject<CurrentUser>(content);
                    JObject jObject = JObject.Parse(content);

                    var resultCode = jObject.SelectToken("resultCode").ToObject<int>();
                    if (0 == resultCode) // UUID에 매칭되는 해안 사용자 정보 있음
                    {
                        CurrentUser.Instance.EmpNo = jObject.SelectToken("resultMessage").ToObject<string>();
                        CurrentUser.Instance.AutodeskUserId = jObject.SelectToken("resultAUTODESK_ID").ToObject<string>();
                        CurrentUser.Instance.UserName = jObject.SelectToken("resultUserName").ToObject<string>();
                        CurrentUser.Instance.DeptName = jObject.SelectToken("resultDeptName").ToObject<string>();
                        CurrentUser.Instance.TitleName = jObject.SelectToken("resultTitleName").ToObject<string>();
                        CurrentUser.Instance.Email = jObject.SelectToken("resultMail").ToObject<string>();

                        return LoginResult.UuidLoginSucceeded;
                    }
                    else // UUID에 매칭되는 해안 사용자 정보 없음
                    {
                        return LoginResult.UuidLoginFailed;
                    }
                }
                else
                {
                    return LoginResult.UuidLoginFailed;
                }
            }
        }

        // 사용자 Id, 패스워드 받아서 해안 sso 로그인 시도
        public static async Task<LoginResult> LoginSso(string userId, string password)
        {
            var stringContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("UserID", userId),
                new KeyValuePair<string, string>("PW", password),
                new KeyValuePair<string, string>("AuthKey", AuthKey)
            });

            using (var client = new HttpClient())
            {
                client.BaseAddress = BaseUri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP POST
                HttpResponseMessage response = await client.PostAsync("api/loginsso", stringContent);
                if (response.IsSuccessStatusCode)
                {
                    // CurrentUser.Instance에 반환된 값들 set
                    var content = await response.Content.ReadAsStringAsync();
                    JObject jObject = JObject.Parse(content);

                    var resultCode = jObject.SelectToken("resultCode").ToObject<int>();
                    if (0 == resultCode) // UserID, PW에 매칭되는 해안 사용자 정보 있음
                    {
                        CurrentUser.Instance.EmpNo = jObject.SelectToken("resultMessage").ToObject<string>();
                        CurrentUser.Instance.UserName = jObject.SelectToken("resultUserName").ToObject<string>();
                        CurrentUser.Instance.DeptName = jObject.SelectToken("resultDeptName").ToObject<string>();
                        CurrentUser.Instance.TitleName = jObject.SelectToken("resultTitleName").ToObject<string>();
                        CurrentUser.Instance.Email = jObject.SelectToken("resultMail").ToObject<string>();

                        return LoginResult.LoginSsoSucceeded;
                    }
                    else // UserID, PW에 매칭되는 해안 사용자 정보 없음
                    {
                        return LoginResult.LoginSsoFailed;
                    }
                }
                else
                {
                    return LoginResult.LoginSsoFailed;
                }
            }
        }

        // 사용자 사번과 오토데스트 Id를 해안 DB에 등록하고(오토데스트 Id는 등록만 하고 사용은 안 함, 추후 사용을 위해)
        // 사용자 사번으로 UUID를 반환 받음
        public static async Task<LoginResult> HUserLogin()
        {
            var stringContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("EMP_NO", CurrentUser.Instance.EmpNo),
                new KeyValuePair<string, string>("AUTODESK_ID", CurrentUser.Instance.AutodeskUserId)
            });

            using (var client = new HttpClient())
            {
                client.BaseAddress = BaseUri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP POST
                HttpResponseMessage response = await client.PostAsync("api/huserlogin", stringContent);
                if (response.IsSuccessStatusCode)
                {
                    // 성공, 실패해도 일단 별 처리 안 하고 넘어감
                    var content = await response.Content.ReadAsStringAsync();
                    JObject jObject = JObject.Parse(content);

                    var resultCode = jObject.SelectToken("resultCode").ToObject<int>();
                    if (0 == resultCode)
                    {
                        CurrentUser.Instance.Uuid = jObject.SelectToken("resultUUID").ToObject<string>();

                        return LoginResult.HUserLoginSucceeded;
                    }
                    else
                    {
                        return LoginResult.HUserLoginFailed;
                    }
                }
                else
                {
                    return LoginResult.HUserLoginFailed;
                }
            }
        }
    }
}
