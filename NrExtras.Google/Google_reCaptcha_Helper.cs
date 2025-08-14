using Newtonsoft.Json;

namespace NrExtras.Google
{
    public class Google_reCaptcha_Helper
    {
        /// <summary>
        /// Helper class for validating reCaptcha v3 tokens using the Google reCaptcha API.
        /// </summary>
        public static class ReCaptcha_v3
        {
            const double MinScore = 0.3; // Minimum score to consider a request as valid

            /// <summary>
            /// Result of reCaptcha validation helper class for returning validation result
            /// </summary>
            public class RecaptchaValidationResult
            {
                public bool Success { get; set; }
                public float? Score { get; set; }
                public string? Action { get; set; }
                public string? Hostname { get; set; }
                public string? Error { get; set; }
            }

            /// <summary>
            /// Validate reCaptcha token using secret key
            /// </summary>
            /// <param name="token">reCaptcha token</param>
            /// <param name="secretKey">secret key</param>
            /// <param name="minScore">min score to pass - default is MinScore at the head of this class</param>
            /// <returns>RecaptchaValidationResult object</returns>
            public static async Task<RecaptchaValidationResult> ValidateRecaptchaDetailedAsync(string token, string secretKey, double minScore = MinScore)
            {
                // Validate inputs
                if (minScore < 0 || minScore > 1)
                    throw new ArgumentOutOfRangeException(nameof(minScore), "Minimum score must be between 0 and 1.");
                if (string.IsNullOrWhiteSpace(token))
                    throw new ArgumentException("reCaptcha token cannot be null or empty.", nameof(token));
                if (string.IsNullOrWhiteSpace(secretKey))
                    throw new ArgumentException("reCaptcha secret key cannot be null or empty.", nameof(secretKey));

                // Make a request to the reCAPTCHA API to verify the token
                using var client = new HttpClient();
                using var response = await client.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}",
                    null);
                // Check if the response is successful
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var recaptchaResponse = JsonConvert.DeserializeObject<RecaptchaResponse>(jsonResponse);

                // Check if the response is null or has an invalid format
                if (recaptchaResponse == null)
                    return new RecaptchaValidationResult
                    {
                        Success = false,
                        Error = "Invalid reCAPTCHA response format."
                    };

                // Check if the reCAPTCHA validation was successful and meets the minimum score requirement
                bool success = recaptchaResponse.Success && recaptchaResponse.Score >= MinScore;
                return new RecaptchaValidationResult
                {
                    Success = success,
                    Score = recaptchaResponse.Score,
                    Action = recaptchaResponse.Action,
                    Hostname = recaptchaResponse.Hostname,
                    Error = success ? null : $"Score too low: {recaptchaResponse.Score}"
                };
            }

            //reCaptcha response
            private class RecaptchaResponse
            {
                [JsonProperty("success")]
                public bool Success { get; set; }

                [JsonProperty("score")]
                public float Score { get; set; }

                [JsonProperty("action")]
                public string Action { get; set; } = string.Empty;

                [JsonProperty("challenge_ts")]
                public DateTime ChallengeTimestamp { get; set; }

                [JsonProperty("hostname")]
                public string Hostname { get; set; } = string.Empty;
            }
        }

        /// <summary>
        /// Helper class for validating reCaptcha v2 tokens using the Google reCaptcha API.
        /// </summary>
        public static class ReCaptcha_v2
        {
            /// <summary>
            /// Validates a reCaptcha v2 token using the provided secret key.
            /// </summary>
            /// <param name="token"></param>
            /// <param name="secretKey"></param>
            /// <returns></returns>
            public static async Task<bool> ValidateRecaptchaAsync(string token, string secretKey)
            {
                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(secretKey))
                    return false;

                using var client = new HttpClient();
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", secretKey),
                    new KeyValuePair<string, string>("response", token)
                });

                var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<RecaptchaResponse>(json);

                return result?.Success == true;
            }

            /// <summary>
            /// Response class for reCaptcha v2 validation.
            /// </summary>
            private class RecaptchaResponse
            {
                [JsonProperty("success")]
                public bool Success { get; set; }
            }
        }
    }
}