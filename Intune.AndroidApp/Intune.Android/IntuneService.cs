using System;
using RestSharp;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Intune.Android
{
    public static class IntuneService
    {
        const string intuneServerUri = @"http://intune-1.apphb.com/";

        public static User SignIn(string email, string password)
        {
            var user = new User { Email = email, Password = password };
            var body = JsonConvert.SerializeObject(user);
            var request = new RestRequest(@"api/user/signin/", Method.POST);
            request.AddParameter("text/json", body, ParameterType.RequestBody);
            var client = new RestClient(intuneServerUri);
            var response = client.Execute<User>(request);
            if (response.StatusCode == HttpStatusCode.OK)
                return response.Data;

            return null;
        }

        public static User RegiterUser(User user)
        {
            var body = JsonConvert.SerializeObject(user);
            var request = new RestRequest(@"api/user/register/", Method.POST);
            request.AddParameter("text/json", body, ParameterType.RequestBody);
            var client = new RestClient(intuneServerUri);
            var response = client.Execute<User>(request);
            if (response.StatusCode == HttpStatusCode.OK)
                return response.Data;

            return null;
        }

        public static void ForgotPassword(string email)
        {
            var request = new RestRequest(@"api/user/forgotpassword/", Method.GET);
            request.AddParameter("email", email);
            var client = new RestClient(intuneServerUri);
            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("Cannot send your password.");
        }

        public static List<Account> GetAllAccounts(int userId, int contactId)
        {
            var request = new RestRequest(@"api/account/allaccounts/", Method.GET);
            request.AddParameter("userId", userId);
            request.AddParameter("contactId", contactId);
            var client = new RestClient(intuneServerUri);
            var response = client.Execute<List<Account>>(request);
            if (response.StatusCode == HttpStatusCode.OK)
                return response.Data as List<Account>;

            return null;
        }
    }
}