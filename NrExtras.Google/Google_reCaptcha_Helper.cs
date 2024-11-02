using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NrExtras.Google
{
    public static class Google_reCaptcha_Helper
    {
        /// <summary>
        /// Validate reCaptcha token using secret key
        /// </summary>
        /// <param name="token">reCaptcha token</param>
        /// <param name="secretKey">secret key</param>
        /// <returns>true incase of good, false otherwise</returns>
        public static async Task<bool> ValidateRecaptchaAsync(string token, string secretKey)
        {
            using (var client = new HttpClient())
            {
                using (var response = await client.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}",
                    null))
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var recaptchaResponse = JsonConvert.DeserializeObject<RecaptchaResponse>(jsonResponse);

                    // Consider a threshold score for deciding (e.g., 0.5)
                    return recaptchaResponse?.Success == true && recaptchaResponse.Score >= 0.5;
                }
            }
        }

        //reCaptcha response
        private class RecaptchaResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("score")]
            public float Score { get; set; }

            [JsonProperty("action")]
            public string Action { get; set; }

            [JsonProperty("challenge_ts")]
            public DateTime ChallengeTimestamp { get; set; }

            [JsonProperty("hostname")]
            public string Hostname { get; set; }
        }
    }
}