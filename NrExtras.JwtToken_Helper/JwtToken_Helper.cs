using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NrExtras.JwtToken_Helper
{
    public static class JwtToken_Helper
    {
        public const string ClaimsRole = "UserRole";

        /// <summary>
        /// Generate jwt token valid for x seconds
        /// </summary>
        /// <param name="secretKey">jwt encryption key</param>
        /// <param name="issuer">who creater this token</param>
        /// <param name="audience">audience - to who this token ment</param>
        /// <param name="userId">userId</param>
        /// <param name="role">user role. string can be user, administrator, editor and etc</param>
        /// <param name="expireAfterX_seconds">key valid for x seconds. default is 7 days</param>
        /// <returns>jwt string token</returns>
        public static string GenerateToken(string secretKey, string issuer, string audience, string userId, string role, int expireAfterX_seconds = 604800)
        {
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimsRole, role),
                    }),
                Expires = DateTime.UtcNow.AddSeconds(expireAfterX_seconds),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate jwt token valid for x seconds
        /// </summary>
        /// <param name="secretKey">jwt encryption key</param>
        /// <param name="issuer">who creater this token</param>
        /// <param name="audience">audience - to who this token ment</param>
        /// <param name="userId">userId</param>
        /// <param name="extraClaims">extra claims to add to this jwt</param>
        /// <param name="expireAfterX_seconds">key valid for x seconds. default is 7 days</param>
        /// <returns>jwt string token</returns>
        public static string GenerateToken(string secretKey, string issuer, string audience, string userId, Dictionary<string, object> extraClaims, int expireAfterX_seconds = 604800)
        {
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    }),
                Expires = DateTime.UtcNow.AddSeconds(expireAfterX_seconds),
                Claims = extraClaims,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validate token
        /// </summary>
        /// <param name="secretKey">jwt encryption key</param>
        /// <param name="issuer">who creater this token</param>
        /// <param name="audience">audience - to who this token ment</param>
        /// <param name="token">the jwt token</param>
        /// <returns>true if valid, throw exception if invalid</returns>
        public static bool ValidateCurrentToken(string secretKey, string issuer, string audience, string token)
        {
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = mySecurityKey,
                    LifetimeValidator = CustomLifetimeValidator,
                }, out SecurityToken validatedToken);
            }
            catch
            {
                throw;
            }
            return true;
        }
        //helper function to check jwt expire
        private static bool CustomLifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken tokenToValidate, TokenValidationParameters @param)
        {
            if (expires != null)
            {
                if (expires > DateTime.UtcNow)
                    return true; //still good
                else
                    return false; //jwt expired - retrun false
            }
            else
                return false; //no expire value - false as well
        }
        /// <summary>
        /// Get claims our of jwt
        /// </summary>
        /// <param name="token">jwt token</param>
        /// <returns>claims dictionary</returns>
        /// <exception cref="Exception">incase of invalid token</exception>
        public static Dictionary<string, object> GetClaims(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            //validate token
            if (securityToken == null)
                throw new Exception("Invalid token");

            //convert claims do Dictionary for easier access
            Dictionary<string, object> dic_Claims = new Dictionary<string, object>();
            foreach (var claim in securityToken.Claims)
                dic_Claims.Add(claim.Type, claim.Value);

            //return results
            return dic_Claims;
        }

        /// <summary>
        /// Get user id out of token
        /// </summary>
        /// <param name="token">jwt</param>
        /// <returns>userId</returns>
        /// <exception cref="Exception">incasw no userId found in token</exception>
        public static string GetUserIdClaim(string token)
        {
            string uid = GetClaim(token, "nameid");
            if (string.IsNullOrEmpty(uid)) throw new Exception("No userId found in token");

            return uid;
        }

        /// <summary>
        /// Get specific claim out of token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="claimType">key name of a claim. as default get the ClaimsRole but it can be any name of a claim</param>
        /// <returns>the value of this claim key</returns>
        public static string GetClaim(string token, string claimType = ClaimsRole)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            //validate token
            if (securityToken == null)
                throw new Exception("Invalid token");

            //return results
            var stringClaimValue = securityToken.Claims.First(claim => claim.Type == claimType).Value;
            return stringClaimValue;
        }
    }
}